Shader "Custom/EmissiveOutline"
{
    Properties
    {
        _MainColor      ("Main Color",      Color)        = (0, 0, 0, 1)
        _OutlineColor   ("Outline Color",   Color)        = (0, 0.5, 1, 1)
        _OutlineWidth   ("Outline Width",   Range(0.01, 0.49)) = 0.05
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 3.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            float4 _MainColor;
            float4 _OutlineColor;
            float  _OutlineWidth;
            float  _EmissionIntensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Distance from each edge (0 = at edge, 0.5 = center)
                float left   = uv.x;
                float right  = 1.0 - uv.x;
                float bottom = uv.y;
                float top    = 1.0 - uv.y;

                float minEdge = min(min(left, right), min(bottom, top));

                // 1 on border, 0 in center
                float border = step(minEdge, _OutlineWidth);

                float4 color = lerp(_MainColor, _OutlineColor * _EmissionIntensity, border);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
