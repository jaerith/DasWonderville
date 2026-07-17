using UnityEngine;

public class TorpedoMover : MonoBehaviour
{
    [Header("Drift")]
    [Tooltip("Small sideways drift, similar to ShipMover's lateral drift.")]
    [SerializeField] private float lateralDriftSpeed = 0.25f;

    [Tooltip("-1 = left, +1 = right, 0 = straight ahead.")]
    [Range(-1f, 1f)]
    [SerializeField] private float driftDirection = 0f;

    private bool affectedByDecoy = false;
    private Vector3 direction;
    private Vector3 startPosition;
    private float speed;
    private float maxDistance;

    public bool AffectedByDecoy
    {
        get => affectedByDecoy;
        set => affectedByDecoy = value;
    }

    public float DriftDirection
    {
        get => driftDirection;
        set => driftDirection = value;
    }

    public float LateralDriftSpeed
    {
        get => lateralDriftSpeed;
        set => lateralDriftSpeed = value;
    }

    public Vector3 Direction => direction;

    public void Initialize(Vector3 moveDirection, float moveSpeed, float destroyDistance, float driftDirectionValue = 0f)
    {
        direction = moveDirection;
        direction.y = 0f;
        direction.Normalize();

        speed = moveSpeed;
        maxDistance = destroyDistance;
        startPosition = transform.position;
        driftDirection = driftDirectionValue;    
    }

    private void Awake()
    {
        AffectedByDecoy = false;
    }

    private void Update()
    {
        Vector3 lateralDir = Vector3.Cross(Vector3.up, direction);

        Vector3 movement =
            direction * speed +
            lateralDir * driftDirection * lateralDriftSpeed;

        transform.position += movement * Time.deltaTime;

        Vector3 flatOffset = transform.position - startPosition;
        flatOffset.y = 0f;

        if (flatOffset.magnitude >= maxDistance)
            Destroy(gameObject);
    }
}