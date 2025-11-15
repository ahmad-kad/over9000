using UnityEngine;
using UnityEngine.SceneManagement;
using ScouterXR.Core;

namespace ScouterXR.Core
{
    /// <summary>
    /// Scene runner for creating and managing the XR HUD test scene
    /// Attach this to an empty GameObject in a scene to set up the test environment
    /// </summary>
    public class XRTestSceneRunner : MonoBehaviour
    {
        [Header("Scene Setup")]
        public bool autoSetupOnStart = true;
        public XRTestSceneSetup sceneSetup;

        [Header("Test Controls")]
        public KeyCode startScanKey = KeyCode.Space;
        public KeyCode stopScanKey = KeyCode.Escape;
        public KeyCode toggleWebcamKey = KeyCode.W;
        public KeyCode toggleDebugKey = KeyCode.D;
        public KeyCode toggleUIKey = KeyCode.U;

        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTestScene();
            }
        }

        void Update()
        {
            HandleTestControls();
        }

        public void SetupTestScene()
        {
            SystemLogger.LogInfo("XRTestSceneRunner", "Setting up XR HUD test scene...");

            // Create scene setup if not assigned
            if (sceneSetup == null)
            {
                GameObject setupObj = new GameObject("XR Test Scene Setup");
                sceneSetup = setupObj.AddComponent<XRTestSceneSetup>();

                // Configure for testing
                sceneSetup.useWebcamFeed = true;
                sceneSetup.enablePoseEstimation = true;
                sceneSetup.enableHandRecognition = true;
                sceneSetup.showDebugOverlay = true;
                sceneSetup.showSpatialUI = true;
                sceneSetup.showScreenUI = true;
            }

            // The XRTestSceneSetup will handle the actual scene setup
            SystemLogger.LogInfo("XRTestSceneRunner", "Test scene setup complete. Use keyboard controls to test features.");
            DisplayHelpText();
        }

        private void HandleTestControls()
        {
            if (sceneSetup == null) return;

            // Scan controls
            if (Input.GetKeyDown(startScanKey))
            {
                sceneSetup.StartTestScan();
                SystemLogger.LogInfo("XRTestSceneRunner", "Starting test scan...");
            }

            if (Input.GetKeyDown(stopScanKey))
            {
                sceneSetup.StopTestScan();
                SystemLogger.LogInfo("XRTestSceneRunner", "Stopping test scan...");
            }

            // Display toggles
            if (Input.GetKeyDown(toggleWebcamKey))
            {
                sceneSetup.ToggleWebcamDisplay();
                SystemLogger.LogInfo("XRTestSceneRunner", "Toggled webcam display");
            }

            if (Input.GetKeyDown(toggleDebugKey))
            {
                sceneSetup.ToggleDebugOverlay();
                SystemLogger.LogInfo("XRTestSceneRunner", "Toggled debug overlay");
            }
        }

        private void DisplayHelpText()
        {
            string helpText = @"
=== XR HUD Test Scene Controls ===

Scan Controls:
  SPACE - Start test scan (simulates pointing gesture)
  ESC - Stop current scan

Display Toggles:
  W - Toggle webcam display
  D - Toggle debug overlay

Features:
- Webcam feed integration
- Pose estimation (mock data)
- Hand gesture recognition
- Spatial UI with power levels
- Audio feedback linked to states
- Real-time streaming data pipeline

Status indicators show:
- Pose detection status
- Hand detection status
- Scanning progress

Look for debug logs in console for detailed information.
";
            Debug.Log(helpText);
        }

        // Public API for external control
        public void StartScan() => sceneSetup?.StartTestScan();
        public void StopScan() => sceneSetup?.StopTestScan();
        public void ToggleWebcam() => sceneSetup?.ToggleWebcamDisplay();
        public void ToggleDebug() => sceneSetup?.ToggleDebugOverlay();

        // Scene management
        public static void LoadTestScene()
        {
            // Create a new scene programmatically
            Scene testScene = SceneManager.CreateScene("XR HUD Test Scene");

            // Create scene root
            GameObject sceneRoot = new GameObject("Test Scene Root");

            // Add the test scene runner
            XRTestSceneRunner runner = sceneRoot.AddComponent<XRTestSceneRunner>();
            runner.autoSetupOnStart = true;

            // Move to the new scene
            SceneManager.MoveGameObjectToScene(sceneRoot, testScene);
            SceneManager.SetActiveScene(testScene);

            Debug.Log("Created XR HUD Test Scene. Components will be set up automatically.");
        }

        // Menu item for easy access (would work in Unity Editor)
        [UnityEditor.MenuItem("ScouterXR/Create Test Scene")]
        static void CreateTestSceneMenuItem()
        {
            LoadTestScene();
        }
    }
}
