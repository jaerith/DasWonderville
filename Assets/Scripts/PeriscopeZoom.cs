using System.Collections;
using UnityEngine;

public class PeriscopeZoom : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Camera playerCamera;

    [Header("Zoom Settings")]
    [SerializeField] private float normalFieldOfView = 60f;
    [SerializeField] private float zoomedFieldOfView = 6f;
    [SerializeField] private float zoomTransitionSeconds = 0.4f;

    [Header("Panel Magnification")]
    [Tooltip("How much larger the scope panel itself grows while zoomed, on top of the narrower field of view.")]
    [SerializeField] private float zoomedPanelScaleMultiplier = 1.5f;

    // Wired by CreatePeriscopeZoomHudPrefab.cs when the HUD prefab is built.
    public Camera scopeCamera;
    public GameObject scopeDisplayRoot;

    private bool isZoomed;
    private Coroutine zoomRoutine;
    private Vector3 normalPanelScale = Vector3.one;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (scopeCamera != null)
        {
            scopeCamera.fieldOfView = normalFieldOfView;
            scopeCamera.enabled = false;
        }

        if (scopeDisplayRoot != null)
        {
            normalPanelScale = scopeDisplayRoot.transform.localScale;
            scopeDisplayRoot.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!isZoomed || playerCamera == null || scopeCamera == null)
            return;

        scopeCamera.transform.SetPositionAndRotation(
            playerCamera.transform.position,
            playerCamera.transform.rotation);
    }

    public void ToggleZoom()
    {
        if (isZoomed)
            ZoomOut();
        else
            ZoomIn();
    }

    public void ZoomIn()
    {
        if (scopeCamera == null || isZoomed)
            return;

        isZoomed = true;

        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(ZoomInRoutine());
    }

    public void ZoomOut()
    {
        if (scopeCamera == null || !isZoomed)
            return;

        isZoomed = false;

        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(ZoomOutRoutine());
    }

    private IEnumerator ZoomInRoutine()
    {
        if (scopeDisplayRoot != null)
            scopeDisplayRoot.SetActive(true);

        scopeCamera.enabled = true;

        float startFov = scopeCamera.fieldOfView;
        Vector3 startScale = scopeDisplayRoot != null ? scopeDisplayRoot.transform.localScale : normalPanelScale;
        Vector3 targetScale = normalPanelScale * zoomedPanelScaleMultiplier;
        float elapsed = 0f;

        while (elapsed < zoomTransitionSeconds)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomTransitionSeconds;
            scopeCamera.fieldOfView = Mathf.Lerp(startFov, zoomedFieldOfView, t);

            if (scopeDisplayRoot != null)
                scopeDisplayRoot.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        scopeCamera.fieldOfView = zoomedFieldOfView;

        if (scopeDisplayRoot != null)
            scopeDisplayRoot.transform.localScale = targetScale;

        zoomRoutine = null;
    }

    private IEnumerator ZoomOutRoutine()
    {
        float startFov = scopeCamera.fieldOfView;
        Vector3 startScale = scopeDisplayRoot != null ? scopeDisplayRoot.transform.localScale : normalPanelScale;
        float elapsed = 0f;

        while (elapsed < zoomTransitionSeconds)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomTransitionSeconds;
            scopeCamera.fieldOfView = Mathf.Lerp(startFov, normalFieldOfView, t);

            if (scopeDisplayRoot != null)
                scopeDisplayRoot.transform.localScale = Vector3.Lerp(startScale, normalPanelScale, t);

            yield return null;
        }

        scopeCamera.fieldOfView = normalFieldOfView;

        if (scopeDisplayRoot != null)
        {
            scopeDisplayRoot.transform.localScale = normalPanelScale;
            scopeDisplayRoot.SetActive(false);
        }

        scopeCamera.enabled = false;
        zoomRoutine = null;
    }
}
