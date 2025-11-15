using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ScouterXR.AI;
using ScouterXR.Core;
using ScouterXR.UI;

namespace ScouterXR.Core
{
    public class XRScouterManager : MonoBehaviour
    {
        [Header("XR Components")]
        public ARRaycastManager raycastManager;
        public Camera arCamera;

        [Header("Scanning Settings")]
        public float scanDuration = 3f; // How long scanning takes
        public float maxScanDistance = 10f; // Maximum raycast distance
        public LayerMask scanLayerMask = -1; // What layers to scan

        [Header("References")]
        public HandPointingRecognizer handRecognizer;
        public ScouterUI scouterUI;
        public SpatialAudioController audioController;
        public ScouterXR.UI.XRSpatialUIManager spatialUIManager;

        // Scanning state
        private bool isScanning = false;
        private float scanStartTime = 0f;
        private float currentScanProgress = 0f;
        private Vector3 scanTargetPosition = Vector3.zero;
        private GameObject scanTargetObject = null;

        // Raycast results
        private TrackableType raycastTrackableTypes = TrackableType.PlaneWithinPolygon |
                                                     TrackableType.PlaneWithinBounds |
                                                     TrackableType.FeaturePoint;

        void Start()
        {
            SystemLogger.LogInfo("XRScouterManager", "Mock XR Scouter Manager initialized");

            // Auto-find components if not assigned
            if (raycastManager == null)
            {
                raycastManager = FindObjectOfType<ARRaycastManager>();
            }

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }

            if (handRecognizer == null)
            {
                handRecognizer = FindObjectOfType<HandPointingRecognizer>();
            }

            if (spatialUIManager == null)
            {
                spatialUIManager = FindObjectOfType<ScouterXR.UI.XRSpatialUIManager>();
            }
        }

        void Update()
        {
            if (isScanning)
            {
                UpdateScanning();
            }
        }

        // Called by HandPointingRecognizer when pointing gesture is detected
        public void OnPointingGestureDetected(Vector3 pointingDirection, Vector3 tipPosition)
        {
            if (isScanning)
            {
                SystemLogger.LogWarning("XRScouterManager", "Already scanning, ignoring new gesture");
                return;
            }

            SystemLogger.LogInfo("XRScouterManager", "Pointing gesture detected, starting XR scan");

            // Perform raycast from pointing direction
            if (PerformPointingRaycast(pointingDirection, tipPosition))
            {
                // Notify spatial UI of new target detection
                if (spatialUIManager != null)
                {
                    spatialUIManager.OnTargetDetected(scanTargetPosition, scanTargetObject);
                }

                StartScanning();
            }
            else
            {
                SystemLogger.LogInfo("XRScouterManager", "No valid scan target found");
                // Could play "no target" sound here
            }
        }

        // Called when pointing gesture is lost
        public void OnPointingGestureLost()
        {
            if (isScanning)
            {
                SystemLogger.LogInfo("XRScouterManager", "Pointing gesture lost, stopping scan");
                StopScanning();
            }
        }

        private bool PerformPointingRaycast(Vector3 pointingDirection, Vector3 tipPosition)
        {
            if (raycastManager == null || arCamera == null)
            {
                // Mock raycast for testing without AR Foundation
                return PerformMockRaycast(pointingDirection, tipPosition);
            }

            // Real AR Foundation raycast
            Vector3 rayOrigin = tipPosition;
            Vector3 rayDirection = pointingDirection;

            var hits = new System.Collections.Generic.List<ARRaycastHit>();
            if (raycastManager.Raycast(rayOrigin, rayDirection, hits, raycastTrackableTypes, maxScanDistance))
            {
                if (hits.Count > 0)
                {
                    var hit = hits[0]; // Take closest hit
                    scanTargetPosition = hit.pose.position;
                    scanTargetObject = hit.trackable.gameObject;

                    SystemLogger.LogInfo("XRScouterManager", $"Raycast hit at {scanTargetPosition}, distance: {hit.distance}m");
                    return true;
                }
            }

            return false;
        }

