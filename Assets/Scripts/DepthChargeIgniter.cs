using UnityEngine;

public class DepthChargeIgniter : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] public GameObject mainCamera;
    [SerializeField] private float explosionDistanceInFront = 3f;

    [Header("Sound")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;

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

        /*
        Vector3 flatForward = mainCamera.transform.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = mainCamera.transform.up;

        flatForward.Normalize();

        Vector3 spawnPosition = mainCamera.transform.position + flatForward * explosionDistanceInFront;
        */

        Vector3 spawnPosition = transform.position + transform.forward * explosionDistanceInFront;

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
    }

    private Transform FindDepthChargeAncestor()
    {
        Transform current = transform.parent;

        while (current != null)
        {
            if (current.CompareTag("DepthCharge"))
                return current;

            current = current.parent;
        }

        return null;
    }
}
