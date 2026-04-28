using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty fireAction;

    [Header("References")]
    [SerializeField] private GameObject torpedoPrefab;
    [SerializeField] private Transform viewer; // XR Main Camera

    [Header("Spawn Settings")]
    [SerializeField] private float launchDistanceInFront = 0.75f;
    [SerializeField] private float spawnYOffset = -0.25f;

    [Header("Rotation Settings")]
    [SerializeField] private Vector3 launchRotationOffsetEuler = new Vector3(0f, 45f, 0f);

    [Header("Torpedo Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxDistance = 50f;

    private bool wasPressed;

    private void Update()
    {
        float value = fireAction.action != null
            ? fireAction.action.ReadValue<float>()
            : 0f;

        bool isPressed = value > 0f;

        if (isPressed && !wasPressed)
            FireTorpedo();

        wasPressed = isPressed;
    }

    private void FireTorpedo()
    {
        if (torpedoPrefab == null || viewer == null)
        {
            Debug.LogWarning("TorpedoLauncher is missing torpedoPrefab or viewer.");
            return;
        }

        Vector3 flatForward = viewer.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = transform.forward;

        flatForward.Normalize();

        Vector3 spawnPosition =
            viewer.position +
            flatForward * launchDistanceInFront +
            Vector3.up * spawnYOffset;

        Quaternion baseRotation = Quaternion.LookRotation(flatForward, Vector3.up);

        Quaternion spawnRotation =
            baseRotation * Quaternion.Euler(launchRotationOffsetEuler);

        GameObject torpedo = Instantiate(torpedoPrefab, spawnPosition, spawnRotation);

        TorpedoMover mover = torpedo.GetComponent<TorpedoMover>();
        if (mover == null)
            mover = torpedo.AddComponent<TorpedoMover>();

        torpedo.SetActive(true);

        mover.Initialize(flatForward, speed, maxDistance);
    }
}