Shader "Custom/Particle"
{
    Properties
    {
        _MainTex("Texture", 2D) = "black" {}
        _Radius("Particle Radius", Float) = 0.05
        _ParticleColor("Particle Color", Color) = (1, 1, 1, 1)
        _ClipThreshold("Clip Threshold", Float) = 0.1
        //        _AlphaPower("Alpha Power", Float) = 0.01
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "Queue" = "Geometry"
                "IgnoreProjector" = "True"
                "RenderType" = "Opaque"
                "PreviewType" = "Plane"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "./ParticleStruct.hlsl"

            struct V2G
            {
                float4 pos : SV_POSITION;
            };

            struct G2F
            {
                float4 pos : POSITION;
                float2 tex : TEXCOORD0;
            };

            uniform sampler2D _MainTex;
            uniform float4 _ParticleColor;
            uniform float _ClipThreshold, _Radius;
            StructuredBuffer<Particle> _ParticlesBuffer;

            V2G vert(uint id : SV_VertexID)
            {
                V2G o;
                o.pos = float4(_ParticlesBuffer[id].position.xy, 0, 1);
                return o;
            }

            [maxvertexcount(4)]
            void geom(point V2G input[1], inout TriangleStream<G2F> tri_stream)
            {
                G2F output;
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        float4x4 billboard_matrix = UNITY_MATRIX_V;
                        billboard_matrix._m03 = billboard_matrix._m13 = 0;
                        billboard_matrix._m23 = billboard_matrix._m33 = 0;

                        float2 uv = float2(x, y);
                        float4 pos = input[0].pos + mul(
                            float4((uv * 2 - float2(1, 1)) * _Radius, 0, 1), billboard_matrix
                        );

                        output.pos = mul(UNITY_MATRIX_VP, pos);
                        output.tex = uv;

                        tri_stream.Append(output);
                    }
                }
                tri_stream.RestartStrip();
            }

            float4 frag(G2F input) : SV_Target
            {
                float4 c = tex2D(_MainTex, input.tex) * _ParticleColor;
                clip(c.a - _ClipThreshold);
                return c;
            }
            ENDCG
        }

        //        Pass{
        //            Blend SrcAlpha OneMinusSrcAlpha, One One
        //
        //            CGPROGRAM
        //            #pragma target 5.0
        //            #pragma vertex vert
        //            #pragma geometry geom
        //            #pragma fragment frag
        //
        //            uniform float _AlphaPower;
        //            uniform int _TargetColorIndex;
        //
        //            float4 frag(G2F input) : SV_Target{
        //                if(input.colorIndex != _TargetColorIndex){
        //                    discard;
        //                }
        //
        //                float4 c = tex2D(_MainTex, input.tex) * input.color;
        //
        //                float2 uv = input.tex - 0.5;
        //                float a = 1.0 / (uv.x * uv.x + uv.y * uv.y);
        //                a *= _AlphaPower;
        //
        //                clip(a - _ClipThreshold);
        //
        //                //if(!(c.r > 0.5 && c.g > 0.5 && c.b > 0.5)){
        //                //                 discard;   
        //                //}
        //                //return float4(a, 0.0f, 0.0f, a);
        //                return float4(c.rgb, saturate(a));
        //            }
        //            ENDCG
        //        }
    }
}