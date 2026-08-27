#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateDepthChargePrefab
{
    private const string OutputFolder = "Assets/Wonderville/Props";
    private const string PrefabPath = OutputFolder + "/PF_DepthCharge.prefab";
    private const string MaterialPath = OutputFolder + "/DepthCharge_Body_Mat.mat";

    // WWII depth charge drums were roughly 45cm in diameter and 78cm tall.
    private const float BodyRadius = 0.23f;
    private const float BodyHeight = 0.78f;

    private const int FinCount = 4;
    private const float FinHeight = BodyHeight * 0.35f;
    private const float FinThickness = 0.015f;
    private const float FinProtrusion = 0.09f;
    private const float FinYPosition = BodyHeight * 0.22f;

    private static readonly Color BodyColor = new Color(0.10f, 0.10f, 0.11f, 1f);

    [MenuItem("Tools/Wonderville/Create Depth Charge Prefab")]
    public static void Create()
    {
        EnsureFolder(OutputFolder);

        Material bodyMat = CreateMaterial(MaterialPath, "DepthCharge_Body_Mat", BodyColor);

        GameObject root = new GameObject("PF_DepthCharge");

        CreateBody(root.transform, bodyMat);

        for (int i = 0; i < FinCount; i++)
        {
            float angle = i * (360f / FinCount);
            CreateFin(root.transform, bodyMat, angle);
        }

        string prefabPath = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) != null ? PrefabPath : null;
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!string.IsNullOrEmpty(prefabPath))
        {
            Debug.Log("Created depth charge prefab at: " + prefabPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void CreateBody(Transform parent, Material mat)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(parent, false);

        // Sits with its base at the root's origin so the prefab rests naturally
        // on the ground/rack when placed in a scene.
        body.transform.localPosition = new Vector3(0f, BodyHeight * 0.5f, 0f);
        body.transform.localScale = new Vector3(
            BodyRadius / 0.5f,
            BodyHeight / 2f,
            BodyRadius / 0.5f);

        body.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void CreateFin(Transform parent, Material mat, float angleDegrees)
    {
        GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fin.name = "Fin";
        fin.transform.SetParent(parent, false);

        // The fin's own box collider would just add an odd thin collision
        // shape sticking out of the drum; the body's capsule collider is
        // enough for gameplay purposes.
        Object.DestroyImmediate(fin.GetComponent<Collider>());

        fin.transform.localScale = new Vector3(FinProtrusion, FinHeight, FinThickness);

        Quaternion rotation = Quaternion.Euler(0f, angleDegrees, 0f);
        Vector3 basePosition = new Vector3(BodyRadius + FinProtrusion * 0.5f, FinYPosition, 0f);

        fin.transform.localPosition = rotation * basePosition;
        fin.transform.localRotation = rotation;

        fin.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static Material CreateMaterial(string path, string name, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null)
            return mat;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit") ??
            Shader.Find("Standard") ??
            Shader.Find("Legacy Shaders/Diffuse");

        mat = new Material(shader);
        mat.name = name;

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0.3f);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.35f);

        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", 0.35f);

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}
#endif
