using UnityEngine;
using UnityEngine.XR.ARFoundation;
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

        void Awake()
        {
            SetupTestScene();
        }

        void Start()
        {
            StartCoroutine(InitializeWebcam());
            InitializeMVPUI();
            SystemLogger.LogInfo("XRTestSceneSetup", "Test scene initialization complete");
        }

        private void SetupTestScene()
        {
            SystemLogger.LogInfo("XRTestSceneSetup", "Setting up XR HUD test scene...");

            // Create camera if needed
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Test Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = Color.black;
            }

            // Create canvas for UI
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("Test Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Setup core XR components
            SetupXRComponents();

            // Setup AI components
            SetupAIComponents();

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
            // Create AR Session Origin for compatibility
            if (FindObjectOfType<ARSessionOrigin>() == null)
            {
                GameObject sessionOriginObj = new GameObject("AR Session Origin");
                var sessionOrigin = sessionOriginObj.AddComponent<ARSessionOrigin>();
                sessionOrigin.camera = mainCamera;
            }

            // Create XR Scouter Manager
            GameObject xrObj = new GameObject("XR Scouter Manager");
            xrManager = xrObj.AddComponent<XRScouterManager>();
            xrManager.arCamera = mainCamera;
            xrManager.scanDuration = 2f; // Shorter for testing
            xrManager.maxScanDistance = 5f;
        }

        private void SetupAIComponents()
        {
            // Create MediaPipe Model Manager
            GameObject modelObj = new GameObject("MediaPipe Model Manager");
            var modelManager = modelObj.AddComponent<MediaPipeModelManager>();

            // Create Pose Estimator
            if (enablePoseEstimation)
            {
                GameObject poseObj = new GameObject("Pose Estimator");
                poseEstimator = poseObj.AddComponent<MediaPipePoseEstimator>();
                poseEstimator.arCamera = mainCamera;
                poseEstimator.modelManager = modelManager;
                poseEstimator.useRealPoseDetection = false; // Use mock for testing
                poseEstimator.debugMode = true;
            }

            // Create Hand Pointing Recognizer
            if (enableHandRecognition)
            {
                GameObject handObj = new GameObject("Hand Pointing Recognizer");
                handRecognizer = handObj.AddComponent<HandPointingRecognizer>();
                handRecognizer.arCamera = mainCamera;
                handRecognizer.enableHandTracking = true;
                handRecognizer.targetWidth = webcamWidth;
                handRecognizer.targetHeight = webcamHeight;
                handRecognizer.inferenceInterval = 1f / uiUpdateRate;
                handRecognizer.debugMode = true;
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
            }
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
            if (!useWebcamFeed) yield break;

            // Check for webcam devices
            if (WebCamTexture.devices.Length == 0)
            {
                SystemLogger.LogError("XRTestSceneSetup", "No webcam devices found!");
                yield break;
            }

            // Create webcam texture
            string deviceName = WebCamTexture.devices[0].name;
            webcamTexture = new WebCamTexture(deviceName, webcamWidth, webcamHeight, webcamFPS);

            // Start webcam
            webcamTexture.Play();

            // Wait for webcam to initialize
            yield return new WaitUntil(() => webcamTexture.width > 100);

            SystemLogger.LogInfo("XRTestSceneSetup", $"Webcam initialized: {deviceName} ({webcamTexture.width}x{webcamTexture.height})");

            // Create webcam display if needed
            if (webcamDisplay == null)
            {
                CreateWebcamDisplay();
            }

            // Assign webcam texture to display
            if (webcamDisplay != null)
            {
                webcamDisplay.texture = webcamTexture;
            }

            // Pass webcam texture to AI components
            if (handRecognizer != null)
            {
                // Note: HandPointingRecognizer creates its own webcam texture
                // We could pass this one if we modify the component
            }
        }

        private void CreateWebcamDisplay()
        {
            if (mainCanvas == null) return;

            GameObject webcamObj = new GameObject("Webcam Display");
            webcamObj.transform.SetParent(mainCanvas.transform, false);

            webcamDisplay = webcamObj.AddComponent<RawImage>();
            webcamDisplay.color = Color.white;

            // Position in bottom-right corner
            RectTransform rect = webcamDisplay.rectTransform;
            rect.anchorMin = new Vector2(0.7f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.3f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Add border
            var border = webcamObj.AddComponent<Outline>();
            border.effectColor = Color.green;
            border.effectDistance = new Vector2(2, 2);
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
            var tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmpText.text = "Power Level: Ready";
            tmpText.fontSize = 36;
            tmpText.alignment = TMPro.TextAlignmentOptions.Center;
            tmpText.color = Color.cyan;

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

            var poseText = poseStatusObj.AddComponent<TMPro.TextMeshProUGUI>();
            poseText.text = "Pose: --";
            poseText.fontSize = 18;
            poseText.color = Color.yellow;

            // Hand detection status
            GameObject handStatusObj = new GameObject("Hand Status");
            handStatusObj.transform.SetParent(parent.transform, false);

            var handRect = handStatusObj.AddComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.35f, 0.1f);
            handRect.anchorMax = new Vector2(0.6f, 0.3f);
            handRect.offsetMin = Vector2.zero;
            handRect.offsetMax = Vector2.zero;

            var handText = handStatusObj.AddComponent<TMPro.TextMeshProUGUI>();
            handText.text = "Hand: --";
            handText.fontSize = 18;
            handText.color = Color.yellow;

            // Scanning status
            GameObject scanStatusObj = new GameObject("Scan Status");
            scanStatusObj.transform.SetParent(parent.transform, false);

            var scanRect = scanStatusObj.AddComponent<RectTransform>();
            scanRect.anchorMin = new Vector2(0.65f, 0.1f);
            scanRect.anchorMax = new Vector2(0.95f, 0.3f);
            scanRect.offsetMin = Vector2.zero;
            scanRect.offsetMax = Vector2.zero;

            var scanText = scanStatusObj.AddComponent<TMPro.TextMeshProUGUI>();
            scanText.text = "Scan: Ready";
            scanText.fontSize = 18;
            scanText.color = Color.green;
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
            }

            if (spatialUI != null)
            {
                spatialUI.screenUI = screenUI;
                spatialUI.scouterManager = xrManager;
            }

            // Start status updates
            StartCoroutine(UpdateUIStatus());
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
            if (poseEstimator != null)
            {
                var poseText = GameObject.Find("Pose Status")?.GetComponent<TMPro.TextMeshProUGUI>();
                if (poseText != null)
                {
                    bool poseValid = poseEstimator.currentPose.IsValid;
                    poseText.text = $"Pose: {(poseValid ? "OK" : "--")}";
                    poseText.color = poseValid ? Color.green : Color.red;

                    if (poseValid && screenUI != null)
                    {
                        float powerLevel = poseEstimator.GetPowerLevelFromCurrentPose();
                        screenUI.SetPowerLevel(powerLevel);
                    }
                }
            }

            // Update hand status
            if (handRecognizer != null)
            {
                var handText = GameObject.Find("Hand Status")?.GetComponent<TMPro.TextMeshProUGUI>();
                if (handText != null)
                {
                    bool handDetected = handRecognizer.IsHandDetected();
                    bool pointing = handRecognizer.IsPointingGestureActive();
                    handText.text = $"Hand: {(handDetected ? (pointing ? "POINTING" : "OK") : "--")}";
                    handText.color = handDetected ? (pointing ? Color.blue : Color.green) : Color.red;
                }
            }

            // Update scan status
            if (xrManager != null)
            {
                var scanText = GameObject.Find("Scan Status")?.GetComponent<TMPro.TextMeshProUGUI>();
                if (scanText != null)
                {
                    bool isScanning = xrManager.IsScanning();
                    float progress = xrManager.GetScanProgress();
                    scanText.text = isScanning ? $"Scan: {progress:P0}" : "Scan: Ready";
                    scanText.color = isScanning ? Color.yellow : Color.green;
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
