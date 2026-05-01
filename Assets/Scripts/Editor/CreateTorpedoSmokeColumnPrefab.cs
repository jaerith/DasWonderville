#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateTorpedoSmokeColumnPrefab
{
    private const string OutputFolder = "Assets/Wonderville/Effects";
    private const string PrefabPath = OutputFolder + "/TorpedoImpactSmokeColumn.prefab";
    private const string MaterialPath = OutputFolder + "/TorpedoImpactSmoke_Mat.mat";

    [MenuItem("Tools/Wonderville/Create Torpedo Impact Smoke Column")]
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
            mat.name = "TorpedoImpactSmoke_Mat";

            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", new Color(0.18f, 0.18f, 0.18f, 0.55f));

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(0.18f, 0.18f, 0.18f, 0.55f));

            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        GameObject root = new GameObject("TorpedoImpactSmokeColumn");

        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = -2f;

        var main = ps.main;
        main.duration = 4f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 0.85f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.45f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.12f, 0.12f, 0.12f, 0.7f),
            new Color(0.45f, 0.45f, 0.45f, 0.25f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 180;
        main.gravityModifier = -0.08f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 40),
            new ParticleSystem.Burst(0.25f, 25),
            new ParticleSystem.Burst(0.55f, 18)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 16f;
        shape.radius = 0.18f;
        shape.length = 0.75f;
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = new ParticleSystem.MinMaxCurve(0.7f, 1.8f);
        velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);

        var force = ps.forceOverLifetime;
        force.enabled = true;
        force.space = ParticleSystemSimulationSpace.World;
        force.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        force.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.25f),
            new Keyframe(0.25f, 1.0f),
            new Keyframe(0.7f, 1.8f),
            new Keyframe(1f, 2.4f)
        );
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.08f, 0.08f, 0.08f), 0f),
                new GradientColorKey(new Color(0.35f, 0.35f, 0.35f), 0.45f),
                new GradientColorKey(new Color(0.62f, 0.62f, 0.62f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.65f, 0.12f),
                new GradientAlphaKey(0.35f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = gradient;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.42f;
        noise.frequency = 0.85f;
        noise.scrollSpeed = 0.35f;
        noise.octaveCount = 2;

        var rotation = ps.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-0.7f, 0.7f);

        var rendererModule = ps.GetComponent<ParticleSystemRenderer>();
        rendererModule.sharedMaterial = mat;
        rendererModule.minParticleSize = 0.02f;
        rendererModule.maxParticleSize = 1.25f;

        string prefabPath = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) != null ? PrefabPath : null;
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!string.IsNullOrEmpty(prefabPath))
        {
            Debug.Log("Created torpedo impact smoke column prefab at: " + prefabPath);
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