using ScouterXR.AI;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

namespace ScouterXR.Core
{
    /// <summary>
    /// Minimal scene bootstrapper that ensures an XR origin, pose estimator, and hand recognizer exist.
    /// Modular, independent setup that auto-configures all core AI and XR components.
    /// </summary>
    public sealed class XRTestSceneSetup : MonoBehaviour
    {
        [Header("Scene Configuration")]
        public bool autoSetupOnAwake = true;
        public bool useWebcamFeed = true;
        public bool enablePoseEstimation = true;
        public bool enableHandRecognition = true;

        [Header("Webcam Settings")]
        public int webcamWidth = 640;
        public int webcamHeight = 480;
        public int webcamFPS = 30;

        [Header("References")]
        public Camera mainCamera;
        public MediaPipeModelManager modelManager;
        public TFLiteModelRunner tfliteRunner;
        public MediaPipePoseEstimator poseEstimator;
        public TensorFlowLiteInference handInference;
        public HandPointingRecognizer handRecognizer;
        public RawImage webcamDisplay;
        public Canvas mainCanvas;

        // Component references
        private WebCamTexture webcamTexture;

        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                // Basic setup only
                EnsureMainCamera();
                SetupXROrigin();
                SetupCanvas();
            }
        }

        private void Start()
        {
            if (autoSetupOnAwake)
            {
                StartCoroutine(SetupTestSceneAsync());
            }
            
            if (useWebcamFeed)
            {
                StartCoroutine(InitializeWebcam());
            }
        }

        private System.Collections.IEnumerator SetupTestSceneAsync()
        {
            Debug.Log("[XRTestSceneSetup] Setting up XR test scene with ML components...");
            
            // Wait one frame for scene to stabilize
            yield return null;
            
            // Create ModelManager
            SetupAIComponents();
            
            // Wait for ModelManager to fully load models
            yield return StartCoroutine(WaitForModelManager());
            
            // Now create remaining AI components that depend on ModelManager
            // (WaitForModelManager already calls SetupRemainingComponents)
            
            // Wait a bit more for components to initialize
            yield return new WaitForSeconds(0.3f);
            
            // Setup visualizer last
            SetupPoseVisualizer();
            
            Debug.Log("[XRTestSceneSetup] XR test scene setup complete - Core pipeline: video stream → MediaPipe → pose visualization");
        }

        public void SetupTestScene()
        {
            Debug.Log("[XRTestSceneSetup] Setting up XR test scene (legacy sync method)...");
            
            EnsureMainCamera();
            SetupXROrigin();
            SetupCanvas();
            
            Debug.Log("[XRTestSceneSetup] Use autoSetupOnAwake for full async initialization");
        }

        private void EnsureMainCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                var cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
                Debug.Log("[XRTestSceneSetup] Created new Main Camera");
            }
        }

        private void SetupXROrigin()
        {
            var existingOrigin = FindFirstObjectByType<XROrigin>();
            if (existingOrigin != null)
            {
                Debug.Log("[XRTestSceneSetup] XR Origin already exists");
                return;
            }

            GameObject originObj = new GameObject("XR Origin");
            
            // Add TrackedPoseDriver to camera BEFORE creating AutoXROrigin
            // This prevents the warning from XROrigin.Awake()
#if ENABLE_INPUT_SYSTEM
            if (mainCamera != null && mainCamera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>() == null)
            {
                var driver = mainCamera.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                driver.trackingType = UnityEngine.InputSystem.XR.TrackedPoseDriver.TrackingType.RotationAndPosition;
                driver.updateType = UnityEngine.InputSystem.XR.TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
                Debug.Log("[XRTestSceneSetup] Added TrackedPoseDriver to Main Camera");
            }
#endif
            
            var autoOrigin = originObj.AddComponent<AutoXROrigin>();
            autoOrigin.Camera = mainCamera;
            
            Debug.Log("[XRTestSceneSetup] Created XR Origin with auto-configured camera");
        }

        private void SetupAIComponents()
        {
            // Create MediaPipe Model Manager FIRST
            if (modelManager == null)
            {
                var modelManagerObj = new GameObject("MediaPipe Model Manager");
                modelManager = modelManagerObj.AddComponent<MediaPipeModelManager>();
                Debug.Log("[XRTestSceneSetup] Created MediaPipe Model Manager - waiting for models to load...");

                // Wait for ModelManager's Awake to complete before creating dependent components
                // This is called from SetupTestSceneAsync which already handles timing
            }
        }

        private System.Collections.IEnumerator WaitForModelManager()
        {
            // Wait for ModelManager to be created
            int maxWait = 60; // 3 seconds max
            int waited = 0;
            
            while (modelManager == null && waited < maxWait)
            {
                yield return new WaitForSeconds(0.05f);
                waited++;
            }
            
            if (modelManager == null)
            {
                Debug.LogError("[XRTestSceneSetup] ModelManager failed to initialize after 3 seconds");
                yield break;
            }
            
            // Now wait for MODELS TO ACTUALLY LOAD
            Debug.Log("[XRTestSceneSetup] ModelManager exists, waiting for models to load...");
            waited = 0;
            maxWait = 100; // 5 seconds max for model loading

            while (!modelManager.ModelsLoaded && waited < maxWait)
            {
                yield return new WaitForSeconds(0.05f);
                waited++;
            }

            if (!modelManager.ModelsLoaded)
            {
                Debug.LogError("[XRTestSceneSetup] Models failed to load after 5 seconds - cannot continue AI setup");
                Debug.LogError("[XRTestSceneSetup] Please check that model files exist in Assets/StreamingAssets/Models/ or Assets/Resources/Models/");
                yield break;
            }
            
            Debug.Log("[XRTestSceneSetup] Models loaded successfully, creating AI components...");
            SetupRemainingComponents();
        }

        private void SetupRemainingComponents()
        {
            if (modelManager == null)
            {
                Debug.LogError("[XRTestSceneSetup] ModelManager is still null - cannot continue AI setup");
                return;
            }

            // Create TFLite Runner (create inactive, configure, then activate)
            if (tfliteRunner == null)
            {
                var runnerObj = new GameObject("TFLite Runner");
                runnerObj.SetActive(false); // Prevent Start() from running
                
                tfliteRunner = runnerObj.AddComponent<TFLiteModelRunner>();
                tfliteRunner.modelManager = modelManager; // Set BEFORE activating
                tfliteRunner.debugMode = true;
                
                runnerObj.SetActive(true); // Now Start() will run with modelManager set
                Debug.Log("[XRTestSceneSetup] Created TFLite Runner with ModelManager assigned");
            }

            // Create Pose Estimator (create inactive, configure, then activate)
            if (enablePoseEstimation && poseEstimator == null)
            {
                var poseObj = new GameObject("MediaPipe Pose Estimator");
                poseObj.SetActive(false); // Prevent Start() from running
                
                poseEstimator = poseObj.AddComponent<MediaPipePoseEstimator>();
                poseEstimator.arCamera = mainCamera;
                poseEstimator.modelManager = modelManager;
                poseEstimator.debugMode = true;
                
                poseObj.SetActive(true); // Now components can initialize properly
                Debug.Log("[XRTestSceneSetup] Created MediaPipe Pose Estimator with dependencies");
            }

            // Create Hand Inference Engine (create inactive, configure, then activate)
            if (enableHandRecognition && handInference == null)
            {
                var inferenceObj = new GameObject("Hand Inference");
                inferenceObj.SetActive(false);
                
                handInference = inferenceObj.AddComponent<TensorFlowLiteInference>();
                handInference.modelManager = modelManager;
                handInference.debugMode = true;
                
                inferenceObj.SetActive(true);
                Debug.Log("[XRTestSceneSetup] Created TensorFlow Lite Hand Inference");
            }

            // Create Hand Pointing Recognizer (create inactive, configure, then activate)
            if (enableHandRecognition && handRecognizer == null)
            {
                var handObj = new GameObject("Hand Pointing Recognizer");
                handObj.SetActive(false);
                
                handRecognizer = handObj.AddComponent<HandPointingRecognizer>();
                handRecognizer.poseEstimator = poseEstimator;
                
                // Attach inference engine for hand model
                var inference = handObj.AddComponent<TensorFlowLiteInference>();
                inference.modelManager = modelManager;
                inference.debugMode = true;
                
                handObj.SetActive(true);
                Debug.Log("[XRTestSceneSetup] Created Hand Pointing Recognizer");
            }
        }

        private void SetupCanvas()
        {
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("Main Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);

                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[XRTestSceneSetup] Created main canvas");
            }
        }

        private System.Collections.IEnumerator InitializeWebcam()
        {
            Debug.Log("[XRTestSceneSetup] Initializing webcam...");

            // Check for webcam devices
            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogError("[XRTestSceneSetup] No webcam devices found!");
                yield break;
            }

            Debug.Log($"[XRTestSceneSetup] Found {WebCamTexture.devices.Length} webcam device(s)");

            // Create webcam texture
            string deviceName = WebCamTexture.devices[0].name;
                webcamTexture = new WebCamTexture(deviceName, webcamWidth, webcamHeight, webcamFPS);
                webcamTexture.Play();

            // Wait for webcam to initialize
            float timeout = 5f;
            float startTime = Time.time;
            while (webcamTexture.width <= 100 && Time.time - startTime < timeout)
            {
                yield return null;
            }

            if (webcamTexture.width <= 100)
            {
                Debug.LogError("[XRTestSceneSetup] Webcam initialization timeout!");
                yield break;
            }

            Debug.Log($"[XRTestSceneSetup] Webcam initialized: {webcamTexture.width}x{webcamTexture.height}");

            // Create webcam display if needed
            if (webcamDisplay == null && mainCanvas != null)
            {
                CreateWebcamDisplay();
            }

            // Assign webcam texture to display
            if (webcamDisplay != null)
            {
                webcamDisplay.texture = webcamTexture;
                webcamDisplay.color = Color.white;
                webcamDisplay.gameObject.SetActive(true);
                Debug.Log("[XRTestSceneSetup] Webcam texture assigned to display");
            }

            // Share webcam texture with AI components
            // Note: MediaPipePoseEstimator and HandPointingRecognizer manage their own webcams
            // Remove SetWebcamTexture calls as these methods don't exist
            Debug.Log("[XRTestSceneSetup] Webcam initialized and ready for AI components");
        }

        private void CreateWebcamDisplay()
        {
            if (mainCanvas == null)
            {
                Debug.LogError("[XRTestSceneSetup] Cannot create webcam display - main canvas is null!");
                return;
            }

            GameObject webcamObj = new GameObject("Webcam Display");
            webcamObj.transform.SetParent(mainCanvas.transform, false);

            webcamDisplay = webcamObj.AddComponent<RawImage>();
            webcamDisplay.color = new Color(0.1f, 0.1f, 0.1f, 1.0f);

            // Full screen display
            RectTransform rect = webcamDisplay.rectTransform;
            rect.anchorMin = new Vector2(0.05f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.95f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Send to back
            webcamObj.transform.SetAsFirstSibling();
            webcamObj.SetActive(false);

            Debug.Log("[XRTestSceneSetup] Created webcam display");
        }

        private void SetupPoseVisualizer()
        {
            if (poseEstimator == null || mainCanvas == null)
            {
                Debug.LogWarning("[XRTestSceneSetup] Cannot create pose visualizer - missing dependencies");
                return;
            }

            GameObject visualizerObj = new GameObject("Enhanced Pose Visualizer");
            var visualizer = visualizerObj.AddComponent<EnhancedPoseVisualizer>();
            
            // Setup target tracking (the person being scanned)
            visualizer.targetPoseEstimator = poseEstimator;
            visualizer.overlayCanvas = mainCanvas;
            visualizer.webcamDisplay = webcamDisplay;
            
            // Target visualization settings (RED for target)
            visualizer.showTargetPose = true;
            visualizer.targetLandmarkColor = Color.red;
            visualizer.targetBoneColor = Color.yellow;
            visualizer.targetBoxColor = Color.green;
            visualizer.targetLandmarkSize = 10f;
            visualizer.targetBoneWidth = 3f;
            visualizer.targetBoxWidth = 4f;
            
            // User hand tracking (CYAN for user)
            visualizer.showUserHands = true;
            visualizer.userHandColor = Color.cyan;
            visualizer.userHandSize = 8f;
            
            // Find and link hand tracker
            var handRecognizer = FindFirstObjectByType<HandPointingRecognizer>();
            if (handRecognizer != null)
            {
                visualizer.userHandTracker = handRecognizer;
            }
            
            // Power level scoring
            visualizer.showPowerLevel = true;
            visualizer.scoreUpdateInterval = 0.1f;
            
            visualizer.updateEveryNFrames = 1; // Update every frame for smooth visualization

            Debug.Log("[XRTestSceneSetup] Created EnhancedPoseVisualizer:");
            Debug.Log("  - Target pose tracking with bounding box + skeleton bones (RED/YELLOW/GREEN)");
            Debug.Log("  - User hand tracking (CYAN)");
            Debug.Log("  - Power level scoring system (Scouter-style readings)");
        }

        private void OnDestroy()
        {
            // Cleanup webcam
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
            }
        }
    }
}
