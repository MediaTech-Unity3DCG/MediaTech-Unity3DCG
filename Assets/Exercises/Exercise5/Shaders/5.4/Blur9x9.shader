Shader "Hidden/Exercise5/Blur9x9"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // x = 1/width, y = 1/height
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
            static float kernelWeights[9] = {1.0/256,8.0/256,28.0/256,56.0/256,70.0/256,56.0/256,28.0/256,8.0/256,1.0/256};
            float4 fragX (v2f i) : SV_Target
            {
                float2 texelSize = float2(_MainTex_TexelSize.x, 0);
                float4 col = float4(0,0,0,0);
                // Gaussian weights for 9 samples
                [unroll]
                for (int j = -4; j <= 4; j++)
                {
                    col += tex2D(_MainTex, i.uv + texelSize * j) * kernelWeights[j + 4];
                }
                return col;
            }
            float4 fragY (v2f i) : SV_Target
            {
                float2 texelSize = float2(0, _MainTex_TexelSize.y);
                float4 col = float4(0,0,0,0);
                // Gaussian weights for 9 samples
                [unroll]
                for (int j = -4; j <= 4; j++)
                {
                    col += tex2D(_MainTex, i.uv + texelSize * j) * kernelWeights[j + 4];
                }
                return col;
            }
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragX
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragY
            ENDCG
        }
    }
}
