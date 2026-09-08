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

    private const int ScreenshotBaseWidth = 1080;
    private const int ScreenshotBaseHeight = 1080;

    [SerializeField] private UnityEvent onFireDecoy;

    [SerializeField] private UnityEvent onForceWin;

    [SerializeField] private UnityEvent onRestartGame;

    [SerializeField] private UnityEvent onToggleZoom;

    [SerializeField] private UnityEvent onSnapTurnLeft;

    [SerializeField] private UnityEvent onSnapTurnRight;

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Do nothing on an actual on-device Android/Quest build, as we don't want to handle keyboard input there
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        // Fire left torpedo
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            onRestartGame.Invoke();
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            onForceWin.Invoke();
        }

        // Fire sonar decoy
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            onFireDecoy.Invoke();
        }

        // Toggle periscope zoom
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            onToggleZoom.Invoke();
        }

        // Snap turn left
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            onSnapTurnLeft.Invoke();
        }

        // Snap turn right
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            onSnapTurnRight.Invoke();
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
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("TakeScreenshot: no main camera found.");
            return;
        }

        int width = ScreenshotBaseWidth * resolutionMultiplier;
        int height = ScreenshotBaseHeight * resolutionMultiplier;

        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        RenderTexture previousTargetTexture = camera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        StereoTargetEyeMask previousStereoTargetEye = camera.stereoTargetEye;

        camera.stereoTargetEye = StereoTargetEyeMask.None;
        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        camera.targetTexture = previousTargetTexture;
        camera.stereoTargetEye = previousStereoTargetEye;
        RenderTexture.active = previousActive;
        Destroy(renderTexture);

        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "SubmarineSimulatorScreenshots");

        Directory.CreateDirectory(folder);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"SubmarineScreenshot_{timestamp}.png";
        string fullPath = Path.Combine(folder, filename);

        File.WriteAllBytes(fullPath, screenshot.EncodeToPNG());
        Destroy(screenshot);

        Debug.Log($"Screenshot saved: {fullPath}");
    }
}