// 혼잡·온도 같은 0~1 스칼라를 색 램프(파랑→청록→녹→황→적)로 그리는 URP Unlit 셰이더.
// docs/phase3/06-shadergraph-viz.html 의 "히트맵" 절과 같은 아이디어를 손코딩으로 구현한 것.
// 에디터에서는 같은 그래프를 Shader Graph 로 만들 수 있다.
Shader "FabViz/Heatmap"
{
    Properties
    {
        _Value ("Congestion", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float _Value;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float v = saturate(_Value);
                half3 c;
                if (v < 0.25)
                {
                    c = lerp(half3(0.10, 0.30, 0.85), half3(0.10, 0.75, 0.80), v / 0.25);
                }
                else if (v < 0.5)
                {
                    c = lerp(half3(0.10, 0.75, 0.80), half3(0.20, 0.80, 0.25), (v - 0.25) / 0.25);
                }
                else if (v < 0.75)
                {
                    c = lerp(half3(0.20, 0.80, 0.25), half3(0.95, 0.80, 0.15), (v - 0.5) / 0.25);
                }
                else
                {
                    c = lerp(half3(0.95, 0.80, 0.15), half3(0.90, 0.18, 0.13), (v - 0.75) / 0.25);
                }
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
