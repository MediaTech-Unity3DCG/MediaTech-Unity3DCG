Shader "Unlit/Exercise5/PointLighting"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "DepthUtil.hlsl"

            #define SHADOWMAP_TYPE_NONE     0
            #define SHADOWMAP_TYPE_OMNIDIR  1
            #define SHADOWMAP_TYPE_DUALPARA 2
            
            // マテリアル情報
            sampler2D _MainTex;
            float4    _MainTex_ST;
            // 光源情報
            float4    _Color;
            float3    _LightPosition;
            float3    _LightDirection;
            float     _LightIntensity;
            float3    _LightColor;
            float4x4  _LightViewMatrix;
            int       _LightShadowMapType;
            sampler2D _LightShadowMap_0;
            sampler2D _LightShadowMap_1;
            sampler2D _LightShadowMap_2;
            sampler2D _LightShadowMap_3;
            sampler2D _LightShadowMap_4;
            sampler2D _LightShadowMap_5;
            float     _LightShadowBias;
            float _LightShadowNearPlane;
            float _LightShadowFarPlane;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex  : SV_POSITION;
                float3 worldPos: POSITION1;
                float3 normal  : NORMAL;
            };
            // 深度値を非線形深度値に変換する
            float PerspectiveDepth(float viewZ, float nearPlane, float farPlane)
            {
            #if defined(UNITY_REVERSED_Z )
                return nearPlane * (-viewZ + farPlane) / (viewZ * (farPlane - nearPlane));
            #else
                return farPlane * (viewZ - nearPlane) / (viewZ * (farPlane - nearPlane));
            #endif
            }
            // Cubemapの対応する面の非線形深度を計算する
            // 関数宣言
            float4 LightingMain(float3 surfPos, float3 surfNorm, float4 diffColor);
            v2f vert (appdata v){
                v2f o;o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.normal=UnityObjectToWorldNormal(v.normal);
                o.worldPos=mul(unity_ObjectToWorld,v.vertex).xyz;
                return o;
            } 
            float4 frag (v2f i) : SV_Target{
                float2 screenUV = i.vertex.xy / _ScreenParams.xy;
                float4 diffuseColor=tex2D(_MainTex,i.uv) *_Color;
                float4 col= LightingMain(i.worldPos,i.normal,diffuseColor);
                return col;
            }      
            float4 LightingMain(float3 surfPos, float3 surfNorm, float4 diffColor) {
                // 光源の方向ベクトル
                float3 lightVec = surfPos - _LightPosition;
                float  distance = length(lightVec);
                float3 lightDir = normalize(lightVec);
                float3 lightIntensity = _LightIntensity * _LightColor / (distance * distance + 0.01);

                // シャドウマッピング（点光源キューブマップ）
                if (_LightShadowMapType == SHADOWMAP_TYPE_OMNIDIR){
                    // 光源からサーフェスへのベクトル
                    float3 toSurface = mul(_LightViewMatrix, float4(surfPos, 1)).xyz;
                    float absX = abs(toSurface.x);
                    float absY = abs(toSurface.y);
                    float absZ = abs(toSurface.z);
                    float2 uv;
                    float sampledDepth = 0;
                    float surfDepth = 0;
                    // どの面かを判定
                    if (absX >= absY && absX >= absZ) {
                        // X面
                        if (toSurface.x > 0) {
                            // +X
                            uv = float2(-toSurface.z, toSurface.y) / absX * 0.5 + 0.5;
                            sampledDepth = tex2D(_LightShadowMap_0, uv).r;
                        } else {
                            // -X
                            uv = float2(toSurface.z, toSurface.y) / absX * 0.5 + 0.5;
                            sampledDepth = tex2D(_LightShadowMap_1, uv).r;
                        }
                        surfDepth = absX;
                    } else if (absY >= absX && absY >= absZ) {
                        // Y面
                        if (toSurface.y > 0) {
                            // +Y
                            uv = float2(toSurface.x, -toSurface.z) / absY * 0.5 + 0.5;
                            sampledDepth = tex2D(_LightShadowMap_2, uv).r;
                        } else {
                            // -Y 
                            uv = float2(toSurface.x, toSurface.z) / absY * 0.5 + 0.5;
                            sampledDepth = tex2D(_LightShadowMap_3, uv).r;
                        }
                        surfDepth = absY;
                    } else {
                        // Z面
                        if (toSurface.z > 0) {
                            // +Z
                            uv = float2(toSurface.x,toSurface.y) / absZ * 0.5 + 0.5;
                            sampledDepth = tex2D(_LightShadowMap_4, uv).r;
                        } else {
                            // -Z
                            uv = float2(-toSurface.x, toSurface.y) / absZ * 0.5 + 0.5;
                            sampledDepth = tex2D(_LightShadowMap_5, uv).r;
                        }
                        surfDepth = absZ;
                    }
                    // サーフェスまでの深度を0-1に正規化
                    float compDepth =PerspectiveDepth(surfDepth,_LightShadowNearPlane,_LightShadowFarPlane);
                    // シャドウ判定
                    if (sampledDepth > compDepth + _LightShadowBias) {
                        lightIntensity *= 0.05;
                    }
                }
                else if (_LightShadowMapType == SHADOWMAP_TYPE_DUALPARA){
                    // 光源からサーフェスへのベクトル
                    float3 toSurface = mul(_LightViewMatrix, float4(surfPos, 1)).xyz;
                    float2 uv;
                    float sampledDepth = 0;
                    // (u,v) = (x,y)/(1+|z|)
                    float compDepth = length(toSurface);
                    float3 dir = toSurface * rcp(compDepth);
                    uv = dir.xy / (1.0 + abs(dir.z)) * 0.5 + 0.5;
                    compDepth = Linear01DepthFromViewZ(compDepth,_LightShadowNearPlane,_LightShadowFarPlane);
                    if (dir.z <= 0.0) {
                        uv.x = 1.0 - uv.x;
                        sampledDepth = tex2D(_LightShadowMap_0, uv).r;
                    } else{
                        sampledDepth = tex2D(_LightShadowMap_1, uv).r;
                    }
                    //return float4(compDepth,0,0,1);
                    // シャドウ判定
                    if (sampledDepth > compDepth + _LightShadowBias) {
                        lightIntensity *= 0.05;
                    }
                }
                // ライトベクトルと法線ベクトルの内積
                float NdotL = max(-dot(surfNorm, lightDir), 0);
                float3 res = diffColor.rgb * lightIntensity * NdotL * UNITY_INV_PI;
                return float4(res,1);
            }
            ENDCG
        }
    }
}
