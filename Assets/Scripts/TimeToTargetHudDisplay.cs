using TMPro;
using UnityEngine;

public class TimeToTargetHudDisplay : MonoBehaviour
{
    private const string NoTargetLabel = "--:--";

    [SerializeField] private InputManager inputManager;
    [SerializeField] private string targetTag = "Ship";

    public TextMeshPro timeToTargetText;

    private void Update()
    {
        if (timeToTargetText == null || inputManager == null)
            return;

        Transform viewer = inputManager.Viewer;
        float torpedoSpeed = inputManager.TorpedoSpeed;

        if (viewer == null || torpedoSpeed <= 0f)
        {
            timeToTargetText.text = NoTargetLabel;
            return;
        }

        // Cast from the same point a torpedo would actually spawn from (not the
        // viewer itself), otherwise this immediately hits the player's own hull
        // collider, which also carries the "Ship" tag.
        Vector3 launchDirection = inputManager.GetTorpedoLaunchDirection();
        Vector3 launchPosition = inputManager.GetTorpedoSpawnPosition(launchDirection);

        bool hitShip = Physics.Raycast(
            launchPosition,
            launchDirection,
            out RaycastHit hit,
            inputManager.TorpedoMaxDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide) && hit.collider.CompareTag(targetTag);

        timeToTargetText.text = hitShip
            ? FormatTime(hit.distance / torpedoSpeed)
            : NoTargetLabel;
    }

    private static string FormatTime(float totalSeconds)
    {
        int wholeSeconds = Mathf.Max(0, Mathf.RoundToInt(totalSeconds));
        int minutes = wholeSeconds / 60;
        int seconds = wholeSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}
