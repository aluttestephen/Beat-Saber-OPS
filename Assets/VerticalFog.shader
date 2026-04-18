Shader "Custom/VerticalFog"
{
    Properties
    {
        _FogColor   ("Fog Color",   Color)         = (0.05, 0, 0.15, 1)
        _FogStart   ("Fog Start Y", Range(-5, 10)) = 0.0
        _FogEnd     ("Fog End Y",   Range(-5, 10)) = 3.0
        _FogDensity ("Fog Density", Range(0, 1))   = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
            };

            float4 _FogColor;
            float  _FogStart;
            float  _FogEnd;
            float  _FogDensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos    = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS    = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos       = worldPos;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = saturate((IN.worldPos.y - _FogStart) / (_FogEnd - _FogStart));
                float alpha = (1.0 - t) * _FogDensity;
                return half4(_FogColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
