#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateSubmarineBubbleBurstPrefab
{
    private const string OutputFolder = "Assets/Wonderville/Effects";
    private const string PrefabPath = OutputFolder + "/SubmarineBubbleBurst.prefab";
    private const string MaterialPath = OutputFolder + "/SubmarineBubbleBurst_Mat.mat";

    [MenuItem("Tools/Wonderville/Create Submarine Bubble Burst Prefab")]
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
            mat.name = "SubmarineBubbleBurst_Mat";

            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", new Color(0.75f, 0.95f, 1f, 0.45f));

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(0.75f, 0.95f, 1f, 0.45f));

            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        GameObject root = new GameObject("SubmarineBubbleBurst");

        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = -1f;

        var main = ps.main;
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 6.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.12f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.7f, 0.95f, 1f, 0.65f),
            new Color(1f, 1f, 1f, 0.2f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 220;
        main.gravityModifier = -0.15f;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 90f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 80),
            new ParticleSystem.Burst(0.35f, 45),
            new ParticleSystem.Burst(0.8f, 30)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.35f;
        shape.length = 0.75f;
        shape.position = new Vector3(0f, -0.15f, 0.4f);
        shape.rotation = Vector3.zero;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.z = new ParticleSystem.MinMaxCurve(1.0f, 3.0f);
        velocity.x = new ParticleSystem.MinMaxCurve(-0.9f, 0.9f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.4f, 1.4f);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.25f),
                new Keyframe(0.25f, 1f),
                new Keyframe(1f, 1.8f)
            )
        );

        var color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.65f, 0.9f, 1f), 0f),
                new GradientColorKey(new Color(1f, 1f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.55f, 0.12f),
                new GradientAlphaKey(0.25f, 0.65f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = gradient;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 2.2f;
        noise.scrollSpeed = 1.5f;
        noise.octaveCount = 2;

        var rendererModule = ps.GetComponent<ParticleSystemRenderer>();
        rendererModule.sharedMaterial = mat;
        rendererModule.minParticleSize = 0.005f;
        rendererModule.maxParticleSize = 0.18f;

        string prefabPath =
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) != null
                ? PrefabPath
                : null;

        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!string.IsNullOrEmpty(prefabPath))
        {
            Debug.Log("Created submarine bubble burst prefab at: " + prefabPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

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