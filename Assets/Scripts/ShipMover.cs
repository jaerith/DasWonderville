using UnityEngine;

public class ShipMover : MonoBehaviour
{
    [Header("Bow Direction")]
    [Tooltip("Use the ship's local forward direction (bow).")]
    [SerializeField] private bool useLocalBow = true;

    [Tooltip("If not using local bow, define a world-space forward direction.")]
    [SerializeField] private Vector3 worldBowDirection = new Vector3(0f, 0f, 1f);


    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 2.0f;

    [Tooltip("Small sideways drift (port/starboard).")]
    [SerializeField] private float lateralDriftSpeed = 0.25f;


    [Header("Drift Direction")]
    [Tooltip("-1 = port (left), +1 = starboard (right)")]
    [Range(-1f, 1f)]
    [SerializeField] private float driftDirection = 0.2f;


    [Header("Optional Natural Motion")]
    [SerializeField] private bool addDriftNoise = true;
    [SerializeField] private float driftNoiseStrength = 0.1f;
    [SerializeField] private float driftNoiseSpeed = 0.5f;

    private float noiseOffset;


    void Start()
    {
        noiseOffset = Random.Range(0f, 100f);
    }


    void Update()
    {
        MoveShip();
    }


    private void MoveShip()
    {
        // Determine bow (forward) direction
        Vector3 bowDir;

        if (useLocalBow)
        {
            bowDir = transform.forward;
        }
        else
        {
            bowDir = worldBowDirection.normalized;
        }

        // Determine lateral (port/starboard)
        Vector3 lateralDir = useLocalBow ? transform.right : Vector3.right;

        float drift = driftDirection;

        // Optional noise for realism
        if (addDriftNoise)
        {
            float noise = Mathf.PerlinNoise(Time.time * driftNoiseSpeed, noiseOffset) - 0.5f;
            drift += noise * driftNoiseStrength;
        }

        // Final movement
        Vector3 movement =
            bowDir * forwardSpeed +
            lateralDir * drift * lateralDriftSpeed;

        transform.position += movement * Time.deltaTime;

        // Gentle turning drift
        // transform.Rotate(0f, drift * 10f * Time.deltaTime, 0f);
        transform.Rotate(0f, drift * Time.deltaTime, 0f);

        // Gentle bobbing
        float bob = Mathf.Sin(Time.time * 1.5f) * 0.03f;
        transform.position += Vector3.up * bob * Time.deltaTime;
    }
}