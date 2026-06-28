using TMPro;
using UnityEngine;

public class StatsHudDisplay : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    public TextMeshPro statusText;
    public TextMeshPro torpedoTimerText;
    public TextMeshPro escapeTimerText;

    private Color defaultColor = Color.white;

    private void Awake()
    {
        defaultColor = statusText.color;
    }

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

    public void DisplayStatusDefault()
    {
        statusText.text = "System Status";
        statusText.color = defaultColor;
    }

    public void DisplayStatusUnderAttack()
    {
        statusText.text = "UNDER ATTACK";
        statusText.color = Color.yellow;
    }

    public void DisplayStatusGameOver()
    {
        statusText.text  = "GAME OVER";
        statusText.color = Color.red;
    }
}
