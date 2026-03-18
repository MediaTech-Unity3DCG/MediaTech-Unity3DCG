Shader "Unlit/basicPhong"{
    Properties{
    _Color("Color", Color) = (1,1,1,1)
    _Shininess ("Shininess",float) = 0.001
    _MaxSamples("Max Samples",int) = 100
    }
    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Pass {
            CGPROGRAM
            #pragma vertex vertDefault
            #pragma fragment frag
            #include "BasicShading.hlsl"
            #include "utility.hlsl"
            float3 _Color;int _MaxSamples;float _Shininess;
            float3 implPhong(float2 uv, out float3 lL){
                lL = uniformHemiCosine (uv, _Shininess);
                return _Color;
            }
            float4 frag(v2f i) : SV_Target {
                float4 col = float4(_Color, 1);
                uint seed=i.vertex.y*_ScreenParams.x+i.vertex.x;
                initRandom(seed);
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 R = reflect(-V, i.normal);
                float3x3 onb = calcONB(R);
                [loop]
                for (int j = 0; j < _MaxSamples; ++j) {
                float2 uv    = Random2f();float3 lL;
                float3 coeff = implPhong(uv, lL);
                float3 wL    = mul(lL,onb);
                col.xyz += sampleEnvmapLod(wL,0) * coeff;
                }
                col.xyz /= _MaxSamples;
                return col;
            }
            ENDCG
        }
    }
}