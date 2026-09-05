Shader "SIH26169/ImageNoise"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _NoiseType ("Noise Type", Float) = 0
        _Intensity ("Intensity", Float) = 10
        _Coverage ("Coverage", Float) = 0.1
        _TimeSeed ("Time Seed", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _NoiseType;
            float _Intensity;
            float _Coverage;
            float _TimeSeed;

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            float RandomValue(float2 uv)
            {
                return frac(
                    sin(
                        dot(
                            uv + _TimeSeed,
                            float2(12.9898, 78.233)
                        )
                    ) * 43758.5453
                );
            }

            float GaussianRandom(float2 uv)
            {
                float u1 =
                    max(RandomValue(uv), 0.0001);

                float u2 =
                    RandomValue(uv + 17.23);

                return sqrt(-2.0 * log(u1))
                    * cos(6.2831853 * u2);
            }

            float3 ApplySaltAndPepper(
                float3 color,
                float2 uv
            )
            {
                float randomValue =
                    RandomValue(uv);

                if (randomValue < _Coverage * 0.5)
                    return float3(0, 0, 0);

                if (randomValue < _Coverage)
                    return float3(1, 1, 1);

                return color;
            }

            float3 ApplyGaussian(
                float3 color,
                float2 uv
            )
            {
                float noise =
                    GaussianRandom(uv);

                float amount =
                    _Intensity / 255.0;

                color += noise * amount;

                return saturate(color);
            }

            float PoissonSample(
                float value,
                float2 uv
            )
            {
                float gaussian =
                    GaussianRandom(uv);

                float variance =
                    sqrt(max(value, 0.0001));

                return value +
                    gaussian * variance;
            }

            float3 ApplyPoisson(
                float3 color,
                float2 uv
            )
            {
                float3 result;

                result.r =
                    PoissonSample(
                        color.r * 255.0,
                        uv
                    ) / 255.0;

                result.g =
                    PoissonSample(
                        color.g * 255.0,
                        uv + 11.7
                    ) / 255.0;

                result.b =
                    PoissonSample(
                        color.b * 255.0,
                        uv + 23.4
                    ) / 255.0;

                float strength =
                    _Intensity / 20.0;

                result =
                    lerp(
                        color,
                        result,
                        strength
                    );

                return saturate(result);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float4 source =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    );

                float3 color = source.rgb;

                if (_NoiseType == 1)
                {
                    color =
                        ApplySaltAndPepper(
                            color,
                            input.uv
                        );
                }
                else if (_NoiseType == 2)
                {
                    color =
                        ApplyGaussian(
                            color,
                            input.uv
                        );
                }
                else if (_NoiseType == 3)
                {
                    color =
                        ApplyPoisson(
                            color,
                            input.uv
                        );
                }

                return half4(
                    color,
                    source.a
                );
            }

            ENDHLSL
        }
    }
}