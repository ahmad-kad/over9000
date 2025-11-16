using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using ScouterXR.AI;
using System.Collections;

namespace ScouterXR.Core
{
    /// <summary>
    /// Single source of truth for scene setup.
    /// Ensures NO duplicate components and proper initialization order.
    /// </summary>
    public sealed class UnifiedSceneSetup : MonoBehaviour
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

        [Header("Singleton Check")]
        private static UnifiedSceneSetup instance;

        // Component references
        private Camera mainCamera;
        private MediaPipeModelManager modelManager;
        private TFLiteModelRunner tfliteRunner;
        private MediaPipePoseEstimator poseEstimator;
        private HandPointingRecognizer handRecognizer;
        private Canvas mainCanvas;

        private void Awake()
        {
            // SINGLETON: Ensure only ONE instance exists
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[UnifiedSceneSetup] Another instance already exists - destroying duplicate");
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (autoSetupOnAwake)
            {
                Debug.Log("[UnifiedSceneSetup] Starting scene setup...");
                SetupCamera();
                SetupXROrigin();
                SetupCanvas();
            }
        }

        private void Start()
        {
            if (autoSetupOnAwake)
            {
                StartCoroutine(SetupAIComponentsAsync());
            }
        }

        private void SetupCamera()
        {
            mainCamera = FindFirstObjectByType<Camera>();
            if (mainCamera == null)
            {
                var cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = Color.black;
                mainCamera.tag = "MainCamera";
                cameraObj.AddComponent<AudioListener>();
                Debug.Log("[UnifiedSceneSetup] Created Main Camera");
            }
        }

        private void SetupXROrigin()
        {
            var existingOrigin = FindFirstObjectByType<XROrigin>();
            if (existingOrigin != null)
            {
                Debug.Log("[UnifiedSceneSetup] XR Origin already exists");
                return;
            }

            // Add TrackedPoseDriver to camera BEFORE creating AutoXROrigin
#if ENABLE_INPUT_SYSTEM
            if (mainCamera != null && mainCamera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>() == null)
            {
                var driver = mainCamera.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                driver.trackingType = UnityEngine.InputSystem.XR.TrackedPoseDriver.TrackingType.RotationAndPosition;
                driver.updateType = UnityEngine.InputSystem.XR.TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
                Debug.Log("[UnifiedSceneSetup] Added TrackedPoseDriver to Main Camera");
            }
#endif

            GameObject originObj = new GameObject("XR Origin");
            var autoOrigin = originObj.AddComponent<AutoXROrigin>();
            autoOrigin.Camera = mainCamera;

            Debug.Log("[UnifiedSceneSetup] Created XR Origin with camera");
        }

        private void SetupCanvas()
        {
            mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                var canvasObj = new GameObject("Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // Create webcam display
                CreateWebcamDisplay();
                
                Debug.Log("[UnifiedSceneSetup] Created Canvas with webcam display");
            }
        }
        
        private void CreateWebcamDisplay()
        {
            GameObject webcamObj = new GameObject("Webcam Display");
            webcamObj.transform.SetParent(mainCanvas.transform, false);
            
            var rawImage = webcamObj.AddComponent<RawImage>();
            rawImage.color = Color.white;
            
            // Full screen display behind UI elements
            RectTransform rect = rawImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Set as first child so it renders behind everything
            webcamObj.transform.SetAsFirstSibling();
            
            Debug.Log("[UnifiedSceneSetup] Created Webcam Display");
        }

