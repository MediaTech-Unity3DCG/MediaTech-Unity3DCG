Shader "Unlit/viewEnvmap"
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
            #pragma vertex vertDefault
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "BasicShading.hlsl"
            #include "ViewCube.hlsl"

            float3      _BackgroundColor;
            float       _Lod;

            float4 frag(v2f i) : SV_Target
            {
                float3 pos = CalculateUV2TexCoord(i.uv);
                if (pos.x == 2.0){
                    return float4(_BackgroundColor,1);
                }
                // sample the texture
                float4 col = UNITY_SAMPLE_TEXCUBE_LOD(_Envmap, pos, _Lod);
                return col;
            }
            ENDCG
        }
    }
}
