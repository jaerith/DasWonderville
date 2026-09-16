#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CreateLeviathanPrefab
{
    private const string SourceModelPath = "Assets/Prefabs/monster/Leviathan.gltf";
    private const string OutputFolder = "Assets/Prefabs/monster";
    private const string PrefabPath = OutputFolder + "/PF_Leviathan.prefab";
    private const string ControllerPath = OutputFolder + "/AC_Leviathan.controller";

    [MenuItem("Tools/Wonderville/Create Leviathan Prefab")]
    public static void Create()
    {
        GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(SourceModelPath);
        if (sourceModel == null)
        {
            Debug.LogError($"Could not load Leviathan model at: {SourceModelPath}");
            return;
        }

        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(SourceModelPath)
            .OfType<AnimationClip>()
            .ToArray();

        if (clips.Length == 0)
        {
            Debug.LogError($"No animation clips found on Leviathan model at: {SourceModelPath}");
            return;
        }

        // The Leviathan's rig carries a couple of imported clips (a full-skeleton
        // swim/idle "Take 001|BaseLayer" take plus a minor single-node action).
        // Prefer the full-body take as the one that should start automatically.
        AnimationClip startClip =
            clips.FirstOrDefault(c => c.name.IndexOf("BaseLayer", StringComparison.OrdinalIgnoreCase) >= 0) ??
            clips.FirstOrDefault(c => c.name.IndexOf("Take", StringComparison.OrdinalIgnoreCase) >= 0) ??
            clips[0];

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourceModel);
        instance.name = "PF_Leviathan";

        AnimatorController controller = CreateController(startClip);

        Animator animator = instance.GetComponent<Animator>();
        if (animator == null)
            animator = instance.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        UnityEngine.Object.DestroyImmediate(instance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created Leviathan prefab at {PrefabPath} (start animation: {startClip.name})");
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(PrefabPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AnimatorController CreateController(AnimationClip startClip)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState state = stateMachine.AddState(startClip.name);
        state.motion = startClip;
        stateMachine.defaultState = state;

        return controller;
    }
}
#endif
