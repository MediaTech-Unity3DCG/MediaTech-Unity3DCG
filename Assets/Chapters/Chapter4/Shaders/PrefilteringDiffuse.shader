Shader "Hidden/prefilteringDiffuse"{
    Properties {}
    SubShader{
        Cull Off ZWrite Off ZTest Always // Image Effect 特有の設定
        Pass{
        CGPROGRAM
        #pragma vertex vertScreen
        #pragma fragment frag
        #include "BasicShading.hlsl"
        #include "utility.hlsl"
        int _Width;
        int _MaxSamples;
        float3 implDiffuse(float2 uv, out float3 lL){
            lL = uniformHemiCosine(uv);
            return 1.0;
        }
        float4 prefiltering3D(float3 cubeDir, uint seed) {
            float3x3 onb = calcONB(cubeDir);
            initRandom(seed);
            float2 offset = Random2f();
            float4 col = float4(0,0,0,1);
            int numSamples = _MaxSamples;
            for (uint i = 0;i<numSamples;++i){
                float2 uv = frac(Hammersley2d(i,numSamples) + offset + 0.5);
                float3 lL;
                float3 coeff = implDiffuse(uv,lL);
                float3 wL = mul(lL,onb);
                col.xyz += coeff * sampleEnvmapLod(wL,0);
            }
            col.xyz /= (float)numSamples;
            return col;
        }
        float4 frag(float4 i : SV_Position) : SV_Target {
            uint2 screenPos = uint2(i.xy);
            uint faceIndex  = screenPos.x / _Width;
            uint faceX = screenPos.x % _Width;
            uint faceY = screenPos.y;
            float2 faceUV  = float2(faceX+0.5,faceY+0.5)/ _Width;
            float3 cubePos = calcCubePosition(faceUV,faceIndex);
            float3 cubeDir = normalize(cubePos);
            uint seed = faceY * (_Width*6) + faceX;
            return prefiltering3D(cubeDir,seed);
        }
        ENDCG
        }
    }
}