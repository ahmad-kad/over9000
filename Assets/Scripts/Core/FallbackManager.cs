using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ScouterXR.AI;
using ScouterXR.UI;
using ScouterXR.AR;
using ScouterXR.Core;

namespace ScouterXR.Core
{
    public class FallbackManager : MonoBehaviour
    {
        [Header("System References")]
        public MediaPipePoseEstimator poseEstimator;
        public ScouterUI scouterUI;
        public HaloColorController haloController;
        public ArFeatureManager arFeatureManager;
        public SpatialAudioController spatialAudio;
        public PerformanceMonitor performanceMonitor;

        [Header("Fallback Settings")]
        public bool enableAutomaticFallbacks = true;
        public float fallbackCheckInterval = 2f;

        private float lastFallbackCheck = 0f;

        void Update()
        {
            if (!enableAutomaticFallbacks) return;

            if (Time.time - lastFallbackCheck >= fallbackCheckInterval)
            {
                CheckAndApplyFallbacks();
                lastFallbackCheck = Time.time;
            }
        }

        private void CheckAndApplyFallbacks()
        {
            // 1. Pose Detection Fallbacks
            CheckPoseDetectionFallback();

            // 2. Depth Sensing Fallbacks
            CheckDepthSensingFallback();

            // 3. Audio System Fallbacks
            CheckAudioSystemFallback();

            // 4. UI Rendering Fallbacks
            CheckUIRenderingFallback();

            // 5. Performance Fallbacks
            CheckPerformanceFallback();

            // 6. Hardware Connectivity Fallbacks
            CheckHardwareConnectivityFallback();
        }

