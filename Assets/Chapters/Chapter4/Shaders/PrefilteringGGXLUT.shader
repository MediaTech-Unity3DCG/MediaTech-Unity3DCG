Shader "Hidden/PrefilteringGGXLUT"{
    Properties { }
    SubShader{
        Cull Off ZWrite Off ZTest Always // Image Effect 特有の設定
        Pass{
        CGPROGRAM
        #pragma vertex vertScreen
        #pragma fragment frag
        #define PREFILTERING_ENABLE 1
        #include "BasicShading.hlsl"
        #include "utility.hlsl"
        int _Width;
        float3 implGGX(float2 uv, float alpha, float3 lV, inout float3 lH, inout float3 lL){
            lH = uniformGGXCosine(uv,alpha);
            lL = normalize(reflect(-lV,lH));
            float3 ggx_g = GGX_G2(lV, lL, lH, alpha);
            float3 ggx_f = GGX_F (lV, lH, float3(0,1,0));
            float  hDotL = max(dot(lH,lL),0);
            float hDotV = max(dot(lH,lV),0);
            float nDotL = max(lL.z,0);
            float nDotV = max(lV.z,0);
            float nDotH = max(lH.z,0);
            if ((nDotL<=0)||(nDotV<=0)||(nDotH<=0)){return 0;}
            return ggx_g*ggx_f*(hDotV/(nDotV*nDotH));
        }
        float4 prefiltering2D(float2 faceUV) {
            float roughness = faceUV.y;
            float nDotV = faceUV.x;
            float alpha = roughness*roughness;
            float3 col = float3(0,0,0);
                for (uint i = 0;i<256;++i){
                float2 uv = Hammersley2d(i,256);
                float3 lL; float3 lH;
                float3 lV = float3(sqrt(1-nDotV*nDotV),0,nDotV);
                float3 coeff = implGGX(uv,alpha,lV,lH,lL);
                col += coeff;
            }
            col /= 256;
            float b = col.x;
            float a = col.y - col.x;
            return float4(a,b,0,1);
        }
        float4 frag (float4 i: SV_Position) : SV_Target {
            uint2  screenPos = uint2(i.xy);
            uint   faceX     = screenPos.x;
            uint   faceY     = screenPos.y;
            float2 faceUV    = float2(faceX,faceY)/ (_Width-1);
            return prefiltering2D(faceUV);
        }
        ENDCG
        }
    }
}