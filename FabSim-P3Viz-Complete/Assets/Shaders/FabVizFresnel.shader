// 시선과 스칠수록(가장자리) 빛나는 프레넬 림 셰이더. URP Transparent.
// docs/phase3/06-shadergraph-viz.html 의 "선택 하이라이트/아웃라인", "반투명 존",
// "시스루/X-ray" 를 하나의 손코딩 셰이더로 커버한다 — 존은 base alpha를 올리고,
// 선택 헤일로는 rim만 강하게 쓰면 된다. 에디터에서는 Fresnel 노드로 동일하게 만든다.
Shader "FabViz/FresnelRim"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.10, 0.55, 0.85, 0.12)
        _RimColor ("Rim Color", Color) = (0.35, 0.90, 1.0, 1.0)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _RimColor;
            float _RimPower;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = pos.positionCS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(pos.positionWS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 n = normalize(IN.normalWS);
                float3 v = normalize(IN.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(n, v)), _RimPower);
                half3 col = _BaseColor.rgb + _RimColor.rgb * fresnel;
                half a = saturate(_BaseColor.a + fresnel);
                return half4(col, a);
            }
            ENDHLSL
        }
    }
}
