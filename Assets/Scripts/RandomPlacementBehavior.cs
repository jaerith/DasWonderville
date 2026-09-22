using UnityEngine;

public class RandomPlacementBehavior : MonoBehaviour
{
    private const float PlayerFacingOffsetDegrees = 10f;

    [Header("Random Placement")]
    [SerializeField] private bool randomizePositionOnStart = true;
    [SerializeField] private float offsetYAfterPlacement = 0f;
    [SerializeField] private Transform cornerA;
    [SerializeField] private Transform cornerB;
    [SerializeField] private Transform cornerC;
    [SerializeField] private Transform cornerD;

    [Header("Facing")]
    [SerializeField] private Transform player;

    private float startingY;

    private void Awake()
    {
        // Captured once so repeated relocations don't keep stacking the Y offset.
        startingY = transform.position.y;

        if (randomizePositionOnStart)
            RelocateWithinPlaneCorners();
    }

    public void RelocateWithinPlaneCorners()
    {
        if (cornerA == null || cornerB == null || cornerC == null || cornerD == null)
            return;

        Debug.Log("RandomPlacementBehavior: relocating (" + this.gameObject.name + ") within plane corners.");

        Debug.Log("RandomPlacementBehavior: object (" + this.gameObject.name + ") => current position = " + transform.position);

        float u = Random.value;
        float v = Random.value;

        Vector3 bottomEdge = Vector3.Lerp(cornerA.position, cornerB.position, u);
        Vector3 topEdge = Vector3.Lerp(cornerD.position, cornerC.position, u);

        Vector3 randomPosition = Vector3.Lerp(bottomEdge, topEdge, v);

        randomPosition.y = startingY + offsetYAfterPlacement;
        transform.position = randomPosition;

        // A Rigidbody caches its own position separately from the Transform, so
        // keep it in sync if this object has one.
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
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

                if (rb != null)
                    rb.rotation = facing;
            }
        }

        Debug.Log("RandomPlacementBehavior: object (" + this.gameObject.name + ") => after relocation = " + transform.position);
    }
}
