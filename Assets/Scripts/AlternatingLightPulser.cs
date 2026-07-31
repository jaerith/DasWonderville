using System.Collections;
using UnityEngine;

public class AlternatingLightPulser : MonoBehaviour
{
    private bool isPulsing = false;

    [Header("Lights")]
    [SerializeField] private Light lightA;

    [SerializeField] private Light lightB;

    [Header("Timing")]
    [Tooltip("Number of times per second the lights alternate.")]
    [SerializeField] private float frequency = 2f;

    private Coroutine pulseCoroutine;

    private void OnEnable()
    {
        Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    public bool IsPulsing => isPulsing;

    public void Play()
    {
        if (lightA == null || lightB == null)
        {
            // Debug.LogWarning("AlternatingLightPulser is missing one or more Light references.");
            return;
        }
        else
        {
            // Debug.LogWarning("AlternatingLightPulser is playing.");
        }

        Stop();

        isPulsing = true;

        lightA.gameObject.SetActive(true);
        lightB.gameObject.SetActive(true);

        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    public void Stop()
    {
        Debug.LogWarning("AlternatingLightPulser is stopping.");

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        if (lightA != null)
        {
            lightA.enabled = false;
            lightA.gameObject.SetActive(false);
        }

        if (lightB != null)
        {
            lightB.enabled = false;
            lightB.gameObject.SetActive(false);
        }

        isPulsing = false;
    }

    private IEnumerator PulseRoutine()
    {
        float interval = 1f / Mathf.Max(0.01f, frequency);

        while (true)
        {
            lightA.enabled = true;
            lightB.enabled = false;

            yield return new WaitForSeconds(interval);

            lightA.enabled = false;
            lightB.enabled = true;

            yield return new WaitForSeconds(interval);
        }
    }
}
