using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DebugKeyboardControls : MonoBehaviour
{
    [SerializeField]
    [Min(1)]
    private int resolutionMultiplier = 3;

    [SerializeField] private UnityEvent onFireDecoy;

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

        // Fire sonar decoy
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            onFireDecoy.Invoke();
        }

        // Quit the application
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            TakeScreenshot();
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

    public void TakeScreenshot()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "SubmarineSimulatorScreenshots");

        Directory.CreateDirectory(folder);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"SubmarineScreenshot_{timestamp}.png";
        string fullPath = Path.Combine(folder, filename);

        ScreenCapture.CaptureScreenshot(fullPath, resolutionMultiplier);

        Debug.Log($"Screenshot requested: {fullPath}");
    }
}