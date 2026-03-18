Shader "Unlit/Exercise5/ParaboloidLightDepth"
{
    Properties{}
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass {
            ColorMask 0
            Cull Off
            ZWrite On
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "../DepthUtil.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            #define UV_BIAS 0.04

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float3 viewPos  : TEXCOORD0;
            };

            v2f DualParaboloidMapping(float4 pos)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, pos).xyz;
                float3 viewPos  = mul(UNITY_MATRIX_V, float4(worldPos, 1.0)).xyz;
                float len = length(viewPos);
                float3 dir = viewPos * rcp(len);
                float absZ = abs(dir.z);
                o.vertex.xy = (1.0 + UV_BIAS) * dir.xy / (1.0 + absZ);
                o.vertex.z = 0.0;
                o.vertex.w = 1.0;
                o.viewPos = viewPos;
                return o;
            }
            v2f vert(appdata v)
            {
                return DualParaboloidMapping(v.vertex);
            }
            void frag(v2f i, out float depth : SV_Depth)
            {
                if (i.viewPos.z < 0)
                    discard;

                float len = length(i.viewPos);
                depth = Linear01DepthFromViewZ(len, _ProjectionParams.y, _ProjectionParams.z);
            }
            ENDCG
        }
    }
}