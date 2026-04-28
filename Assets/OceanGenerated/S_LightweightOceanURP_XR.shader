
Shader "Custom/LightweightOceanURP_XR"
{
    Properties
    {
        _WaterColor ("Water Color", Color) = (0.02, 0.22, 0.45, 1)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _WaveAmplitude ("Wave Amplitude", Float) = 0.35
        _WaveFrequency ("Wave Frequency", Float) = 0.18
        _WaveSpeed ("Wave Speed", Float) = 1.2
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.72
        _FresnelPower ("Fresnel Power", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _WaterColor;
                float4 _FoamColor;
                float _WaveAmplitude;
                float _WaveFrequency;
                float _WaveSpeed;
                float _FoamAmount;
                float _FresnelPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float waveHeight : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 pos = input.positionOS.xyz;

                float t = _Time.y * _WaveSpeed;

                float wave1 = sin((pos.x * _WaveFrequency) + t);
                float wave2 = sin((pos.z * _WaveFrequency * 1.37) + t * 1.3);
                float wave3 = sin(((pos.x + pos.z) * _WaveFrequency * 0.73) + t * 0.8);

                float wave = (wave1 + wave2 + wave3) / 3.0;

                pos.y += wave * _WaveAmplitude;

                float3 positionWS = TransformObjectToWorld(pos);

                output.positionHCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.waveHeight = saturate((wave + 1.0) * 0.5);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));

                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), _FresnelPower);
                float crest = smoothstep(_FoamAmount, 1.0, input.waveHeight);

                float3 color = _WaterColor.rgb;
                color += fresnel * 0.2;
                color = lerp(color, _FoamColor.rgb, crest * 0.35);

                return half4(color, 1.0);
            }

            ENDHLSL
        }
    }
}
