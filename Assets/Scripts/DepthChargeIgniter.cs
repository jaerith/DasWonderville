using UnityEngine;
using UnityEngine.Events;

public class DepthChargeIgniter : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] public Transform playerTransform;
    [SerializeField] private float explosionDistanceInFront = 3f;

    [Header("Sound")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;

    public UnityEvent onExplosionCompleted;

    public void PlayHitSound()
    {
        if (hitSound == null)
            return;

        GameObject audioObj = new GameObject("DepthChargeExplosionSound");
        audioObj.transform.position = transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = hitSound;
        source.volume = hitVolume;

        // Important: make it 2D while testing.
        source.spatialBlend = 0f;

        source.playOnAwake = false;
        source.loop = false;

        source.Play();

        Destroy(audioObj, hitSound.length + 0.25f);
    }

    public void SpawnExplosion()
    {
        if (explosionPrefab == null)
            return;

        Vector3 flatForward = playerTransform.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = playerTransform.up;

        flatForward.Normalize();

        Vector3 spawnPosition = playerTransform.position + flatForward * explosionDistanceInFront;

        GameObject explosion =
            Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);

        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            Destroy(explosion, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(explosion, 3f);
        }

        Debug.Log("Depth charge exploded at position: " + spawnPosition);
        onExplosionCompleted?.Invoke();
        Debug.Log("Called onExplosionCompleted event after explosion.");
    }
}
