Shader "Unlit/Exercise4/ImageSliceDiffuse"{
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _Phi("Phi", Range(0,360)) = 90
    }
    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Pass{
            CGPROGRAM
            #pragma vertex vertScreen
            #pragma fragment frag
            #include "BasicShading.hlsl"
            #include "utility.hlsl"
            float3 _Color; 
            float _Phi;
            float4 frag(float4 i : SV_Position) : SV_Target {
                float2 uv       = i.xy / _ScreenParams.xy;
                float theta_h   = uv.x * UNITY_PI / 2.0;
                float theta_d   = uv.y * UNITY_PI / 2.0;
                float phi_d     = _Phi * UNITY_PI / 180.0;
                float cosPhiD   = cos(phi_d);
                float sinPhiD   = sin(phi_d);
                float cosThetaD = cos(theta_d);
                float sinThetaD = sin(theta_d);
                float cosThetaH = cos(theta_h);
                float sinThetaH = sin(theta_h);
                float3 normal   = float3(sinThetaH, 0, cosThetaH);
                float3 wH   = float3(0,0,1);
                float3 wV   = float3(-sinThetaD * cosPhiD, -sinThetaD * sinPhiD, cosThetaD);
                float3 wL   = float3(+sinThetaD * cosPhiD, +sinThetaD * sinPhiD, cosThetaD);
                float nDotL = max(dot(normal, wL), 0);
                float nDotV = max(dot(normal, wV), 0);
                if (nDotL <= 0 || nDotV <= 0) {
                    return float4(0,0,0,1);
                }
                return float4(_Color / UNITY_PI,1);
            }
            ENDCG
        }
    }
}