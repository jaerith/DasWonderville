using UnityEngine;

public class TorpedoImpact : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioSource explosionAudio;

    [Header("Filtering")]
    [SerializeField] private string targetTag = "Ship";

    private bool hasExploded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded)
            return;

        if (!other.CompareTag(targetTag))
            return;

        Vector3 hitPoint = GetClosestPoint(other);

        TriggerExplosion(hitPoint);
    }

    private void TriggerExplosion(Vector3 position)
    {
        hasExploded = true;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

            var ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(explosion, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(explosion, 2f);
            }
        }

        if (explosionAudio != null)
        {
            explosionAudio.Play();
        }

        Destroy(gameObject); // destroy torpedo
    }

    private Vector3 GetClosestPoint(Collider other)
    {
        // More accurate than transform.position for triggers
        return other.ClosestPoint(transform.position);
    }
}