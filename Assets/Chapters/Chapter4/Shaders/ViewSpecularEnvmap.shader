Shader "Unlit/ViewSpecularEnvmap"
{
    Properties
    {
        _BackgroundColor("Background Color", Color) = (1,1,1,1)
        _Lod("Lod", Range(0, 12)) = 0
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
            samplerCUBE _SpecularEnvmap;
            float       _Lod;

            float4 frag(v2f i) : SV_Target
            {
                float3 pos = CalculateUV2TexCoord(i.uv);
                if (pos.x == 2.0){
                    return float4(_BackgroundColor,1);
                }
                // sample the texture
                float4 col = texCUBElod(_SpecularEnvmap,float4(pos,_Lod));
                return col;
            }
            ENDCG
        }
    }
}
