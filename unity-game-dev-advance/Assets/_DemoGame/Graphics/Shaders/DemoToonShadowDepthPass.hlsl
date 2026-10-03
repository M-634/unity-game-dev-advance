#ifndef DEMO_TOON_SHADOW_DEPTH_PASS_INCLUDED
#define DEMO_TOON_SHADOW_DEPTH_PASS_INCLUDED

// ShadowCaster / DepthOnly / DepthNormals で共通。
// URP 標準のパスを使わないのは、消滅（_Dissolve）で抜けた部分を影と深度からも抜くため。
// 抜かないと、消えたはずの部分に影と輪郭線が残る

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

// ShadowUtils.SetupShadowCasterConstantBuffer が設定する
float3 _LightDirection;
float3 _LightPosition;

struct ShadowDepthAttributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct ShadowDepthVaryings
{
    float4 positionCS   : SV_POSITION;
    float4 uv           : TEXCOORD0;    // xy: Base / zw: Noise
    half3 normalWS      : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

ShadowDepthVaryings InitShadowDepthVaryings(ShadowDepthAttributes input)
{
    ShadowDepthVaryings output = (ShadowDepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv = float4(TRANSFORM_TEX(input.texcoord, _BaseMap), TRANSFORM_TEX(input.texcoord, _NoiseMap));
    output.normalWS = NormalizeNormalPerVertex(TransformObjectToWorldNormal(input.normalOS));
    return output;
}

void ClipShadowDepth(ShadowDepthVaryings input)
{
#if defined(_ALPHATEST_ON)
    Alpha(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv.xy).a, _BaseColor, _Cutoff);
#endif
    ApplyDissolve(input.uv.zw, _Dissolve);
}

// ---- ShadowCaster ----

ShadowDepthVaryings ShadowPassVertex(ShadowDepthAttributes input)
{
    ShadowDepthVaryings output = InitShadowDepthVaryings(input);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

#if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    output.positionCS = ApplyShadowClamping(positionCS);
    return output;
}

half4 ShadowPassFragment(ShadowDepthVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    ClipShadowDepth(input);
    return 0;
}

// ---- DepthOnly ----

ShadowDepthVaryings DepthOnlyVertex(ShadowDepthAttributes input)
{
    ShadowDepthVaryings output = InitShadowDepthVaryings(input);
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

half DepthOnlyFragment(ShadowDepthVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    ClipShadowDepth(input);
    return input.positionCS.z;
}

// ---- DepthNormals（SSAO と輪郭線が使う）----

ShadowDepthVaryings DepthNormalsVertex(ShadowDepthAttributes input)
{
    ShadowDepthVaryings output = InitShadowDepthVaryings(input);
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

void DepthNormalsFragment(
    ShadowDepthVaryings input
    , out half4 outNormalWS : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out uint outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    ClipShadowDepth(input);

#if defined(_GBUFFER_NORMALS_OCT)
    float2 octNormalWS = PackNormalOctQuadEncode(normalize(input.normalWS));
    outNormalWS = half4(PackFloat2To888(saturate(octNormalWS * 0.5 + 0.5)), 0.0);
#else
    outNormalWS = half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
#endif

    // A は URP が使っていない。輪郭線を弱める量を書く（0 = 線を全部出す。ほかのシェーダーも 0 を書くので今までどおり）
    outNormalWS.a = 1.0h - _OutlineStrength;

#ifdef _WRITE_RENDERING_LAYERS
    outRenderingLayers = EncodeMeshRenderingLayer();
#endif
}

#endif
