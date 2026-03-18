Shader "Unlit/viewSpecularBrdfLUT"
{
    Properties{
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
            #include "BasicShading.hlsl"

            sampler2D _SpecularBrdfLut;
            float4    _SpecularBrdfLut_ST;
            float     _Lod;

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_SpecularBrdfLut, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
