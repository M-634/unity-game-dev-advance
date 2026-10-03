// チャージ中に画面の縁が燃える演出。FlameVignettePass から全画面に1回だけ描く
// 設計：doc/demo-game/11-lighting.md §5.3
Shader "Hidden/DemoGame/FlameVignette"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "FlameVignette"

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment FlameFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_FlameNoise);
            SAMPLER(sampler_FlameNoise);

            // FlameVignettePass が FlameVignetteVolume の値を毎フレーム入れる
            half _FlameIntensity;
            half4 _FlameInnerColor;
            half4 _FlameOuterColor;
            float _FlameWidth;
            float _FlameBottomBoost;
            float _FlameSpeed;
            float _FlameNoiseScale;

            half4 FlameFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // 画面の縁への近さ（0：中央側 ／ 1：縁）。下側を強く
                float2 centered = abs(uv * 2.0 - 1.0);
                float edge = max(centered.x, centered.y);
                float near = saturate((edge - (1.0 - _FlameWidth)) / max(_FlameWidth, 1e-3));
                near += (1.0 - uv.y) * _FlameBottomBoost * saturate(1.0 - uv.y * 2.0);

                // ノイズを上へ流して炎の形にする。2つの細かさを重ねる
                float aspect = _ScreenSize.x / max(_ScreenSize.y, 1.0);
                float2 noiseUV = float2(uv.x * aspect, uv.y) * _FlameNoiseScale;
                float t = _Time.y * _FlameSpeed;
                half n1 = SAMPLE_TEXTURE2D(_FlameNoise, sampler_FlameNoise, noiseUV + float2(0.0, -t)).r;
                half n2 = SAMPLE_TEXTURE2D(_FlameNoise, sampler_FlameNoise, noiseUV * 2.1 + float2(0.37, -t * 1.7)).r;
                half noise = n1 * 0.65 + n2 * 0.35;

                // 強さが上がるほど内側まで燃える
                float flame = near * (0.6 + _FlameIntensity * 0.8) - noise * 0.7;

                // セルルックに合わせて、境目のくっきりした2段にする
                half outer = smoothstep(0.0, 0.04, flame);
                half inner = smoothstep(0.25, 0.29, flame);

                half3 color = lerp(_FlameOuterColor.rgb, _FlameInnerColor.rgb, inner) * outer;
                sceneColor.rgb += color * _FlameIntensity;
                return sceneColor;
            }
            ENDHLSL
        }
    }
}
