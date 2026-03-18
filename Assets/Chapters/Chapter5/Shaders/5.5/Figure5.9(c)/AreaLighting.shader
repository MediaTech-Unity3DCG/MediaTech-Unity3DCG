Shader "Unlit/5.5/Figure5.9(c)/AreaLighting"
{
    Properties{}
    SubShader
    {
        Pass {
            Tags { "RenderType"="Opaque" }
            LOD 100
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

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(1,0,0,1);
            }
            ENDCG
        }
        Pass{
            Name "DirectLighting"// スクリプトで指定するパス名(必須)
            Tags { "LightMode"="RayTracing" }//照明モードをRayTracing に設定(必須)
            HLSLPROGRAM
            #include "RTCommon.hlsl"
            // レイトレーシングシェーダであることを明示(必須,main の部分はなんでも可)
            #pragma raytracing main
            [shader("closesthit")]
            void ClosestHitForSurface(inout RayPayloadData payload :SV_RayPayload,
                in RayAttributeData attrib : SV_IntersectionAttributes){
                float3 ray_origin = WorldRayOrigin();// ray の始点
                float3 ray_direction = WorldRayDirection();// ray の方向
                float ray_distance = RayTCurrent();// ray の交差点までの距離
                float3 hit_point = ray_origin + ray_distance * ray_direction;
                SurfaceData surface_data = GetSurfaceData(attrib.barycentrics);
                float3 vnormal = surface_data.vNormal;// 頂点法線を取得
                payload.color = 0.5*vnormal+0.5;
            }
            ENDHLSL
        }
    }
}
