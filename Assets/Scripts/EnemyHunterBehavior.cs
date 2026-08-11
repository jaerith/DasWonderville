using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class EnemyHunterBehavior : MonoBehaviour
{
    private const float PlayerFacingOffsetDegrees = 10f;

    [Header("Normal Movement")]
    [SerializeField] private bool useLocalBow = true;
    [SerializeField] private Vector3 worldBowDirection = new Vector3(0f, 0f, 1f);
    [SerializeField] private float forwardSpeed = 2f;
    [SerializeField] private float lateralDriftSpeed = 0.25f;

    [Tooltip("-1 = port, +1 = starboard")]
    [Range(-1f, 1f)]
    [SerializeField] private float driftDirection = 0.2f;

    [Header("Alert Behavior")]
    [SerializeField] private GameObject depthChargeIndicator;
    [SerializeField] private AlternatingLightPulser lightPulser;
    [SerializeField] private Transform player;
    [SerializeField] private float pursuitSpeed = 3f;
    [SerializeField] private float stopDistanceFromPlayer = 1.5f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private UnityEvent onAttackDetection;

    [Tooltip("Use this if the destroyer's bow is not aligned with local +Z.")]
    [SerializeField] private float yawOffsetDegrees = 0f;

    [Header("Torpedo Firing")]
    [SerializeField] private GameObject torpedoPrefab;
    [SerializeField] private Transform torpedoSpawnPoint;
    [SerializeField] private float fireIntervalSeconds = 30f;
    [SerializeField] private float torpedoSpeed = 12f;
    [SerializeField] private float torpedoMaxDistance = 100f;

    [Header("Torpedo Rotation Settings")]
    [SerializeField] private Vector3 launchRotationOffsetEuler = new Vector3(0f, 90f, 0f);

    [Header("Random Placement")]
    [SerializeField] private bool randomizePositionOnStart = false;
    [SerializeField] private Transform cornerA;
    [SerializeField] private Transform cornerB;
    [SerializeField] private Transform cornerC;
    [SerializeField] private Transform cornerD;
    [SerializeField] private RandomPlaneTransporter playerRandomPlaneTransporter;

    [Header("Depth Charge Detection")]
    [SerializeField] private float depthChargeCheckIntervalSeconds = 1f;

    private Rigidbody rb;
    private Collider[] ownColliders;
    private float startingY;

    public bool isAlerted { get; protected set; } = false;
    public bool isSinking { get; protected set; } = false;
    private float nextFireTime;

    public float PursuitSpeed
    {
        get => pursuitSpeed;
        set => pursuitSpeed = value;
    }

    public float RotationSpeed
    {
        get => rotationSpeed;
        set => rotationSpeed = value;
    }

    public float FireIntervalSeconds
    {
        get => fireIntervalSeconds;
        set => fireIntervalSeconds = value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        ownColliders = GetComponentsInChildren<Collider>(true);

        if (randomizePositionOnStart)
            RelocateWithinPlaneCorners();

        startingY = rb.position.y;
    }

    private void Start()
    {
        StartCoroutine(CheckPlayerDistanceForDepthCharges());
    }

    private IEnumerator CheckPlayerDistanceForDepthCharges()
    {
        WaitForSeconds wait = new WaitForSeconds(depthChargeCheckIntervalSeconds);

        while (true)
        {
            if (depthChargeIndicator != null && player != null && playerRandomPlaneTransporter != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                bool withinRange = distanceToPlayer <= playerRandomPlaneTransporter.HunterDetectionDistanceForDepthCharges;

                if (depthChargeIndicator.activeSelf != withinRange)
                    depthChargeIndicator.SetActive(withinRange);
            }

            yield return wait;
        }
    }

    private void RelocateWithinPlaneCorners()
    {
        if (cornerA == null || cornerB == null || cornerC == null || cornerD == null)
            return;

        Debug.Log("EnemyHunterBehavior: relocating (" + this.gameObject.name + ") within plane corners.");

        Debug.Log("EnemyHunterBehavior: ship (" + this.gameObject.name+ ") => current position = " + transform.position);

        float u = Random.value;
        float v = Random.value;

        Vector3 bottomEdge = Vector3.Lerp(cornerA.position, cornerB.position, u);
        Vector3 topEdge = Vector3.Lerp(cornerD.position, cornerC.position, u);

        Vector3 randomPosition = Vector3.Lerp(bottomEdge, topEdge, v);

        randomPosition.y = transform.position.y;
        transform.position = randomPosition;

        // rb.isKinematic Rigidbodies cache their own position separately from the
        // Transform. Without this, rb.position still reports the pre-relocation
        // spot until physics syncs on its own schedule, so the very first
        // MoveNormally() call in FixedUpdate reads the stale rb.position and
        // moves from there — visually snapping the ship back to where it started.
        rb.position = randomPosition;

        if (player != null)
        {
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                Quaternion facePlayer = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                Quaternion facing = facePlayer * Quaternion.Euler(0f, PlayerFacingOffsetDegrees, 0f);

                transform.rotation = facing;
                rb.rotation = facing;
            }
        }

        Debug.Log("EnemyHunterBehavior: ship (" + this.gameObject.name + ") => after relocation = " + transform.position);
    }

    private void Update()
    {
        // Firing is time-based, not physics-based, so it stays in Update.
        if (isAlerted)
            TryFireTorpedo();
    }

    private void FixedUpdate()
    {
        MoveNormally();
        ClampToStartingHeight();
    }

    // Catches any upward drift (e.g. from collisions) so the ship can sink but never fly.
    private void ClampToStartingHeight()
    {
        if (rb.position.y > startingY)
        {
            Vector3 clampedPosition = rb.position;
            clampedPosition.y = startingY;
            rb.position = clampedPosition;
        }
    }

    private void MoveNormally()
    {
        Quaternion newRot = rb.rotation;

        if (!isSinking && isAlerted && player != null)
        {
            if (lightPulser != null && !lightPulser.IsPulsing)
            {
                Debug.Log("EnemyHunterBehavior: starting light pulser on alert.");
                lightPulser.Play();
            }

            Vector3 toPlayer = player.position - rb.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer, Vector3.up);
                newRot = Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(newRot);
            }

            if (toPlayer.magnitude <= stopDistanceFromPlayer)
                return;

            Vector3 alertForward = newRot * Vector3.forward;
            alertForward.y = 0f;
            alertForward.Normalize();
            rb.MovePosition(rb.position + alertForward * pursuitSpeed * Time.fixedDeltaTime);
            return;
        }
        else if (lightPulser != null && lightPulser.IsPulsing)
        {
            lightPulser.Stop();
        }

        Vector3 bowDir = newRot * Vector3.forward;
        bowDir.y = 0f;
        bowDir.Normalize();

        Vector3 lateralDir = newRot * Vector3.right;
        lateralDir.y = 0f;
        lateralDir.Normalize();

        rb.MovePosition(rb.position + (bowDir * forwardSpeed + lateralDir * driftDirection * lateralDriftSpeed) * Time.fixedDeltaTime);
    }

    private void TryFireTorpedo()
    {
        if (Time.time < nextFireTime)
            return;

        if (!IsBowAimedAtPlayer())
            return;

        FireTorpedoAtPlayer();
        nextFireTime = Time.time + fireIntervalSeconds;
    }

    private bool IsBowAimedAtPlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - rb.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return false;

        Vector3 bowForward = rb.rotation * Vector3.forward;
        bowForward.y = 0f;
        bowForward.Normalize();

        return Vector3.Angle(bowForward, toPlayer.normalized) <= 25f;
    }

    private void FireTorpedoAtPlayer()
    {
        if (torpedoPrefab == null || player == null)
        {
            Debug.LogWarning($"{name}: cannot fire — torpedoPrefab={(torpedoPrefab == null ? "NULL" : "ok")}, player={(player == null ? "NULL" : "ok")}");
            return;
        }

        Vector3 spawnPosition = torpedoSpawnPoint != null
            ? torpedoSpawnPoint.position
            : transform.position + transform.forward * 1.5f;

        Vector3 direction = player.position - spawnPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        direction.Normalize();

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        Quaternion spawnRotation = rotation * Quaternion.Euler(launchRotationOffsetEuler);

        GameObject torpedo = Instantiate(
            torpedoPrefab,
            spawnPosition,
            spawnRotation
        );

        torpedo.SetActive(true);

        TorpedoMover mover = torpedo.GetComponent<TorpedoMover>();

        if (mover == null)
            mover = torpedo.AddComponent<TorpedoMover>();

        mover.Initialize(direction, torpedoSpeed, torpedoMaxDistance);

        IgnoreCollisionsWithSelf(torpedo);
    }

    private void IgnoreCollisionsWithSelf(GameObject torpedo)
    {
        if (ownColliders == null || ownColliders.Length == 0)
            return;

        // A torpedo may carry more than one collider (e.g. a hull collider plus a
        // trigger), so handle every collider on the torpedo against every collider
        // on this ship.
        Collider[] torpedoColliders = torpedo.GetComponentsInChildren<Collider>(true);

        foreach (Collider torpedoCollider in torpedoColliders)
        {
            if (torpedoCollider == null)
                continue;

            foreach (Collider shipCollider in ownColliders)
            {
                if (shipCollider == null)
                    continue;

                Physics.IgnoreCollision(torpedoCollider, shipCollider, true);
            }
        }
    }

    public void AlertDestroyer()
    {
        if (isAlerted)
            return;

        isAlerted = true;

        nextFireTime = Time.time + 10f;

        if (name != null && rb != null)
        {
            Debug.Log($"{name} alerted at {rb.position}");
        }

        onAttackDetection?.Invoke();
    }

    public void DisableAlert()
    {
        isAlerted = false;
    }

    public void OnAnyShipHit(Transform hitShip)
    {
        AlertDestroyer();
    }

    public void IndicateShipIsSinking()
    {
        isSinking = true;
    }

}