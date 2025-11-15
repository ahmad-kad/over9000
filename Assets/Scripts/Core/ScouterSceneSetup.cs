using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ScouterXR.AI;
using ScouterXR.UI;
using ScouterXR.AR;
using ScouterXR.Core;

namespace ScouterXR.Core
{
    public class ScouterSceneSetup : MonoBehaviour
    {
        [Header("Auto-Setup Components")]
        public bool autoSetupScene = true;

        void Awake()
        {
            if (autoSetupScene)
            {
                SetupScene();
            }
        }

        public void SetupScene()
        {
            Debug.Log("Setting up DBZ Scouter XR scene...");

            // Create AR Session if not exists
            if (FindObjectOfType<ARSession>() == null)
            {
                GameObject arSessionObj = new GameObject("AR Session");
                arSessionObj.AddComponent<ARSession>();
                Debug.Log("Created AR Session");
            }

            // Create AR Session Origin if not exists
            if (FindObjectOfType<ARSessionOrigin>() == null)
            {
                GameObject sessionOriginObj = new GameObject("AR Session Origin");
                ARSessionOrigin sessionOrigin = sessionOriginObj.AddComponent<ARSessionOrigin>();

                // Add AR Camera
                GameObject cameraObj = new GameObject("AR Camera");
                cameraObj.transform.SetParent(sessionOriginObj.transform);
                Camera arCamera = cameraObj.AddComponent<Camera>();
                arCamera.clearFlags = CameraClearFlags.SolidColor;
                arCamera.backgroundColor = Color.black;
                arCamera.tag = "MainCamera";

                sessionOrigin.camera = arCamera;
                Debug.Log("Created AR Session Origin with Camera");
            }

            // Create Scouter Manager if not exists
            if (FindObjectOfType<ScouterManager>() == null)
            {
                GameObject managerObj = new GameObject("Scouter Manager");
                ScouterManager manager = managerObj.AddComponent<ScouterManager>();

                // Auto-assign references if possible
                manager.arSession = FindObjectOfType<ARSession>();
                manager.sessionOrigin = FindObjectOfType<ARSessionOrigin>();
                Debug.Log("Created Scouter Manager");
            }

            // Create MediaPipe Model Manager if not exists
            if (FindObjectOfType<ScouterXR.AI.MediaPipeModelManager>() == null)
            {
                GameObject modelObj = new GameObject("MediaPipe Model Manager");
                var modelManager = modelObj.AddComponent<ScouterXR.AI.MediaPipeModelManager>();

                // Auto-load models from Resources/Models/ or StreamingAssets
                // Models are in Assets/models/ folder
                Debug.Log("Created MediaPipe Model Manager");
            }

            // Create AI System if not exists
            var poseEstimator = FindObjectOfType<ScouterXR.AI.MediaPipePoseEstimator>();
            if (poseEstimator == null)
            {
                GameObject aiObj = new GameObject("Pose Estimator");
                poseEstimator = aiObj.AddComponent<ScouterXR.AI.MediaPipePoseEstimator>();

                // Auto-assign references
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    poseEstimator.arCamera = mainCam;
                }

                var modelManager = FindObjectOfType<ScouterXR.AI.MediaPipeModelManager>();
                if (modelManager != null)
                {
                    poseEstimator.modelManager = modelManager;
                }

                // Configure for development (use mock data by default)
                poseEstimator.useRealPoseDetection = false; // Set to true when MediaPipe is fully integrated

                Debug.Log("Created Pose Estimator with model manager reference");
            }

            // Create UI Canvas if not exists
            if (FindObjectOfType<Canvas>() == null)
            {
                CreateUICanvas();
            }

            // Create Halo Color Controller if not exists
            if (FindObjectOfType<ScouterXR.UI.HaloColorController>() == null)
            {
                GameObject haloObj = new GameObject("Halo Color Controller");
                ScouterXR.UI.HaloColorController haloController = haloObj.AddComponent<ScouterXR.UI.HaloColorController>();

                // Auto-assign references
                haloController.poseEstimator = poseEstimator;

                // Create and assign material
                Material haloMaterial = new Material(Shader.Find("Scouter/DepthHalo"));
                haloController.haloMaterial = haloMaterial;

                Debug.Log("Created Halo Color Controller with depth-based shader");
            }

