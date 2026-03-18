Shader "Unlit/5.3.1/BasicLighting"
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
            
            // マテリアル情報
            sampler2D _MainTex;
            float4    _MainTex_ST;

            // 光源タイプ定義
            #define LIGHT_TYPE_NONE  0
            #define LIGHT_TYPE_POINT 1
            #define LIGHT_TYPE_DIR   2
            #define LIGHT_TYPE_SPOT  3

            // 光源情報
            float4    _Color;
            int       _LightType;
            float3    _LightPosition;
            float3    _LightDirection;
            float     _LightIntensity;
            float3    _LightColor;
            sampler2D _LightCookie;

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
                float4 vertex : SV_POSITION;
                float3 worldPos: POSITION1;
                float3 normal: NORMAL;
            };;
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
                float4 diffuseColor=tex2D(_MainTex,i.uv) *_Color;
                float4 col=LightingMain(i.worldPos,i.normal,diffuseColor);
                return col;
            }
            
            float4 LightingMain(float3 surfPos, float3 surfNorm, float4 diffColor) {return diffColor ;}
            ENDCG
        }
    }
}
