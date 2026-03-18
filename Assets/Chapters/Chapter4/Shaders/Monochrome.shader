Shader "Hidden/Monochrome"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                o.uv = v.uv * float2(1, -1) + float2(0, 1); // Flip UV for Unity
                return o;
            }

            sampler2D _MainTex;
            float rgbToLuminance(float3 color) {
                return dot(color, float3(0.299, 0.587, 0.114));
            }
            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float luminance = rgbToLuminance(col.rgb);
                return float4(luminance, luminance, luminance, col.a);
            }
            ENDCG
        }
    }
}
