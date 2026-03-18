Shader "Hidden/Exercise4/prefilteringGGX"{
    Properties {
        [Toggle]
        _UseMip ("Use Mip", Float) = 1
    }
    SubShader{
        Cull Off ZWrite Off ZTest Always // Image Effect 特有の設定
        Pass{
        CGPROGRAM
        #pragma vertex vertScreen
        #pragma fragment frag
        #include "BasicShading.hlsl"
        #include "utility.hlsl"
        #pragma multi_compile _ _USEMIP_ON
        int   _Width;
        float _Roughness;
        float4 prefiltering3D(float3 cubeDir, uint seed) {
            float3x3 onb = calcONB(cubeDir); float4 col = float4(0,0,0,1);
            float alpha = _Roughness*_Roughness; initRandom(seed);
            float3 weight = float3(0,0,0); float3 lV = float3(0,0,1);
            float2 offset = Random2f();
            int2 envmapSize = getEnvmapTexelSize();
            for (uint i = 0;i<512;++i){
                float2 uv       = frac(Hammersley2d(i,512) + offset + 0.5);
                float3 lH       = normalize(uniformGGXCosine(uv,alpha));
            #ifdef _USEMIP_ON
                float  D        = GGX_D(lH,alpha);
                float  pdf      = (D * lH.z)/(4 *max(dot(lV,lH),0)) + 0.0001;
                float  saTexel  = 4 * 3.14159265 / (6.0 * envmapSize.x * envmapSize.y);
                float  saSample = 1.0 / (512.0 * pdf + 0.0001);
                float  mipLevel = _Roughness==0 ? 0.0 : 0.5 * log2(saSample/saTexel);
            #else
                float mipLevel = 0;
            #endif
                float3 lL       = normalize(reflect(-lV,lH));
                float  lDotN    = max(lL.z,0); float3 wL = mul(lL,onb);

                col.xyz        += lDotN * sampleEnvmapLod(wL,mipLevel); weight += lDotN;
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