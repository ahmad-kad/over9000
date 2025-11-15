using UnityEngine;
using ScouterXR.AI;
using ScouterXR.UI;
using ScouterXR.Core;

namespace ScouterXR.Core
{
    /// <summary>
    /// Validates the streaming data pipeline from webcam → AI → UI → Audio
    /// Ensures all components are properly connected and data flows correctly
    /// </summary>
    public class StreamingPipelineValidator : MonoBehaviour
    {
        [Header("Pipeline Components")]
        public MediaPipePoseEstimator poseEstimator;
        public HandPointingRecognizer handRecognizer;
        public XRScouterManager xrManager;
        public XRSpatialUIManager spatialUI;
        public ScouterUI screenUI;
        public SpatialAudioController audioController;

        [Header("Validation Settings")]
        public bool enableValidation = true;
        public float validationInterval = 2f;
        public bool logValidationResults = true;

        private float lastValidationTime = 0f;
        private int validationCycleCount = 0;

        void Start()
        {
            if (enableValidation)
            {
                AutoAssignComponents();
                ValidatePipelineIntegrity();
            }
        }

        void Update()
        {
            if (!enableValidation) return;

            if (Time.time - lastValidationTime >= validationInterval)
            {
                RunStreamingValidation();
                lastValidationTime = Time.time;
            }
        }

        private void AutoAssignComponents()
        {
            // Auto-find components if not assigned
            if (poseEstimator == null)
                poseEstimator = FindObjectOfType<MediaPipePoseEstimator>();

            if (handRecognizer == null)
                handRecognizer = FindObjectOfType<HandPointingRecognizer>();

            if (xrManager == null)
                xrManager = FindObjectOfType<XRScouterManager>();

            if (spatialUI == null)
                spatialUI = FindObjectOfType<XRSpatialUIManager>();

            if (screenUI == null)
                screenUI = FindObjectOfType<ScouterUI>();

            if (audioController == null)
                audioController = FindObjectOfType<SpatialAudioController>();
        }

        private void ValidatePipelineIntegrity()
        {
            SystemLogger.LogInfo("StreamingPipelineValidator", "=== PIPELINE INTEGRITY CHECK ===");

            // Check component existence
            ValidateComponent("Pose Estimator", poseEstimator);
            ValidateComponent("Hand Recognizer", handRecognizer);
            ValidateComponent("XR Manager", xrManager);
            ValidateComponent("Spatial UI", spatialUI);
            ValidateComponent("Screen UI", screenUI);
            ValidateComponent("Audio Controller", audioController);

            // Check component connections
            ValidateConnections();

            SystemLogger.LogInfo("StreamingPipelineValidator", "=== INTEGRITY CHECK COMPLETE ===");
        }

        private void ValidateComponent(string componentName, MonoBehaviour component)
        {
            bool exists = component != null;
            string status = exists ? "✓ FOUND" : "✗ MISSING";

            if (logValidationResults)
            {
                SystemLogger.LogInfo("StreamingPipelineValidator", $"{componentName}: {status}");
            }

            if (!exists)
            {
                SystemLogger.LogWarning("StreamingPipelineValidator", $"{componentName} is missing - pipeline may not function correctly");
            }
        }

        private void ValidateConnections()
        {
            // XR Manager connections
            if (xrManager != null)
            {
                bool hasHandRef = xrManager.handRecognizer != null;
                bool hasUIRef = xrManager.scouterUI != null;
                bool hasAudioRef = xrManager.audioController != null;

                SystemLogger.LogInfo("StreamingPipelineValidator", $"XR Manager connections - Hand: {(hasHandRef ? "✓" : "✗")}, UI: {(hasUIRef ? "✓" : "✗")}, Audio: {(hasAudioRef ? "✓" : "✗")}");

                if (!hasHandRef || !hasUIRef || !hasAudioRef)
                {
                    SystemLogger.LogWarning("StreamingPipelineValidator", "XR Manager missing some references - auto-assigning");
                    AutoAssignXRManagerRefs();
                }
            }

            // Spatial UI connections
            if (spatialUI != null)
            {
                bool hasPoseRef = spatialUI.poseEstimator != null;
                bool hasManagerRef = spatialUI.scouterManager != null;

                SystemLogger.LogInfo("StreamingPipelineValidator", $"Spatial UI connections - Pose: {(hasPoseRef ? "✓" : "✗")}, Manager: {(hasManagerRef ? "✓" : "✗")}");

                if (!hasPoseRef || !hasManagerRef)
                {
                    SystemLogger.LogWarning("StreamingPipelineValidator", "Spatial UI missing some references - auto-assigning");
                    AutoAssignSpatialUIRefs();
                }
            }
        }

        private void AutoAssignXRManagerRefs()
        {
            if (xrManager != null)
            {
                if (xrManager.handRecognizer == null)
                    xrManager.handRecognizer = handRecognizer;

                if (xrManager.scouterUI == null)
                    xrManager.scouterUI = screenUI;

                if (xrManager.audioController == null)
                    xrManager.audioController = audioController;

                if (xrManager.spatialUIManager == null)
                    xrManager.spatialUIManager = spatialUI;
            }
        }

        private void AutoAssignSpatialUIRefs()
        {
            if (spatialUI != null)
            {
                if (spatialUI.poseEstimator == null)
                    spatialUI.poseEstimator = poseEstimator;

                if (spatialUI.scouterManager == null)
                    spatialUI.scouterManager = xrManager;

                if (spatialUI.screenUI == null)
                    spatialUI.screenUI = screenUI;
            }
        }

