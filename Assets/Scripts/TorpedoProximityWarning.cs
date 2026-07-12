using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TorpedoProximityWarning : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip pingSound;

    [Header("Ping Configuration")]
    [SerializeField] private float detectionRadius = 40f;
    [SerializeField] private float maxPingInterval = 3f;
    [SerializeField] private float minPingInterval = 0.25f;

    private GameObject audioObject = null;
    private AudioSource audioSource = null;
    private float nextPingTime;

    private void Awake()
    {
        audioObject = new GameObject("Ping");
        audioObject.transform.position = player.transform.position;

        audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = pingSound;
        audioSource.volume = 1;
        audioSource.pitch  = 2;

        // Important: make it 2D while testing.
        audioSource.spatialBlend = 0f;

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Update()
    {
        if (Time.time < nextPingTime)
            return;

        float closest = ClosestTorpedoDistance();
        if (closest < 0f)
            return;

        float t = 1f - Mathf.Clamp01(closest / detectionRadius);
        float interval = Mathf.Lerp(maxPingInterval, minPingInterval, t);

        audioSource.PlayOneShot(pingSound);
        nextPingTime = Time.time + interval;
    }

    private float ClosestTorpedoDistance()
    {
        GameObject[] torpedoes = GameObject.FindGameObjectsWithTag("Torpedo");
        Vector3 origin = player != null ? player.position : transform.position;
        float closest = float.MaxValue;

        foreach (GameObject torpedo in torpedoes)
        {
            float dist = Vector3.Distance(torpedo.transform.position, origin);
            if (dist < detectionRadius && dist < closest)
                closest = dist;
        }

        return closest == float.MaxValue ? -1f : closest;
    }
}
