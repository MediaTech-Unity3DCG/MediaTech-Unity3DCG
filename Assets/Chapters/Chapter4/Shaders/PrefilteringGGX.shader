Shader "Hidden/Chapter4/prefilteringGGX"{
    Properties {}
    SubShader{
        Cull Off ZWrite Off ZTest Always // Image Effect 特有の設定
        Pass{
        CGPROGRAM
        #pragma vertex vertScreen
        #pragma fragment frag
        #include "BasicShading.hlsl"
        #include "utility.hlsl"
        #define MAX_SAMPLES 2048
        int _Width;
        int _MaxSamples;
        float _Roughness;
        float4 prefiltering3D(float3 cubeDir, uint seed) {
            float3x3 onb = calcONB(cubeDir); float4 col = float4(0,0,0,1);
            float alpha = _Roughness*_Roughness; initRandom(seed);
            float3 weight = float3(0,0,0); float3 lV = float3(0,0,1);
            const int numJitters = 4;
            const int numSamples = min(_MaxSamples,MAX_SAMPLES);
            for (int j = 0; j < numJitters;++j){
                float2 offset = Random2f();
                for (uint i = 0;i<numSamples;++i){
                    float2 uv = frac(Hammersley2d(i,numSamples) + offset + 0.5);
                    float3 lH = normalize(uniformGGXCosine(uv,alpha));
                    float3 lL = normalize(reflect(-lV,lH));
                    float  lDotN = max(lL.z,0); float3 wL = mul(lL,onb);
                    col.xyz += lDotN * sampleEnvmapLod(wL,0); weight += lDotN;
                }
            }
            col.xyz /= weight;
            return col;
        }
        float4 frag(float4 i : SV_Position) : SV_Target {
            uint2 screenPos  = uint2(i.xy);
            uint  faceIndex  = screenPos.x / _Width;
            uint  faceX      = screenPos.x % _Width;
            uint  faceY      = screenPos.y;
            float2 faceUV   = float2(faceX+0.5,faceY+0.5)/ _Width;
            float3 cubePos  = calcCubePosition(faceUV,faceIndex);
            float3 cubeDir  = normalize(cubePos);
            uint   seed     = faceY * (_Width*6) + faceX;
            return prefiltering3D(cubeDir,seed);
        }
        ENDCG
        }
    }
}