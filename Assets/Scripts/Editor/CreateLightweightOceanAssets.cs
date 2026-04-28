using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class CreateLightweightOceanAssets
{
    private const string FolderPath = "Assets/OceanGenerated";
    private const string ShaderPath = FolderPath + "/S_LightweightOceanURP_XR.shader";
    private const string MaterialPath = FolderPath + "/M_Ocean_Lightweight.mat";
    private const string MeshPath = FolderPath + "/OceanGrid_64.asset";
    private const string PrefabPath = "Assets/PF_Ocean_Lightweight.prefab";

    [MenuItem("Tools/Ocean/Rebuild Lightweight XR Ocean Assets")]
    public static void RebuildOceanAssets()
    {
        EnsureFolder();

        File.WriteAllText(ShaderPath, ShaderCode);
        AssetDatabase.ImportAsset(ShaderPath);

        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null)
        {
            Debug.LogError("Ocean shader failed to compile/load.");
            return;
        }

        Mesh mesh = CreateGridMesh(64, 200f);
        AssetDatabase.CreateAsset(mesh, MeshPath);

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        mat.shader = shader;
        mat.SetColor("_WaterColor", new Color(0.02f, 0.22f, 0.45f, 1f));
        mat.SetColor("_FoamColor", Color.white);
        mat.SetFloat("_WaveAmplitude", 0.35f);
        mat.SetFloat("_WaveFrequency", 0.18f);
        mat.SetFloat("_WaveSpeed", 1.2f);
        mat.SetFloat("_FoamAmount", 0.72f);
        mat.SetFloat("_FresnelPower", 4.0f);

        EditorUtility.SetDirty(mat);

        GameObject ocean = new GameObject("PF_Ocean_Lightweight");

        MeshFilter filter = ocean.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = ocean.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        PrefabUtility.SaveAsPrefabAsset(ocean, PrefabPath);
        Object.DestroyImmediate(ocean);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Rebuilt lightweight XR ocean prefab and assets.");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets", "OceanGenerated");
    }

    private static Mesh CreateGridMesh(int resolution, float size)
    {
        Mesh mesh = new Mesh();
        mesh.name = "OceanGrid_64";

        int vertexCount = (resolution + 1) * (resolution + 1);

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[resolution * resolution * 6];

        int v = 0;

        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float px = ((float)x / resolution - 0.5f) * size;
                float pz = ((float)z / resolution - 0.5f) * size;

                vertices[v] = new Vector3(px, 0f, pz);
                uvs[v] = new Vector2((float)x / resolution, (float)z / resolution);
                v++;
            }
        }

        int t = 0;

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * (resolution + 1) + x;

                triangles[t++] = i;
                triangles[t++] = i + resolution + 1;
                triangles[t++] = i + 1;

                triangles[t++] = i + 1;
                triangles[t++] = i + resolution + 1;
                triangles[t++] = i + resolution + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private const string ShaderCode = @"
Shader ""Custom/LightweightOceanURP_XR""
{
    Properties
    {
        _WaterColor (""Water Color"", Color) = (0.02, 0.22, 0.45, 1)
        _FoamColor (""Foam Color"", Color) = (1, 1, 1, 1)
        _WaveAmplitude (""Wave Amplitude"", Float) = 0.35
        _WaveFrequency (""Wave Frequency"", Float) = 0.18
        _WaveSpeed (""Wave Speed"", Float) = 1.2
        _FoamAmount (""Foam Amount"", Range(0, 1)) = 0.72
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
            #pragma multi_compile_instancing

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
";
}