            // Create AR Feature Manager if not exists
            if (FindObjectOfType<ScouterXR.AR.ArFeatureManager>() == null)
            {
                GameObject arObj = new GameObject("AR Feature Manager");
                ScouterXR.AR.ArFeatureManager arManager = arObj.AddComponent<ScouterXR.AR.ArFeatureManager>();

                // Add required AR components
                if (arManager.humanBodyManager == null)
                {
                    arManager.humanBodyManager = arObj.AddComponent<UnityEngine.XR.ARFoundation.ARHumanBodyManager>();
                }

                if (arManager.occlusionManager == null)
                {
                    arManager.occlusionManager = arObj.AddComponent<UnityEngine.XR.ARFoundation.AROcclusionManager>();
                }

                // Assign halo material from the controller we just created
                var haloController = FindObjectOfType<ScouterXR.UI.HaloColorController>();
                if (haloController != null)
                {
                    arManager.haloMaterial = haloController.haloMaterial;
                }

                Debug.Log("Created AR Feature Manager with depth shader material");
            }

            // Create XR Spatial UI Manager if not exists
            if (FindObjectOfType<ScouterXR.UI.XRSpatialUIManager>() == null)
            {
                GameObject spatialUIObj = new GameObject("XR Spatial UI Manager");
                ScouterXR.UI.XRSpatialUIManager spatialUIManager = spatialUIObj.AddComponent<ScouterXR.UI.XRSpatialUIManager>();

                // Auto-assign AR Foundation components
                spatialUIManager.anchorManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARAnchorManager>();
                spatialUIManager.raycastManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARRaycastManager>();
                spatialUIManager.arCamera = Camera.main;

                // Auto-assign existing UI components
                spatialUIManager.screenUI = FindObjectOfType<ScouterXR.UI.ScouterUI>();
                spatialUIManager.haloController = FindObjectOfType<ScouterXR.UI.HaloColorController>();
                spatialUIManager.scouterManager = FindObjectOfType<ScouterXR.Core.XRScouterManager>();
                spatialUIManager.poseEstimator = poseEstimator;

                Debug.Log("Created XR Spatial UI Manager with spatial anchoring");
            }

            // Create Monitoring Systems
            if (FindObjectOfType<PerformanceMonitor>() == null)
            {
                GameObject monitorObj = new GameObject("Performance Monitor");
                PerformanceMonitor monitor = monitorObj.AddComponent<PerformanceMonitor>();

                // Configure for mobile XR
                monitor.targetFPS = 60f;
                monitor.warningMemoryMB = 400f; // Lower threshold for mobile
                monitor.criticalMemoryMB = 600f;
                monitor.enableMonitoring = true;
                monitor.logToConsole = false; // Reduce spam in production

                Debug.Log("Created Performance Monitor");
            }

            // Create System Logger
            if (FindObjectOfType<SystemLogger>() == null)
            {
                GameObject loggerObj = new GameObject("System Logger");
                SystemLogger logger = loggerObj.AddComponent<SystemLogger>();

                // Configure logging
                logger.enableFileLogging = true;
                logger.enableConsoleLogging = true;
                logger.minimumLogLevel = SystemLogger.LogLevel.Info;
                logger.enableErrorReporting = false; // Set to true for production error tracking

                Debug.Log("Created System Logger");
            }

            // Create Fallback Manager
            if (FindObjectOfType<FallbackManager>() == null)
            {
                GameObject fallbackObj = new GameObject("Fallback Manager");
                FallbackManager fallback = fallbackObj.AddComponent<FallbackManager>();

                // Auto-assign references
                fallback.poseEstimator = poseEstimator;
                fallback.scouterUI = FindObjectOfType<ScouterUI>();
                fallback.haloController = FindObjectOfType<HaloColorController>();
                fallback.arFeatureManager = FindObjectOfType<ArFeatureManager>();
                fallback.spatialAudio = FindObjectOfType<SpatialAudioController>();
                fallback.performanceMonitor = FindObjectOfType<PerformanceMonitor>();

                fallback.enableAutomaticFallbacks = true;
                fallback.fallbackCheckInterval = 2f;

                Debug.Log("Created Fallback Manager with auto-assigned references");
            }

