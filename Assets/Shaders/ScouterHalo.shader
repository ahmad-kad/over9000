Shader "Scouter/SimpleHalo"
{
    Properties
    {
        _HaloColor ("Halo Color", Color) = (0,1,0,1)
        _PowerLevel ("Power Level", Range(1000, 10000)) = 1000
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1
        _AiEstimatedDepth ("AI Estimated Depth", Range(1, 10)) = 3.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _HaloColor;
            float _PowerLevel;
            float _EdgeSoftness;
            float _AiEstimatedDepth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Simple radial gradient from center (represents person outline)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // Create soft circular outline
                float outlineStrength = 1.0 - smoothstep(0.4 - _EdgeSoftness, 0.4 + _EdgeSoftness, dist);
                outlineStrength *= smoothstep(0.35, 0.4, dist); // Inner cutoff

                // Power-based color intensity
                float powerNormalized = (_PowerLevel - 1000) / 9000;
                float intensity = 0.6 + (powerNormalized * 0.4);

                // AI depth affects outline thickness (closer = thicker outline)
                float depthFactor = saturate(1.0 / (_AiEstimatedDepth * 0.3));
                outlineStrength *= (0.8 + depthFactor * 0.4);

                float4 finalColor = _HaloColor;
                finalColor.a = outlineStrength * intensity;

                return finalColor;
            }
            ENDCG
        }
    }

    Fallback "Transparent/Diffuse"
}
