using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreatePeriscopeHudPrefab
{
    private const string FolderPath = "Assets/PeriscopeHUD";
    private const string ShaderPath = FolderPath + "/S_PeriscopeHUD_URP_XR.shader";
    private const string MaterialPath = FolderPath + "/M_PeriscopeHUD.mat";
    private const string PrefabPath = FolderPath + "/PF_PeriscopeHUD.prefab";

    [MenuItem("Tools/HUD/Create Periscope HUD Prefab")]
    public static void CreateHud()
    {
        EnsureFolder();

        File.WriteAllText(ShaderPath, ShaderCode);
        AssetDatabase.ImportAsset(ShaderPath);

        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null)
        {
            Debug.LogError("Failed to load generated periscope HUD shader.");
            return;
        }

        Material mat = new Material(shader);
        mat.SetColor("_OutsideColor", new Color(0f, 0f, 0f, 0.82f));
        mat.SetColor("_InsideTintColor", new Color(0.05f, 0.18f, 0.16f, 0.10f));
        mat.SetColor("_RingColor", new Color(0.03f, 0.95f, 0.75f, 0.85f));
        mat.SetFloat("_Radius", 0.37f);
        mat.SetFloat("_Softness", 0.015f);
        mat.SetFloat("_RingThickness", 0.012f);

        AssetDatabase.CreateAsset(mat, MaterialPath);

        GameObject root = new GameObject("PF_PeriscopeHUD");
        root.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 500;

        root.AddComponent<GraphicRaycaster>();

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(4000f, 3000f);
        rootRect.localScale = Vector3.one * 0.001f;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1000;

        CreateOverlay(root.transform, mat);
        CreateCrosshair(root.transform);
        CreateRangeTicks(root.transform);
        CreateLabels(root.transform);

        PeriscopeHudFollower follower = root.AddComponent<PeriscopeHudFollower>();
        follower.distanceFromCamera = 0.85f;
        follower.localOffset = new Vector3(0f, -0.02f, 0f);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created periscope HUD prefab: {PrefabPath}");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets", "PeriscopeHUD");
    }

    private static void CreateOverlay(Transform parent, Material unused)
    {
        string texturePath = FolderPath + "/T_PeriscopeOverlay.png";

        Texture2D texture = GeneratePeriscopeOverlayTexture(2048, 2048);
        File.WriteAllBytes(texturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Could not get TextureImporter for " + texturePath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);

        if (sprite == null)
        {
            Debug.LogError("Periscope overlay sprite failed to load: " + texturePath);
            return;
        }

        GameObject obj = new GameObject("Periscope Mask And Glass");
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.color = Color.white;
        img.raycastTarget = false;

        obj.transform.SetAsFirstSibling();
    }

    private static void CreateCrosshair(Transform parent)
    {
        CreateLine(parent, "Vertical Crosshair", new Vector2(0f, 0f), new Vector2(2f, 520f));
        CreateLine(parent, "Horizontal Crosshair", new Vector2(0f, 0f), new Vector2(520f, 2f));

        CreateLine(parent, "Upper Gap Line", new Vector2(0f, 210f), new Vector2(2f, 70f));
        CreateLine(parent, "Lower Gap Line", new Vector2(0f, -210f), new Vector2(2f, 70f));
        CreateLine(parent, "Left Gap Line", new Vector2(-210f, 0f), new Vector2(70f, 2f));
        CreateLine(parent, "Right Gap Line", new Vector2(210f, 0f), new Vector2(70f, 2f));
    }

    private static void CreateRangeTicks(Transform parent)
    {
        for (int i = -4; i <= 4; i++)
        {
            if (i == 0) continue;

            CreateLine(parent, $"Range Tick X {i}", new Vector2(i * 55f, -12f), new Vector2(2f, 24f));
            CreateLine(parent, $"Range Tick Y {i}", new Vector2(12f, i * 55f), new Vector2(24f, 2f));
        }
    }

    private static void CreateLabels(Transform parent)
    {
        CreateText(parent, "BEARING 000", new Vector2(0f, 315f), 28);
        CreateText(parent, "RANGE ---", new Vector2(0f, -315f), 24);
        CreateText(parent, "DEPTH ---", new Vector2(-430f, -315f), 22);
        CreateText(parent, "SCOPE ACTIVE", new Vector2(430f, -315f), 22);
    }

    private static void CreateLine(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.02f, 1f, 0.75f, 0.72f);
        img.raycastTarget = false;
    }

    private static void CreateText(Transform parent, string text, Vector2 position, int fontSize)
    {
        GameObject obj = new GameObject(text);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400f, 60f);

        Text label = obj.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.02f, 1f, 0.75f, 0.8f);
        label.raycastTarget = false;
    }

    private static Texture2D GeneratePeriscopeOverlayTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color outside = new Color(0f, 0f, 0f, 0.88f);
        Color inside = new Color(0.03f, 0.18f, 0.16f, 0.12f);
        Color ring = new Color(0.02f, 1f, 0.75f, 0.85f);

        float radius = 0.20f;
        float ringThickness = 0.012f;
        float aspect = 16f / 9f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);

                float cx = (u - 0.5f) * aspect;
                float cy = v - 0.5f;
                float d = Mathf.Sqrt(cx * cx + cy * cy);

                Color c = d <= radius ? inside : outside;

                if (Mathf.Abs(d - radius) < ringThickness)
                    c = ring;

                // Subtle scanline effect inside the scope.
                if (d <= radius)
                {
                    float scan = Mathf.Sin(v * 900f) * 0.015f;
                    c.r += scan;
                    c.g += scan;
                    c.b += scan;
                }

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return tex;
    }

    private const string ShaderCode = @"
Shader ""Custom/PeriscopeHUD_URP_XR""
{
    Properties
    {
        _MainTex (""Texture"", 2D) = ""white"" {}
        _OutsideColor (""Outside Color"", Color) = (0, 0, 0, 0.82)
        _InsideTintColor (""Inside Tint Color"", Color) = (0.05, 0.18, 0.16, 0.18)
        _RingColor (""Ring Color"", Color) = (0.03, 0.95, 0.75, 0.85)
        _Radius (""Radius"", Range(0.1, 0.5)) = 0.37
        _Softness (""Softness"", Range(0.001, 0.1)) = 0.015
        _RingThickness (""Ring Thickness"", Range(0.001, 0.08)) = 0.012
    }

    SubShader
    {
        Tags
        {
            ""RenderPipeline"" = ""UniversalPipeline""
            ""Queue"" = ""Transparent""
            ""RenderType"" = ""Transparent""
            ""IgnoreProjector"" = ""True""
            ""CanUseSpriteAtlas"" = ""True""
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

            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""

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
";
}