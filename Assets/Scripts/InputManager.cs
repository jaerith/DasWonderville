using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty fireAction;
    [SerializeField] private InputActionProperty moveAction;
    [SerializeField] private InputActionProperty escapeAction;

    [Header("References")]
    [SerializeField] private GameObject torpedoPrefab;
    [SerializeField] private Transform viewer;
    [SerializeField] private Transform playerRoot;

    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private RandomPlaneTransporter transporter;

    [Header("Torpedo Limit")]
    [SerializeField] private int maxTorpedoesPerWindow = 2;
    [SerializeField] private float fireWindowSeconds = 15f;

    [Header("Spawn Settings")]
    [SerializeField] private float launchDistanceInFront = 0.75f;
    [SerializeField] private float spawnYOffset = -0.25f;

    [Header("Rotation Settings")]
    [SerializeField] private Vector3 launchRotationOffsetEuler = new Vector3(0f, 45f, 0f);

    [Header("Torpedo Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxDistance = 50f;

    private bool wasPressed;
    private bool wasEscapePressed;
    private readonly Queue<float> fireTimes = new Queue<float>();

    private void OnEnable()
    {
        fireAction.action?.Enable();
        moveAction.action?.Enable();
        escapeAction.action?.Enable();
    }

    private void OnDisable()
    {
        fireAction.action?.Disable();
        moveAction.action?.Disable();
        escapeAction.action?.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleFireInput();
        HandleEscape();
    }

    private void HandleMovement()
    {
        if (moveAction.action == null || playerRoot == null || viewer == null)
            return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 forward = viewer.forward;
        Vector3 right = viewer.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = forward * input.y + right * input.x;
        playerRoot.position += movement * moveSpeed * Time.deltaTime;
    }

    private void HandleEscape()
    {
        float value = escapeAction.action != null
            ? escapeAction.action.ReadValue<float>()
            : 0f;

        bool isPressed = value > 0f;

        if (isPressed && !wasEscapePressed)
        {
            if (CanEscape())
                Escape();
            else
                Debug.Log("Escape reload window active.");
        }

        wasEscapePressed = isPressed;
    }

    private void HandleFireInput()
    {
        float value = fireAction.action != null
            ? fireAction.action.ReadValue<float>()
            : 0f;

        bool isPressed = value > 0f;

        if (isPressed && !wasPressed)
        {
            if (CanFireTorpedo())
                FireTorpedo();
            else
                Debug.Log("Torpedo reload window active.");
        }

        wasPressed = isPressed;
    }

    private bool CanEscape()
    {
        // NOTE: Future constraints will be established
        return (transporter != null);
    }

    private bool CanFireTorpedo()
    {
        float now = Time.time;

        while (fireTimes.Count > 0 && now - fireTimes.Peek() > fireWindowSeconds)
            fireTimes.Dequeue();

        if (fireTimes.Count >= maxTorpedoesPerWindow)
            return false;

        fireTimes.Enqueue(now);
        return true;
    }

    public void Escape()
    {
        // NOTE: Future escape behavior will be implemented here.
        transporter.RandomTransport();
        Debug.Log("Escape triggered!");
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
        Quaternion spawnRotation = baseRotation * Quaternion.Euler(launchRotationOffsetEuler);

        GameObject torpedo = Instantiate(torpedoPrefab, spawnPosition, spawnRotation);

        TorpedoMover mover = torpedo.GetComponent<TorpedoMover>();
        if (mover == null)
            mover = torpedo.AddComponent<TorpedoMover>();

        torpedo.SetActive(true);
        mover.Initialize(flatForward, speed, maxDistance);
    }
}