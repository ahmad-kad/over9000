using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using UnityEngine.UI;
using ScouterXR.AI;
using ScouterXR.UI;
using ScouterXR.Core;

namespace ScouterXR.Core
{
    /// <summary>
    /// Dedicated setup script for XR HUD test scene with webcam feed and MVP UI
    /// Focuses on testing pose estimation, hand recognition, and streaming data pipeline
    /// </summary>
    public class XRTestSceneSetup : MonoBehaviour
    {
        [Header("Scene Configuration")]
        public bool useWebcamFeed = true;
        public bool enablePoseEstimation = true;
        public bool enableHandRecognition = true;
        public bool showDebugOverlay = true;

        [Header("Webcam Settings")]
        public int webcamWidth = 640;
        public int webcamHeight = 480;
        public int webcamFPS = 30;

        [Header("UI Settings")]
        public bool showSpatialUI = true;
        public bool showScreenUI = true;
        public float uiUpdateRate = 10f; // Updates per second

        [Header("References")]
        public Camera mainCamera;
        public RawImage webcamDisplay;
        public Canvas mainCanvas;

        // Component references
        private WebCamTexture webcamTexture;
        private MediaPipePoseEstimator poseEstimator;
        private HandPointingRecognizer handRecognizer;
        private XRSpatialUIManager spatialUI;
        private ScouterUI screenUI;
        private XRScouterManager xrManager;
        private SpatialAudioController audioController;
        private DebugOverlay debugOverlay;
        private StreamingPipelineValidator pipelineValidator;

        // UI element references for status updates (support both TMP and regular UI Text)
        private UnityEngine.RectTransform poseStatusRect;
        private UnityEngine.RectTransform handStatusRect;
        private UnityEngine.RectTransform scanStatusRect;

        void Awake()
        {
            Debug.Log("XRTestSceneSetup: Awake() called on gameObject: " + gameObject.name);
            SystemLogger.LogInfo("XRTestSceneSetup", "XRTestSceneSetup Awake() called - starting scene setup");
            SetupTestScene();
            SystemLogger.LogInfo("XRTestSceneSetup", "XRTestSceneSetup Awake() completed");
        }

        void Start()
        {
            SystemLogger.LogInfo("XRTestSceneSetup", "XRTestSceneSetup Start() called");

            // Fallback: if Awake didn't run SetupTestScene, run it now
            if (mainCanvas == null)
            {
                SystemLogger.LogWarning("XRTestSceneSetup", "mainCanvas is null - Awake() may not have run. Running SetupTestScene from Start()");
                SetupTestScene();
            }

            // Ensure webcam display is created early
            if (webcamDisplay == null && useWebcamFeed)
            {
                CreateWebcamDisplay();
                SystemLogger.LogInfo("XRTestSceneSetup", "Webcam display created early");
            }

            StartCoroutine(InitializeWebcam());
            InitializeMVPUI();
            SystemLogger.LogInfo("XRTestSceneSetup", "Test scene initialization complete");
        }

        private void SetupTestScene()
        {
            Debug.Log("XRTestSceneSetup: SetupTestScene() method called!");
            SystemLogger.LogInfo("XRTestSceneSetup", "STARTING XR HUD TEST SCENE SETUP...");

            // Use existing Main Camera from scene
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    // Find by tag if Camera.main doesn't work
                    GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
                    if (cameraObj != null)
                    {
                        mainCamera = cameraObj.GetComponent<Camera>();
                    }
                }

                if (mainCamera == null)
                {
                    // Last resort: create new camera
                    GameObject cameraObj = new GameObject("Main Camera");
                    mainCamera = cameraObj.AddComponent<Camera>();
                    mainCamera.tag = "MainCamera";
                    SystemLogger.LogWarning("XRTestSceneSetup", "Created new Main Camera as fallback");
                }
                else
                {
                    SystemLogger.LogInfo("XRTestSceneSetup", $"Using existing Main Camera: {mainCamera.gameObject.name}");
                }
            }

            // Create canvas for UI first (needed for test indicator)
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("Test Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);

                canvasObj.AddComponent<GraphicRaycaster>();
                SystemLogger.LogInfo("XRTestSceneSetup", "Created main canvas");
            }

            // Create a visible test indicator to ensure scene is working
            CreateTestIndicator();

            // Setup core XR components
            Debug.Log("XRTestSceneSetup: About to call SetupXRComponents()");
            SetupXRComponents();
            Debug.Log("XRTestSceneSetup: SetupXRComponents() completed");