        private bool PerformMockRaycast(Vector3 pointingDirection, Vector3 tipPosition)
        {
            // Mock raycast for development/testing without AR hardware
            Vector3 rayOrigin = tipPosition;
            Vector3 rayDirection = pointingDirection;

            // Cast ray and find hit point
            Ray ray = new Ray(rayOrigin, rayDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxScanDistance, scanLayerMask))
            {
                scanTargetPosition = hit.point;
                scanTargetObject = hit.collider.gameObject;

                SystemLogger.LogInfo("XRScouterManager", $"Mock raycast hit at {scanTargetPosition}, distance: {hit.distance}m, object: {hit.collider.name}");
                return true;
            }
            else
            {
                // Mock hit on invisible plane at 3m distance
                scanTargetPosition = rayOrigin + rayDirection * 3f;
                scanTargetObject = null; // No real object

                SystemLogger.LogInfo("XRScouterManager", $"Mock raycast hit on plane at {scanTargetPosition} (3m away)");
                return true;
            }
        }

        private void StartScanning()
        {
            isScanning = true;
            scanStartTime = Time.time;
            currentScanProgress = 0f;

            SystemLogger.LogInfo("XRScouterManager", $"Starting scan at {scanTargetPosition}");

            // Start scanning audio
            if (audioController != null)
            {
                audioController.StartScanningBeeps();
            }


            // Show scanning UI
            if (scouterUI != null)
            {
                scouterUI.ShowScouter();
            }

            // Notify other systems
            var poseEstimator = FindObjectOfType<MediaPipePoseEstimator>();
            if (poseEstimator != null)
            {
                poseEstimator.OnScanningStarted(scanTargetPosition);
            }
        }

        private void UpdateScanning()
        {
            float elapsedTime = Time.time - scanStartTime;
            currentScanProgress = Mathf.Clamp01(elapsedTime / scanDuration);

            // Update UI with scanning progress
            if (scouterUI != null)
            {
                // Show scanning progress (could be a progress bar or animated text)
                float mockPowerLevel = 1000f + (currentScanProgress * 8000f); // Ramp up power
                scouterUI.UpdateTarget(arCamera.WorldToScreenPoint(scanTargetPosition), mockPowerLevel);
            }

            // Update spatial UI with current scanning progress
            if (spatialUIManager != null)
            {
                float currentPowerLevel = 1000f + (currentScanProgress * 8000f);
                spatialUIManager.UpdateScanningProgress(currentScanProgress, currentPowerLevel);
            }

            // Check if scan is complete
            if (currentScanProgress >= 1f)
            {
                CompleteScanning();
            }
        }

        private void CompleteScanning()
        {
            isScanning = false;

            // Calculate final power level (mock with some variation)
            float finalPowerLevel = 1000f + Random.Range(0f, 9000f);
            bool isOver9000 = finalPowerLevel >= 9000f;

            SystemLogger.LogInfo("XRScouterManager", $"Scan complete! Power level: {finalPowerLevel:F0} (Over 9000: {isOver9000})");

            // Stop scanning audio
            if (audioController != null)
            {
                audioController.StopAudio();

                if (isOver9000)
                {
                    // Play overload sequence
                    audioController.PlayOverload();
                }
                else
                {
                    // Play normal power level reading
                    audioController.PlayPowerLevelReading();
                }
            }

            // Update final UI
            if (scouterUI != null)
            {
                scouterUI.UpdateTarget(arCamera.WorldToScreenPoint(scanTargetPosition), finalPowerLevel);
            }

            // Update spatial UI with final power level
            if (spatialUIManager != null && spatialUIManager.HasActiveTarget())
            {
                var activeTarget = spatialUIManager.GetActiveTarget();
                spatialUIManager.UpdateTargetData(activeTarget.id, finalPowerLevel);
                spatialUIManager.StopScanningProgress();
            }

            // Trigger overload effect if over 9000
            if (isOver9000)
            {
                TriggerOverloadEffect();
            }

            // Reset for next scan
            scanTargetPosition = Vector3.zero;
            scanTargetObject = null;
        }

        private void StopScanning()
        {
            isScanning = false;
            currentScanProgress = 0f;

            // Stop audio
            if (audioController != null)
            {
                audioController.StopAudio();
            }

            // Hide scanning UI
            if (scouterUI != null)
            {
                // Could show "Scan cancelled" message
            }

            // Hide spatial UI
            if (spatialUIManager != null)
            {
                spatialUIManager.HideWorldSpaceUI();
                spatialUIManager.StopScanningProgress();
            }

            SystemLogger.LogInfo("XRScouterManager", "Scan cancelled");
        }

        private void TriggerOverloadEffect()
        {
            SystemLogger.LogInfo("XRScouterManager", "TRIGGERING OVER 9000 EFFECT!");

            // Find and trigger overload effects
            var haloController = FindObjectOfType<HaloColorController>();
            if (haloController != null)
            {
                // Rapid color cycling for overload effect
                StartCoroutine(OverloadHaloEffect(haloController));
            }

            // Could trigger screen shake, particle effects, etc.
        }

        private System.Collections.IEnumerator OverloadHaloEffect(HaloColorController haloController)
        {
            float overloadDuration = 2f;
            float startTime = Time.time;

            while (Time.time - startTime < overloadDuration)
            {
                // Rapid power level changes for visual effect
                float randomPower = 9000f + Random.Range(-500f, 500f);
                haloController.SetPowerLevel(randomPower);

                yield return new WaitForSeconds(0.1f);
            }

            // Final overload power level
            haloController.SetPowerLevel(9500f);
        }

        // Public API
        public bool IsScanning()
        {
            return isScanning;
        }

        public float GetScanProgress()
        {
            return currentScanProgress;
        }

        public Vector3 GetScanTargetPosition()
        {
            return scanTargetPosition;
        }

        public GameObject GetScanTargetObject()
        {
            return scanTargetObject;
        }

        // Debug visualization
        void OnDrawGizmos()
        {
            if (isScanning && scanTargetPosition != Vector3.zero)
            {
                // Draw scan target
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(scanTargetPosition, 0.05f);

                // Draw scan progress ring
                Gizmos.color = Color.yellow;
                float ringRadius = currentScanProgress * 0.3f;
                Gizmos.DrawWireSphere(scanTargetPosition, ringRadius);
            }

            // Draw pointing ray if hand is pointing
            if (handRecognizer != null && handRecognizer.IsPointingGestureActive())
            {
                Gizmos.color = Color.green;
                Vector3 tipPos = handRecognizer.GetPointingTipPosition();
                Vector3 direction = handRecognizer.GetPointingDirection();
                Gizmos.DrawRay(tipPos, direction * maxScanDistance);
            }
        }
    }
}