        private IEnumerator SetupAIComponentsAsync()
        {
            Debug.Log("[UnifiedSceneSetup] Setting up AI components...");

            // Wait one frame for scene to stabilize
            yield return null;

            // STEP 1: Create ModelManager FIRST
            if (FindFirstObjectByType<MediaPipeModelManager>() == null)
            {
                var modelManagerObj = new GameObject("MediaPipe Model Manager");
                modelManager = modelManagerObj.AddComponent<MediaPipeModelManager>();
                Debug.Log("[UnifiedSceneSetup] Created MediaPipe Model Manager");
            }
            else
            {
                modelManager = FindFirstObjectByType<MediaPipeModelManager>();
                Debug.Log("[UnifiedSceneSetup] Found existing MediaPipe Model Manager");
            }

            // STEP 2: Wait for models to load
            yield return StartCoroutine(WaitForModelsToLoad());

            // STEP 3: Create TFLiteModelRunner
            if (FindFirstObjectByType<TFLiteModelRunner>() == null)
            {
                var runnerObj = new GameObject("TFLite Runner");
                runnerObj.SetActive(false);

                tfliteRunner = runnerObj.AddComponent<TFLiteModelRunner>();
                tfliteRunner.modelManager = modelManager;
                tfliteRunner.debugMode = true;

                runnerObj.SetActive(true);
                Debug.Log("[UnifiedSceneSetup] Created TFLite Runner");
            }
            else
            {
                tfliteRunner = FindFirstObjectByType<TFLiteModelRunner>();
                tfliteRunner.modelManager = modelManager;
                Debug.Log("[UnifiedSceneSetup] Found existing TFLite Runner - updated modelManager reference");
            }

            // STEP 4: Create MediaPipePoseEstimator
            if (enablePoseEstimation && FindFirstObjectByType<MediaPipePoseEstimator>() == null)
            {
                var poseObj = new GameObject("MediaPipe Pose Estimator");
                poseObj.SetActive(false);

                poseEstimator = poseObj.AddComponent<MediaPipePoseEstimator>();
                poseEstimator.arCamera = mainCamera;
                poseEstimator.modelManager = modelManager;
                poseEstimator.tfliteRunner = tfliteRunner; // Explicitly set reference
                poseEstimator.debugMode = true;
                poseEstimator.useWebcamFeed = useWebcamFeed;
                poseEstimator.webcamWidth = webcamWidth;
                poseEstimator.webcamHeight = webcamHeight;
                poseEstimator.webcamFPS = webcamFPS;

                poseObj.SetActive(true);
                Debug.Log("[UnifiedSceneSetup] Created MediaPipe Pose Estimator");
            }
            else if (enablePoseEstimation)
            {
                poseEstimator = FindFirstObjectByType<MediaPipePoseEstimator>();
                poseEstimator.modelManager = modelManager;
                poseEstimator.tfliteRunner = tfliteRunner; // Explicitly set reference
                Debug.Log("[UnifiedSceneSetup] Found existing Pose Estimator - updated references");
            }

            // STEP 5: Create Hand Recognizer
            if (enableHandRecognition && FindFirstObjectByType<HandPointingRecognizer>() == null)
            {
                var handObj = new GameObject("Hand Pointing Recognizer");
                handObj.SetActive(false);

                handRecognizer = handObj.AddComponent<HandPointingRecognizer>();
                handRecognizer.poseEstimator = poseEstimator;

                handObj.SetActive(true);
                Debug.Log("[UnifiedSceneSetup] Created Hand Pointing Recognizer");
            }

            // STEP 6: Create Pose Visualizer
            if (FindFirstObjectByType<EnhancedPoseVisualizer>() == null)
            {
                var visualizerObj = new GameObject("Enhanced Pose Visualizer");
                visualizerObj.SetActive(false);

                var visualizer = visualizerObj.AddComponent<EnhancedPoseVisualizer>();
                visualizer.targetPoseEstimator = poseEstimator;
                visualizer.userHandTracker = handRecognizer;
                visualizer.overlayCanvas = mainCanvas;
                visualizer.debugMode = true; // Enable debug logging for keypoint updates

                visualizerObj.SetActive(true);
                Debug.Log("[UnifiedSceneSetup] Created Enhanced Pose Visualizer");
            }

            Debug.Log("[UnifiedSceneSetup] ✅ Scene setup complete!");
        }

        private IEnumerator WaitForModelsToLoad()
        {
            if (modelManager == null)
            {
                Debug.LogError("[UnifiedSceneSetup] ModelManager is null!");
                yield break;
            }

            Debug.Log("[UnifiedSceneSetup] Waiting for models to load...");
            int waited = 0;
            int maxWait = 100; // 5 seconds

            while (!modelManager.ModelsLoaded && waited < maxWait)
            {
                yield return new WaitForSeconds(0.05f);
                waited++;
            }

            if (!modelManager.ModelsLoaded)
            {
                Debug.LogError("[UnifiedSceneSetup] Models failed to load after 5 seconds!");
                Debug.LogError("[UnifiedSceneSetup] Check Assets/StreamingAssets/Models/ for .tflite files");
            }
            else
            {
                Debug.Log("[UnifiedSceneSetup] Models loaded successfully!");
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}

