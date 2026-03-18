Shader "Unlit/Exercise4/BasicGGXGlass"
{
    Properties
    {
        _IOR ("Index of Refraction", Range(0.75, 1.7)) = 1.5
        _Roughness ("Roughness",Range(0,1)) = 0.001
        _RoughnessTexture ("Roughness Texture", 2D) = "white" {}
        _MaxSamples("Max Samples",int) = 100
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vertDefault
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            

            float _IOR;
            float _Roughness;
            int   _MaxSamples;
            sampler2D _RoughnessTexture;

            #include "BasicShading.hlsl"
            #include "Utility.hlsl"
            float2 implGGXGlass(float2 uv, float ior, float alpha, float3 lV, inout float3 lH, inout float3 lL_refl, inout float3 lL_refr){
                float F0 = (ior - 1) * (ior - 1) / ((ior + 1) * (ior + 1));
                lH = uniformGGXCosine(uv,alpha);
                // 反射方向と屈折方向を求める
                float3 refl = normalize(reflect(-lV,lH));
                float3 refr = normalize(refract(-lV,lH,ior));
                // それぞれの寄与を計算する
                float ggx_d      = GGX_D(lH, alpha);
                float ggx_f      = GGX_F(lV, lH, F0);
                float ggx_g_refl = GGX_G2(lV, refl, lH, alpha);
                float ggx_g_refr = GGX_G2(lV,-refr, lH, alpha);
                float hDotV      = max(dot(lH,lV),0);
                float nDotH      = max(lH.z,0);
                float nDotV      = max(lV.z,0);
                lL_refl = refl;
                lL_refr = refr;
                float nDotL_refl = max( lL_refl.z,0);
                float nDotL_refr = max(-lL_refr.z,0);
                if ((nDotV<=0)||(nDotH<=0)){return float2(0,0);}
                float2 ret = float2(ggx_g_refl*ggx_f,ggx_g_refr*(1-ggx_f)) *hDotV /(nDotH * nDotV);
                if (nDotL_refl <= 0) {ret.x = 0;}
                if (nDotL_refr <= 0) {ret.y = 0;}
                return ret;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 V = normalize(_WorldSpaceCameraPos-i.worldPos);
                float3 N;
                if (dot(V, i.normal) < 0.0) {
                    N = -i.normal;
                } else {
                    N = i.normal;
                }
                N = normalize(N);
                float3 R = reflect(-V, N);
                float rDotN  = dot(V, N);
                uint seed    = uint(i.vertex.y*_ScreenParams.x+i.vertex.x);
                initRandom(seed);
                float roughness = _Roughness * tex2D(_RoughnessTexture, i.uv).r;
                float3x3 onb    = calcONB(N);
                float3   wV     = V;
                float3   lV     = mul(onb,wV);
                float    alpha  = roughness*roughness;
                float3   res    = float3(0,0,0);
                    [loop]
                    for (int j = 0; j < _MaxSamples; ++j) {
                    float2 uv      = Random2f();float3 lH, lL_refl, lL_refr;
                    float2 coeff   = implGGXGlass(uv,_IOR,alpha,lV,lH,lL_refl,lL_refr);
                    float3 wL_refl = mul(lL_refl,onb);
                    // 反射した場合
                    res += sampleEnvmapLod(wL_refl,0) * coeff.x;
                    // 透過した場合, 今度は通常のガラスで反射, 屈折する
                    float3 wL_refr = mul(lL_refr,onb);
                    float3 V2 = -wL_refr;
                    float3 R2 = reflect(-V2,N);
                    float3 T2 = refract(-V2,N,1.0/_IOR);
                    float  rDotN2 = dot(V2,N);
                    float sinRDotN2 = 1 - (rDotN2 * rDotN2);
                    float sinTDotN2 = sinRDotN2 / (_IOR * _IOR);
                    if (sinTDotN2 <= 1.0){
                        float F0  = (_IOR - 1) * (_IOR - 1) / ((_IOR + 1) * (_IOR + 1));
                        float Fr2 = F0 + (1 - F0) * pow(1 - rDotN2, 5);
                        float Ft2 = 1.0 - Fr2;
                        res += Ft2 * sampleEnvmapLod(T2,0) * coeff.y;
                    }
                }
                res /= _MaxSamples;
                return float4(res, 1.0);
            }
            ENDCG
        }
    }
}
