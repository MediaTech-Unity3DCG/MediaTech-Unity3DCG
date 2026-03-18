Shader "Unlit/BasicShader"{
    Properties{ _Color ("Main Color", Color) = (1,1,1,1) }
    SubShader{
        LOD 100
        Pass{
            Tags { "RenderType"="Opaque" }
            CGPROGRAM
            #pragma vertex vertDefault
            #pragma fragment frag
            #include "BasicShading.hlsl"
            float4 _Color;
            float4 frag(v2f i) : SV_Target {
                return _Color;
            }
            ENDCG
        }
    }
}