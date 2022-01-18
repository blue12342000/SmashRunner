Shader "Custom/TowerWallShader"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _MaskTex("Mask Tex", 2D) = "white" {}
        _MaskDistance("Mask Distance", Range(0, 100)) = 5
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 200

            zwrite on
            ColorMask 0
            CGPROGRAM

            #pragma surface surf _NoLight nolight keepalpha noambient noforwardadd nolightmap novertexlights noshadow

            struct Input
            {
                float2 color:COLOR;
            };

            void surf(Input IN, inout SurfaceOutput o)
            {
            }
            float4 Lighting_NoLight(SurfaceOutput s, float3 lightDir, float atten)
            {
                return 0.0f;
            }
            ENDCG

            zwrite off
            CGPROGRAM
                // Physically based Standard lighting model, and enable shadows on all light types
                #pragma surface surf Lambert vertex:verf alpha:blend

                // Use shader model 3.0 target, to get nicer looking lighting
                #pragma target 3.0

                sampler2D _MainTex;
                sampler2D _MaskTex;
                float _MaskDistance;

                struct Input
                {
                    float2 uv_MainTex;
                    float4 screenPos;
                    float3 worldPos;
                };

                void verf(inout appdata_full v)
                {
                }

                void surf(Input IN, inout SurfaceOutput o)
                {
                    float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                    fixed4 mask = tex2D(_MaskTex, screenUV);
                    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

                    float dist = distance(_WorldSpaceCameraPos, IN.worldPos);
                    float t = dist / _MaskDistance;

                    if (t > 1) t = 1;
                    o.Albedo = c.rgb;
                    o.Alpha = t;
                }

                ENDCG
        }
            FallBack "Diffuse"
}
