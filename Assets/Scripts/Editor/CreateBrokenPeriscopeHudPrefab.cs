using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreateBrokenPeriscopeHudPrefab
{
    private const string ImagePath  = "Assets/Media/Images/broken_periscope.png";
    private const string FolderPath = "Assets/Prefabs/PeriscopeBrokenPeriscopeHUD";
    private const string PrefabPath = FolderPath + "/PF_BrokenPeriscopeHUD.prefab";

    private const int CanvasSize = 512;
    private const int SortOrder  = 504;

    [MenuItem("Tools/HUD/Create Broken Periscope HUD Prefab")]
    public static void CreateHud()
    {
        EnsureFolder();

        Sprite brokenPeriscopeSprite = EnsureSpriteImport(ImagePath);
        if (brokenPeriscopeSprite == null)
        {
            Debug.LogError("BrokenPeriscopeHUD: failed to load sprite at " + ImagePath);
            return;
        }

        // ── Root ─────────────────────────────────────────────────────────────
        GameObject root = new GameObject("PF_BrokenPeriscopeHUD");
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

        // ── Broken periscope image ───────────────────────────────────────────
        GameObject imageObj = new GameObject("BrokenPeriscopeImage");
        imageObj.transform.SetParent(root.transform, false);

        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        imageRect.sizeDelta        = new Vector2(CanvasSize, CanvasSize);
        imageRect.anchoredPosition = Vector2.zero;

        Image image = imageObj.AddComponent<Image>();
        image.sprite         = brokenPeriscopeSprite;
        image.raycastTarget  = false;
        image.preserveAspect = true;

        // ── Follower ─────────────────────────────────────────────────────────
        PeriscopeHudFollower follower = root.AddComponent<PeriscopeHudFollower>();
        follower.distanceFromCamera = 0.85f;
        follower.localOffset        = Vector3.zero;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(PrefabPath);
        Debug.Log("Created Broken Periscope HUD prefab: " + PrefabPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets/Prefabs", "PeriscopeBrokenPeriscopeHUD");
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
