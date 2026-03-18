Shader "Unlit/viewEnvmapAll"
{
    Properties
    {
        _BackgroundColor("Background Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex   vertDefault
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "ViewCube.hlsl"
            #include "BasicShading.hlsl"

            float3      _BackgroundColor;

            float4 viewCubeFace(float2 uv, float lod){
                if (uv.x > 1.0 || uv.y >1.0){
                    return float4(_BackgroundColor,1);
                }
                float3 pos = CalculateUV2TexCoord(uv);
                if (pos.x != 2.0){
                     float3 pos = CalculateUV2TexCoord(uv);
                     float4 col = UNITY_SAMPLE_TEXCUBE_LOD(_Envmap, pos, lod);
                     return col;
                } else{
                    return float4(_BackgroundColor,1);
                } 
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 scaledUV = float2(i.uv.x * 1.5, i.uv.y);
                if (scaledUV.x <= 1.0){
                    return viewCubeFace(scaledUV,0);
                } else{
                    scaledUV.x -= 1.0;
                    scaledUV   *= 2.0;
                    int d1; int d2; int mipCount;
                    _Envmap.GetDimensions(0,d1,d2, mipCount);
                    for (int m = 1; m < mipCount; m++){
                         if (scaledUV.y <= 1.0){
                             return viewCubeFace(scaledUV,m);
                         } else {
                            scaledUV.y -= 1.0;
                            scaledUV   *= 2.0;
                         }
                    }
                }
                return float4(_BackgroundColor,1);
            }
            ENDCG
        }
    }
}
