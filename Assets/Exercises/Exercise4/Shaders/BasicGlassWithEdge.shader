Shader "Unlit/Exercise4/BasicGlassWithEdge"
{
    Properties
    {
        _IOR ("Index of Refraction", Range(0.75 , 1.7)) = 1.5
        _Thickness ("Thickness"    , Range(0.001, 1)) = 0.01
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

            #include "BasicShading.hlsl"


            float _IOR;
            float _Thickness;

            float4 frag (v2f i) : SV_Target
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
                
                float3 R    = reflect(-V, N);
                float rDotN = dot(V, N);
                float3 colR  = sampleEnvmapLod(R, 0);

                float sinRDotN2 = 1 - rDotN * rDotN;
                float sinTDotN2 = _IOR * _IOR * sinRDotN2;
                if (sinTDotN2 > 1.0){ return float4(colR, 1.0); }

                float3 T     = refract(-V, N, _IOR);
                float  tDotN =-dot(T, N);
                float3 colT  = sampleEnvmapLod(-V, 0);

                float Rparl = (_IOR * rDotN - tDotN) / (_IOR  * rDotN + tDotN);
                float Rperp = (rDotN - _IOR * tDotN) / (rDotN + _IOR  * tDotN);
                float Fr    = 0.5 * (Rparl * Rparl + Rperp * Rperp);
                float Ft    = 1.0 - Fr;
                // 
                float  edgeLen   = _Thickness * tDotN;
                float4 edgeLen4  = (i.uv.xxyy * float4(-1,1,-1,1)) + float2(edgeLen,edgeLen-1.0).xyxy;

                bool4  isEdge4   = edgeLen4 > 0.0;
                if (any(isEdge4)){
                    // 接空間ベクトルを計算する
                    float3 dpdx = ddx(i.worldPos);
                    float3 dpdy = ddy(i.worldPos);

                    // 接ベクトル（Tangent）を計算
                    float3 tangent = normalize(dpdx - dot(dpdx, N) * N);
                    // バイタンジェント
                    float3 bitangent = normalize(cross(N, tangent));
                    // 二次入射方向は透過方向
                    float3 V2 = T;

                    float4 tempLen = isEdge4 ? edgeLen4 : 1e20f;
                    float  minLenX = min(tempLen.x,tempLen.y);
                    float  minLenY = min(tempLen.z,tempLen.w);
                    minLenX = minLenX > 1e10 ? 0.0 : minLenX;
                    minLenY = minLenY > 1e10 ? 0.0 : minLenY;

                    bool isEdge4X = any(isEdge4.xy);
                    bool isEdge4Y = any(isEdge4.zw);
                    if (isEdge4X && isEdge4Y){
                        if (minLenX > minLenY){
                            isEdge4Y = false;
                        }
                        else{
                            isEdge4X = false;
                        }
                    }
                    float3 N2 = isEdge4X ? tangent : bitangent;
                    if (dot(N2, V2) < 0.0) {
                        N2 = -N2;
                    } 
                    

                    // // 法線を接線に置き換えて屈折を計算し直す

                    float3 R2    = reflect(-V2, N2);
                    float3 T2    = refract(-V2, N2, 1.0/_IOR);
                    float rDotN2 =  dot(V2, N2);
                    float tDotN2 = -dot(T2, N2);

                    float  Rparl2 = (_IOR * rDotN2 - tDotN2) / (_IOR * rDotN2 + tDotN2);
                    float  Rperp2 = (rDotN2 - _IOR * tDotN2) / (rDotN2 + _IOR * tDotN2);
                    float  Fr2    = 0.5 * (Rparl2 * Rparl2 + Rperp2 * Rperp2);
                    float  Ft2    = 1.0 - Fr2;

                    float3 col = Fr * colR;
                    // 全反射でない場合
                    if (Ft2 > 0.0){
                        float3 colT2  = sampleEnvmapLod(T2, 0);
                        col += Ft * Ft2 * colT2;
                    }
                    else{
                        Fr2 = 1.0;
                    }
                    
                    float3 T3    = reflect(-V, N2);
                    float3 colT3 = sampleEnvmapLod(T3, 0);
                    float3 colT4 = sampleEnvmapLod(V, 0);

                    col += Ft * Fr2/(1+Fr) * (Fr*colT4+colT3);
                    return float4(col, 1.0);
                }else{

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
            }
            ENDCG
        }
    }
}
