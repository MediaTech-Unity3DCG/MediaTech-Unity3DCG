Shader "Hidden/4.4.2/Figure4.8(a)/prefilteringDiffuse"{
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
        float3 implDiffuse(float2 uv, out float3 lL){
            lL = uniformHemiCosine(uv);
            return 1.0;
        }
        float4 prefiltering3D(float3 cubeDir, uint seed) {
            float3x3 onb = calcONB(cubeDir);
            float4 col = float4(0,0,0,1);
            for (uint i = 0;i<256;++i){
                float2 uv = Hammersley2d(i,256);
                float3 lL;
                float3 coeff = implDiffuse(uv,lL);
                float3 wL = mul(lL,onb);
                col.xyz += coeff * sampleEnvmapLod(wL,0);
            }
            col.xyz /= 256;
            return col;
        }
        float4 frag(float4 i : SV_Position) : SV_Target {
            uint2 screenPos = uint2(i.xy);
            uint faceIndex = screenPos.x / _Width;
            uint faceX = screenPos.x % _Width;
            uint faceY = screenPos.y;
            float2 faceUV = float2(faceX,faceY)/ _Width;
            float3 cubePos = calcCubePosition(faceUV,faceIndex);
            float3 cubeDir = normalize(cubePos);
            uint seed = faceY * (_Width*6) + faceX;
            return prefiltering3D(cubeDir,seed);
        }
        ENDCG
        }
    }
}