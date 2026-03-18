Shader "Unlit/Exercise5/ParaboloidLightDepthWithTessellation"
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
            #pragma hull hull
            #pragma domain domain
            #define UV_BIAS 0.01

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
            //------------------------------------------------------------
            // Vertex shader
            //------------------------------------------------------------
            appdata vert(appdata v)
            {
                return v;
            }
            //------------------------------------------------------------
            // Tessellation factor control
            //------------------------------------------------------------
            float TessEdge(appdata v0, appdata v1, appdata v2)
            {
                // カメラ距離に応じてテセレーション係数を変化
                float3 w0 = mul(unity_ObjectToWorld, v0.vertex).xyz;
                float3 w1 = mul(unity_ObjectToWorld, v1.vertex).xyz;
                float3 w2 = mul(unity_ObjectToWorld, v2.vertex).xyz;
                float3 camPos = _WorldSpaceCameraPos;
                float dist = (distance(camPos, w0) + distance(camPos, w1) + distance(camPos, w2)) / 3.0;
                // 近いほど細かく、遠いほど粗く
                float tess = saturate(1.0 - dist / 50.0) * 8.0 + 1.0;
                return tess;
            }
            //------------------------------------------------------------
            // Hull shader
            //------------------------------------------------------------
            [domain("tri")]
            [partitioning("fractional_odd")]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("PatchConst")]
            [outputcontrolpoints(3)]
            appdata hull(InputPatch<appdata, 3> patch, uint i : SV_OutputControlPointID)
            {
                return patch[i];
            }

            struct HS_CONSTANT_DATA_OUTPUT
            {
                float EdgeTess[3] : SV_TessFactor;
                float InsideTess  : SV_InsideTessFactor;
            };

            HS_CONSTANT_DATA_OUTPUT PatchConst(InputPatch<appdata, 3> patch)
            {
                HS_CONSTANT_DATA_OUTPUT o;
                float tess = TessEdge(patch[0], patch[1], patch[2]);
                o.EdgeTess[0] = tess;
                o.EdgeTess[1] = tess;
                o.EdgeTess[2] = tess;
                o.InsideTess  = tess;
                return o;
            }

            //------------------------------------------------------------
            // Domain shader
            //------------------------------------------------------------
            [domain("tri")]
            v2f domain(HS_CONSTANT_DATA_OUTPUT input, const OutputPatch<appdata, 3> patch, float3 bary : SV_DomainLocation)
            {
                // 補間した頂点位置
                float4 pos = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
                return DualParaboloidMapping(pos);
            }

            //------------------------------------------------------------
            // Fragment shader
            //------------------------------------------------------------
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