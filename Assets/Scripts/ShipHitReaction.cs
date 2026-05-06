using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ShipHitReaction : MonoBehaviour
{
    [Header("Impact Effects")]
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;

    [Header("Sinking")]
    [SerializeField] private int hitsBeforeSinking = 2;
    [SerializeField] private float sinkDistance = 300f;
    [SerializeField] private float sinkSpeed = 5f;
    [SerializeField] private bool rotateWhileSinking = true;
    [SerializeField] private Vector3 sinkingRotationPerSecond = new Vector3(0f, 0f, 5f);

    private int hitCount = 0;
    private bool isSinking = false;

    public void RegisterHit(Vector3 hitPosition)
    {
        if (isSinking)
            return;

        hitCount++;

        SpawnSmoke(hitPosition);
        PlayHitSound(hitPosition);

        if (hitCount >= hitsBeforeSinking)
        {
            StartCoroutine(SinkAndDestroy());
        }
    }

    private void SpawnSmoke(Vector3 position)
    {
        if (smokePrefab == null)
            return;

        Instantiate(smokePrefab, position, Quaternion.identity);
    }

    /*
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

    private void PlayHitSound(Vector3 position)
    {
        if (hitSound == null)
            return;

        // AudioSource.PlayClipAtPoint(hitSound, position, hitVolume);

        GameObject audioObj = new GameObject("ExplosionSound");
        audioObj.transform.position = position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = hitSound;
        source.volume = 1;

        // Important: make it 2D while testing.
        source.spatialBlend = 0f;

        source.playOnAwake = false;
        source.loop = false;

        source.Play();

        Destroy(audioObj, hitSound.length + 0.25f);
    }

    private IEnumerator SinkAndDestroy()
    {
        isSinking = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * sinkDistance;

        while (Vector3.Distance(transform.position, endPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                endPosition,
                sinkSpeed * Time.deltaTime
            );

            if (rotateWhileSinking)
            {
                transform.Rotate(sinkingRotationPerSecond * Time.deltaTime, Space.Self);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
