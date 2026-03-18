Shader "Unlit/Exercise4/ImageSliceGGX"{
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _Phi("Phi", Range(0,360)) = 90
        _Roughness("Roughness", Range(0,1)) = 0.5
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
            float _Roughness;
            float _Phi;
            float3 implDisneyDiffuse(float2 uv, float3 color, float3 lV, inout float3 lH,  out float3 lL){
                lL = uniformHemiCosine(uv);
                float nDotV = lV.z;
                float nDotL = lL.z;
                lH = normalize(lV + lL);
                float f_d90 = Disney_F90(lH,lL,_Roughness);
                float f_l   = Disney_F(f_d90,nDotL);
                float f_v   = Disney_F(f_d90,nDotV);
                return color * f_l * f_v;
            }
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
                float3x3 onb = calcONB(normal);
                float3 lV = mul(onb,wV);
                float3 lL = mul(onb,wL);
                float3 lH = mul(onb,wH);

                float alpha = _Roughness*_Roughness;
                float  ggx_d = GGX_D(lH,alpha);
                float3 ggx_g = GGX_G2(lV, lL, lH, alpha);
                float3 ggx_f = GGX_F (lV, lH,_Color);

                return float4(ggx_d * ggx_g * ggx_f / (4 * nDotL * nDotV + 0.001), 1.0);
            }
            ENDCG
        }
    }
}