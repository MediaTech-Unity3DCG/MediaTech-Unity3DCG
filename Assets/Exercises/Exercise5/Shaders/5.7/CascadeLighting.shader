Shader "Unlit/Exercise5/CascadeLighting"
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
            
            // マテリアル
            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _Color;

            // 光源基本情報
            int       _LightType;
            float3    _LightDirection;
            float     _LightIntensity;
            float3    _LightColor;

            // カスケードシャドウ情報
            int       _CascadeCount;
            sampler2D _ShadowMap0, _ShadowMap1, _ShadowMap2, _ShadowMap3;
            float4x4  _ShadowVP0 , _ShadowVP1 , _ShadowVP2 , _ShadowVP3;
            float4    _CascadeDistances; 
            float4    _CascadeBiases;

            int _VisualizeCascades; // デバッグ用: カスケードの可視化フラグ

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos: POSITION1;
                float3 normal: NORMAL;
                float  viewZ : TEXCOORD1; // カメラからの深度
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // カメラ空間の深度(Z)を取得
                o.viewZ = -UnityObjectToViewPos(v.vertex).z;
                return o;
            }
            float3 hsv2rgb(float3 c) {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }
            // 指定されたインデックスのカスケードから影の減衰率を取得
            float GetCascadeShadow(int idx, float3 worldPos) {
                float4 shadowProjPos;
                float shadowDepth = 0;
                
                // 行列適用（分岐はシェーダーモデルに応じて最適化される）
                if (idx == 0) shadowProjPos = mul(_ShadowVP0, float4(worldPos, 1));
                else if (idx == 1) shadowProjPos = mul(_ShadowVP1, float4(worldPos, 1));
                else if (idx == 2) shadowProjPos = mul(_ShadowVP2, float4(worldPos, 1));
                else shadowProjPos = mul(_ShadowVP3, float4(worldPos, 1));

                // 同次座標系からNDCへ変換
                shadowProjPos.xyz /= shadowProjPos.w;
                float2 shadowUV = shadowProjPos.xy * 0.5 + 0.5;

                // 範囲外チェック
                if (shadowUV.x < 0 || shadowUV.x > 1 || shadowUV.y < 0 || shadowUV.y > 1 || shadowProjPos.z < 0 || shadowProjPos.z > 1) {
                    return 1.0;
                }

                // シャドウマップサンプリング
                if (idx == 0) shadowDepth = tex2D(_ShadowMap0, shadowUV).r;
                else if (idx == 1) shadowDepth = tex2D(_ShadowMap1, shadowUV).r;
                else if (idx == 2) shadowDepth = tex2D(_ShadowMap2, shadowUV).r;
                else shadowDepth = tex2D(_ShadowMap3, shadowUV).r;
                // 深度比較 (BasicLighting.shader のロジックを継承)
                // depth > z + bias の時、影（0.5）を返す
                if (shadowDepth> shadowProjPos.z+ _CascadeBiases[idx]) {
                    return 0.0; // 影の濃さ
                }
                return 1.0;
            }

            float4 frag (v2f i) : SV_Target {
                float4 diffuseColor = tex2D(_MainTex, i.uv) * _Color;
                if (_LightType == 0) return diffuseColor;

                // 1. 適切なカスケードを選択
                float shadowAtten = 1.0;
                float3 debugColor = float3(1, 1, 1); // デバッグ用: カスケードの色
                [loop]
                for (int c = 0; c < _CascadeCount; c++) {
                    if (i.viewZ < _CascadeDistances[c]) {
                        shadowAtten = GetCascadeShadow(c, i.worldPos);

                        // デバッグ用: カスケードごとに色分け
                        if (_VisualizeCascades == 1) {
                            debugColor = hsv2rgb(float3(c / (float)_CascadeCount, 1, 1));
                        }
                        break;
                    }
                }

                // 2. ライティング計算 (Directional Light想定)
                float3 lightDir = normalize(_LightDirection);
                float3 lightIntensity = _LightIntensity * _LightColor;
                float NdotL = max(-dot(i.normal, lightDir), 0);

                float3 res = diffuseColor.rgb * lightIntensity * NdotL * shadowAtten * UNITY_INV_PI;
                
                return float4(res*debugColor, 1);
            }
            ENDCG
        }
    }
}