        private void RunStreamingValidation()
        {
            validationCycleCount++;

            SystemLogger.LogInfo("StreamingPipelineValidator", $"=== STREAMING VALIDATION CYCLE {validationCycleCount} ===");

            // Test data flow through pipeline
            ValidateWebcamToPoseFlow();
            ValidatePoseToUIFlow();
            ValidateUIToAudioFlow();
            ValidateGestureToScanFlow();

            // Test overall pipeline health
            bool pipelineHealthy = IsPipelineHealthy();
            SystemLogger.LogInfo("StreamingPipelineValidator", $"Pipeline Health: {(pipelineHealthy ? "✓ HEALTHY" : "✗ ISSUES DETECTED")}");

            if (!pipelineHealthy)
            {
                LogPipelineIssues();
            }
        }

        private void ValidateWebcamToPoseFlow()
        {
            // Check if webcam data is reaching pose estimator
            if (poseEstimator != null)
            {
                float depth = poseEstimator.GetCurrentEstimatedDepth();
                float powerLevel = poseEstimator.GetPowerLevelFromCurrentPose();

                bool validDepth = depth > 0 && depth < 10;
                bool validPower = powerLevel >= 1000 && powerLevel <= 10000;

                SystemLogger.LogInfo("StreamingPipelineValidator",
                    $"Webcam→Pose: Depth={depth:F1}m {(validDepth ? "✓" : "✗")}, Power={powerLevel:F0} {(validPower ? "✓" : "✗")}");
            }
        }

        private void ValidatePoseToUIFlow()
        {
            // Check if pose data is reaching UI
            if (screenUI != null && poseEstimator != null)
            {
                // This would require exposing current power level from UI
                // For now, just check if UI exists and is functional
                SystemLogger.LogInfo("StreamingPipelineValidator", "Pose→UI: Flow validation requires UI power level exposure");
            }
        }

        private void ValidateUIToAudioFlow()
        {
            // Check if UI can trigger audio
            if (screenUI != null && audioController != null)
            {
                bool hasAudioRef = screenUI.audioController != null;
                SystemLogger.LogInfo("StreamingPipelineValidator", $"UI→Audio: Connected={(hasAudioRef ? "✓" : "✗")}");

                if (!hasAudioRef)
                {
                    screenUI.audioController = audioController;
                    SystemLogger.LogInfo("StreamingPipelineValidator", "Auto-assigned audio controller to UI");
                }
            }
        }

        private void ValidateGestureToScanFlow()
        {
            // Check gesture recognition to scanning flow
            if (handRecognizer != null && xrManager != null)
            {
                bool handDetected = handRecognizer.IsHandDetected();
                bool pointing = handRecognizer.IsPointingGestureActive();
                bool scanning = xrManager.IsScanning();

                SystemLogger.LogInfo("StreamingPipelineValidator",
                    $"Gesture→Scan: Hands={(handDetected ? "✓" : "✗")}, Pointing={(pointing ? "✓" : "✗")}, Scanning={(scanning ? "✓" : "✗")}");

                // Test simulated gesture trigger
                if (!pointing && !scanning)
                {
                    SystemLogger.LogInfo("StreamingPipelineValidator", "Ready for gesture input - use SPACE key to test");
                }
            }
        }

        private bool IsPipelineHealthy()
        {
            // Check all critical components exist
            bool componentsExist = poseEstimator != null &&
                                  handRecognizer != null &&
                                  xrManager != null &&
                                  spatialUI != null &&
                                  screenUI != null &&
                                  audioController != null;

            if (!componentsExist) return false;

            // Check critical connections
            bool connectionsValid = xrManager.handRecognizer != null &&
                                   xrManager.scouterUI != null &&
                                   xrManager.audioController != null &&
                                   spatialUI.poseEstimator != null &&
                                   screenUI.audioController != null;

            return componentsExist && connectionsValid;
        }

        private void LogPipelineIssues()
        {
            SystemLogger.LogWarning("StreamingPipelineValidator", "=== PIPELINE ISSUES DETECTED ===");

            if (poseEstimator == null)
                SystemLogger.LogWarning("StreamingPipelineValidator", "- Pose Estimator missing");

            if (handRecognizer == null)
                SystemLogger.LogWarning("StreamingPipelineValidator", "- Hand Recognizer missing");

            if (xrManager == null)
                SystemLogger.LogWarning("StreamingPipelineValidator", "- XR Manager missing");

            if (spatialUI == null)
                SystemLogger.LogWarning("StreamingPipelineValidator", "- Spatial UI missing");

            if (screenUI == null)
                SystemLogger.LogWarning("StreamingPipelineValidator", "- Screen UI missing");

            if (audioController == null)
                SystemLogger.LogWarning("StreamingPipelineValidator", "- Audio Controller missing");

            SystemLogger.LogWarning("StreamingPipelineValidator", "=== END ISSUES ===");
        }

        // Public API for manual validation
        public void RunManualValidation()
        {
            SystemLogger.LogInfo("StreamingPipelineValidator", "Running manual pipeline validation...");
            ValidatePipelineIntegrity();
            RunStreamingValidation();
        }

        public bool IsPipelineOperational()
        {
            return IsPipelineHealthy();
        }

        public string GetPipelineStatus()
        {
            if (!IsPipelineHealthy())
            {
                return "PIPELINE HAS ISSUES - Check logs for details";
            }

            return $"PIPELINE OPERATIONAL - {validationCycleCount} validation cycles completed";
        }
    }
}