            // Create Spatial Audio Controller if not exists
            if (FindObjectOfType<ScouterXR.Core.SpatialAudioController>() == null)
            {
                GameObject audioObj = new GameObject("Spatial Audio Controller");
                var audioController = audioObj.AddComponent<ScouterXR.Core.SpatialAudioController>();

                // Configure spatial audio settings
                audioController.minDistance = 1f;
                audioController.maxDistance = 15f;

                // Auto-assign audio clips from Resources
                AssignAudioClips(audioController);

                Debug.Log("Created Spatial Audio Controller with assigned clips");
            }

            // Create Test Runner for automated testing
            if (FindObjectOfType<ScouterXR.Tests.Utilities.TestRunner>() == null)
            {
                GameObject testObj = new GameObject("Test Runner");
                var testRunner = testObj.AddComponent<ScouterXR.Tests.Utilities.TestRunner>();

                // Configure for production (disabled by default)
                testRunner.runOnStart = false; // Don't run tests on scene start
                testRunner.runUnitTests = true;
                testRunner.runIntegrationTests = true;
                testRunner.testInterval = 60f; // Run every minute in debug mode

                Debug.Log("Created Test Runner (automated testing system)");
            }

            // Create Hand Pointing Recognizer if not exists
            if (FindObjectOfType<ScouterXR.AI.HandPointingRecognizer>() == null)
            {
                GameObject handObj = new GameObject("Hand Pointing Recognizer");
                var handRecognizer = handObj.AddComponent<ScouterXR.AI.HandPointingRecognizer>();

                // Auto-assign references
                handRecognizer.arCamera = Camera.main;
                handRecognizer.poseEstimator = FindObjectOfType<ScouterXR.AI.MediaPipePoseEstimator>();

                handRecognizer.enableHandTracking = true;
                handRecognizer.pointingThreshold = 0.7f;
                handRecognizer.gestureHoldTime = 0.5f;

                Debug.Log("Created Hand Pointing Recognizer with pose estimator reference");
            }

            // Create TensorFlow Lite Inference Engine if not exists
            if (FindObjectOfType<ScouterXR.AI.TensorFlowLiteInference>() == null)
            {
                // This is created automatically by HandPointingRecognizer
                // But we can create a global one if needed
                Debug.Log("TensorFlow Lite Inference Engine will be created by HandPointingRecognizer");
            }

            // Create XR Scouter Manager if not exists
            if (FindObjectOfType<ScouterXR.Core.XRScouterManager>() == null)
            {
                GameObject xrObj = new GameObject("XR Scouter Manager");
                var xrManager = xrObj.AddComponent<ScouterXR.Core.XRScouterManager>();

                // Auto-assign references
                xrManager.raycastManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARRaycastManager>();
                xrManager.arCamera = Camera.main;
                xrManager.handRecognizer = FindObjectOfType<ScouterXR.AI.HandPointingRecognizer>();
                xrManager.scouterUI = FindObjectOfType<ScouterXR.UI.ScouterUI>();
                xrManager.audioController = FindObjectOfType<ScouterXR.Core.SpatialAudioController>();

                xrManager.scanDuration = 3f;
                xrManager.maxScanDistance = 10f;

                Debug.Log("Created XR Scouter Manager with all references assigned");
            }


            Debug.Log("Scene setup complete! Ready for DBZ Scouter XR demo.");
        }

