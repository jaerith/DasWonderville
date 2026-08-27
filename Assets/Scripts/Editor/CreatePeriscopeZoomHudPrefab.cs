using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreatePeriscopeZoomHudPrefab
{
    private const string FolderPath = "Assets/PeriscopeZoomHUD";
    private const string RenderTexturePath = FolderPath + "/RT_PeriscopeZoom.renderTexture";
    private const string PrefabPath = FolderPath + "/PF_PeriscopeZoomHUD.prefab";

    private const int CanvasSize = 480;
    private const int SortOrder = 503;
    private const int RenderTextureSize = 512;

    private static readonly Color TealBright = new Color(0.02f, 1.00f, 0.75f, 0.90f);
    private static readonly Color BgColor = new Color(0.00f, 0.04f, 0.07f, 0.95f);

    [MenuItem("Tools/HUD/Create Periscope Zoom HUD Prefab")]
    public static void CreateHud()
    {
        EnsureFolder();

        RenderTexture renderTexture = CreateRenderTexture();
        if (renderTexture == null)
        {
            Debug.LogError("PeriscopeZoomHUD: failed to create render texture.");
            return;
        }

        // ── Root ─────────────────────────────────────────────────────────────
        GameObject root = new GameObject("PF_PeriscopeZoomHUD");
        root.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = SortOrder;

        root.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1000;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(CanvasSize, CanvasSize);
        rootRect.localScale = Vector3.one * 0.0004f;

        // ── Scope visual (background + render texture + border) ─────────────
        GameObject scopeVisual = new GameObject("ScopeVisual");
        scopeVisual.transform.SetParent(root.transform, false);

        RectTransform visualRect = scopeVisual.AddComponent<RectTransform>();
        visualRect.sizeDelta = new Vector2(CanvasSize, CanvasSize);
        visualRect.anchoredPosition = Vector2.zero;

        CreatePanelImage(scopeVisual.transform, "Background", Vector2.zero, new Vector2(CanvasSize, CanvasSize), BgColor);
        CreateRawImage(scopeVisual.transform, "ScopeDisplay", new Vector2(CanvasSize - 16f, CanvasSize - 16f), renderTexture);
        CreateBorder(scopeVisual.transform, CanvasSize);
        CreateLabel(scopeVisual.transform, "ZOOM", new Vector2(0f, -(CanvasSize / 2f) + 20f), 22);

        scopeVisual.SetActive(false);

        // ── Scope camera ─────────────────────────────────────────────────────
        GameObject cameraObj = new GameObject("ScopeCameraRig");
        cameraObj.transform.SetParent(root.transform, false);

        Camera scopeCamera = cameraObj.AddComponent<Camera>();
        scopeCamera.targetTexture = renderTexture;
        scopeCamera.fieldOfView = 60f;
        scopeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        scopeCamera.enabled = false;

        // ── Follower (mirrors Radar's offset onto the right side) ───────────
        PeriscopeHudFollower follower = root.AddComponent<PeriscopeHudFollower>();
        follower.distanceFromCamera = 0.85f;
        follower.localOffset = new Vector3(0.38f, -0.22f, 0f);

        // ── Zoom controller ──────────────────────────────────────────────────
        PeriscopeZoom zoom = root.AddComponent<PeriscopeZoom>();
        zoom.scopeCamera = scopeCamera;
        zoom.scopeDisplayRoot = scopeVisual;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created Periscope Zoom HUD prefab: " + PrefabPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets", "PeriscopeZoomHUD");
    }

    private static RenderTexture CreateRenderTexture()
    {
        RenderTexture existing = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
        if (existing != null)
            return existing;

        RenderTexture rt = new RenderTexture(RenderTextureSize, RenderTextureSize, 16);
        rt.name = "RT_PeriscopeZoom";

        AssetDatabase.CreateAsset(rt, RenderTexturePath);
        return AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
    }

    private static void CreatePanelImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private static void CreateRawImage(Transform parent, string name, Vector2 size, RenderTexture texture)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        RawImage img = obj.AddComponent<RawImage>();
        img.texture = texture;
        img.raycastTarget = false;
    }

    private static void CreateBorder(Transform parent, float size)
    {
        float half = size / 2f;
        const float thickness = 4f;

        CreatePanelImage(parent, "Border Top", new Vector2(0f, half), new Vector2(size, thickness), TealBright);
        CreatePanelImage(parent, "Border Bottom", new Vector2(0f, -half), new Vector2(size, thickness), TealBright);
        CreatePanelImage(parent, "Border Left", new Vector2(-half, 0f), new Vector2(thickness, size), TealBright);
        CreatePanelImage(parent, "Border Right", new Vector2(half, 0f), new Vector2(thickness, size), TealBright);
    }

    private static void CreateLabel(Transform parent, string text, Vector2 position, int fontSize)
    {
        GameObject obj = new GameObject(text);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300f, 40f);

        Text label = obj.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = TealBright;
        label.raycastTarget = false;
    }
}
