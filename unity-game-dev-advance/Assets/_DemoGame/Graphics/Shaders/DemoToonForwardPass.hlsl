#ifndef DEMO_TOON_FORWARD_PASS_INCLUDED
#define DEMO_TOON_FORWARD_PASS_INCLUDED

#include "DemoToonLighting.hlsl"
#if defined(_TOON_EFFECT)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#endif

struct Attributes
{
    float4 positionOS       : POSITION;
    float3 normalOS         : NORMAL;
    // xy: UV / zw: パーティクルの Custom Data（Custom Vertex Streams の Custom1.xy）
    float4 texcoord         : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    half4 color             : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS       : SV_POSITION;
    float4 uv               : TEXCOORD0;    // xy: Base / zw: Noise
    float3 positionWS       : TEXCOORD1;
    half3 normalWS          : TEXCOORD2;
    half fogFactor          : TEXCOORD3;
#if defined(_TOON_EFFECT)
    half4 color             : TEXCOORD4;
    half2 customData        : TEXCOORD5;    // x: 消滅の量 / y: 発光の強さ
    float4 positionNDC      : TEXCOORD6;
#endif
#if defined(LIGHTMAP_ON)
    float2 staticLightmapUV : TEXCOORD7;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings ToonPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

    float2 baseUV = TRANSFORM_TEX(input.texcoord.xy, _BaseMap);
    float2 noiseUV = TRANSFORM_TEX(input.texcoord.xy, _NoiseMap);
    // 時間で流す（エフェクト用。不透明は 0 のまま）
    baseUV += _ScrollSpeed.xy * _Time.y;
    noiseUV += _ScrollSpeed.zw * _Time.y;

    output.positionCS = vertexInput.positionCS;
    output.positionWS = vertexInput.positionWS;
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
    output.uv = float4(baseUV, noiseUV);
    output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

#if defined(_TOON_EFFECT)
    output.color = input.color;
    output.customData = input.texcoord.zw;
    output.positionNDC = vertexInput.positionNDC;
#endif

#if defined(LIGHTMAP_ON)
    output.staticLightmapUV = input.staticLightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
#endif

    return output;
}

#if defined(_TOON_EFFECT)

// エフェクト：ライティングを通さない。色・消滅・発光はパーティクルの頂点から受け取る
half4 ToonEffectFragment(Varyings input)
{
    half4 base = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv.xy) * _BaseColor * input.color;

    half edge = ApplyDissolve(input.uv.zw, _Dissolve + input.customData.x);

    half3 color = base.rgb * (1.0h + input.customData.y) + _EmissionColor.rgb;
    color += _DissolveEdgeColor.rgb * edge;

    half alpha = base.a;

    // ソフトパーティクル：地面との交線を消す
    if (_SoftParticleDistance > 0.0h)
    {
        float2 screenUV = input.positionNDC.xy / input.positionNDC.w;
        float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
        float selfDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
        alpha *= saturate((sceneDepth - selfDepth) / _SoftParticleDistance);
    }

    color = MixFog(color, InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor));
    return half4(color, alpha);
}

#else

half4 ToonOpaqueFragment(Varyings input)
{
    half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv.xy);
    half alpha = Alpha(baseSample.a, _BaseColor, _Cutoff);

    half edge = ApplyDissolve(input.uv.zw, _Dissolve);

    half3 shadeSource = lerp(baseSample.rgb, SAMPLE_TEXTURE2D(_ShadeMap, sampler_ShadeMap, input.uv.xy).rgb, _ShadeMapBlend);

    ToonSurface surface;
    surface.baseColor = baseSample.rgb * _BaseColor.rgb;
    surface.shadeColor = shadeSource * _ShadeColor.rgb;
    surface.shade2Color = shadeSource * _Shade2Color.rgb;
    surface.emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv.xy).rgb * _EmissionColor.rgb;

    half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

    InputData inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    inputData.normalWS = normalWS;
    inputData.viewDirectionWS = viewDirWS;
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

    float2 staticLightmapUV = 0;
#if defined(LIGHTMAP_ON)
    staticLightmapUV = input.staticLightmapUV;
#endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, half4(1, 1, 1, 1));

    half3 color = ToonMainLight(mainLight, normalWS, surface);
    color += ToonAdditionalLights(inputData, normalWS, surface);
    color += ToonAmbient(staticLightmapUV, normalWS) * surface.baseColor;
    color *= ToonOcclusion(inputData.normalizedScreenSpaceUV);
    color += ToonRim(normalWS, viewDirWS);
    color += surface.emission;

    color = lerp(color, _HitFlashColor.rgb, _HitFlash);
    color += _DissolveEdgeColor.rgb * edge;

    color = MixFog(color, InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor));
    return half4(color, alpha);
}

#endif

half4 ToonPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if defined(_TOON_EFFECT)
    return ToonEffectFragment(input);
#else
    return ToonOpaqueFragment(input);
#endif
}

#endif
