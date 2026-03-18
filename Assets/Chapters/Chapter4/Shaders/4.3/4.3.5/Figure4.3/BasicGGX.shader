Shader "Unlit/4.3.5/Figure4.3/basicGGX"{
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _Roughness ("Roughness",Range(0,1)) = 0.001
        _MaxSamples("Max Samples",int) = 100
    }
    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Pass{
            CGPROGRAM
            #pragma vertex vertDefault
            #pragma fragment frag
            #include "BasicShading.hlsl"
            #include "utility.hlsl"
            float3 _Color;int _MaxSamples;
            float _Roughness;
            float3 implGGX(float2 uv, float alpha, float3 lV, inout float3 lH, inout float3 lL){
                lH = uniformHemiSphere(uv);
                lL = normalize(reflect(-lV,lH));
                float3 ggx_d = GGX_D (lH, alpha);
                float3 ggx_g = GGX_G2(lV, lL, lH, alpha);
                float3 ggx_f = GGX_F (lV, lH, _Color);
                float hDotV = max(dot(lH,lV),0);
                float nDotL = max(lL.z,0);
                float nDotV = max(lV.z,0);
                float nDotH = max(lH.z,0);
                if ((nDotL<=0)||(nDotV<=0)||(nDotH<=0)){return 0;}
                return ggx_d*ggx_g*ggx_f*(hDotV/nDotV)*2*UNITY_PI;
            }
            float4 frag(v2f i) : SV_Target {
                float4 col = float4(_Color, 1);
                uint2 screenPos = uint2(i.vertex.xy);
                uint seed=screenPos.y*_ScreenParams.x+ screenPos.x;
                initRandom(seed); float3x3 onb = calcONB(i.normal);
                float3 wV=normalize(_WorldSpaceCameraPos-i.worldPos);
                float3 lV = mul(onb,wV);
                float alpha = _Roughness*_Roughness;
                [loop]
                for (int j = 0; j < _MaxSamples; ++j) {
                float2 uv = Random2f();float3 lH, lL;
                float3 coeff = implGGX(uv,alpha,lV,lH,lL);
                float3 wL = mul(lL,onb);
                col.xyz += sampleEnvmapLod(wL,0) * coeff;
                }
                col.xyz /= _MaxSamples;
                return col;
            }
            ENDCG
        }
    }
}