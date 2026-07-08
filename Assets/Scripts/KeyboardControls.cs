using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DebugKeyboardControls : MonoBehaviour
{
    [SerializeField] private UnityEvent onRestartGame;

    private void Update()
    {
#if UNITY_ANDROID
        // Do nothing on Android, as we don't want to handle keyboard input there
#elif UNITY_STANDALONE_WIN
        // Fire left torpedo
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            onRestartGame.Invoke();
        }

        // Quit the application
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
        }
#endif
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}