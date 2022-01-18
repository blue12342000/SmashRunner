Shader "Unlit/OutLineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ActiveRadius("ActiveRadius", Float) = 10
        _OutlineTex("OutlineTexture", 2D) = "white" {}
        _OutlineColor("OutlineColor", Color) = (1,1,1,1)
        _OutlineSpeed("OutlineSpeed", Range(0,2)) = 1
        _Outline("Outline", Range(0, 1)) = 0.02
        _FarOutlineScale("FarOutlineScale", Range(1, 2.5)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Name "Outline Pass"
            ZWrite off
            ZTest Always
            cull front
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv_OutlineTex : TEXCOORD1;
                float4 color : COLOR;
            };

            float _ActiveRadius;
            sampler2D _OutlineTex;
            float4 _OutlineColor;
            float _Outline;
            float _OutlineSpeed;
            float _FarOutlineScale;

            v2f vert(appdata_full v)
            {
                v2f o;
                float outline = _Outline * (1 + (_FarOutlineScale - 1) * UnityObjectToClipPos(v.vertex).w);
                o.vertex = UnityObjectToClipPos(v.vertex + (v.normal + v.tangent.xyz * v.tangent.w) * outline);
                o.uv_OutlineTex = v.texcoord1.xy;

                o.color = float4(0, 0, 0, 0);
                float dist = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex)) / _ActiveRadius - 1;
                if (dist < 0) { o.color.a = 1; }
                else if (dist < 1) { o.color.a = 1 - dist; }
                else { o.color.a = 0; }
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 outlineAlpha = tex2D(_OutlineTex, i.uv_OutlineTex + _Time.y * _OutlineSpeed);
                float4 final = float4(_OutlineColor.rgb * outlineAlpha.rgb,1);
                final.a = outlineAlpha.r * i.color.a;
                return final;
            }
            ENDCG
        }

        Name "Surface Pass"
        Cull back
        CGPROGRAM
        #include "UnityCG.cginc"
        #pragma surface surf Outline vertex:vert
        
        float4 _Color;
        sampler2D _MainTex;
        
        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void vert(inout appdata_full v)
        {
            v.tangent.xyz = v.normal;
        }
        
        void surf(Input In, inout SurfaceOutput o)
        {
            float4 c = tex2D(_MainTex, In.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        
        float4 LightingOutline(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            float ndotl = dot(s.Normal, lightDir) * 0.5 + 0.5;
        
            if (ndotl > 0.9) {
                ndotl = 0.8;
            }
            else if (ndotl > 0.6) {
                ndotl = 0.6;
            }
            else if (ndotl > 0.4) {
                ndotl = 0.3;
            }
            else if (ndotl > 0.2) {
                ndotl = 0.15;
            }
            else {
                ndotl = 0;
            }

            float4 final;
            final.rgb = s.Albedo * ndotl * _LightColor0.rgb;
            final.a = s.Alpha;
        
            return final;
        }
        ENDCG
    }
}
