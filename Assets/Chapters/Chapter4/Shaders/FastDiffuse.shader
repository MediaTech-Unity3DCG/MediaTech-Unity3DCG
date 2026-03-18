Shader "Unlit/FastDiffuse"{
	Properties{
		_Color ("Color", Color) = (1,1,1,1)
		_DiffuseEnvmap("DiffuseEnvmap", CUBE) = "" {}
	}
	SubShader{
	Tags { "RenderType"="Opaque" }
	LOD 100
	Pass{
		CGPROGRAM
			#pragma vertex vertDefault
			#pragma fragment frag
			#include "BasicShading.hlsl"
			float3 _Color; samplerCUBE _DiffuseEnvmap;
			float4 frag (v2f i) : SV_Target{
				return float4(_Color,1)*texCUBE(_DiffuseEnvmap,i.normal);
			}
			ENDCG
		}
	}
}