Shader "Unlit/FastGGX"{ 
    Properties{
        _Color ("Color", Color) = (1,1,1,1)
        _Roughness ("Roughness", Range(0,1)) = 0.5
    }
    SubShader{
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass{
            HLSLPROGRAM
            #pragma vertex vertDefault
            #pragma fragment frag
            #include "BasicShading.hlsl"
            float3 _Color; float _Roughness;
            UNITY_DECLARE_TEXCUBE(_SpecularEnvmap);
            UNITY_DECLARE_TEX2D(_SpecularBrdfLut);
            float4 frag(v2f i) : SV_Target {
                float3 wV       = normalize(_WorldSpaceCameraPos-i.worldPos);
                float3 wL       = normalize(reflect(-wV, i.normal));
                float  nDotV    = max(dot(normalize(i.normal), wV),0);
                float2 uv       = float2(nDotV, _Roughness);
                float3 brdf     = UNITY_SAMPLE_TEX2D(_SpecularBrdfLut,uv).rgb;
                float3 specular = _Color*brdf.x+brdf.y;
                uint d1,d2,levels;
                _SpecularEnvmap.GetDimensions(0,d1,d2,levels);
                // 
                float lod = (levels-1) * _Roughness;
                float3 lit = UNITY_SAMPLE_TEXCUBE_LOD(_SpecularEnvmap,wL,lod).rgb;
                return float4(specular*lit,1); 
            }
            ENDHLSL
        }
    }
}