            // Setup AI components
            Debug.Log("XRTestSceneSetup: About to call SetupAIComponents()");
            SetupAIComponents();
            Debug.Log("XRTestSceneSetup: SetupAIComponents() completed");

            // Setup UI components
            SetupUIComponents();

            // Setup audio
            SetupAudioComponents();

            // Setup pipeline validator
            SetupPipelineValidator();

            SystemLogger.LogInfo("XRTestSceneSetup", "Core scene components created");
        }

        private void SetupXRComponents()
        {
            // Use existing Main Camera or create new one if needed
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    GameObject cameraObj = GameObject.Find("Main Camera");
                    if (cameraObj != null)
                    {
                        mainCamera = cameraObj.GetComponent<Camera>();
                    }
                }
            }

            // Create XR Origin with proper setup
            if (FindFirstObjectByType<XROrigin>() == null)
            {
                GameObject xrOriginObj = new GameObject("XR Origin");
                var xrOrigin = xrOriginObj.AddComponent<XROrigin>();
                xrOrigin.Camera = mainCamera;

                // Add Camera Floor Offset for proper floor tracking
                GameObject floorOffsetObj = new GameObject("Camera Floor Offset");
                floorOffsetObj.transform.SetParent(xrOriginObj.transform);
                xrOrigin.CameraFloorOffsetObject = floorOffsetObj;

                // Add Tracked Pose Driver to the XR Origin camera for XR input
                if (mainCamera != null)
                {
                    var trackedPoseDriver = mainCamera.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                    trackedPoseDriver.positionAction = new UnityEngine.InputSystem.InputAction("Position", UnityEngine.InputSystem.InputActionType.Value, "<XRHMD>/centerEyePosition");
                    trackedPoseDriver.rotationAction = new UnityEngine.InputSystem.InputAction("Rotation", UnityEngine.InputSystem.InputActionType.Value, "<XRHMD>/centerEyeRotation");
                    trackedPoseDriver.trackingType = UnityEngine.InputSystem.XR.TrackedPoseDriver.TrackingType.RotationAndPosition;
                    trackedPoseDriver.updateType = UnityEngine.InputSystem.XR.TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
                }

                SystemLogger.LogInfo("XRTestSceneSetup", "Created XR Origin with floor offset and tracked pose driver");
            }

            // Create XR Scouter Manager
            GameObject xrObj = new GameObject("XR Scouter Manager");
            xrManager = xrObj.AddComponent<XRScouterManager>();
            xrManager.arCamera = mainCamera;
            xrManager.scanDuration = 2f; // Shorter for testing
            xrManager.maxScanDistance = 5f;

            // Setup XR Management if not present
            SetupXRManagement();
        }

        private void SetupXRManagement()
        {
            // XR Management is automatically configured through packages in Unity 6
            // Just verify it's available
            var xrGeneralSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance;
            if (xrGeneralSettings != null && xrGeneralSettings.Manager != null)
            {
                SystemLogger.LogInfo("XRTestSceneSetup", "XR Management is properly configured");
            }
            else
            {
                SystemLogger.LogWarning("XRTestSceneSetup", "XR Management may not be properly configured. Please check Project Settings > XR Plug-in Management");
            }
        }

        private void SetupAIComponents()
        {
            Debug.Log($"XRTestSceneSetup: SETUP_AI_COMPONENTS called with enablePoseEstimation={enablePoseEstimation}, enableHandRecognition={enableHandRecognition}");
            SystemLogger.LogInfo("XRTestSceneSetup", $"SETUP_AI_COMPONENTS: enablePoseEstimation={enablePoseEstimation}, enableHandRecognition={enableHandRecognition}");

            // Create MediaPipe Model Manager
            Debug.Log("XRTestSceneSetup: Creating MediaPipe Model Manager GameObject");
            GameObject modelObj = new GameObject("MediaPipe Model Manager");
            var modelManager = modelObj.AddComponent<MediaPipeModelManager>();
            Debug.Log($"XRTestSceneSetup: MediaPipe Model Manager created on {modelObj.name}");
            SystemLogger.LogInfo("XRTestSceneSetup", "Created MediaPipe Model Manager");

            // Create Pose Estimator
            if (enablePoseEstimation)
            {
                Debug.Log("XRTestSceneSetup: Creating Pose Estimator GameObject");
                SystemLogger.LogInfo("XRTestSceneSetup", "Creating MediaPipe Pose Estimator...");
                GameObject poseObj = new GameObject("Pose Estimator");
                poseEstimator = poseObj.AddComponent<MediaPipePoseEstimator>();
                Debug.Log($"XRTestSceneSetup: Pose Estimator component added to {poseObj.name}");

                // Set properties BEFORE Start() is called
                poseEstimator.arCamera = mainCamera;
                poseEstimator.modelManager = modelManager;
                poseEstimator.useRealPoseDetection = true; // Use real MediaPipe processing
                poseEstimator.debugMode = true;

                Debug.Log($"XRTestSceneSetup: Set poseEstimator properties - useRealPoseDetection={poseEstimator.useRealPoseDetection}, debugMode={poseEstimator.debugMode}");

                SystemLogger.LogInfo("XRTestSceneSetup", $"Created pose estimator with real MediaPipe processing - GameObject: {poseObj.name}");
                Debug.Log("XRTestSceneSetup: Pose Estimator setup complete");
            }
            else
            {
                Debug.Log("XRTestSceneSetup: Pose estimation is DISABLED");
                SystemLogger.LogWarning("XRTestSceneSetup", "Pose estimation is DISABLED");
            }

            // Create Hand Pointing Recognizer
            if (enableHandRecognition)
            {
                Debug.Log("XRTestSceneSetup: Creating Hand Pointing Recognizer GameObject");
                SystemLogger.LogInfo("XRTestSceneSetup", "Creating MediaPipe Hand Pointing Recognizer...");
                GameObject handObj = new GameObject("Hand Pointing Recognizer");
                handRecognizer = handObj.AddComponent<HandPointingRecognizer>();
                Debug.Log($"XRTestSceneSetup: Hand Recognizer component added to {handObj.name}");

                // Set properties BEFORE Start() is called
                handRecognizer.arCamera = mainCamera;
                handRecognizer.enableHandTracking = true;
                handRecognizer.targetWidth = webcamWidth;
                handRecognizer.targetHeight = webcamHeight;
                handRecognizer.inferenceInterval = 1f / uiUpdateRate;
                handRecognizer.debugMode = true;

                Debug.Log($"XRTestSceneSetup: Set handRecognizer properties - enableHandTracking={handRecognizer.enableHandTracking}, debugMode={handRecognizer.debugMode}");

                SystemLogger.LogInfo("XRTestSceneSetup", $"Created hand pointing recognizer - GameObject: {handObj.name}");
                Debug.Log("XRTestSceneSetup: Hand Recognizer setup complete");
            }
            else
            {
                Debug.Log("XRTestSceneSetup: Hand recognition is DISABLED");
                SystemLogger.LogWarning("XRTestSceneSetup", "Hand recognition is DISABLED");
            }
        }

        private void SetupUIComponents()
        {
            // Create Spatial UI Manager
            if (showSpatialUI)
            {
                GameObject spatialUIObj = new GameObject("Spatial UI Manager");
                spatialUI = spatialUIObj.AddComponent<XRSpatialUIManager>();
                spatialUI.arCamera = mainCamera;
                spatialUI.uiDistanceFromTarget = 0.3f; // Closer for testing
                spatialUI.uiScale = 0.0005f; // Smaller scale for testing
                spatialUI.poseEstimator = poseEstimator;
                spatialUI.scouterManager = xrManager;
            }

            // Create Screen UI
            if (showScreenUI)
            {
                GameObject screenUIObj = new GameObject("Screen UI");
                screenUI = screenUIObj.AddComponent<ScouterUI>();

                // Create UI elements
                CreateScreenUIElements(screenUIObj);
            }

            // Create Debug Overlay
            if (showDebugOverlay)
            {
                GameObject debugObj = new GameObject("Debug Overlay");
                debugOverlay = debugObj.AddComponent<DebugOverlay>();
                debugOverlay.poseEstimator = poseEstimator;
                debugOverlay.handRecognizer = handRecognizer;
                debugOverlay.showPoseData = enablePoseEstimation;
                debugOverlay.showHandData = enableHandRecognition;
                debugOverlay.showWebcamFeed = useWebcamFeed;
                debugOverlay.showByDefault = true; // Show debug overlay by default so user can see pose/hand data
            }

            // Create Pose Visualizer for rendering pose/hand landmarks on screen
            CreatePoseVisualizer();
        }

        private void SetupAudioComponents()
        {
            // Create Spatial Audio Controller
            GameObject audioObj = new GameObject("Spatial Audio Controller");
            audioController = audioObj.AddComponent<SpatialAudioController>();

            // Assign audio clips (mock assignments for testing)
            // In real implementation, these would be loaded from Resources
            audioController.minDistance = 0.5f;
            audioController.maxDistance = 3f;

            // Link to XR Manager
            if (xrManager != null)
            {
                xrManager.audioController = audioController;
            }
        }

        private void SetupPipelineValidator()
        {
            // Create Streaming Pipeline Validator
            GameObject validatorObj = new GameObject("Streaming Pipeline Validator");
            pipelineValidator = validatorObj.AddComponent<StreamingPipelineValidator>();

            // Auto-assign all components to validator
            pipelineValidator.poseEstimator = poseEstimator;
            pipelineValidator.handRecognizer = handRecognizer;
            pipelineValidator.xrManager = xrManager;
            pipelineValidator.spatialUI = spatialUI;
            pipelineValidator.screenUI = screenUI;
            pipelineValidator.audioController = audioController;
            pipelineValidator.enableValidation = true;
            pipelineValidator.logValidationResults = true;

            SystemLogger.LogInfo("XRTestSceneSetup", "Created Streaming Pipeline Validator for real-time monitoring");
        }

        private System.Collections.IEnumerator InitializeWebcam()
        {
            if (!useWebcamFeed)
            {
                SystemLogger.LogInfo("XRTestSceneSetup", "Webcam feed disabled");
                yield break;
            }

            SystemLogger.LogInfo("XRTestSceneSetup", "Initializing webcam...");

            // Check for webcam devices
            SystemLogger.LogInfo("XRTestSceneSetup", $"Checking for webcam devices... Available: {WebCamTexture.devices.Length}");
            if (WebCamTexture.devices.Length == 0)
            {
                SystemLogger.LogError("XRTestSceneSetup", "No webcam devices found! Make sure webcam permissions are granted.");
                // Still create display but with error message
                CreateErrorDisplay();
                yield break;
            }

            SystemLogger.LogInfo("XRTestSceneSetup", $"Found {WebCamTexture.devices.Length} webcam device(s)");
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                SystemLogger.LogInfo("XRTestSceneSetup", $"Device {i}: {WebCamTexture.devices[i].name}");
            }

            // Create webcam texture
            string deviceName = WebCamTexture.devices[0].name;
            SystemLogger.LogInfo("XRTestSceneSetup", $"Creating webcam texture for device: {deviceName} at {webcamWidth}x{webcamHeight}@{webcamFPS}fps");

            try
            {
                webcamTexture = new WebCamTexture(deviceName, webcamWidth, webcamHeight, webcamFPS);
                SystemLogger.LogInfo("XRTestSceneSetup", $"Webcam texture created successfully");

                // Start webcam
                webcamTexture.Play();
                SystemLogger.LogInfo("XRTestSceneSetup", "Webcam playback started");
            }
            catch (System.Exception e)
            {
                SystemLogger.LogError("XRTestSceneSetup", $"Failed to create/start webcam: {e.Message}");
                CreateErrorDisplay();
                yield break;
            }

            // Wait for webcam to initialize
            float timeout = 5f;
            float startTime = Time.time;
            while (webcamTexture.width <= 100 && Time.time - startTime < timeout)
            {
                yield return null;
            }

            if (webcamTexture.width <= 100)
            {
                SystemLogger.LogError("XRTestSceneSetup", $"Webcam initialization timeout! Width: {webcamTexture.width}, IsPlaying: {webcamTexture.isPlaying}");
                CreateErrorDisplay();
                yield break;
            }

            SystemLogger.LogInfo("XRTestSceneSetup", $"Webcam initialized successfully: {deviceName} ({webcamTexture.width}x{webcamTexture.height}), Playing: {webcamTexture.isPlaying}");

            // Ensure webcam display exists
            if (webcamDisplay == null)
            {
                SystemLogger.LogInfo("XRTestSceneSetup", "Creating webcam display...");
                CreateWebcamDisplay();
                yield return null; // Wait one frame for UI to update
            }

            // Assign webcam texture to display
            if (webcamDisplay != null)
            {
                webcamDisplay.texture = webcamTexture;
                webcamDisplay.color = Color.white; // Reset color to show texture
                webcamDisplay.gameObject.SetActive(true);
                SystemLogger.LogInfo("XRTestSceneSetup", $"Webcam texture assigned to display. Texture: {webcamTexture}, Display active: {webcamDisplay.gameObject.activeSelf}");

                // Force refresh the UI
                Canvas.ForceUpdateCanvases();
            }
            else
            {
                SystemLogger.LogError("XRTestSceneSetup", "Webcam display is null after creation attempt!");
            }

            // Pass webcam texture to AI components
            Debug.Log($"XRTestSceneSetup: Sharing webcam texture with AI components - poseEstimator={poseEstimator != null}, handRecognizer={handRecognizer != null}");
            if (poseEstimator != null)
            {
                // Set webcam texture on pose estimator
                poseEstimator.SetWebcamTexture(webcamTexture);
                Debug.Log("XRTestSceneSetup: Webcam texture shared with Pose Estimator");
            }
            else
            {
                Debug.LogError("XRTestSceneSetup: Pose Estimator is null, cannot share webcam texture");
            }

            if (handRecognizer != null)
            {
                // Set webcam texture on hand recognizer
                handRecognizer.SetWebcamTexture(webcamTexture);
                Debug.Log("XRTestSceneSetup: Webcam texture shared with Hand Recognizer");
            }
            else
            {
                Debug.LogError("XRTestSceneSetup: Hand Recognizer is null, cannot share webcam texture");
            }

            SystemLogger.LogInfo("XRTestSceneSetup", "Webcam texture shared with AI components for real-time processing");
        }

        private void CreateWebcamDisplay()
        {
            if (mainCanvas == null)
            {
                SystemLogger.LogError("XRTestSceneSetup", "Cannot create webcam display - main canvas is null!");
                return;
            }

            GameObject webcamObj = new GameObject("Webcam Display");
            webcamObj.transform.SetParent(mainCanvas.transform, false);

            webcamDisplay = webcamObj.AddComponent<RawImage>();
            // Start with a dark gray color as fallback background
            webcamDisplay.color = new Color(0.1f, 0.1f, 0.1f, 1.0f);

            // Make webcam display much larger for XR - nearly full screen
            RectTransform rect = webcamDisplay.rectTransform;
            rect.anchorMin = new Vector2(0.05f, 0.05f); // Small margin from edges
            rect.anchorMax = new Vector2(0.95f, 0.95f); // Nearly full screen
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Send to back so UI overlays appear on top
            webcamObj.transform.SetAsFirstSibling();

            SystemLogger.LogInfo("XRTestSceneSetup", "Created large webcam display for XR view with dark background fallback");
        }

        private void CreateTestIndicator()
        {
            if (mainCanvas == null) return;

            // Create a simple colored rectangle to prove the scene is working
            GameObject indicatorObj = new GameObject("Scene Test Indicator");
            indicatorObj.transform.SetParent(mainCanvas.transform, false);

            var image = indicatorObj.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.red; // Bright red to be clearly visible

            var rect = indicatorObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.8f); // Top-left area
            rect.anchorMax = new Vector2(0.3f, 0.9f); // Small rectangle
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            SystemLogger.LogInfo("XRTestSceneSetup", "Created test indicator - you should see a red rectangle in the top-left");
        }

        private void CreateErrorDisplay()
        {
            if (mainCanvas == null) return;

            GameObject errorObj = new GameObject("Webcam Error Display");
            errorObj.transform.SetParent(mainCanvas.transform, false);

            var errorImage = errorObj.AddComponent<RawImage>();
            errorImage.color = new Color(0.8f, 0.2f, 0.2f, 1.0f); // Red background

            var rect = errorObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.95f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Add text to show error message
            var textObj = new GameObject("Error Text");
            textObj.transform.SetParent(errorObj.transform, false);

            try
            {
                var tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
                tmpText.text = "NO WEBCAM FOUND\n\nMake sure:\n1. Webcam is connected\n2. Webcam permissions are granted\n3. Try refreshing the scene";
                tmpText.fontSize = 32;
                tmpText.alignment = TMPro.TextAlignmentOptions.Center;
                tmpText.color = Color.white;
            }
            catch
            {
                var uiText = textObj.AddComponent<UnityEngine.UI.Text>();
                uiText.text = "NO WEBCAM FOUND\n\nMake sure:\n1. Webcam is connected\n2. Webcam permissions are granted\n3. Try refreshing the scene";
                uiText.fontSize = 32;
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.color = Color.white;
            }

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(50, 50);
            textRect.offsetMax = new Vector2(-50, -50);

            errorObj.transform.SetAsFirstSibling();

            SystemLogger.LogWarning("XRTestSceneSetup", "Created error display - no webcam devices detected");
        }

        private void CreateScreenUIElements(GameObject screenUIObj)
        {
            screenUIObj.transform.SetParent(mainCanvas.transform, false);

            // Create main panel
            GameObject panelObj = new GameObject("Main Panel");
            panelObj.transform.SetParent(screenUIObj.transform, false);

            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.7f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add canvas group for fading
            var canvasGroup = panelObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            // Add background
            var bgImage = panelObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // Create power level text
            GameObject textObj = new GameObject("Power Level Text");
            textObj.transform.SetParent(panelObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 20);
            textRect.offsetMax = new Vector2(-20, -20);

            // Add TextMeshPro if available, otherwise Unity UI Text
            try
            {
                var tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
                tmpText.text = "Power Level: Ready";
                tmpText.fontSize = 36;
                tmpText.alignment = TMPro.TextAlignmentOptions.Center;
                tmpText.color = Color.cyan;
            }
            catch (System.Exception e)
            {
                SystemLogger.LogWarning("XRTestSceneSetup", $"TMP not available, falling back to Unity UI Text: {e.Message}");
                // Fallback to Unity UI Text
                var uiText = textObj.AddComponent<UnityEngine.UI.Text>();
                uiText.text = "Power Level: Ready";
                uiText.fontSize = 36;
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.color = Color.cyan;
            }

            // Assign to ScouterUI
            screenUI.scouterReadout = canvasGroup;
            screenUI.powerLevelText = textRect;

            // Create status indicators
            CreateStatusIndicators(panelObj);
        }

        private void CreateStatusIndicators(GameObject parent)
        {
            // Pose detection status
            GameObject poseStatusObj = new GameObject("Pose Status");
            poseStatusObj.transform.SetParent(parent.transform, false);

            var poseRect = poseStatusObj.AddComponent<RectTransform>();
            poseRect.anchorMin = new Vector2(0.05f, 0.1f);
            poseRect.anchorMax = new Vector2(0.3f, 0.3f);
            poseRect.offsetMin = Vector2.zero;
            poseRect.offsetMax = Vector2.zero;

            try
            {
                var tmpText = poseStatusObj.AddComponent<TMPro.TextMeshProUGUI>();
                tmpText.text = "Pose: --";
                tmpText.fontSize = 18;
                tmpText.color = Color.yellow;
                poseStatusRect = poseRect;
            }
            catch (System.Exception)
            {
                var uiText = poseStatusObj.AddComponent<UnityEngine.UI.Text>();
                uiText.text = "Pose: --";
                uiText.fontSize = 18;
                uiText.color = Color.yellow;
                poseStatusRect = poseRect;
            }

            // Hand detection status
            GameObject handStatusObj = new GameObject("Hand Status");
            handStatusObj.transform.SetParent(parent.transform, false);

            var handRect = handStatusObj.AddComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.35f, 0.1f);
            handRect.anchorMax = new Vector2(0.6f, 0.3f);
            handRect.offsetMin = Vector2.zero;
            handRect.offsetMax = Vector2.zero;

            try
            {
                var tmpText = handStatusObj.AddComponent<TMPro.TextMeshProUGUI>();
                tmpText.text = "Hand: --";
                tmpText.fontSize = 18;
                tmpText.color = Color.yellow;
                handStatusRect = handRect;
            }
            catch (System.Exception)
            {
                var uiText = handStatusObj.AddComponent<UnityEngine.UI.Text>();
                uiText.text = "Hand: --";
                uiText.fontSize = 18;
                uiText.color = Color.yellow;
                handStatusRect = handRect;
            }

            // Scanning status
            GameObject scanStatusObj = new GameObject("Scan Status");
            scanStatusObj.transform.SetParent(parent.transform, false);

            var scanRect = scanStatusObj.AddComponent<RectTransform>();
            scanRect.anchorMin = new Vector2(0.65f, 0.1f);
            scanRect.anchorMax = new Vector2(0.95f, 0.3f);
            scanRect.offsetMin = Vector2.zero;
            scanRect.offsetMax = Vector2.zero;

            try
            {
                var tmpText = scanStatusObj.AddComponent<TMPro.TextMeshProUGUI>();
                tmpText.text = "Scan: Ready";
                tmpText.fontSize = 18;
                tmpText.color = Color.green;
                scanStatusRect = scanRect;
            }
            catch (System.Exception)
            {
                var uiText = scanStatusObj.AddComponent<UnityEngine.UI.Text>();
                uiText.text = "Scan: Ready";
                uiText.fontSize = 18;
                uiText.color = Color.green;
                scanStatusRect = scanRect;
            }
        }

        private void InitializeMVPUI()
        {
            // Connect all components together
            if (xrManager != null)
            {
                xrManager.handRecognizer = handRecognizer;
                xrManager.scouterUI = screenUI;
                xrManager.spatialUIManager = spatialUI;
                xrManager.audioController = audioController;
                SystemLogger.LogInfo("XRTestSceneSetup", "Connected AI components to XR Manager");
            }

            // Connect AI components to UI elements
            if (poseEstimator != null)
            {
                poseEstimator.scouterUI = screenUI;
                poseEstimator.haloColorController = FindFirstObjectByType<HaloColorController>();
                Debug.Log("XRTestSceneSetup: Connected poseEstimator to UI components");
                screenUI.ShowScouter(); // Show the scouter UI initially
            }

            if (handRecognizer != null)
            {
                // Hand recognizer doesn't have direct UI connections, but ensure it's connected to xrManager
                Debug.Log("XRTestSceneSetup: Hand recognizer connected to XR Manager");
            }

            // Connect pose estimator to debug overlay
            if (debugOverlay != null && poseEstimator != null)
            {
                debugOverlay.poseEstimator = poseEstimator;
                debugOverlay.handRecognizer = handRecognizer;
                SystemLogger.LogInfo("XRTestSceneSetup", "Connected AI components to debug overlay");
            }

            if (spatialUI != null)
            {
                spatialUI.screenUI = screenUI;
                spatialUI.scouterManager = xrManager;
            }

            // Start status updates (only if UI components are ready)
            if (screenUI != null && poseStatusRect != null && handStatusRect != null && scanStatusRect != null)
            {
                StartCoroutine(UpdateUIStatus());
            }
            else
            {
                SystemLogger.LogWarning("XRTestSceneSetup", "UI components not fully initialized, skipping status updates");
            }
        }

        private System.Collections.IEnumerator UpdateUIStatus()
        {
            while (true)
            {
                UpdateStatusIndicators();
                yield return new WaitForSeconds(1f / uiUpdateRate);
            }
        }

        private void UpdateStatusIndicators()
        {
            // Update pose status
            if (poseEstimator != null && poseStatusRect != null)
            {
                // Add null check for currentPose
                bool poseValid = poseEstimator.currentPose != null && poseEstimator.currentPose.IsValid;
                string poseText = $"Pose: {(poseValid ? "OK" : "--")}";

                // Get text component and update it
                var tmpComponent = poseStatusRect.GetComponent<TMPro.TextMeshProUGUI>();
                var uiComponent = poseStatusRect.GetComponent<UnityEngine.UI.Text>();
                if (tmpComponent != null)
                {
                    tmpComponent.text = poseText;
                    tmpComponent.color = poseValid ? Color.green : Color.red;
                }
                else if (uiComponent != null)
                {
                    uiComponent.text = poseText;
                    uiComponent.color = poseValid ? Color.green : Color.red;
                }

                if (poseValid && screenUI != null)
                {
                    try
                    {
                        float powerLevel = poseEstimator.GetPowerLevelFromCurrentPose();
                        screenUI.SetPowerLevel(powerLevel);
                    }
                    catch (System.Exception e)
                    {
                        SystemLogger.LogError("XRTestSceneSetup", $"Error updating power level: {e.Message}");
                    }
                }
            }

            // Update hand status
            if (handRecognizer != null && handStatusRect != null)
            {
                bool handDetected = handRecognizer.IsHandDetected();
                bool pointing = handRecognizer.IsPointingGestureActive();
                string handText = $"Hand: {(handDetected ? (pointing ? "POINTING" : "OK") : "--")}";

                var tmpComponent = handStatusRect.GetComponent<TMPro.TextMeshProUGUI>();
                var uiComponent = handStatusRect.GetComponent<UnityEngine.UI.Text>();
                if (tmpComponent != null)
                {
                    tmpComponent.text = handText;
                    tmpComponent.color = handDetected ? (pointing ? Color.blue : Color.green) : Color.red;
                }
                else if (uiComponent != null)
                {
                    uiComponent.text = handText;
                    uiComponent.color = handDetected ? (pointing ? Color.blue : Color.green) : Color.red;
                }
            }

            // Update scan status
            if (xrManager != null && scanStatusRect != null)
            {
                bool isScanning = xrManager.IsScanning();
                float progress = xrManager.GetScanProgress();
                string scanText = isScanning ? $"Scan: {progress:P0}" : "Scan: Ready";

                var tmpComponent = scanStatusRect.GetComponent<TMPro.TextMeshProUGUI>();
                var uiComponent = scanStatusRect.GetComponent<UnityEngine.UI.Text>();
                if (tmpComponent != null)
                {
                    tmpComponent.text = scanText;
                    tmpComponent.color = isScanning ? Color.yellow : Color.green;
                }
                else if (uiComponent != null)
                {
                    uiComponent.text = scanText;
                    uiComponent.color = isScanning ? Color.yellow : Color.green;
                }
            }
        }

        // Public methods for testing
        public void StartTestScan()
        {
            if (xrManager != null && handRecognizer != null && handRecognizer.IsPointingGestureActive())
            {
                // Simulate pointing gesture detection
                Vector3 pointingDir = handRecognizer.GetPointingDirection();
                Vector3 tipPos = handRecognizer.GetPointingTipPosition();
                xrManager.OnPointingGestureDetected(pointingDir, tipPos);
            }
            else
            {
                SystemLogger.LogWarning("XRTestSceneSetup", "Cannot start scan - no active pointing gesture");
            }
        }

        public void StopTestScan()
        {
            if (xrManager != null)
            {
                xrManager.OnPointingGestureLost();
            }
        }

        public void ToggleWebcamDisplay()
        {
            if (webcamDisplay != null)
            {
                webcamDisplay.gameObject.SetActive(!webcamDisplay.gameObject.activeSelf);
            }
        }

        public void ToggleDebugOverlay()
        {
            if (debugOverlay != null)
            {
                debugOverlay.gameObject.SetActive(!debugOverlay.gameObject.activeSelf);
            }
        }

        public void RestartWebcam()
        {
            SystemLogger.LogInfo("XRTestSceneSetup", "Restarting webcam...");

            // Stop current webcam
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
            }

            // Restart initialization
            StopCoroutine(InitializeWebcam());
            StartCoroutine(InitializeWebcam());
        }

        public void ValidateAIComponents()
        {
            SystemLogger.LogInfo("XRTestSceneSetup", "VALIDATION REPORT - AI Components Status:");

            // Check Pose Estimator
            if (poseEstimator != null)
            {
                bool hasPose = poseEstimator.currentPose != null;
                bool poseValid = hasPose && poseEstimator.currentPose.IsValid;
                SystemLogger.LogInfo("XRTestSceneSetup", $"POSE ESTIMATOR: Active, HasPose={hasPose}, Valid={poseValid}, RealDetection={poseEstimator.useRealPoseDetection}");
            }
            else
            {
                SystemLogger.LogError("XRTestSceneSetup", "POSE ESTIMATOR: NULL - Not created!");
            }

            // Check Hand Recognizer
            if (handRecognizer != null)
            {
                bool initialized = handRecognizer.GetType().GetField("isInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(handRecognizer) as bool? ?? false;
                bool handDetected = handRecognizer.IsHandDetected();
                bool pointing = handRecognizer.IsPointingGestureActive();
                SystemLogger.LogInfo("XRTestSceneSetup", $"HAND RECOGNIZER: Active, Initialized={initialized}, HandDetected={handDetected}, Pointing={pointing}");
            }
            else
            {
                SystemLogger.LogError("XRTestSceneSetup", "HAND RECOGNIZER: NULL - Not created!");
            }

            // Check Webcam
            if (webcamTexture != null)
            {
                SystemLogger.LogInfo("XRTestSceneSetup", $"WEBCAM: Active, Playing={webcamTexture.isPlaying}, Size={webcamTexture.width}x{webcamTexture.height}");
            }
            else
            {
                SystemLogger.LogError("XRTestSceneSetup", "WEBCAM: NULL - Not initialized!");
            }

            // Check Model Manager
            var modelManager = FindFirstObjectByType(typeof(MediaPipeModelManager)) as MediaPipeModelManager;
            if (modelManager != null)
            {
                SystemLogger.LogInfo("XRTestSceneSetup", "MODEL MANAGER: Active");
            }
            else
            {
                SystemLogger.LogError("XRTestSceneSetup", "MODEL MANAGER: NULL - Not found!");
            }

            SystemLogger.LogInfo("XRTestSceneSetup", "VALIDATION COMPLETE - Check logs above for issues");
        }

        private void CreatePoseVisualizer()
        {
            Debug.Log("XRTestSceneSetup: Creating Pose Visualizer");

            // Create a GameObject for pose visualization
            GameObject visualizerObj = new GameObject("Pose Visualizer");
            var poseVisualizer = visualizerObj.AddComponent<PoseVisualizer>();
            poseVisualizer.poseEstimator = poseEstimator;
            poseVisualizer.handRecognizer = handRecognizer;
            poseVisualizer.mainCamera = mainCamera;

            Debug.Log("XRTestSceneSetup: Pose Visualizer created and connected to AI components");
        }

        void OnDestroy()
        {
            // Cleanup webcam
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
            }
        }
    }
}
