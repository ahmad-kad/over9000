Shader "Custom/DBZ_AnimeGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _ScanLineSpeed ("Scan Line Speed", Range(0, 10)) = 2.0
        _RGBShift ("RGB Shift Amount", Range(0, 0.01)) = 0.005
        _NoiseAmount ("Noise Amount", Range(0, 1)) = 0.1
        _Pixelation ("Pixelation", Range(1, 100)) = 50
        _Distortion ("Distortion", Range(0, 0.1)) = 0.02
        _EnergyPulse ("Energy Pulse", Range(0, 5)) = 1.0
        _TimeScale ("Time Scale", Range(0, 5)) = 1.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _GlitchIntensity;
            float _ScanLineSpeed;
            float _RGBShift;
            float _NoiseAmount;
            float _Pixelation;
            float _Distortion;
            float _EnergyPulse;
            float _TimeScale;

            // Simple noise function
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Energy pulse function
            float energyPulse(float time)
            {
                return sin(time * _EnergyPulse) * 0.5 + 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _TimeScale;

                // Pixelation effect
                float2 pixelUV = floor(i.uv * _Pixelation) / _Pixelation;

                // Scan lines
                float scanLine = sin(i.uv.y * 100.0 + time * _ScanLineSpeed) * 0.1;
                float verticalScan = sin(i.uv.x * 50.0 + time * _ScanLineSpeed * 0.5) * 0.05;

                // RGB shift with energy pulsing
                float shiftAmount = _RGBShift * _GlitchIntensity * energyPulse(time);
                float2 rUV = i.uv + float2(shiftAmount, 0);
                float2 gUV = i.uv;
                float2 bUV = i.uv - float2(shiftAmount, 0);

                // Add distortion
                float distortion = sin(time + i.uv.y * 10) * _Distortion * _GlitchIntensity;
                rUV.x += distortion;
                gUV.x += distortion * 0.5;
                bUV.x -= distortion;

                // Sample colors with bounds checking
                fixed4 colR = tex2D(_MainTex, clamp(rUV, 0, 1));
                fixed4 colG = tex2D(_MainTex, clamp(gUV, 0, 1));
                fixed4 colB = tex2D(_MainTex, clamp(bUV, 0, 1));

                // Combine RGB channels
                fixed4 col = fixed4(colR.r, colG.g, colB.b, 1.0);

                // Add noise
                float n = noise(i.uv + time) * _NoiseAmount * _GlitchIntensity;
                col.rgb += n;

                // Add scan line effects
                col.rgb += scanLine + verticalScan;

                // Energy interference lines
                float interference = sin(i.uv.y * 200 + time * 3) * energyPulse(time) * _GlitchIntensity * 0.2;
                col.rgb += interference;

                // Boost contrast for anime-style effect
                col.rgb = pow(col.rgb, 1.2);

                return col;
            }
            ENDCG
        }
    }
}
