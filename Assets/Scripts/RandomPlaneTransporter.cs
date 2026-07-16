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
    [SerializeField] private Transform player;

    [Header("Transport Effect")]
    [SerializeField] private ParticleSystem transportParticles;
    [SerializeField] private float effectDurationSeconds = 2.0f;
    [SerializeField] private float submergeDepth = 3.0f;

    private Coroutine transportRoutine;

    public void RandomTransport()
    {
        if (transportRoutine != null)
            StopCoroutine(transportRoutine);

        transportRoutine = StartCoroutine(RandomTransportRoutine());
    }

    private IEnumerator RandomTransportRoutine()
    {
        if (!IsValid())
            yield break;

        float surfaceY = player.position.y;
        float submergedY = surfaceY - submergeDepth;

        PlayParticles();
        yield return AnimateDepth(surfaceY, submergedY, effectDurationSeconds);

        player.position = GetRandomPointInPlane();

        PlayParticles();
        yield return AnimateDepth(submergedY, surfaceY, effectDurationSeconds);

        StopParticles();

        transportRoutine = null;
    }

    private IEnumerator AnimateDepth(float fromY, float toY, float duration)
    {
        Vector3 position = player.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            position.y = Mathf.Lerp(fromY, toY, Mathf.Clamp01(elapsed / duration));
            player.position = position;
            yield return null;
        }

        position.y = toY;
        player.position = position;
    }

    private Vector3 GetRandomPointInPlane()
    {
        float u = Random.value;
        float v = Random.value;

        Vector3 bottomEdge = Vector3.Lerp(cornerA.position, cornerB.position, u);
        Vector3 topEdge = Vector3.Lerp(cornerD.position, cornerC.position, u);

        Vector3 randomPosition = Vector3.Lerp(bottomEdge, topEdge, v);

        randomPosition.y = player.position.y;
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
        if (cornerA == null || cornerB == null || cornerC == null || cornerD == null || player == null)
        {
            Debug.LogWarning("RandomPlaneTransporter is missing one or more required Transform references.");
            return false;
        }

        return true;
    }
}
