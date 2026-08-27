using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreateRadarHudPrefab
{
    private const string FolderPath  = "Assets/RadarHUD";
    private const string TexturePath = FolderPath + "/T_RadarFace.png";
    private const string PrefabPath  = FolderPath + "/PF_RadarHUD.prefab";

    private const int   CanvasSize         = 512;
    private const int   SortOrder          = 502;
    private const float BlipContainerRadius = 195f;

    private static readonly Color TealBright = new Color(0.02f, 1.00f, 0.75f, 0.90f);
    private static readonly Color TealDim    = new Color(0.02f, 1.00f, 0.75f, 0.35f);
    private static readonly Color BgColor    = new Color(0.00f, 0.04f, 0.07f, 0.90f);

    [MenuItem("Tools/HUD/Create Radar HUD Prefab")]
    public static void CreateHud()
    {
        EnsureFolder();

        Sprite face = GenerateAndImportRadarSprite();
        if (face == null)
        {
            Debug.LogError("RadarHUD: failed to generate radar face sprite.");
            return;
        }

        // ── Root ─────────────────────────────────────────────────────────────
        GameObject root = new GameObject("PF_RadarHUD");
        root.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.sortingOrder = SortOrder;

        root.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1000;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta  = new Vector2(CanvasSize, CanvasSize);
        rootRect.localScale = Vector3.one * 0.0004f;

        // ── Radar face image ─────────────────────────────────────────────────
        GameObject faceObj = new GameObject("RadarFace");
        faceObj.transform.SetParent(root.transform, false);

        RectTransform faceRect = faceObj.AddComponent<RectTransform>();
        faceRect.sizeDelta        = new Vector2(CanvasSize, CanvasSize);
        faceRect.anchoredPosition = Vector2.zero;

        Image faceImg = faceObj.AddComponent<Image>();
        faceImg.sprite        = face;
        faceImg.raycastTarget = false;

        // ── Blip container ───────────────────────────────────────────────────
        GameObject blipContObj = new GameObject("BlipContainer");
        blipContObj.transform.SetParent(root.transform, false);

        RectTransform blipContRect = blipContObj.AddComponent<RectTransform>();
        blipContRect.sizeDelta        = new Vector2(CanvasSize, CanvasSize);
        blipContRect.anchoredPosition = Vector2.zero;

        // ── Label ────────────────────────────────────────────────────────────
        CreateLabel(root.transform, "SONAR", new Vector2(0f, -220f), 22);

        // ── Follower ─────────────────────────────────────────────────────────
        PeriscopeHudFollower follower = root.AddComponent<PeriscopeHudFollower>();
        follower.distanceFromCamera = 0.85f;
        follower.localOffset        = new Vector3(-0.38f, -0.22f, 0f);

        // ── Radar display driver ─────────────────────────────────────────────
        RadarDisplay radar = root.AddComponent<RadarDisplay>();
        radar.blipContainer       = blipContRect;
        radar.blipContainerRadius = BlipContainerRadius;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created Radar HUD prefab: " + PrefabPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets", "RadarHUD");
    }

    private static void CreateLabel(Transform parent, string text, Vector2 position, int fontSize)
    {
        GameObject obj = new GameObject(text);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta        = new Vector2(300f, 40f);

        Text label = obj.AddComponent<Text>();
        label.text            = text;
        label.font            = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize        = fontSize;
        label.alignment       = TextAnchor.MiddleCenter;
        label.color           = TealDim;
        label.raycastTarget   = false;
    }

    private static Sprite GenerateAndImportRadarSprite()
    {
        int size = 512;
        Texture2D tex = GenerateRadarTexture(size, size);
        File.WriteAllBytes(TexturePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        if (importer == null) return null;

        importer.textureType         = TextureImporterType.Sprite;
        importer.spriteImportMode    = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled       = false;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
    }

    private static Texture2D GenerateRadarTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Radii in normalized coords (cx/cy range -0.5 to 0.5).
        const float outerRing     = 0.42f;
        const float innerRing     = 0.40f;
        const float rangeRing1    = innerRing / 3f;
        const float rangeRing2    = innerRing * 2f / 3f;
        const float rangeHalfW    = 0.006f;
        const float centerDotR    = 0.018f;
        const float fwdTickHalfW  = 0.012f;

        Color outside = Color.clear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float cx = (float)x / (width  - 1) - 0.5f;
                float cy = (float)y / (height - 1) - 0.5f;
                float d  = Mathf.Sqrt(cx * cx + cy * cy);

                Color c;

                if (d > outerRing)
                {
                    c = outside;
                }
                else if (d >= innerRing)
                {
                    // Outer border ring.
                    c = TealBright;
                }
                else
                {
                    c = BgColor;

                    // Concentric range rings.
                    if (Mathf.Abs(d - rangeRing1) < rangeHalfW ||
                        Mathf.Abs(d - rangeRing2) < rangeHalfW)
                        c = TealDim;

                    // Forward tick: bright mark at the top-center of the dial.
                    if (cy > 0f && d > innerRing - 0.025f &&
                        Mathf.Abs(cx) < fwdTickHalfW)
                        c = TealBright;

                    // Center dot (player position).
                    if (d < centerDotR)
                        c = TealBright;
                }

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return tex;
    }
}
