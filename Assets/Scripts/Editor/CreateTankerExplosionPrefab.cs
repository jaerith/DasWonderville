#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateTankerExplosionPrefab
{
    private const string OutputFolder = "Assets/Wonderville/Effects";
    private const string PrefabPath = OutputFolder + "/TankerShipExplosion.prefab";
    private const string FireMaterialPath = OutputFolder + "/TankerExplosion_Fire_Mat.mat";
    private const string SmokeMaterialPath = OutputFolder + "/TankerExplosion_Smoke_Mat.mat";

    [MenuItem("Tools/Wonderville/Create Tanker Ship Explosion Prefab")]
    public static void Create()
    {
        EnsureFolder(OutputFolder);

        Material fireMat = CreateMaterial(
            FireMaterialPath,
            "TankerExplosion_Fire_Mat",
            new Color(1f, 0.35f, 0.05f, 0.85f)
        );

        Material smokeMat = CreateMaterial(
            SmokeMaterialPath,
            "TankerExplosion_Smoke_Mat",
            new Color(0.08f, 0.08f, 0.08f, 0.65f)
        );

        GameObject root = new GameObject("TankerShipExplosion");

        CreateFireball(root.transform, fireMat);
        CreateSmokeColumn(root.transform, smokeMat);
        CreateDebrisSparks(root.transform, fireMat);
        CreateShockwave(root.transform, fireMat);

        string prefabPath = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath) != null ? PrefabPath : null;
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!string.IsNullOrEmpty(prefabPath))
        {
            Debug.Log("Created tanker ship explosion prefab at: " + prefabPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
        }
    }

    private static Material CreateMaterial(string path, string name, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null)
            return mat;

        Shader shader =
            Shader.Find("Particles/Standard Unlit") ??
            Shader.Find("Mobile/Particles/Alpha Blended") ??
            Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        mat = new Material(shader);
        mat.name = name;

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static void CreateFireball(Transform parent, Material mat)
    {
        GameObject go = new GameObject("Fireball");
        go.transform.SetParent(parent, false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        var main = ps.main;
        main.duration = 1.4f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.75f, 0.1f, 0.95f),
            new Color(1f, 0.12f, 0.02f, 0.7f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 120;
        main.gravityModifier = -0.05f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 55),
            new ParticleSystem.Burst(0.12f, 35)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.45f;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.25f, 1.4f),
                new Keyframe(1f, 2.6f)
            )
        );

        var color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.95f, 0.35f), 0f),
                new GradientColorKey(new Color(1f, 0.35f, 0.05f), 0.35f),
                new GradientColorKey(new Color(0.18f, 0.05f, 0.02f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.95f, 0.08f),
                new GradientAlphaKey(0.55f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = g;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.55f;
        noise.frequency = 1.2f;
        noise.scrollSpeed = 0.4f;
    }

    private static void CreateSmokeColumn(Transform parent, Material mat)
    {
        GameObject go = new GameObject("BlackSmokeColumn");
        go.transform.SetParent(parent, false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        var main = ps.main;
        main.duration = 6f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.0f, 2.8f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.03f, 0.03f, 0.03f, 0.8f),
            new Color(0.22f, 0.22f, 0.22f, 0.35f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 220;
        main.gravityModifier = -0.12f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 18f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 45),
            new ParticleSystem.Burst(0.6f, 25)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.8f;
        shape.length = 1.8f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = new ParticleSystem.MinMaxCurve(2.2f, 5.5f);
        velocity.x = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.35f),
                new Keyframe(0.35f, 1.1f),
                new Keyframe(1f, 3.8f)
            )
        );

        var color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.02f, 0.02f, 0.02f), 0f),
                new GradientColorKey(new Color(0.12f, 0.12f, 0.12f), 0.5f),
                new GradientColorKey(new Color(0.45f, 0.45f, 0.45f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.75f, 0.12f),
                new GradientAlphaKey(0.45f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = g;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.75f;
        noise.frequency = 0.65f;
        noise.scrollSpeed = 0.25f;
        noise.octaveCount = 2;
    }

    private static void CreateDebrisSparks(Transform parent, Material mat)
    {
        GameObject go = new GameObject("DebrisSparks");
        go.transform.SetParent(parent, false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2.5f;
        renderer.velocityScale = 0.65f;

        var main = ps.main;
        main.duration = 1.2f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 1.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 20f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.8f, 0.2f, 1f),
            new Color(1f, 0.2f, 0.02f, 0.8f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 90;
        main.gravityModifier = 1.2f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 70) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        var color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.15f, 0.02f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.65f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = g;
    }

    private static void CreateShockwave(Transform parent, Material mat)
    {
        GameObject go = new GameObject("ShockwaveFlash");
        go.transform.SetParent(parent, false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.45f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startColor = new Color(1f, 0.55f, 0.08f, 0.55f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 4;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 2) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.1f),
                new Keyframe(1f, 5.5f)
            )
        );

        var color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.55f, 0.08f), 0f) },
            new[]
            {
                new GradientAlphaKey(0.65f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = g;
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
