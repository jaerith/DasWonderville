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

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded)
            return;

        if (!other.CompareTag(targetTag))
            return;

        hasExploded = true;

        Vector3 hitPoint = other.ClosestPoint(transform.position);

        TriggerImpact(hitPoint);
    }
    */

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded)
            return;

        if (!other.CompareTag(targetTag))
            return;

        hasExploded = true;

        Vector3 hitPoint = other.ClosestPoint(transform.position);

        ShipHitReaction ship = other.GetComponentInParent<ShipHitReaction>();
        if (ship != null)
        {
            ship.RegisterHit(hitPoint);
        }

        Destroy(gameObject);
    }

    /*
    private void PlayExplosionSound(Vector3 position)
    {
        GameObject audioObj = new GameObject("ExplosionSound");
        audioObj.transform.position = position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = explosionSound;
        source.volume = volume;

        // Important: make it 2D while testing.
        source.spatialBlend = 0f;

        source.playOnAwake = false;
        source.loop = false;

        source.Play();

        Destroy(audioObj, explosionSound.length + 0.25f);
    }

    private void TriggerImpact(Vector3 position)
    {
        // 💥 Spawn explosion
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

        // 🔊 Play sound
        if (explosionSound != null)
        {
            // AudioSource.PlayClipAtPoint(explosionSound, position, volume);
            // AudioSource.PlayClipAtPoint(explosionSound, cameraPosition, volume);
            PlayExplosionSound(cameraPosition);

            Debug.Log("Played explosion sound at camera position: " + cameraPosition);
        }

        Destroy(gameObject); // destroy torpedo
    }
    */
}