Shader "Unlit/BasicDiffuse"{
    Properties{
    _Color("Color", Color) = (1,1,1,1)
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
                float3 _Color; int _MaxSamples;
                float3 implDiffuse(float2 uv, out float3 lL){
                    lL = uniformHemiSphere(uv);
                    return 2* _Color * lL.z;
                }
                float4 frag(v2f i) : SV_Target {
                    float4 col = float4(_Color, 1);
                    float3x3 onb = calcONB(i.normal);
                    uint2 screenPos = uint2(i.vertex.xy);
                    initRandom(screenPos.y * _ScreenParams.x + screenPos.x);
                    [loop]
                    for (int j = 0; j < _MaxSamples; ++j) {
                    float2 uv = Random2f(); float3 lL;
                    float3 coeff = implDiffuse(uv,lL);
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