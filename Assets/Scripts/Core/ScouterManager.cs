using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ScouterXR.AI;
using ScouterXR.UI;
using ScouterXR.AR;
using ScouterXR.Core;

namespace ScouterXR.Core
{
    public class ScouterManager : MonoBehaviour
    {
        [Header("Core Systems")]
        public MediaPipePoseEstimator poseEstimator;
        public ScouterUI scouterUI;
        public HaloColorController haloColorController;
        public ArFeatureManager arFeatureManager;
        public SpatialAudioController spatialAudio;

        [Header("AR Foundation")]
        public ARSession arSession;
        public ARSessionOrigin sessionOrigin;

        [Header("Debug")]
        public bool showDebugInfo = false;

        void Start()
        {
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("Initializing DBZ Scouter XR Systems...");

            // 1. Initialize AR Session
            if (arSession != null)
            {
                arSession.attemptUpdate = true;
                Debug.Log("AR Session initialized");
            }

            // 2. Configure cross-platform AR features
            if (arFeatureManager != null)
            {
                Debug.Log($"AR Feature Mode: {arFeatureManager.GetCurrentOcclusionMode()}");
            }

            // 3. Set up spatial audio
            if (spatialAudio != null)
            {
                spatialAudio.PlayScouterHum();
                Debug.Log("Spatial audio initialized");
            }

            // 4. Connect AI to UI systems (already handled in pose estimator)

            Debug.Log("All systems initialized successfully!");
        }

        void Update()
        {
            // Update audio based on current power level
            if (poseEstimator != null && spatialAudio != null)
            {
                // Get current power level from UI (smoothed value)
                if (scouterUI != null)
                {
                    // This is a simplified approach - in practice you'd want
                    // to expose the smoothed power level from ScouterUI
                    float currentPower = 2000f + (Mathf.Sin(Time.time) * 1000f); // Mock
                    spatialAudio.UpdatePowerLevel(currentPower);
                }
            }

            // Debug info
            if (showDebugInfo && poseEstimator != null)
            {
                Debug.Log($"Pose Valid: {poseEstimator.GetType().GetProperty("CurrentPoseValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(poseEstimator)}");
            }
        }

        // Public methods for external control
        public void StartScanning()
        {
            if (scouterUI != null)
            {
                scouterUI.ShowScouter();
            }

            if (spatialAudio != null)
            {
                spatialAudio.PlayScouterHum();
            }
        }

        public void StopScanning()
        {
            if (scouterUI != null)
            {
                scouterUI.HideScouter();
            }

            if (spatialAudio != null)
            {
                spatialAudio.StopAudio();
            }
        }

        public void TriggerOverload()
        {
            if (spatialAudio != null)
            {
                spatialAudio.PlayOverload();
            }

            // Could trigger screen effects here
            Debug.Log("OVER 9000!!!");
        }

        // Platform info for debugging
        public string GetPlatformInfo()
        {
            string info = $"Platform: {Application.platform}\n";
            info += $"AR Features: {arFeatureManager?.GetCurrentOcclusionMode()}\n";
            info += $"Unity Version: {Application.unityVersion}\n";
            info += $"Target Frame Rate: {Application.targetFrameRate}\n";
            return info;
        }
    }
}
