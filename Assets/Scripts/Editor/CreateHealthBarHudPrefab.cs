using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreateHealthBarHudPrefab
{
    private const string FolderPath = "Assets/HealthBarHUD";
    private const string PrefabPath = FolderPath + "/PF_HealthBarHUD.prefab";

    // Rendered on top of the other periscope HUD boxes (StatsHUD 501, RadarHUD 502, PeriscopeZoomHUD 503, TimeToTargetHUD 504)
    private const int SortOrder = 505;

    private const float CanvasScale = 0.0004f;
    private const float BarWidth  = 80f;
    private const float BarHeight = 360f;
    private const float BorderThickness = 6f;

    private static readonly Color TealDim   = new Color(0.02f, 1.00f, 0.75f, 0.35f);
    private static readonly Color BgColor   = new Color(0.00f, 0.04f, 0.07f, 0.90f);
    private static readonly Color FullColor = new Color(0.85f, 0.05f, 0.05f, 1f);

    [MenuItem("Tools/HUD/Create Health Bar HUD Prefab")]
    public static void CreateHud()
    {
        EnsureFolder();

        // ── Root ─────────────────────────────────────────────────────────────
        GameObject root = new GameObject("PF_HealthBarHUD");
        root.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = SortOrder;

        root.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1000;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta  = new Vector2(BarWidth + BorderThickness * 2f, BarHeight + BorderThickness * 2f);
        rootRect.localScale = Vector3.one * CanvasScale;

        // ── Frame (border) ──────────────────────────────────────────────────
        Image frameImg = CreateImage(root.transform, "Frame",
            new Vector2(BarWidth + BorderThickness * 2f, BarHeight + BorderThickness * 2f),
            TealDim);
        frameImg.raycastTarget = false;

        // ── Background (empty portion of the bar) ───────────────────────────
        Image bgImg = CreateImage(root.transform, "Background",
            new Vector2(BarWidth, BarHeight),
            BgColor);
        bgImg.raycastTarget = false;

        // ── Fill (health readout, vertical fill from the bottom up) ─────────
        Image fillImg = CreateImage(root.transform, "Fill",
            new Vector2(BarWidth, BarHeight),
            FullColor);
        fillImg.raycastTarget = false;
        fillImg.type          = Image.Type.Filled;
        fillImg.fillMethod     = Image.FillMethod.Vertical;
        fillImg.fillOrigin     = (int)Image.OriginVertical.Bottom;
        fillImg.fillAmount     = 1f;

        // ── Label ────────────────────────────────────────────────────────────
        CreateLabel(root.transform, "HEALTH", new Vector2(0f, BarHeight / 2f + 30f), 22);

        // ── Follower: center-left of the screen ─────────────────────────────
        PeriscopeHudFollower follower = root.AddComponent<PeriscopeHudFollower>();
        follower.distanceFromCamera = 0.85f;
        follower.localOffset        = new Vector3(-0.38f, 0f, 0f);

        // ── Runtime display driver ───────────────────────────────────────────
        HealthBarDisplay display = root.AddComponent<HealthBarDisplay>();
        display.fillImage = fillImg;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created Health Bar HUD prefab: " + PrefabPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets", "HealthBarHUD");
    }

    private static Image CreateImage(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta        = size;
        rect.anchoredPosition = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        return img;
    }

    private static void CreateLabel(Transform parent, string text, Vector2 position, int fontSize)
    {
        GameObject obj = new GameObject(text);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta        = new Vector2(200f, 40f);

        Text label = obj.AddComponent<Text>();
        label.text          = text;
        label.font          = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize      = fontSize;
        label.alignment     = TextAnchor.MiddleCenter;
        label.color         = TealDim;
        label.raycastTarget = false;
    }
}
