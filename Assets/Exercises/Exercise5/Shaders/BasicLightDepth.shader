Shader "Unlit/Exercise5/BasicLightDepth"{
    Properties{}
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass {
            ColorMask 0
            Cull Off
            ZWrite On
        }
    }
}
