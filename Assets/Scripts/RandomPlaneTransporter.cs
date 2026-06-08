using UnityEngine;

public class RandomPlaneTransporter : MonoBehaviour
{
    [Header("Plane Corners")]
    [SerializeField] private Transform cornerA;
    [SerializeField] private Transform cornerB;
    [SerializeField] private Transform cornerC;
    [SerializeField] private Transform cornerD;

    [Header("Player")]
    [SerializeField] private Transform player;

    public void randomTransport()
    {
        if (cornerA == null || cornerB == null || cornerC == null || cornerD == null || player == null)
        {
            Debug.LogWarning("RandomPlaneTransporter is missing one or more required Transform references.");
            return;
        }

        float u = Random.value;
        float v = Random.value;

        Vector3 bottomEdge = Vector3.Lerp(cornerA.position, cornerB.position, u);
        Vector3 topEdge = Vector3.Lerp(cornerD.position, cornerC.position, u);

        Vector3 randomPosition = Vector3.Lerp(bottomEdge, topEdge, v);

        // Keep player's current Y height.
        randomPosition.y = player.position.y;

        player.position = randomPosition;
    }
}
