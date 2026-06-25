using TMPro;
using UnityEngine;

public class StatsHudDisplay : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    public TextMeshPro torpedoTimerText;
    public TextMeshPro escapeTimerText;

    private void Update()
    {
        if (inputManager == null)
            return;

        float torpRemaining = inputManager.TorpedoReloadSecondsRemaining;
        if (torpedoTimerText != null)
            torpedoTimerText.text = torpRemaining > 0f
                ? $"TORPEDO  {torpRemaining:F1}s"
                : "TORPEDO  READY";

        float escRemaining = inputManager.EscapeReloadSecondsRemaining;
        if (escapeTimerText != null)
            escapeTimerText.text = escRemaining > 0f
                ? $"ESCAPE   {escRemaining:F1}s"
                : "ESCAPE   READY";
    }
}
