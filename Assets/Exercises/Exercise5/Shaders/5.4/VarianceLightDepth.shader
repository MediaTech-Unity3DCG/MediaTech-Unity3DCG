Shader "Unlit/Exercise5/VarianceLightDepth"
{
    Properties{}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float depth : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.depth  = -UnityObjectToViewPos(v.vertex).z;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float depth = i.depth;
                float moment1 = depth;
                float moment2 = depth * depth;
                return float4(moment1, moment2, 0, 0);
            }
            ENDCG
        }
    }
}