        private void AssignAudioClips(ScouterXR.Core.SpatialAudioController audioController)
        {
            // Load audio clips from Resources/Audio/SFX/ folder
            // Note: Audio files need to be placed in Resources/Audio/SFX/ for this to work

            // Core audio clips
            audioController.scouterActivationClip = Resources.Load<AudioClip>("Audio/SFX/SCOUTER - AUDIO FROM JAYUZUMI.COM");
            audioController.scouterBeepClip = Resources.Load<AudioClip>("Audio/SFX/db-scouter-made-with-Voicemod");
            audioController.powerLevelReadingClip = Resources.Load<AudioClip>("Audio/SFX/HIS POWER LEVEL IS 1200 - AUDIO FROM JAYUZUMI.COM");
            audioController.overloadClip = Resources.Load<AudioClip>("Audio/SFX/9000 - AUDIO FROM JAYUZUMI.COM");
            audioController.overloadAltClip = Resources.Load<AudioClip>("Audio/SFX/over9000.swf");
            audioController.battleMusicClip = Resources.Load<AudioClip>("Audio/SFX/dragonball_battle");
            audioController.glassShatterClip = Resources.Load<AudioClip>("Audio/SFX/glass_shatter");
            audioController.defeatScreamClip = Resources.Load<AudioClip>("Audio/SFX/NOOOOOO - AUDIO FROM JAYUZUMI.COM");

            // Scanning beeps array - create a sequence of beeps for ramp-up effect
            var scanningBeeps = new AudioClip[]
            {
                Resources.Load<AudioClip>("Audio/SFX/db-scouter-made-with-Voicemod"),
                Resources.Load<AudioClip>("Audio/SFX/db-scouter-made-with-Voicemod"),
                Resources.Load<AudioClip>("Audio/SFX/db-scouter-made-with-Voicemod"),
                Resources.Load<AudioClip>("Audio/SFX/db-scouter-made-with-Voicemod"),
                Resources.Load<AudioClip>("Audio/SFX/db-scouter-made-with-Voicemod")
            };

            audioController.scanningBeeps = scanningBeeps;

            // Log which clips were successfully loaded
            int loadedCount = 0;
            if (audioController.scouterActivationClip != null) loadedCount++;
            if (audioController.scouterBeepClip != null) loadedCount++;
            if (audioController.powerLevelReadingClip != null) loadedCount++;
            if (audioController.overloadClip != null) loadedCount++;
            if (audioController.overloadAltClip != null) loadedCount++;
            if (audioController.battleMusicClip != null) loadedCount++;
            if (audioController.glassShatterClip != null) loadedCount++;
            if (audioController.defeatScreamClip != null) loadedCount++;
            if (scanningBeeps != null && scanningBeeps.Length > 0) loadedCount++;

            Debug.Log($"Audio clips loaded: {loadedCount}/9 clips successfully assigned");

            if (loadedCount < 9)
            {
                Debug.LogWarning("Some audio clips not found in Resources/Audio/SFX/. Make sure audio files are in the correct Resources folder.");
            }
        }

        private void CreateUICanvas()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("Scouter Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Add Canvas Scaler
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            // Add Graphic Raycaster
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create Scouter UI Panel
            GameObject panelObj = new GameObject("Scouter Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.8f);
            panelRect.anchorMax = new Vector2(0.9f, 0.95f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add Canvas Group for fading
            CanvasGroup canvasGroup = panelObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // Add background image
            UnityEngine.UI.Image bgImage = panelObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // Create power level text
            GameObject textObj = new GameObject("Power Level Text");
            textObj.transform.SetParent(panelObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 20);
            textRect.offsetMax = new Vector2(-20, -20);

            // Add TextMeshPro text (if available) or regular Text
            TMPro.TextMeshProUGUI tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmpText.text = "Power Level: Scanning...";
            tmpText.fontSize = 48;
            tmpText.alignment = TMPro.TextAlignmentOptions.Center;
            tmpText.color = Color.green;

            // Create ScouterUI component
            ScouterUI scouterUI = panelObj.AddComponent<ScouterUI>();
            scouterUI.scouterReadout = canvasGroup;
            scouterUI.powerLevelText = textRect;

            Debug.Log("Created UI Canvas with Scouter Panel");
        }

        // Public method to clean up scene (useful for testing)
        public void CleanupScene()
        {
            // Remove all created objects (be careful!)
            Debug.Log("Scene cleanup not implemented yet");
        }
    }
}
