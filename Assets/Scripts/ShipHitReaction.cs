using System.Collections;
using UnityEngine;

public class ShipHitReaction : MonoBehaviour
{
    [Header("Impact Effects")]
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;

    [Header("Nearby Sibling Detection")]
    [SerializeField] private float nearbySiblingDetectionDistance = 25f;
    [SerializeField] private bool logNearbySiblings = true;

    [Header("Sinking")]
    [SerializeField] private int hitsBeforeSinking = 2;
    [SerializeField] private float sinkDistance = 300f;
    [SerializeField] private float sinkSpeed = 5f;
    [SerializeField] private bool rotateWhileSinking = true;
    [SerializeField] private Vector3 sinkingRotationPerSecond = new Vector3(0f, 0f, 5f);

    private int hitCount;
    private bool isSinking;

    public void RegisterHit(Vector3 hitPosition)
    {
        if (isSinking)
            return;

        hitCount++;

        foreach (EnemyHunterBehavior destroyer in FindObjectsByType<EnemyHunterBehavior>(FindObjectsInactive.Include))
        {
            destroyer.OnAnyShipHit(transform);
        }

        PlayHitSound(hitPosition);

        if (hitCount >= hitsBeforeSinking)
        {
            SpawnExplosion(hitPosition);
            DetectNearbySiblingObjects();
            StartCoroutine(SinkAndDestroy());
        }
        else
        {
            SpawnSmoke(hitPosition);
        }
    }

    private void SpawnSmoke(Vector3 position)
    {
        if (smokePrefab == null)
            return;

        Instantiate(smokePrefab, position, Quaternion.identity);
    }

    private void SpawnExplosion(Vector3 position)
    {
        if (explosionPrefab == null)
        {
            SpawnSmoke(position);
            return;
        }

        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

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

    private void DetectNearbySiblingObjects()
    {
        Transform parent = transform.parent;

        if (parent == null)
            return;

        foreach (Transform sibling in parent)
        {
            if (sibling == transform)
                continue;

            float distance = Vector3.Distance(transform.position, sibling.position);

            if (distance <= nearbySiblingDetectionDistance)
            {
                if (logNearbySiblings)
                {
                    Debug.Log(
                        $"Nearby sibling detected: {sibling.name}, distance: {distance:F2}",
                        sibling.gameObject
                    );
                }

                sibling.SendMessage(
                    "OnNearbyShipExplosion",
                    transform,
                    SendMessageOptions.DontRequireReceiver
                );
            }
        }
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
