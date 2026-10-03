// 画面全体の輪郭線。OutlinePass から全画面に1回だけ描く
// 設計：doc/demo-game/05-graphics.md §5
Shader "Hidden/DemoGame/Outline"
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
            Name "Outline"

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment OutlineFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            // OutlinePass が OutlineVolume の値を毎フレーム入れる
            half _OutlineIntensity;
            half4 _OutlineColor;
            float _OutlineThickness;
            float _OutlineDepthThreshold;
            float _OutlineNormalThreshold;
            float _OutlineFadeDistance;

            float SampleEyeDepth(float2 uv)
            {
                return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
            }

            // DemoGame/Toon の DepthNormals が A に書いた「線を弱める量」（05-graphics.md §5.5）
            float SampleOutlineWeaken(float2 uv)
            {
                return saturate(SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_PointClamp, UnityStereoTransformScreenSpaceTex(uv)).a);
            }

            half4 OutlineFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                // 1080p 基準のピクセル数を、今の解像度に換算する
                float2 offset = _ScreenSize.zw * _OutlineThickness * (_ScreenSize.y / 1080.0);

                // Roberts Cross：斜めの2組の差を取る
                float2 uv0 = uv + float2(-offset.x, -offset.y);
                float2 uv1 = uv + float2( offset.x,  offset.y);
                float2 uv2 = uv + float2( offset.x, -offset.y);
                float2 uv3 = uv + float2(-offset.x,  offset.y);

                float d0 = SampleEyeDepth(uv0);
                float d1 = SampleEyeDepth(uv1);
                float d2 = SampleEyeDepth(uv2);
                float d3 = SampleEyeDepth(uv3);
                float centerDepth = SampleEyeDepth(uv);

                // 深度は「差」ではなく「比」で見る。遠景ほど差が大きくなり、線が出すぎるため
                float depthEdge = (abs(d0 - d1) + abs(d2 - d3)) / max(centerDepth, 1e-3);
                depthEdge = step(_OutlineDepthThreshold, depthEdge);

                float3 n0 = SampleSceneNormals(uv0);
                float3 n1 = SampleSceneNormals(uv1);
                float3 n2 = SampleSceneNormals(uv2);
                float3 n3 = SampleSceneNormals(uv3);
                float normalEdge = length(n0 - n1) + length(n2 - n3);
                normalEdge = step(_OutlineNormalThreshold, normalEdge);

                float edge = max(depthEdge, normalEdge);

                // オブジェクトごとの線の強さ（DepthNormals の A ＝ 線を弱める量）。まわりの4点と中心でいちばん弱める物に合わせる
                float weaken = SampleOutlineWeaken(uv);
                weaken = max(weaken, max(SampleOutlineWeaken(uv0), SampleOutlineWeaken(uv1)));
                weaken = max(weaken, max(SampleOutlineWeaken(uv2), SampleOutlineWeaken(uv3)));
                edge *= 1.0 - weaken;

                // 空（何も描かれていない所）には線を出さない
                #if UNITY_REVERSED_Z
                    edge *= step(1e-5, SampleSceneDepth(uv));
                #else
                    edge *= step(SampleSceneDepth(uv), 1.0 - 1e-5);
                #endif

                // 遠くの線は薄くする
                edge *= 1.0 - saturate((centerDepth - _OutlineFadeDistance) / max(_OutlineFadeDistance, 1e-3));

                // 黒で塗らず、下の色を暗くする
                half3 lineColor = sceneColor.rgb * _OutlineColor.rgb;
                sceneColor.rgb = lerp(sceneColor.rgb, lineColor, edge * _OutlineIntensity);
                return sceneColor;
            }
            ENDHLSL
        }
    }
}