        // POSE DETECTION FALLBACKS
        private void CheckPoseDetectionFallback()
        {
            if (poseEstimator == null) return;

            // Check if pose detection is failing
            var currentPose = poseEstimator.GetType().GetProperty("CurrentPoseValid",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(poseEstimator);

            bool poseValid = currentPose != null && (bool)currentPose;

            if (!poseValid)
            {
                SystemLogger.LogWarning("FallbackManager", "Pose detection failing, applying fallbacks");

                // Reduce pose requirements
                // In real implementation, this would adjust MediaPipe confidence thresholds

                // Show user-friendly message
                if (scouterUI != null)
                {
                    // Could show "Searching for target..." message
                }
            }
        }

        // DEPTH SENSING FALLBACKS
        private void CheckDepthSensingFallback()
        {
            if (arFeatureManager == null) return;

            var currentMode = arFeatureManager.GetCurrentOcclusionMode();

            // If we're on a low-quality depth mode, suggest optimizations
            if (currentMode == ArFeatureManager.OcclusionMode.AiDepthEstimation)
            {
                SystemLogger.LogInfo("FallbackManager", $"Using depth fallback mode: {currentMode}");

                // Reduce visual quality to maintain performance
                if (haloController != null && haloController.haloMaterial != null)
                {
                    // Could reduce shader complexity for lower-end depth modes
                    haloController.haloMaterial.SetFloat("_EdgeSoftness", 0.2f); // Softer edges for lower quality depth
                }
            }

            // If no depth at all, provide visual feedback
            if (currentMode == ArFeatureManager.OcclusionMode.None)
            {
                SystemLogger.LogInfo("FallbackManager", "No depth sensing available, using color overlay mode");

                // Ensure UI still provides feedback
                if (scouterUI != null)
                {
                    scouterUI.gameObject.SetActive(true);
                }
            }
        }

        // AUDIO SYSTEM FALLBACKS
        private void CheckAudioSystemFallback()
        {
            if (spatialAudio == null) return;

            // Check if audio source is working
            var audioSource = spatialAudio.GetComponent<AudioSource>();
            if (audioSource != null && !audioSource.isPlaying && spatialAudio.enabled)
            {
                SystemLogger.LogWarning("FallbackManager", "Audio system not playing, checking audio settings");

                // Try to restart audio
                if (spatialAudio.scouterHumClip != null)
                {
                    spatialAudio.PlayScouterHum();
                    SystemLogger.LogInfo("FallbackManager", "Restarted audio playback");
                }
            }

            // Check for audio output device issues
            if (!audioSource.isActiveAndEnabled)
            {
                SystemLogger.LogWarning("FallbackManager", "Audio source disabled, using silent fallback");

                // Continue without audio - UI and visual feedback still work
            }
        }

        // UI RENDERING FALLBACKS
        private void CheckUIRenderingFallback()
        {
            if (scouterUI == null) return;

            // Check if UI elements are visible
            var canvasGroup = scouterUI.scouterReadout;
            if (canvasGroup != null && canvasGroup.alpha < 0.1f)
            {
                SystemLogger.LogInfo("FallbackManager", "UI appears hidden, ensuring visibility");

                // Force UI visibility for critical feedback
                if (poseEstimator != null && poseEstimator.GetCurrentEstimatedDepth() > 0)
                {
                    scouterUI.ShowScouter();
                }
            }

            // Check for missing TextMeshPro fallback
            var textRect = scouterUI.powerLevelText;
            if (textRect != null)
            {
                bool hasTMP = textRect.GetComponentInChildren<TMPro.TextMeshProUGUI>() != null;
                bool hasUnityText = textRect.GetComponentInChildren<UnityEngine.UI.Text>() != null;

                if (!hasTMP && !hasUnityText)
                {
                    SystemLogger.LogError("FallbackManager", "No text component found on UI - critical UI failure");
                }
            }
        }

        // PERFORMANCE FALLBACKS
        private void CheckPerformanceFallback()
        {
            if (performanceMonitor == null) return;

            float avgFPS = performanceMonitor.GetAverageFPS();
            long memoryMB = performanceMonitor.GetMemoryUsageMB();

            // Automatic quality reduction based on performance
            if (avgFPS < 30f)
            {
                SystemLogger.LogWarning("FallbackManager", $"Low FPS detected ({avgFPS}), applying performance fallbacks");

                // Reduce shader quality
                if (haloController != null && haloController.haloMaterial != null)
                {
                    haloController.haloMaterial.SetFloat("_EdgeSoftness", 0.5f); // Very soft edges
                }

                // Reduce audio quality
                if (spatialAudio != null)
                {
                    spatialAudio.UpdatePowerLevel(1000f); // Reset to base to reduce processing
                }
            }

            if (memoryMB > 600) // High memory usage
            {
                SystemLogger.LogWarning("FallbackManager", $"High memory usage ({memoryMB}MB), triggering cleanup");

                // Force garbage collection
                System.GC.Collect();
                Resources.UnloadUnusedAssets();
            }
        }

        // HARDWARE CONNECTIVITY FALLBACKS
        private void CheckHardwareConnectivityFallback()
        {
            // Check camera access
            if (WebCamTexture.devices.Length == 0)
            {
                SystemLogger.LogError("FallbackManager", "No camera devices detected");

                // Could show message to user about camera permissions
            }

            // Check AR subsystem health
            if (arFeatureManager != null)
            {
                var arSession = FindFirstObjectByType<ARSession>();
                if (arSession != null && arSession.subsystem == null)
                {
                    SystemLogger.LogError("FallbackManager", "AR subsystem not available");

                    // Could fall back to non-AR mode or show error message
                }
            }
        }

        // PUBLIC METHODS FOR MANUAL FALLBACK CONTROL
        public void ForceLowQualityMode()
        {
            SystemLogger.LogInfo("FallbackManager", "Manually forcing low quality mode");

            // Reduce all quality settings
            if (haloController != null && haloController.haloMaterial != null)
            {
                haloController.haloMaterial.SetFloat("_EdgeSoftness", 0.5f);
            }

            if (spatialAudio != null)
            {
                spatialAudio.UpdatePowerLevel(1000f);
            }

            QualitySettings.SetQualityLevel(0); // Lowest quality
        }

        public void ForceHighQualityMode()
        {
            SystemLogger.LogInfo("FallbackManager", "Attempting to restore high quality mode");

            // Restore quality settings if performance allows
            if (performanceMonitor != null && performanceMonitor.GetAverageFPS() > 50f)
            {
                QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel() + 1);
            }
        }

        public SystemStatusReport GetSystemStatus()
        {
            return new SystemStatusReport
            {
                poseDetectionWorking = poseEstimator != null,
                depthSensingMode = arFeatureManager?.GetCurrentOcclusionMode().ToString() ?? "Unknown",
                audioWorking = spatialAudio != null && spatialAudio.GetComponent<AudioSource>()?.isPlaying == true,
                uiVisible = scouterUI != null && scouterUI.scouterReadout?.alpha > 0.5f,
                averageFPS = performanceMonitor?.GetAverageFPS() ?? 0f,
                memoryUsageMB = performanceMonitor?.GetMemoryUsageMB() ?? 0
            };
        }

        public struct SystemStatusReport
        {
            public bool poseDetectionWorking;
            public string depthSensingMode;
            public bool audioWorking;
            public bool uiVisible;
            public float averageFPS;
            public long memoryUsageMB;

            public string GetSummary()
            {
                return $"Pose: {(poseDetectionWorking ? "OK" : "FAIL")} | " +
                       $"Depth: {depthSensingMode} | " +
                       $"Audio: {(audioWorking ? "OK" : "FAIL")} | " +
                       $"UI: {(uiVisible ? "OK" : "FAIL")} | " +
                       $"FPS: {averageFPS:F1} | " +
                       $"Memory: {memoryUsageMB}MB";
            }
        }
    }
}
