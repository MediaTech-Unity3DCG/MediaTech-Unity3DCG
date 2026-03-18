Shader "Hidden/4.4.2/Figure4.7(a)/prefilteringDiffuse"{
    Properties { }
    SubShader{
        Cull Off ZWrite Off ZTest Always // Image Effect 特有の設定
        Pass{
        CGPROGRAM
        #pragma vertex vertScreen
        #pragma fragment frag
        #include "BasicShading.hlsl"
        #include "utility.hlsl"
        int _Width;
        float4 prefiltering3D(float3 dir,uint seed){return float4((dir+1)/2,1);}
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