Shader "Unlit/BasicSpecular"{
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
                float3 V = normalize(_WorldSpaceCameraPos-i.worldPos);
                // reflect(I,N) = I - 2*dot(N,I)*N
                float3 R = reflect(-V, i.normal);
                return float4(_Color*sampleEnvmapLod(R,0), 1.0);
            }
            ENDCG
        }
    }
}