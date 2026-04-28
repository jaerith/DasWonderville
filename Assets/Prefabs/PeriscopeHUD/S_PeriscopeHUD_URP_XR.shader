
Shader "Custom/PeriscopeHUD_URP_XR"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutsideColor ("Outside Color", Color) = (0, 0, 0, 0.82)
        _InsideTintColor ("Inside Tint Color", Color) = (0.05, 0.18, 0.16, 0.18)
        _RingColor ("Ring Color", Color) = (0.03, 0.95, 0.75, 0.85)
        _Radius ("Radius", Range(0.1, 0.5)) = 0.37
        _Softness ("Softness", Range(0.001, 0.1)) = 0.015
        _RingThickness ("Ring Thickness", Range(0.001, 0.08)) = 0.012
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _OutsideColor;
                float4 _InsideTintColor;
                float4 _RingColor;
                float _Radius;
                float _Softness;
                float _RingThickness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float2 centered = input.uv - 0.5;
                centered.x *= 16.0 / 9.0;

                float d = length(centered);

                float outside = smoothstep(_Radius - _Softness, _Radius + _Softness, d);

                float ring = 1.0 - smoothstep(
                    _RingThickness,
                    _RingThickness + _Softness,
                    abs(d - _Radius)
                );

                float scanline = sin(input.uv.y * 900.0) * 0.015;

                float4 col = lerp(_InsideTintColor, _OutsideColor, outside);
                col.rgb += scanline;
                col = lerp(col, _RingColor, saturate(ring));

                col *= input.color;
                col.a *= tex.a;

                return col;
            }

            ENDHLSL
        }
    }
}
