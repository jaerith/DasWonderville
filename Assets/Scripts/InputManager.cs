using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty fireAction;
    [SerializeField] private InputActionProperty fireDecoyAction;
    [SerializeField] private InputActionProperty moveAction;
    [SerializeField] private InputActionProperty escapeAction;
    [SerializeField] private InputActionProperty scopeAction;

    [Header("Events")]
    [SerializeField] private UnityEvent scopeEvent;

    [Header("References")]
    [SerializeField] private GameObject torpedoPrefab;
    [SerializeField] private GameObject decoyPrefab;
    [SerializeField] private Transform viewer;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private StatsHudDisplay statsHudDisplay;

    [Header("Win Condition")]
    [SerializeField] private AudioClip winClip;
    [SerializeField] private GameObject winIndicator;
    [SerializeField] private float gameCompletionCheckInterval = 2f;
    [SerializeField] private float winIndicatorDistance = 10f;

    [Header("Sounds")]
    [SerializeField] private AudioClip escapeClip;
    [SerializeField] private AudioClip gameOverClip;

    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private RandomPlaneTransporter transporter;

    [Header("Snap Turn")]
    [SerializeField] private float snapTurnDegrees = 45f;

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
    private bool gameCompleteMode;
    private bool wasFirePressed;
    private bool wasFireDecoyPressed;
    private bool wasEscapePressed;
    private bool wasScopePressed;
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

    public Transform Viewer => viewer;
    public float TorpedoSpeed => speed;
    public float TorpedoMaxDistance => maxDistance;

    public Vector3 GetTorpedoLaunchDirection()
    {
        if (viewer == null)
            return transform.forward;

        Vector3 flatForward = viewer.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = transform.forward;

        flatForward.Normalize();
        return flatForward;
    }

    public Vector3 GetTorpedoSpawnPosition(Vector3 launchDirection)
    {
        return viewer.position +
            launchDirection * launchDistanceInFront +
            Vector3.up * spawnYOffset;
    }

    private void OnEnable()
    {
        fireAction.action?.Enable();
        fireDecoyAction.action?.Enable();
        moveAction.action?.Enable();
        escapeAction.action?.Enable();
        scopeAction.action?.Enable();

        InvokeRepeating(nameof(CheckForGameCompletion), gameCompletionCheckInterval, gameCompletionCheckInterval);
    }

    private void OnDisable()
    {
        fireAction.action?.Disable();
        fireDecoyAction.action?.Disable();
        moveAction.action?.Disable();
        escapeAction.action?.Disable();
        scopeAction.action?.Disable();

        CancelInvoke(nameof(CheckForGameCompletion));
    }

    private void Awake()
    {
    }

    private void Update()
    {
        HandleMovement();
        HandleFireInput();
        HandleFireDecoyInput();
        HandleEscape();
        HandleScope();
    }

    public void ForceWin()
    {
        Camera mainCamera = Camera.main;

        foreach (ShipHitReaction hitReaction in FindObjectsByType<ShipHitReaction>(FindObjectsInactive.Include))
        {
            if (hitReaction.GetComponent<EnemyHunterBehavior>() != null)
                continue;

            if (mainCamera != null && hitReaction.transform.IsChildOf(mainCamera.transform))
                continue;

            if (hitReaction.IsSinking)
                continue;

            hitReaction.DestroyShip();
        }
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

    public void SnapTurnRight()
    {
        SnapTurn(snapTurnDegrees);
    }

    public void SnapTurnLeft()
    {
        SnapTurn(-snapTurnDegrees);
    }

    private void SnapTurn(float degrees)
    {
        if (playerRoot == null)
            return;

        playerRoot.Rotate(Vector3.up, degrees, Space.World);
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

        if (isPressed && !wasFirePressed)
        {
            if (CanFireTorpedo())
                FireTorpedo();
            else
                Debug.Log("Torpedo reload window active.");
        }

        wasFirePressed = isPressed;
    }

    private void HandleFireDecoyInput()
    {
        float value = fireDecoyAction.action != null
            ? fireDecoyAction.action.ReadValue<float>()
            : 0f;

        bool isPressed = value > 0f;

        if (isPressed && !wasFireDecoyPressed)
        {
            if (CanFireTorpedo())
                FireSonarDecoy();
            else
                Debug.Log("Torpedo reload window active.");
        }

        wasFireDecoyPressed = isPressed;
    }

    private void HandleScope()
    {       
        float value = scopeAction.action != null
            ? scopeAction.action.ReadValue<float>()
            : 0f;

        bool isPressed = value > 0f;

        if (isPressed && !wasScopePressed)
        {
            Debug.Log("Scope window triggered.");
            scopeEvent?.Invoke();
        }

        wasScopePressed = isPressed;
    }

    private bool CanEscape()
    {
        if (transporter == null)
            return false;

        if (transporter.IsTransporting)
            return false;

        if (Time.time - lastEscapeTime < escapeWindowSeconds)
            return false;

        lastEscapeTime = Time.time;
        return true;
    }

    private bool CanFireTorpedo()
    {
        if (transporter != null && transporter.IsTransporting)
            return false;

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

    private IEnumerator PlayGameWinSound()
    {
        yield return new WaitForSeconds(2.0f);

        if (winClip != null)
        {
            Debug.Log("Playing win sound...");

            GameObject audioObj = new GameObject("WinSound");
            audioObj.transform.position = this.gameObject.transform.position;

            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = winClip;
            source.volume = 0.7f;
            source.pitch = 0.6f;

            // Important: make it 2D while testing.
            source.spatialBlend = 0f;
            source.loop = false;

            source.Play();
        }
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
        float restartDelay = gameCompleteMode ? 9f : 5f;

        yield return new WaitForSeconds(restartDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckForGameCompletion()
    {
        if (gameOverMode || gameCompleteMode)
            return;

        GameObject[] shipObjects = GameObject.FindGameObjectsWithTag("Ship");

        Camera mainCamera = Camera.main;

        bool allConvoyShipsDestroyed = false;

        foreach (GameObject shipObject in shipObjects)
        {
            if (shipObject.GetComponent<EnemyHunterBehavior>() != null)
                continue;

            if (mainCamera != null && shipObject.transform.IsChildOf(mainCamera.transform))
                continue;

            ShipHitReaction hitReaction = shipObject.GetComponent<ShipHitReaction>();

            if (hitReaction == null || !hitReaction.IsSinking)
                return;

            allConvoyShipsDestroyed = true;
        }

        if (allConvoyShipsDestroyed)
        {
            GameCompleted();
        }
    }

    public void GameCompleted()
    {
        gameCompleteMode = true;

        Debug.Log("Game Completed! You win.");

        CancelInvoke(nameof(CheckForGameCompletion));

        if (statsHudDisplay != null)
        {
            statsHudDisplay.DisplayStatusGameWin();
        }

        if (winIndicator != null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * winIndicatorDistance;
                spawnPosition.y = -25f;

                GameObject winIndicatorInstance = Instantiate(winIndicator, spawnPosition, Quaternion.identity);
                winIndicatorInstance.SetActive(true);
            }

            StartCoroutine(PlayGameWinSound());
        }

        StartCoroutine(RestartAfterDelay());
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

        Vector3 flatForward = GetTorpedoLaunchDirection();
        Vector3 spawnPosition = GetTorpedoSpawnPosition(flatForward);

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