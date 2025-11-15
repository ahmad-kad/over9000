using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
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
        public Key startScanKey = Key.Space;
        public Key stopScanKey = Key.Escape;
        public Key toggleWebcamKey = Key.W;
        public Key toggleDebugKey = Key.D;
        public Key toggleUIKey = Key.U;
        public Key restartWebcamKey = Key.R;
        public Key validateAIKey = Key.V;

        // Input System actions
        private InputAction startScanAction;
        private InputAction stopScanAction;
        private InputAction toggleWebcamAction;
        private InputAction toggleDebugAction;
        private InputAction restartWebcamAction;
        private InputAction validateAIAction;

        void Awake()
        {
            Debug.Log($"XRTestSceneRunner: Awake() called on {gameObject.name}");
            SystemLogger.LogInfo("XRTestSceneRunner", $"AWAKE: XRTestSceneRunner instantiated on {gameObject.name}");
        }

        void Start()
        {
            Debug.Log($"XRTestSceneRunner: Start() called on {gameObject.name}");
            SystemLogger.LogInfo("XRTestSceneRunner", $"START: autoSetupOnStart={autoSetupOnStart}, gameObject.activeSelf={gameObject.activeSelf}");

            if (autoSetupOnStart)
            {
                Debug.Log("XRTestSceneRunner: autoSetupOnStart is TRUE, calling SetupTestScene()");
                SystemLogger.LogInfo("XRTestSceneRunner", "Calling SetupTestScene()...");
                SetupTestScene();
            }
            else
            {
                Debug.Log("XRTestSceneRunner: autoSetupOnStart is FALSE - setup will not run automatically");
                SystemLogger.LogWarning("XRTestSceneRunner", "autoSetupOnStart is FALSE - setup will not run automatically");
            }

            // Initialize Input System actions
            InitializeInputActions();
            SystemLogger.LogInfo("XRTestSceneRunner", "XRTestSceneRunner Start() completed");
        }

        void Update()
        {
            HandleTestControls();
        }

        public void SetupTestScene()
        {
            SystemLogger.LogInfo("XRTestSceneRunner", "XRTestSceneRunner: Setting up XR HUD test scene...");

            // Create scene setup if not assigned
            if (sceneSetup == null)
            {
                SystemLogger.LogInfo("XRTestSceneRunner", "Creating XRTestSceneSetup component...");
                GameObject setupObj = new GameObject("XR Test Scene Setup");
                sceneSetup = setupObj.AddComponent<XRTestSceneSetup>();

                // Configure for testing
                sceneSetup.useWebcamFeed = true;
                sceneSetup.enablePoseEstimation = true;
                sceneSetup.enableHandRecognition = true;
                sceneSetup.showDebugOverlay = true;
                sceneSetup.showSpatialUI = true;
                sceneSetup.showScreenUI = true;

                SystemLogger.LogInfo("XRTestSceneRunner", $"Created XRTestSceneSetup on GameObject: {setupObj.name}");
            }
            else
            {
                SystemLogger.LogInfo("XRTestSceneRunner", "XRTestSceneSetup already exists, using existing component");
            }

            // The XRTestSceneSetup will handle the actual scene setup via its Awake() method
            SystemLogger.LogInfo("XRTestSceneRunner", "Test scene setup complete. Use keyboard controls to test features.");
            DisplayHelpText();
        }

        private void InitializeInputActions()
        {
            // Create input actions for keyboard controls
            startScanAction = new InputAction("StartScan", binding: $"<Keyboard>/{startScanKey}");
            stopScanAction = new InputAction("StopScan", binding: $"<Keyboard>/{stopScanKey}");
            toggleWebcamAction = new InputAction("ToggleWebcam", binding: $"<Keyboard>/{toggleWebcamKey}");
            toggleDebugAction = new InputAction("ToggleDebug", binding: $"<Keyboard>/{toggleDebugKey}");
            restartWebcamAction = new InputAction("RestartWebcam", binding: $"<Keyboard>/{restartWebcamKey}");
            validateAIAction = new InputAction("ValidateAI", binding: $"<Keyboard>/{validateAIKey}");

            // Enable the actions
            startScanAction.Enable();
            stopScanAction.Enable();
            toggleWebcamAction.Enable();
            toggleDebugAction.Enable();
            restartWebcamAction.Enable();
            validateAIAction.Enable();

            // Subscribe to action events
            startScanAction.performed += ctx => sceneSetup?.StartTestScan();
            stopScanAction.performed += ctx => sceneSetup?.StopTestScan();
            toggleWebcamAction.performed += ctx => sceneSetup?.ToggleWebcamDisplay();
            toggleDebugAction.performed += ctx => sceneSetup?.ToggleDebugOverlay();
            restartWebcamAction.performed += ctx => sceneSetup?.RestartWebcam();
            validateAIAction.performed += ctx => sceneSetup?.ValidateAIComponents();
        }

        private void HandleTestControls()
        {
            // Input handling is now done via Input System actions in InitializeInputActions
            // This method is kept for potential future use or additional controls
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
  R - Restart webcam
  V - Validate AI components

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

        private void OnDestroy()
        {
            // Clean up input actions
            startScanAction?.Disable();
            stopScanAction?.Disable();
            toggleWebcamAction?.Disable();
            toggleDebugAction?.Disable();
            restartWebcamAction?.Disable();
            validateAIAction?.Disable();
        }

        // Menu item for easy access (would work in Unity Editor)
        [UnityEditor.MenuItem("ScouterXR/Create Test Scene")]
        static void CreateTestSceneMenuItem()
        {
            LoadTestScene();
        }
    }
}
