Shader "Unlit/Exercise4/BasicGlass"
{
    Properties
    {
        // No/Ni
        _IOR ("Index of Refraction", Range(0.75 , 1.7)) = 1.5
        [Toggle]
        _Schlick ("Schlick Approximation", Float) = 0
        [Toggle]
        _NonLinear ("Non Linear", Float) = 0
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
            #pragma multi_compile _ _NONLINEAR_ON
            #pragma multi_compile _ _SCHLICK_ON

            #include "BasicShading.hlsl"


            float _IOR;
            fixed4 frag (v2f i) : SV_Target
            {

                float3 V = normalize(_WorldSpaceCameraPos-i.worldPos);
                float ior ;
                float3 N;
                if (dot(V, i.normal) < 0.0) {
                    N = -i.normal;
                } else {
                    N = i.normal;
                }
                N = normalize(N);
                
                float3 R     = reflect(-V, N);
                float  rDotN = dot(V, N);

                float3 colR     = sampleEnvmapLod( R, 0);
                float sinRDotN2 = 1 - rDotN * rDotN;
                float sinTDotN2 = _IOR * _IOR * sinRDotN2;
                if (sinTDotN2 > 1.0){ return float4(colR, 1.0); }

                float3 T     = refract(-V, N, _IOR);
                float  tDotN =-dot(T, N);
                float3 colT  = sampleEnvmapLod(-V, 0);

            #ifdef _SCHLICK_ON
                float F0 = (_IOR - 1) * (_IOR - 1) / ((_IOR + 1) * (_IOR + 1));
                float Fr = F0 + (1 - F0) * pow(1 - rDotN, 5);
            #else
                float Rparl  = (_IOR * rDotN - tDotN) / (_IOR  * rDotN + tDotN);
                float Rperp  = (rDotN - _IOR * tDotN) / (rDotN + _IOR  * tDotN);
                float Fr     = 0.5 * (Rparl * Rparl + Rperp * Rperp);
            #endif

                float Ft     = 1.0 - Fr;
            #ifdef _NONLINEAR_ON
                Fr = (1+Ft*Ft/(1-Fr*Fr))*Fr;
                Ft = 1.0 - Fr;
                float3 col  = Fr * colR + Ft * colT;
            #else
                // do nothing
                float3 col  = Fr * colR + Ft * Ft * colT;
            #endif

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}
