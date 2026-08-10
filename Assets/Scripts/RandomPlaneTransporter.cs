using System.Collections;
using UnityEngine;

public class RandomPlaneTransporter : MonoBehaviour
{
    [Header("Plane Corners")]
    [SerializeField] private Transform cornerA;
    [SerializeField] private Transform cornerB;
    [SerializeField] private Transform cornerC;
    [SerializeField] private Transform cornerD;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private HealthBarDisplay healhBarDisplay;

    [Header("Transport Effect")]
    [SerializeField] private ParticleSystem transportParticles;
    [SerializeField] private float effectDurationSeconds = 2.0f;
    [SerializeField] private float submergeDepth = 3.0f;

    [Header("Depth Charge Spawn")]
    [SerializeField] private GameObject depthChargePrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float minSpawnDistanceFromCamera = 5f;
    [SerializeField] private float maxSpawnDistanceFromCamera = 15f;
    [SerializeField] private float heightAboveCameraTop = 3f;
    [SerializeField] private float hunterDetectionDistance = 20f;

    private Coroutine transportRoutine;

    public bool IsTransporting => transportRoutine != null;

    public float HunterDetectionDistanceForDepthCharges => hunterDetectionDistance;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void RandomTransport()
    {
        if (transportRoutine != null)
            StopCoroutine(transportRoutine);

        transportRoutine = StartCoroutine(RandomTransportRoutine());
    }

    private void SpawnDepthCharge()
    {
        if (depthChargePrefab == null || mainCamera == null)
            return;

        if (!IsAnyHunterAlerted())
        {
            Debug.Log("No depth charges deployed - no hunters altered.");
            return;
        }

        if (!IsHunterNearby())
        {
            Debug.Log("No depth charges deployed - no hunters nearby.");
            return;
        }

        Debug.Log("Spawning depth charge above camera.");

        Vector3 flatForward = mainCamera.transform.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = playerTransform != null ? playerTransform.forward : Vector3.forward;

        flatForward.Normalize();

        float distance = Random.Range(minSpawnDistanceFromCamera, maxSpawnDistanceFromCamera);

        Vector3 spawnPosition = mainCamera.transform.position + flatForward * distance;
        spawnPosition.y = mainCamera.transform.position.y + heightAboveCameraTop;

        var depthCharge = Instantiate(depthChargePrefab, spawnPosition, Quaternion.identity);
        depthCharge.gameObject.SetActive(true);

        var lightPulser = depthCharge.GetComponent<AlternatingLightPulser>();
        if (lightPulser != null)
        {
            lightPulser.gameObject.SetActive(true);
            lightPulser.Play();
        }

        var igniter = depthCharge.GetComponent<DepthChargeIgniter>();
        if (igniter != null)
        {
            Debug.Log("Assigning main camera to DepthChargeIgniter.");
            igniter.playerTransform = playerTransform;
            igniter.onExplosionCompleted.AddListener(() => CalculateRandomDamage());
        }
    }

    private bool IsHunterNearby()
    {
        GameObject[] hunters = GameObject.FindGameObjectsWithTag("Hunter");
        if (hunters.Length == 0)
            return false;

        Vector3 referencePosition = playerTransform != null ? playerTransform.position : transform.position;

        foreach (GameObject hunter in hunters)
        {
            if (Vector3.Distance(referencePosition, hunter.transform.position) <= hunterDetectionDistance)
                return true;
        }

        return false;
    }

    private bool IsAnyHunterAlerted()
    {
        foreach (EnemyHunterBehavior hunter in FindObjectsByType<EnemyHunterBehavior>(FindObjectsInactive.Include))
        {
            if (hunter.isAlerted)
                return true;
        }

        return false;
    }

    private IEnumerator RandomTransportRoutine()
    {
        if (!IsValid())
            yield break;

        SpawnDepthCharge();

        float surfaceY = playerTransform.position.y;
        float submergedY = surfaceY - submergeDepth;

        PlayParticles();
        yield return AnimateDepth(surfaceY, submergedY, effectDurationSeconds);

        playerTransform.position = GetRandomPointInPlane();

        PlayParticles();
        yield return AnimateDepth(submergedY, surfaceY, effectDurationSeconds);

        StopParticles();

        transportRoutine = null;
    }

    private IEnumerator AnimateDepth(float fromY, float toY, float duration)
    {
        Vector3 position = playerTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            position.y = Mathf.Lerp(fromY, toY, Mathf.Clamp01(elapsed / duration));
            playerTransform.position = position;
            yield return null;
        }

        position.y = toY;
        playerTransform.position = position;
    }

    private void CalculateRandomDamage()
    {
        if (healhBarDisplay == null)
            return;

        Debug.Log("Inside RandomPlaneTransporter: CalculateRandomDamage called.");

        float damage = Random.Range(25f, 50f);
        Debug.Log($"Applying random damage: {damage}");

        healhBarDisplay.ApplyDamage(damage);
    }

    private Vector3 GetRandomPointInPlane()
    {
        float u = Random.value;
        float v = Random.value;

        Vector3 bottomEdge = Vector3.Lerp(cornerA.position, cornerB.position, u);
        Vector3 topEdge = Vector3.Lerp(cornerD.position, cornerC.position, u);

        Vector3 randomPosition = Vector3.Lerp(bottomEdge, topEdge, v);

        randomPosition.y = playerTransform.position.y;
        return randomPosition;
    }

    private void PlayParticles()
    {
        if (transportParticles == null)
            return;

        transportParticles.gameObject.SetActive(true);
        transportParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        transportParticles.Play(true);
    }

    private void StopParticles()
    {
        if (transportParticles == null)
            return;

        transportParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        transportParticles.gameObject.SetActive(false);
    }

    private bool IsValid()
    {
        if (cornerA == null || cornerB == null || cornerC == null || cornerD == null || playerTransform == null)
        {
            Debug.LogWarning("RandomPlaneTransporter is missing one or more required Transform references.");
            return false;
        }

        return true;
    }
}
