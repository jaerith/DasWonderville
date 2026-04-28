using UnityEditor;
using UnityEngine;

public static class CreateLightweightOceanPrefab
{
    private const string PrefabPath = "Assets/PF_Ocean_Lightweight.prefab";
    private const string MaterialPath = "Assets/M_Ocean_Lightweight.mat";

    [MenuItem("Tools/Ocean/Create Lightweight Ocean Prefab")]
    public static void CreatePrefab()
    {
        // Create material using a built-in URP shader if available.
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit") ??
            Shader.Find("Standard");

        if (shader == null)
        {
            Debug.LogError("Could not find URP Lit or Standard shader.");
            return;
        }

        Material mat = new Material(shader)
        {
            name = "M_Ocean_Lightweight"
        };

        // URP Lit common properties.
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", new Color(0.02f, 0.22f, 0.45f, 1f));

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.85f);

        AssetDatabase.CreateAsset(mat, MaterialPath);

        // Create ocean plane.
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ocean.name = "PF_Ocean_Lightweight";

        ocean.transform.position = Vector3.zero;
        ocean.transform.rotation = Quaternion.identity;
        ocean.transform.localScale = new Vector3(200f, 1f, 200f);

        Renderer renderer = ocean.GetComponent<Renderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        // Add simple ripple animation component.
        OceanRippleAnimator ripple = ocean.AddComponent<OceanRippleAnimator>();
        ripple.scrollSpeed = new Vector2(0.015f, 0.01f);

        // Save prefab.
        PrefabUtility.SaveAsPrefabAsset(ocean, PrefabPath);

        Object.DestroyImmediate(ocean);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created ocean prefab at {PrefabPath}");
    }
}