using UnityEngine;

public class TorpedoImpact : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;

    [Header("Sound")]
    [SerializeField] private Vector3 cameraPosition;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float volume = 1f;

    [Header("Filtering")]
    [SerializeField] private string targetTag = "Ship";

    private bool hasExploded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded)
            return;

        if (!other.CompareTag(targetTag))
            return;

        if (Random.Range(0f, 1f) < 0.25f)
        {
            Debug.Log("Torpedo hit detected but ignored due to random chance.");
            Destroy(gameObject);
        }

        hasExploded = true;

        Vector3 hitPoint = other.ClosestPoint(transform.position);

        ShipHitReaction ship = other.GetComponentInParent<ShipHitReaction>();
        if (ship != null)
        {
            ship.RegisterHit(hitPoint);
        }

        Destroy(gameObject);
    }

}