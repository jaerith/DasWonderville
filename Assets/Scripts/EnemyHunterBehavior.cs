using UnityEngine;

public class EnemyHunterBehavior : MonoBehaviour
{
    [Header("Normal Movement")]
    [SerializeField] private bool useLocalBow = true;
    [SerializeField] private Vector3 worldBowDirection = new Vector3(0f, 0f, 1f);
    [SerializeField] private float forwardSpeed = 2f;
    [SerializeField] private float lateralDriftSpeed = 0.25f;

    [Tooltip("-1 = port, +1 = starboard")]
    [Range(-1f, 1f)]
    [SerializeField] private float driftDirection = 0.2f;

    [Header("Alert Behavior")]
    [SerializeField] private Transform player;
    [SerializeField] private float pursuitSpeed = 3f;
    [SerializeField] private bool rotateTowardPlayer = true;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Torpedo Firing")]
    [SerializeField] private GameObject torpedoPrefab;
    [SerializeField] private Transform torpedoSpawnPoint;
    [SerializeField] private float fireIntervalSeconds = 30f;
    [SerializeField] private float torpedoSpeed = 12f;
    [SerializeField] private float torpedoMaxDistance = 100f;

    private bool isAlerted;
    private float nextFireTime;

    private void Update()
    {
        if (isAlerted)
        {
            MoveTowardPlayer();
            TryFireTorpedo();
        }
        else
        {
            MoveNormally();
        }
    }

    private void MoveNormally()
    {
        Vector3 bowDir = useLocalBow
            ? transform.forward
            : worldBowDirection.normalized;

        Vector3 lateralDir = useLocalBow
            ? transform.right
            : Vector3.right;

        Vector3 movement =
            bowDir * forwardSpeed +
            lateralDir * driftDirection * lateralDriftSpeed;

        transform.position += movement * Time.deltaTime;
    }

    private void MoveTowardPlayer()
    {
        if (player == null)
            return;

        Vector3 shipPosition = transform.position;
        Vector3 playerPosition = player.position;

        // Ignore height differences
        shipPosition.y = 0f;
        playerPosition.y = 0f;

        Vector3 toPlayer = playerPosition - shipPosition;

        if (toPlayer.sqrMagnitude < 0.001f)
            return;

        Vector3 direction = toPlayer.normalized;

        // Move in world space toward the player
        transform.position += direction * pursuitSpeed * Time.deltaTime;

        if (rotateTowardPlayer)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void TryFireTorpedo()
    {
        if (Time.time < nextFireTime)
            return;

        FireTorpedoAtPlayer();
        nextFireTime = Time.time + fireIntervalSeconds;
    }

    private void FireTorpedoAtPlayer()
    {
        if (torpedoPrefab == null || player == null)
            return;

        Vector3 spawnPosition = torpedoSpawnPoint != null
            ? torpedoSpawnPoint.position
            : transform.position + transform.forward * 1.5f;

        Vector3 direction = player.position - spawnPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        direction.Normalize();

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        GameObject torpedo = Instantiate(torpedoPrefab, spawnPosition, rotation);

        TorpedoMover mover = torpedo.GetComponent<TorpedoMover>();
        if (mover == null)
            mover = torpedo.AddComponent<TorpedoMover>();

        mover.Initialize(direction, torpedoSpeed, torpedoMaxDistance);
    }

    public void AlertDestroyer()
    {
        if (isAlerted)
            return;

        isAlerted = true;
        nextFireTime = Time.time + fireIntervalSeconds;
    }

    public void OnAnyShipHit(Transform hitShip)
    {
        AlertDestroyer();
    }
}