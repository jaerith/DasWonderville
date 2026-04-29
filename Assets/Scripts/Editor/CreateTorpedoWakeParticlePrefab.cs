#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateTorpedoWakeParticlePrefab
{
    private const string OutputFolder = "Assets/Wonderville/Effects";
    private const string PrefabPath = OutputFolder + "/TorpedoWaterWake.prefab";
    private const string MaterialPath = OutputFolder + "/TorpedoWaterWake_Mat.mat";

    [MenuItem("Tools/Wonderville/Create Torpedo Water Wake Prefab")]
    public static void Create()
    {
        EnsureFolder(OutputFolder);

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            Shader shader =
                Shader.Find("Particles/Standard Unlit") ??
                Shader.Find("Mobile/Particles/Alpha Blended") ??
                Shader.Find("Legacy Shaders/Particles/Alpha Blended");

            mat = new Material(shader);
            mat.name = "TorpedoWaterWake_Mat";

            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", new Color(0.55f, 0.85f, 1f, 0.35f));

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(0.55f, 0.85f, 1f, 0.35f));

            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        GameObject root = new GameObject("TorpedoWaterWake");

        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = -1f;

        var main = ps.main;
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.75f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.16f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.65f, 0.9f, 1f, 0.45f),
            new Color(0.9f, 1f, 1f, 0.15f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 120;
        main.gravityModifier = 0f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 45f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 18f;
        shape.radius = 0.08f;
        shape.length = 0.25f;
        shape.position = new Vector3(0f, 0f, -0.35f);
        shape.rotation = new Vector3(0f, 180f, 0f);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.z = new ParticleSystem.MinMaxCurve(-0.8f, -1.8f);
        velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.25f),
            new Keyframe(0.25f, 1f),
            new Keyframe(1f, 2.2f)
        );
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.7f, 0.9f, 1f), 0f),
                new GradientColorKey(new Color(0.9f, 1f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.45f, 0.15f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = gradient;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.18f;
        noise.frequency = 1.8f;
        noise.scrollSpeed = 0.35f;

        var rendererModule = ps.GetComponent<ParticleSystemRenderer>();
        rendererModule.sharedMaterial = mat;
        rendererModule.minParticleSize = 0.005f;
        rendererModule.maxParticleSize = 0.3f;

        string prefabPath = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) != null ? PrefabPath : null;
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!string.IsNullOrEmpty(prefabPath))
        {
            Debug.Log("Created torpedo water wake prefab at: " + prefabPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
        }
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