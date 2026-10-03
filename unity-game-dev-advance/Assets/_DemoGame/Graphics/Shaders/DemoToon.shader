// DemoGame のオブジェクト用シェーダー。プレイヤー・敵・レベル・エフェクトのすべてがこの1本を使う
// 設計：doc/demo-game/05-graphics.md
//
// ・種類の違いはキーワード（_TOON_EFFECT / _ALPHATEST_ON）だけで出す
// ・個体の違いはプロパティで出す（キーワードを増やすとバッチが分かれる）
// ・輪郭線のパスは持たない。輪郭線は画面全体に1回だけ描く（OutlineRendererFeature）
Shader "DemoGame/Toon"
{
    Properties
    {
        [Header(Base)]
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Toggle(_ALPHATEST_ON)] _AlphaClip("Alpha Clip", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5

        [Header(Shade)]
        _ShadeMap("Shade Map", 2D) = "white" {}
        _ShadeMapBlend("Shade Map Blend (0 = Base Map を使う)", Range(0, 1)) = 0
        _ShadeColor("1st Shade Color", Color) = (0.6, 0.6, 0.75, 1)
        _Shade2Color("2nd Shade Color", Color) = (0.4, 0.4, 0.55, 1)
        _ShadeThreshold("1st Shade Threshold", Range(0, 1)) = 0.5
        _Shade2Threshold("2nd Shade Threshold (0 = 2段)", Range(0, 1)) = 0.25
        _ShadeSoftness("Shade Softness", Range(0, 0.2)) = 0.02
        _ReceiveShadowStrength("Receive Shadow Strength", Range(0, 1)) = 1
        _AdditionalLightStrength("Additional Light Strength", Range(0, 2)) = 1
        _AdditionalLightThreshold("Additional Light Threshold", Range(0, 1)) = 0.1
        _OcclusionStrength("SSAO Strength", Range(0, 1)) = 0.3

        [Header(Outline)]
        _OutlineStrength("Outline Strength (0 = 線なし)", Range(0, 1)) = 1

        [Header(Rim)]
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.1, 10)) = 4
        _RimStrength("Rim Strength", Range(0, 1)) = 0

        [Header(Emission)]
        _EmissionMap("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)

        [Header(Hit Flash  Runtime)]
        _HitFlashColor("Hit Flash Color", Color) = (1, 1, 1, 1)
        _HitFlash("Hit Flash", Range(0, 1)) = 0

        [Header(Dissolve  Runtime)]
        _NoiseMap("Noise Map", 2D) = "white" {}
        _Dissolve("Dissolve", Range(0, 1)) = 0
        [HDR] _DissolveEdgeColor("Dissolve Edge Color", Color) = (2, 0.8, 0.2, 1)
        _DissolveEdgeWidth("Dissolve Edge Width", Range(0, 0.3)) = 0.05

        [Header(Effect)]
        [Toggle(_TOON_EFFECT)] _ToonEffect("Effect (Unlit)", Float) = 0
        _ScrollSpeed("Scroll Speed (xy: Base, zw: Noise)", Vector) = (0, 0, 0, 0)
        _SoftParticleDistance("Soft Particle Distance (0 = Off)", Float) = 0

        [Header(Render State)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ToonPassVertex
            #pragma fragment ToonPassFragment

            // 種類（ローカルキーワードはこの2つだけ）
            #pragma shader_feature_local _TOON_EFFECT
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            // URP の機能を受けるのに最低限必要なもの
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "DemoToonInput.hlsl"
            #include "DemoToonForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "DemoToonInput.hlsl"
            #include "DemoToonShadowDepthPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing

            #include "DemoToonInput.hlsl"
            #include "DemoToonShadowDepthPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile_instancing

            #include "DemoToonInput.hlsl"
            #include "DemoToonShadowDepthPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex UniversalVertexMeta
            #pragma fragment ToonFragmentMeta
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "DemoToonInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

            half4 ToonFragmentMeta(Varyings input) : SV_Target
            {
                MetaInput metaInput;
                metaInput.Albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                metaInput.Emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                return UniversalFragmentMeta(input, metaInput);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
