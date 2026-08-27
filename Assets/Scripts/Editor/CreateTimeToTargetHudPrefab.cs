using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class CreateTimeToTargetHudPrefab
{
    private const string FolderPath  = "Assets/TimeToTargetHUD";
    private const string ShaderPath  = FolderPath + "/S_TimeToTargetHUDPanel.shader";
    private const string BgMatPath   = FolderPath + "/M_TimeToTargetHUDBg.mat";
    private const string LineMatPath = FolderPath + "/M_TimeToTargetHUDLine.mat";
    private const string PrefabPath  = FolderPath + "/PF_TimeToTargetHUD.prefab";

    // Rendered on top of the other periscope HUD boxes (StatsHUD 501, RadarHUD 502, PeriscopeZoomHUD 503)
    private const int SortOrder = 504;

    private static readonly Color TealBright = new Color(0.02f, 1f, 0.75f, 0.90f);
    private static readonly Color TealDim    = new Color(0.02f, 1f, 0.75f, 0.45f);
    private static readonly Color BgColor    = new Color(0f, 0.05f, 0.05f, 0.85f);

    [MenuItem("Tools/HUD/Create Time To Target HUD Prefab")]
    public static void CreateHud()
    {
        EnsureFolder();

        // Write shader to disk and import it
        File.WriteAllText(ShaderPath, PanelShaderCode);
        AssetDatabase.ImportAsset(ShaderPath);

        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null)
        {
            Debug.LogError("TimeToTargetHUD: failed to load panel shader at " + ShaderPath);
            return;
        }

        // Create and save the two shared materials
        Material bgMat   = MakeMat(shader, BgColor,   BgMatPath);
        Material lineMat = MakeMat(shader, TealDim,   LineMatPath);

        // ── Root ──────────────────────────────────────────────────────────
        GameObject root = new GameObject("PF_TimeToTargetHUD");

        // Background panel (sits 2mm behind text so depth-test keeps text on top)
        MakeQuad(root.transform, "Background", bgMat,
            localPos:   new Vector3(0f, 0f, 0.002f),
            localScale: new Vector3(0.42f, 0.16f, 1f));

        // Border lines
        MakeQuad(root.transform, "Border Top",    lineMat, new Vector3(0f,   0.079f, 0f), new Vector3(0.42f, 0.002f, 1f));
        MakeQuad(root.transform, "Border Bottom", lineMat, new Vector3(0f,  -0.079f, 0f), new Vector3(0.42f, 0.002f, 1f));
        MakeQuad(root.transform, "Border Left",   lineMat, new Vector3(-0.209f, 0f, 0f),  new Vector3(0.002f, 0.16f,  1f));
        MakeQuad(root.transform, "Border Right",  lineMat, new Vector3( 0.209f, 0f, 0f),  new Vector3(0.002f, 0.16f,  1f));

        // Title label
        MakeTMP(root.transform, "Title", "TIME TO TARGET",
            localPos: new Vector3(0f, 0.045f, 0f),
            rectSize: new Vector2(0.40f, 0.045f),
            fontSize: 0.026f,
            color:    TealDim);

        // Time readout (reference kept for TimeToTargetHudDisplay)
        TextMeshPro timeText = MakeTMP(root.transform, "Time Readout", "--:--",
            localPos: new Vector3(0f, -0.020f, 0f),
            rectSize: new Vector2(0.40f, 0.080f),
            fontSize: 0.055f,
            color:    TealBright);

        // Position HUD at top-center of view, above the crosshair
        PeriscopeHudFollower follower = root.AddComponent<PeriscopeHudFollower>();
        follower.distanceFromCamera = 0.85f;
        follower.localOffset = new Vector3(0f, 0.24f, 0f);

        // Runtime display driver
        TimeToTargetHudDisplay display = root.AddComponent<TimeToTargetHudDisplay>();
        display.timeToTargetText = timeText;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created Time To Target HUD prefab: " + PrefabPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets", "TimeToTargetHUD");
    }

    private static Material MakeMat(Shader shader, Color color, string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(assetPath) != null)
            AssetDatabase.DeleteAsset(assetPath);

        Material mat = new Material(shader);
        mat.SetColor("_Color", color);
        AssetDatabase.CreateAsset(mat, assetPath);
        return mat;
    }

    private static void MakeQuad(Transform parent, string name, Material mat,
                                  Vector3 localPos, Vector3 localScale)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localScale    = localScale;

        // Quads don't have a collider in most Unity versions, but remove one if present
        Collider col = obj.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);

        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        mr.sharedMaterial     = mat;
        mr.shadowCastingMode  = ShadowCastingMode.Off;
        mr.receiveShadows     = false;
        mr.sortingOrder       = SortOrder;
    }

    private static TextMeshPro MakeTMP(Transform parent, string name, string text,
                                        Vector3 localPos, Vector2 rectSize,
                                        float fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;

        TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
        tmp.text               = text;
        tmp.fontSize           = fontSize;
        tmp.color              = color;
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.overflowMode       = TextOverflowModes.Overflow;
        tmp.sortingOrder       = SortOrder;

        tmp.rectTransform.sizeDelta = rectSize;

        return tmp;
    }

    // ── Embedded URP / XR panel shader ────────────────────────────────────
    private const string PanelShaderCode = @"
Shader ""Custom/TimeToTargetHUDPanel""
{
    Properties
    {
        _Color (""Color"", Color) = (0, 0.05, 0.05, 0.85)
    }

    SubShader
    {
        Tags
        {
            ""RenderPipeline"" = ""UniversalPipeline""
            ""Queue""          = ""Transparent""
            ""RenderType""     = ""Transparent""
            ""IgnoreProjector"" = ""True""
        }

        Pass
        {
            ZWrite Off
            ZTest  Always
            Cull   Off
            Blend  SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                return _Color;
            }
            ENDHLSL
        }
    }
}
";
}
