Shader "Unlit/Exercise4/BasicDisneyDiffuse"{
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _Roughness("Roughness", Range(0,1)) = 0.5
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
            float3 _Color; 
            float _Roughness;
            int _MaxSamples;
            float3 implDisneyDiffuse(float2 uv, float3 color, float3 lV, inout float3 lH,  out float3 lL){
                lL = uniformHemiCosine(uv);
                float nDotV = lV.z;
                float nDotL = lL.z;
                lH = normalize(lV + lL);
                float f_d90 = Disney_F90(lH,lL,_Roughness);
                float f_l = Disney_F(f_d90,nDotL);
                float f_v = Disney_F(f_d90,nDotV);
                return color * f_l * f_v;
            }
            float4 frag(v2f i) : SV_Target {
                float4 col      = float4(_Color, 1);
                float3x3 onb    = calcONB(i.normal);
                uint2 screenPos = uint2(i.vertex.xy);
                initRandom(screenPos.y * _ScreenParams.x + screenPos.x);
                float3 wV   = normalize(_WorldSpaceCameraPos-i.worldPos);
                float3 lV   = mul(onb,wV);
                [loop]
                for (int j = 0; j < _MaxSamples; ++j) {
                    float2 uv = Random2f(); float3 lL; float3 lH;
                    float3 coeff = implDisneyDiffuse(uv,_Color,lV,lH,lL);
                    float3 wL = mul(lL,onb);
                    col.xyz += coeff * sampleEnvmapLod(wL,0);
                }
                col.xyz /= _MaxSamples;
                return col;
            }
            ENDCG
        }
    }
}