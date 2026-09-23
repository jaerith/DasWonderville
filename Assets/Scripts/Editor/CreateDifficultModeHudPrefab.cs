using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreateDifficultModeHudPrefab
{
    private const string ImagePath  = "Assets/Media/Images/difficult_mode.skull.png";
    private const string FolderPath = "Assets/Prefabs/DifficultModeHUD";
    private const string PrefabPath = FolderPath + "/PF_DifficultModeHUD.prefab";

    private const int CanvasSize = 512;

    // Rendered on top of the other periscope HUD boxes (StatsHUD 501, RadarHUD 502, PeriscopeZoomHUD 503, TimeToTargetHUD 504, BrokenPeriscopeHUD/HealthBarHUD 504-505)
    private const int SortOrder = 506;

    [MenuItem("Tools/HUD/Create Difficult Mode HUD Prefab")]
    public static void CreateHud()
    {
        EnsureFolder();

        Sprite skullSprite = EnsureSpriteImport(ImagePath);
        if (skullSprite == null)
        {
            Debug.LogError("DifficultModeHUD: failed to load sprite at " + ImagePath);
            return;
        }

        // ── Root ─────────────────────────────────────────────────────────────
        GameObject root = new GameObject("PF_DifficultModeHUD");
        root.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = SortOrder;

        root.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1000;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta  = new Vector2(CanvasSize, CanvasSize);
        rootRect.localScale = Vector3.one * 0.0004f;

        // ── Skull image ──────────────────────────────────────────────────────
        GameObject imageObj = new GameObject("DifficultModeImage");
        imageObj.transform.SetParent(root.transform, false);

        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        imageRect.sizeDelta        = new Vector2(CanvasSize, CanvasSize);
        imageRect.anchoredPosition = Vector2.zero;

        Image image = imageObj.AddComponent<Image>();
        image.sprite         = skullSprite;
        image.raycastTarget  = false;
        image.preserveAspect = true;

        // ── Follower: top-right of the screen ───────────────────────────────
        PeriscopeHudFollower follower = root.AddComponent<PeriscopeHudFollower>();
        follower.distanceFromCamera = 0.85f;
        follower.localOffset        = new Vector3(0.38f, 0.24f, 0f);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(PrefabPath);
        Debug.Log("Created Difficult Mode HUD prefab: " + PrefabPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets/Prefabs", "DifficultModeHUD");
    }

    private static Sprite EnsureSpriteImport(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
            return sprite;

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Could not get TextureImporter for " + path);
            return null;
        }

        importer.textureType      = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
