using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateLightweightOceanShader
{
    private const string ShaderPath = "Assets/SG_Ocean_Lightweight_Generated.shader";
    private const string MaterialPath = "Assets/M_Ocean_Lightweight.mat";

    [MenuItem("Tools/Ocean/Create and Apply Lightweight Ocean Shader")]
    public static void CreateAndApply()
    {
        File.WriteAllText(ShaderPath, ShaderCode);
        AssetDatabase.ImportAsset(ShaderPath);
        AssetDatabase.Refresh();

        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null)
        {
            Debug.LogError("Failed to create ocean shader.");
            return;
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        mat.shader = shader;

        mat.SetColor("_WaterColor", new Color(0.02f, 0.22f, 0.45f, 1f));
        mat.SetColor("_FoamColor", Color.white);
        mat.SetFloat("_WaveAmplitude", 0.12f);
        mat.SetFloat("_WaveFrequency", 0.18f);
        mat.SetFloat("_WaveSpeed", 1.2f);
        mat.SetFloat("_FoamAmount", 0.55f);
        mat.SetFloat("_FresnelPower", 4.0f);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();

        Debug.Log("Created and applied lightweight animated ocean shader.");
    }

    private const string ShaderCode = @"
Shader ""Custom/LightweightOceanURP""
{
    Properties
    {
        _WaterColor (""Water Color"", Color) = (0.02, 0.22, 0.45, 1)
        _FoamColor (""Foam Color"", Color) = (1, 1, 1, 1)
        _WaveAmplitude (""Wave Amplitude"", Float) = 0.12
        _WaveFrequency (""Wave Frequency"", Float) = 0.18
        _WaveSpeed (""Wave Speed"", Float) = 1.2
        _FoamAmount (""Foam Amount"", Range(0, 1)) = 0.55
        _FresnelPower (""Fresnel Power"", Float) = 4
    }

    SubShader
    {
        Tags
        {
            ""RenderPipeline"" = ""UniversalPipeline""
            ""RenderType"" = ""Opaque""
            ""Queue"" = ""Geometry""
        }

        Pass
        {
            Name ""ForwardLit""
            Tags { ""LightMode"" = ""UniversalForward"" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""

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
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float waveHeight : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 pos = input.positionOS.xyz;

                float t = _Time.y * _WaveSpeed;

                float wave1 = sin((pos.x + t) * _WaveFrequency * 8.0);
                float wave2 = sin((pos.z * 1.4 + t * 0.8) * _WaveFrequency * 7.0);
                float wave3 = sin((pos.x + pos.z + t * 1.3) * _WaveFrequency * 5.0);

                float wave = (wave1 + wave2 + wave3) / 3.0;

                pos.y += wave * _WaveAmplitude;

                float3 positionWS = TransformObjectToWorld(pos);

                output.positionHCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.waveHeight = wave;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(GetWorldSpaceViewDir(input.positionWS));
                float fresnel = pow(1.0 - saturate(dot(normalize(input.normalWS), viewDir)), _FresnelPower);

                float crest = smoothstep(_FoamAmount, 1.0, input.waveHeight);

                float3 color = _WaterColor.rgb;
                color += fresnel * 0.25;
                color = lerp(color, _FoamColor.rgb, crest * 0.45);

                return half4(color, 1.0);
            }

            ENDHLSL
        }
    }
}
";
}