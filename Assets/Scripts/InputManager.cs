using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty fireAction;
    [SerializeField] private InputActionProperty moveAction;
    [SerializeField] private InputActionProperty escapeAction;

    [Header("References")]
    [SerializeField] private GameObject torpedoPrefab;
    [SerializeField] private GameObject decoyPrefab;
    [SerializeField] private Transform viewer;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private StatsHudDisplay statsHudDisplay;

    [Header("Sounds")]
    [SerializeField] private AudioClip escapeClip;
    [SerializeField] private AudioClip gameOverClip;

    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private RandomPlaneTransporter transporter;

    [Header("Torpedo Limit")]
    [SerializeField] private int maxTorpedoesPerWindow = 2;
    [SerializeField] private float fireWindowSeconds = 15f;

    [Header("Escape Limit")]
    [SerializeField] private float escapeWindowSeconds = 10f;

    [Header("Spawn Settings")]
    [SerializeField] private float launchDistanceInFront = 0.75f;
    [SerializeField] private float spawnYOffset = -0.25f;

    [Header("Rotation Settings")]
    [SerializeField] private Vector3 launchRotationOffsetEuler = new Vector3(0f, 45f, 0f);

    [Header("Torpedo Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxDistance = 50f;

    private bool gameOverMode;
    private bool wasPressed;
    private bool wasEscapePressed;
    private int shotsInWindow;
    private float fireWindowStartTime = float.MinValue;
    private float lastEscapeTime = float.MinValue;

    public float TorpedoReloadSecondsRemaining
    {
        get
        {
            float now = Time.time;

            if (now - fireWindowStartTime >= fireWindowSeconds)
                return 0f;

            if (shotsInWindow < maxTorpedoesPerWindow)
                return 0f;

            return Mathf.Max(0f, fireWindowSeconds - (now - fireWindowStartTime));
        }
    }

    public float EscapeReloadSecondsRemaining =>
        Mathf.Max(0f, escapeWindowSeconds - (Time.time - lastEscapeTime));

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

    private void Awake()
    {
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
        if (transporter == null)
            return false;

        if (Time.time - lastEscapeTime < escapeWindowSeconds)
            return false;

        lastEscapeTime = Time.time;
        return true;
    }

    private bool CanFireTorpedo()
    {
        float now = Time.time;

        if (now - fireWindowStartTime >= fireWindowSeconds)
        {
            fireWindowStartTime = now;
            shotsInWindow = 0;
        }

        if (shotsInWindow >= maxTorpedoesPerWindow)
            return false;

        shotsInWindow++;
        return true;
    }

    public void PlayEscapeSound()
    {
        if (escapeClip == null)
            return;

        GameObject audioObj = new GameObject("DiveAlarmSound");
        audioObj.transform.position = this.gameObject.transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = escapeClip;
        source.volume = 0.7f;
        source.pitch = 0.6f;

        // Important: make it 2D while testing.
        source.spatialBlend = 0f;

        source.playOnAwake = false;
        source.loop = false;

        source.Play();

        Destroy(audioObj, escapeClip.length + 1.0f);
    }

    public void PlayGameOverSound()
    {
        if (gameOverClip == null)
            return;

        GameObject audioObj = new GameObject("ExplosionSound");
        audioObj.transform.position = this.gameObject.transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = gameOverClip;
        source.volume = 0.7f;
        source.pitch = 0.6f;

        // Important: make it 2D while testing.
        source.spatialBlend = 0f;

        source.playOnAwake = false;
        source.loop = true;

        source.Play();
    }

    public void RestartGame()
    {
        gameOverMode = true;

        Debug.Log("Game Over! Restarting the game...");

        PlayGameOverSound();

        if (statsHudDisplay != null)
        {
            statsHudDisplay.DisplayStatusGameOver();
        }

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Escape()
    {
        // NOTE: Future escape behavior will be implemented here.
        PlayEscapeSound();
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

    public void FireSonarDecoy()
    {
        if (decoyPrefab == null || viewer == null)
        {
            Debug.LogWarning("DecoyLauncher is missing decoyPrefab or viewer.");
            return;
        }

        if (!CanFireTorpedo())
        {
            Debug.Log("Torpedo reload window active.");
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

        GameObject decoy = Instantiate(decoyPrefab, spawnPosition, spawnRotation);

        TorpedoMover mover = decoy.GetComponent<TorpedoMover>();
        if (mover == null)
            mover = decoy.AddComponent<TorpedoMover>();

        decoy.SetActive(true);
        mover.Initialize(flatForward, speed, maxDistance);
    }

    public void SetStateDefault()
    {
        if (statsHudDisplay != null)
        {
            statsHudDisplay.DisplayStatusDefault();
        }
    }

    public void SetStateEnemyAlerted()
    {
        if (statsHudDisplay != null)
        {
            statsHudDisplay.DisplayStatusUnderAttack();
        }
    }

    public void SetSystemStatus()
    {
        if (gameOverMode)
        {
            if (statsHudDisplay != null)
            {
                statsHudDisplay.DisplayStatusGameOver();
            }

            return;
        }

        bool isAlerted = false;

        foreach (EnemyHunterBehavior destroyer in FindObjectsByType<EnemyHunterBehavior>(FindObjectsInactive.Include))
        {
            isAlerted |= destroyer.isAlerted;
        }

        if (isAlerted)
        {
            SetStateEnemyAlerted();
        }
        else
        {
            SetStateDefault();
        }
    }

}