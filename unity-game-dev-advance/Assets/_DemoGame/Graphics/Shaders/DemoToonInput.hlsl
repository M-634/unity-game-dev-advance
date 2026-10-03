#ifndef DEMO_TOON_INPUT_INCLUDED
#define DEMO_TOON_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

// SRP Batcher の条件：UnityPerMaterial の中身は「全パス・全バリアントで同じ」にする。
// 使わないパスでも削らないこと。1つでもずれると、このシェーダーは SRP Batcher 非対応になる
CBUFFER_START(UnityPerMaterial)
    // 基本色
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _Cutoff;

    // 陰影
    half4 _ShadeColor;
    half4 _Shade2Color;
    half _ShadeMapBlend;
    half _ShadeThreshold;
    half _Shade2Threshold;
    half _ShadeSoftness;
    half _ReceiveShadowStrength;
    half _AdditionalLightStrength;
    half _AdditionalLightThreshold;
    half _OcclusionStrength;

    // 輪郭線の強さ（DepthNormals の A に 1 - 強さ を書く。05-graphics.md §5.5）
    half _OutlineStrength;

    // リム
    half4 _RimColor;
    half _RimPower;
    half _RimStrength;

    // 発光
    half4 _EmissionColor;

    // 被弾（実行時にスクリプトから変える）
    half4 _HitFlashColor;
    half _HitFlash;

    // 消滅（実行時にスクリプトから変える）
    float4 _NoiseMap_ST;
    half _Dissolve;
    half4 _DissolveEdgeColor;
    half _DissolveEdgeWidth;

    // エフェクト
    float4 _ScrollSpeed;
    half _SoftParticleDistance;
CBUFFER_END

// _BaseMap / _EmissionMap は SurfaceInput.hlsl が宣言している
TEXTURE2D(_ShadeMap);       SAMPLER(sampler_ShadeMap);
TEXTURE2D(_NoiseMap);       SAMPLER(sampler_NoiseMap);

// 消滅の判定。_Dissolve が 0 のあいだは何もしない
// 戻り値は縁の光り具合（0〜1）
half ApplyDissolve(float2 noiseUV, half amount)
{
    if (amount <= 0.0h)
    {
        return 0.0h;
    }

    half noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, noiseUV).r;
    half diff = noise - amount;
    clip(diff);

    return 1.0h - smoothstep(0.0h, max(_DissolveEdgeWidth, 1e-4h), diff);
}

#endif
