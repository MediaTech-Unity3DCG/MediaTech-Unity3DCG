Shader "Unlit/AreaLighting"
{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass {
            Tags { "RenderType"="Opaque" }
            LOD 100
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            UNITY_DECLARE_TEX2D(_MainTex);
            float4    _MainTex_ST;
            float4    _Color;
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
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = UNITY_SAMPLE_TEX2D_LOD(_MainTex, i.uv,0) * _Color;
                return color;
            }
            ENDCG
        }
        Pass{
            Name "DirectLighting"// スクリプトで指定するパス名(必須)
            Tags { "LightMode"="RayTracing" }//照明モードをRayTracing に設定(必須)
            HLSLPROGRAM
            #include "RTCommon.hlsl"
            #pragma raytracing main
            // マテリアル情報
            UNITY_DECLARE_TEX2D(_MainTex);
            float4    _Color;
            [shader("closesthit")]
            void ClosestHitForSurface(inout RayPayloadData payload :SV_RayPayload,
                in RayAttributeData attrib : SV_IntersectionAttributes){
                float3 ray_origin = WorldRayOrigin();// ray の始点
                float3 ray_direction = WorldRayDirection();// ray の方向
                float ray_distance = RayTCurrent();// ray の交差点までの距離
                float3 hit_point = ray_origin + ray_distance * ray_direction;
                SurfaceData surface_data = GetSurfaceData(attrib.barycentrics);
                payload.hitPoint   = hit_point;
                payload.hitNormal  = surface_data.vNormal;
                payload.hitTangent = surface_data.vTangent;
                float2 uv = surface_data.uv;
                float3 color = UNITY_SAMPLE_TEX2D_LOD(_MainTex, uv,0) * _Color;
                payload.diffuseColor = color;
                payload.lightColor = float3(0,0,0);
                payload.isDone     = false;
            }
            ENDHLSL
        }
    }
}
