#ifndef DEMO_TOON_LIGHTING_INCLUDED
#define DEMO_TOON_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct ToonSurface
{
    half3 baseColor;    // 光が当たっている面の色
    half3 shadeColor;   // 1段目の陰の色
    half3 shade2Color;  // 2段目の陰の色
    half3 emission;
};

// 境目だけを少しぼかした階段
half ToonStep(half value, half threshold)
{
    return smoothstep(threshold - _ShadeSoftness, threshold + _ShadeSoftness, value);
}

// 主光源：Half Lambert に影を掛けてから、2つの閾値で3段に分ける。
// 影と陰影を同じ段にそろえるので、影の縁だけ別の色になる破綻が起きない
half3 ToonMainLight(Light light, half3 normalWS, ToonSurface surface)
{
    half halfLambert = dot(normalWS, light.direction) * 0.5h + 0.5h;
    half shadow = lerp(1.0h, light.shadowAttenuation, _ReceiveShadowStrength);
    half value = halfLambert * shadow;

    half lit1 = ToonStep(value, _ShadeThreshold);
    // _Shade2Threshold が 0 なら常に 1 になり、2段（Base / 1st Shade）だけになる
    half lit2 = ToonStep(value, _Shade2Threshold);

    half3 color = lerp(surface.shade2Color, lerp(surface.shadeColor, surface.baseColor, lit1), lit2);
    return color * light.color;
}

// 追加ライト（灯の Point Light）：当たっているか、いないかの1段だけ
half3 ToonAdditionalLight(Light light, half3 normalWS, ToonSurface surface)
{
    half halfLambert = dot(normalWS, light.direction) * 0.5h + 0.5h;
    half value = halfLambert * light.distanceAttenuation * light.shadowAttenuation;
    half lit = ToonStep(value, _AdditionalLightThreshold);

    return surface.baseColor * light.color * lit * _AdditionalLightStrength;
}

half3 ToonAdditionalLights(InputData inputData, half3 normalWS, ToonSurface surface)
{
    half3 color = 0;

#if defined(_ADDITIONAL_LIGHTS)
    half4 shadowMask = half4(1, 1, 1, 1);
    uint pixelLightCount = GetAdditionalLightsCount();

    #if defined(_LIGHT_LAYERS)
    uint meshRenderingLayers = GetMeshRenderingLayer();
    #endif

    #if USE_CLUSTER_LIGHT_LOOP
    // Forward+ では、平行光源の追加ライトだけはクラスタの外で回す
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        #if defined(_LIGHT_LAYERS)
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
            color += ToonAdditionalLight(light, normalWS, surface);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        #if defined(_LIGHT_LAYERS)
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
            color += ToonAdditionalLight(light, normalWS, surface);
        }
    LIGHT_LOOP_END
#endif

    return color;
}

// 環境光：法線ではなく上向き固定で取る。法線で取るとグラデーションが出て、段が崩れる
half3 ToonAmbient(float2 staticLightmapUV, half3 normalWS)
{
#if defined(LIGHTMAP_ON)
    return SampleLightmap(staticLightmapUV, normalWS);
#else
    return EvaluateAmbientProbeSRGB(half3(0, 1, 0));
#endif
}

half ToonOcclusion(float2 normalizedScreenSpaceUV)
{
#if defined(_SCREEN_SPACE_OCCLUSION)
    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUV);
    return lerp(1.0h, aoFactor.indirectAmbientOcclusion, _OcclusionStrength);
#else
    return 1.0h;
#endif
}

// リム：輪郭側だけを段で切って足す。キャラを暗い背景から浮かせる
half3 ToonRim(half3 normalWS, half3 viewDirWS)
{
    half rim = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _RimPower);
    return _RimColor.rgb * ToonStep(rim, 0.5h) * _RimStrength;
}

#endif
