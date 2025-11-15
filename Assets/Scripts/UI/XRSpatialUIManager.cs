using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using ScouterXR.Core;
using ScouterXR.AI;
using TMPro;
using DG.Tweening;
using System.Linq;

namespace ScouterXR.UI
{
    /// <summary>
    /// Complete XR UI Manager with spatial anchoring for DBZ Scouter XR
    /// Handles world-space UI, AR anchoring, and cross-platform XR interactions
    /// </summary>
    public class XRSpatialUIManager : MonoBehaviour
    {
        [Header("AR Foundation Components")]
        public ARAnchorManager anchorManager;
        public ARRaycastManager raycastManager;
        public Camera arCamera;

        [Header("UI Prefabs")]
        public GameObject worldSpaceUIPrefab; // 3D UI that floats in space
        public GameObject screenSpaceUIPrefab; // Traditional 2D UI overlay

        [Header("Spatial Settings")]
        public float uiDistanceFromTarget = 0.5f; // Meters above target
        public Vector3 uiOffset = new Vector3(0, 0.2f, 0); // Offset from target center
        public float uiScale = 0.001f; // World-space UI scale factor
        public bool uiAlwaysFacesUser = true;

        [Header("References")]
        public ScouterUI screenUI; // Existing screen-space UI
        public HaloColorController haloController; // Existing halo effect
        public XRScouterManager scouterManager;
        public MediaPipePoseEstimator poseEstimator;

        [Header("Scanning Progress UI")]
        public GameObject scanningProgressPrefab; // Circular progress indicator
        public float progressUISize = 0.2f; // Size of progress circle in meters
        public Color progressColor = Color.cyan;
        public Color progressCompleteColor = Color.red;

        [Header("Height Display")]
        public bool showHeightDisplay = true; // Hardcoded on as requested

        // Spatial tracking
        private Dictionary<string, TrackedTarget> trackedTargets = new Dictionary<string, TrackedTarget>();
        private int targetCounter = 0;

        // UI instances
        private GameObject activeWorldUI;
        private CanvasGroup worldUICanvas;
        private TextMeshProUGUI worldPowerLevelText;
        private TextMeshProUGUI worldDistanceText;
        private TextMeshProUGUI worldHeightText;

        // Scanning progress UI
        private GameObject scanningProgressUI;
        private UnityEngine.UI.Image progressFillImage;
        private Material progressMaterial;
        private bool isScanning = false;

        void Start()
        {
            InitializeARComponents();
            CreateWorldSpaceUI();
            CreateScanningProgressUI();
            SystemLogger.LogInfo("XRSpatialUIManager", "XR Spatial UI Manager initialized");
        }

        void Update()
        {
            UpdateTrackedTargets();
            UpdateWorldSpaceUI();
        }

        #region AR Foundation Setup

        private void InitializeARComponents()
        {
            // Auto-find AR components if not assigned
            if (anchorManager == null)
                anchorManager = FindObjectOfType<ARAnchorManager>();

            if (raycastManager == null)
                raycastManager = FindObjectOfType<ARRaycastManager>();

            if (arCamera == null)
                arCamera = Camera.main;

            // Create AR components if they don't exist
            if (anchorManager == null)
            {
                var sessionOrigin = FindObjectOfType<ARSessionOrigin>();
                if (sessionOrigin != null)
                {
                    anchorManager = sessionOrigin.gameObject.AddComponent<ARAnchorManager>();
                    SystemLogger.LogInfo("XRSpatialUIManager", "Created ARAnchorManager");
                }
            }
        }

        #endregion

        #region Spatial Target Tracking

        /// <summary>
        /// Called when XRScouterManager detects a new target via pointing gesture
        /// </summary>
        public void OnTargetDetected(Vector3 position, GameObject hitObject = null)
        {
            string targetId = CreateTargetId();

            // Calculate target height from pose estimation
            float targetHeight = CalculateTargetHeight();

            // Create AR anchor for persistent spatial tracking
            ARAnchor anchor = CreateSpatialAnchor(position, targetId);

            // Create tracked target entry
            var trackedTarget = new TrackedTarget
            {
                id = targetId,
                anchor = anchor,
                worldPosition = position,
                screenPosition = arCamera.WorldToScreenPoint(position),
                lastSeenTime = Time.time,
                isActive = true,
                powerLevel = 1000f, // Initial value
                distance = Vector3.Distance(arCamera.transform.position, position),
                height = targetHeight
            };

            trackedTargets[targetId] = trackedTarget;

            // Update UI to show this target
            SetActiveTarget(targetId);

            SystemLogger.LogInfo("XRSpatialUIManager", $"Created spatially anchored target: {targetId} at {position}, height: {targetHeight:F2}m");
        }

        private float CalculateTargetHeight()
        {
            // For now, use estimated depth to calculate approximate height
            // TODO: Enhance with direct pose keypoint access when available
            if (poseEstimator != null)
            {
                float estimatedDistance = poseEstimator.GetCurrentEstimatedDepth();
                if (estimatedDistance > 0)
                {
                    // Estimate height based on distance (people appear smaller when farther away)
                    // Average human height is ~1.75m, scale inversely with distance
                    float baseHeight = 1.75f;
                    float scalingFactor = Mathf.Clamp(3.0f / estimatedDistance, 0.8f, 2.0f);
                    return baseHeight * scalingFactor;
                }
            }

            // Default fallback height
            return 1.75f; // Average human height
        }

        /// <summary>
        /// Create persistent AR anchor for spatial tracking
        /// </summary>
        private ARAnchor CreateSpatialAnchor(Vector3 position, string targetId)
        {
            if (anchorManager == null)
            {
                SystemLogger.LogWarning("XRSpatialUIManager", "No ARAnchorManager available, cannot create spatial anchors");
                return null;
            }

            Pose anchorPose = new Pose(position, Quaternion.identity);
            ARAnchor anchor = anchorManager.AddAnchor(anchorPose);

            if (anchor != null)
            {
                anchor.name = $"TargetAnchor_{targetId}";
                SystemLogger.LogInfo("XRSpatialUIManager", $"Created AR anchor for target {targetId}");
            }

            return anchor;
        }

        /// <summary>
        /// Update target data from pose estimation and scanning
        /// </summary>
        public void UpdateTargetData(string targetId, float powerLevel, Vector3 estimatedPosition = default)
        {
            if (!trackedTargets.ContainsKey(targetId)) return;

            var target = trackedTargets[targetId];
            target.powerLevel = powerLevel;
            target.lastSeenTime = Time.time;

            // Update position if we have better pose estimation data
            if (estimatedPosition != default)
            {
                target.worldPosition = estimatedPosition;
                target.distance = Vector3.Distance(arCamera.transform.position, estimatedPosition);
                target.screenPosition = arCamera.WorldToScreenPoint(estimatedPosition);

                // Update anchor position if anchor exists
                if (target.anchor != null)
                {
                    target.anchor.transform.position = estimatedPosition;
                }
            }

            trackedTargets[targetId] = target;
        }

        /// <summary>
        /// Remove target when lost or scanning stops
        /// </summary>
        public void RemoveTarget(string targetId)
        {
            if (!trackedTargets.ContainsKey(targetId)) return;

            var target = trackedTargets[targetId];

            // Destroy AR anchor
            if (target.anchor != null)
            {
                Destroy(target.anchor.gameObject);
            }

            trackedTargets.Remove(targetId);
            SystemLogger.LogInfo("XRSpatialUIManager", $"Removed spatially anchored target: {targetId}");
        }

        #endregion

        #region World-Space UI Management

        private void CreateWorldSpaceUI()
        {
            if (worldSpaceUIPrefab == null)
            {
                CreateDefaultWorldSpaceUI();
                return;
            }

            activeWorldUI = Instantiate(worldSpaceUIPrefab);
            activeWorldUI.name = "WorldSpaceScouterUI";

            // Find UI components
            worldUICanvas = activeWorldUI.GetComponentInChildren<CanvasGroup>();
            var texts = activeWorldUI.GetComponentsInChildren<TextMeshProUGUI>();

            foreach (var text in texts)
            {
                if (text.name.Contains("Power") || text.name.Contains("Level"))
                    worldPowerLevelText = text;
                else if (text.name.Contains("Distance"))
                    worldDistanceText = text;
                else if (text.name.Contains("Height"))
                    worldHeightText = text;
            }

            // Initially hide
            if (worldUICanvas != null)
                worldUICanvas.alpha = 0f;
        }

        private void CreateDefaultWorldSpaceUI()
        {
            // Create world-space UI canvas
            activeWorldUI = new GameObject("WorldSpaceScouterUI");

            // Add Canvas for world-space rendering
            var canvas = activeWorldUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = arCamera;

            // Add canvas scaler
            var scaler = activeWorldUI.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            // Add graphic raycaster
            activeWorldUI.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create background panel
            var panel = new GameObject("Panel");
            panel.transform.SetParent(activeWorldUI.transform, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(300, 150);

            var panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // Add canvas group for fading
            worldUICanvas = panel.AddComponent<CanvasGroup>();
            worldUICanvas.alpha = 0f;

            // Create power level text
            var powerTextObj = new GameObject("PowerLevelText");
            powerTextObj.transform.SetParent(panel.transform, false);

            var powerRect = powerTextObj.AddComponent<RectTransform>();
            powerRect.anchorMin = Vector2.zero;
            powerRect.anchorMax = Vector2.one;
            powerRect.offsetMin = new Vector2(20, 20);
            powerRect.offsetMax = new Vector2(-20, -20);

            worldPowerLevelText = powerTextObj.AddComponent<TextMeshProUGUI>();
            worldPowerLevelText.text = "Power Level: Scanning...";
            worldPowerLevelText.fontSize = 32;
            worldPowerLevelText.alignment = TextAlignmentOptions.Center;
            worldPowerLevelText.color = Color.green;

            // Create distance text
            var distanceTextObj = new GameObject("DistanceText");
            distanceTextObj.transform.SetParent(panel.transform, false);

            var distRect = distanceTextObj.AddComponent<RectTransform>();
            distRect.anchorMin = new Vector2(0, 0);
            distRect.anchorMax = new Vector2(1, 0.4f);
            distRect.offsetMin = new Vector2(20, 20);
            distRect.offsetMax = new Vector2(-20, -10);

            worldDistanceText = distanceTextObj.AddComponent<TextMeshProUGUI>();
            worldDistanceText.text = "Distance: --m";
            worldDistanceText.fontSize = 24;
            worldDistanceText.alignment = TextAlignmentOptions.BottomLeft;
            worldDistanceText.color = Color.white;

            // Create height text
            var heightTextObj = new GameObject("HeightText");
            heightTextObj.transform.SetParent(panel.transform, false);

            var heightRect = heightTextObj.AddComponent<RectTransform>();
            heightRect.anchorMin = new Vector2(0.5f, 0.3f);
            heightRect.anchorMax = new Vector2(0.9f, 0.7f);
            heightRect.offsetMin = new Vector2(10, 10);
            heightRect.offsetMax = new Vector2(-10, -10);

            worldHeightText = heightTextObj.AddComponent<TextMeshProUGUI>();
            worldHeightText.text = "Height: --m";
            worldHeightText.fontSize = 20;
            worldHeightText.alignment = TextAlignmentOptions.BottomRight;
            worldHeightText.color = Color.yellow;

            SystemLogger.LogInfo("XRSpatialUIManager", "Created default world-space UI with height display");
        }

        private void CreateScanningProgressUI()
        {
            if (scanningProgressPrefab != null)
            {
                // Use provided prefab
                scanningProgressUI = Instantiate(scanningProgressPrefab);
                scanningProgressUI.name = "ScanningProgressUI";

                // Find progress fill image
                progressFillImage = scanningProgressUI.GetComponentInChildren<UnityEngine.UI.Image>();
                if (progressFillImage != null && progressFillImage.type == UnityEngine.UI.Image.Type.Filled)
                {
                    // Good, it's already set up as a filled image
                    progressFillImage.fillAmount = 0f;
                }
            }
            else
            {
                // Create default circular progress UI
                CreateDefaultScanningProgressUI();
            }

            // Initially hide
            if (scanningProgressUI != null)
            {
                scanningProgressUI.SetActive(false);
            }
        }

        private void CreateDefaultScanningProgressUI()
        {
            // Create world-space progress indicator
            scanningProgressUI = new GameObject("ScanningProgressUI");

            // Add Canvas for world-space rendering
            var canvas = scanningProgressUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = arCamera;

            // Add canvas scaler
            var scaler = scanningProgressUI.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;

            // Create circular progress background
            var bgObj = new GameObject("ProgressBackground");
            bgObj.transform.SetParent(scanningProgressUI.transform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(progressUISize * 1000f, progressUISize * 1000f);

            var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.sprite = CreateCircleSprite();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // Create progress fill
            var fillObj = new GameObject("ProgressFill");
            fillObj.transform.SetParent(bgObj.transform, false);

            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            progressFillImage = fillObj.AddComponent<UnityEngine.UI.Image>();
            progressFillImage.sprite = CreateCircleSprite();
            progressFillImage.type = UnityEngine.UI.Image.Type.Filled;
            progressFillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
            progressFillImage.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
            progressFillImage.fillAmount = 0f;
            progressFillImage.color = progressColor;

            SystemLogger.LogInfo("XRSpatialUIManager", "Created default scanning progress UI");
        }

        private UnityEngine.Sprite CreateCircleSprite()
        {
            // Create a simple circular texture
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    colors[index] = distance <= radius ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            return UnityEngine.Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private void UpdateWorldSpaceUI()
        {
            if (activeWorldUI == null || trackedTargets.Count == 0) return;

            // Find active target
            TrackedTarget activeTarget = null;
            foreach (var target in trackedTargets.Values)
            {
                if (target.isActive)
                {
                    activeTarget = target;
                    break;
                }
            }

            if (activeTarget == null || activeTarget.anchor == null) return;

            // Position UI relative to anchor
            Vector3 uiPosition = activeTarget.anchor.transform.position +
                               activeTarget.anchor.transform.TransformVector(uiOffset);

            // Apply distance offset (UI floats at fixed distance from target)
            Vector3 directionToUI = (uiPosition - arCamera.transform.position).normalized;
            uiPosition = activeTarget.anchor.transform.position + directionToUI * uiDistanceFromTarget;

            activeWorldUI.transform.position = uiPosition;
            activeWorldUI.transform.localScale = Vector3.one * uiScale;

            // Make UI face the user
            if (uiAlwaysFacesUser)
            {
                activeWorldUI.transform.LookAt(arCamera.transform);
                activeWorldUI.transform.Rotate(0, 180, 0); // Flip to face user correctly
            }

            // Update text content
            if (worldPowerLevelText != null)
            {
                worldPowerLevelText.text = $"Power Level: {activeTarget.powerLevel:F0}";
                worldPowerLevelText.color = GetPowerLevelColor(activeTarget.powerLevel);
            }

            if (worldDistanceText != null)
            {
                worldDistanceText.text = $"Distance: {activeTarget.distance:F1}m";
            }

            if (worldHeightText != null && showHeightDisplay)
            {
                worldHeightText.text = $"Height: {activeTarget.height:F2}m";
            }
            else if (worldHeightText != null && !showHeightDisplay)
            {
                worldHeightText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region UI State Management

        public void SetActiveTarget(string targetId)
        {
            // Deactivate all targets
            foreach (var target in trackedTargets.Values)
            {
                target.isActive = false;
            }

            // Activate selected target
            if (trackedTargets.ContainsKey(targetId))
            {
                trackedTargets[targetId].isActive = true;
                ShowWorldSpaceUI();
            }
        }

        public void ShowWorldSpaceUI()
        {
            if (worldUICanvas != null)
            {
                worldUICanvas.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
            }

            // Also show screen UI for compatibility
            if (screenUI != null)
            {
                screenUI.ShowScouter();
            }
        }

        public void HideWorldSpaceUI()
        {
            if (worldUICanvas != null)
            {
                worldUICanvas.DOFade(0f, 0.3f).SetEase(Ease.InQuad);
            }

            if (screenUI != null)
            {
                screenUI.HideScouter();
            }
        }

        public void UpdateScanningProgress(float progress, float currentPowerLevel)
        {
            if (trackedTargets.Count == 0) return;

            // Update active target's power level during scanning
            foreach (var target in trackedTargets.Values)
            {
                if (target.isActive)
                {
                    UpdateTargetData(target.id, currentPowerLevel);
                    UpdateScanningProgressUI(progress, target);
                    break;
                }
            }
        }

        private void UpdateScanningProgressUI(float progress, TrackedTarget target)
        {
            if (scanningProgressUI == null) return;

            // Position progress UI based on gaze or finger pointing
            Vector3 progressPosition = GetGazeOrFingerPosition();

            if (progressPosition != Vector3.zero)
            {
                scanningProgressUI.transform.position = progressPosition;
                scanningProgressUI.transform.localScale = Vector3.one * progressUISize;

                // Make progress UI face the user
                scanningProgressUI.transform.LookAt(arCamera.transform);
                scanningProgressUI.transform.Rotate(0, 180, 0);

                // Update progress fill
                if (progressFillImage != null)
                {
                    progressFillImage.fillAmount = progress;

                    // Change color as progress increases
                    if (progress >= 1f)
                    {
                        progressFillImage.color = progressCompleteColor;
                    }
                    else
                    {
                        progressFillImage.color = progressColor;
                    }
                }

                // Show progress UI if scanning
                if (!isScanning)
                {
                    scanningProgressUI.SetActive(true);
                    isScanning = true;
                }
            }
        }

        private Vector3 GetGazeOrFingerPosition()
        {
            // Try eye tracking first (if available)
            Vector3 gazePosition = GetEyeGazePosition();
            if (gazePosition != Vector3.zero)
            {
                return gazePosition;
            }

            // Fallback to finger pointing
            return GetFingerPointingPosition();
        }

        private Vector3 GetEyeGazePosition()
        {
            // TODO: Implement eye tracking integration
            // For now, return zero (will use finger pointing)
            // This would integrate with XR Eye Tracking subsystem
            return Vector3.zero;
        }

        private Vector3 GetFingerPointingPosition()
        {
            // Get finger pointing position from HandPointingRecognizer
            var handRecognizer = FindObjectOfType<ScouterXR.AI.HandPointingRecognizer>();
            if (handRecognizer != null && handRecognizer.IsPointingGestureActive())
            {
                Vector3 tipPosition = handRecognizer.GetPointingTipPosition();
                Vector3 direction = handRecognizer.GetPointingDirection();

                // Project forward from finger tip
                return tipPosition + direction * 0.5f; // 0.5m in front of finger
            }

            return Vector3.zero;
        }

        public void StopScanningProgress()
        {
            if (scanningProgressUI != null)
            {
                scanningProgressUI.SetActive(false);
            }
            isScanning = false;
        }

        #endregion

        #region Utility Methods

        private string CreateTargetId()
        {
            return $"target_{targetCounter++}_{Time.frameCount}";
        }

        private Color GetPowerLevelColor(float powerLevel)
        {
            if (powerLevel >= 9000f)
                return Color.red; // Over 9000 - red
            else if (powerLevel >= 7000f)
                return Color.yellow; // High power - yellow
            else if (powerLevel >= 5000f)
                return new Color(1f, 0.5f, 0f); // Orange
            else if (powerLevel >= 3000f)
                return Color.green; // Medium power - green
            else
                return Color.blue; // Low power - blue
        }

        private void UpdateTrackedTargets()
        {
            // Remove stale targets (not seen for 10 seconds)
            List<string> toRemove = new List<string>();
            foreach (var kvp in trackedTargets)
            {
                if (Time.time - kvp.Value.lastSeenTime > 10f)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var targetId in toRemove)
            {
                RemoveTarget(targetId);
            }

            // Update screen positions for all targets
            foreach (var target in trackedTargets.Values)
            {
                if (target.anchor != null)
                {
                    target.worldPosition = target.anchor.transform.position;
                    target.distance = Vector3.Distance(arCamera.transform.position, target.worldPosition);
                    target.screenPosition = arCamera.WorldToScreenPoint(target.worldPosition);
                }
            }
        }

        #endregion

        #region Public API

        public bool HasActiveTarget()
        {
            return trackedTargets.Count > 0 &&
                   trackedTargets.Values.Any(t => t.isActive);
        }

        public TrackedTarget GetActiveTarget()
        {
            return trackedTargets.Values.FirstOrDefault(t => t.isActive);
        }

        public List<TrackedTarget> GetAllTargets()
        {
            return new List<TrackedTarget>(trackedTargets.Values);
        }

        public void ClearAllTargets()
        {
            foreach (var targetId in new List<string>(trackedTargets.Keys))
            {
                RemoveTarget(targetId);
            }
            HideWorldSpaceUI();
        }

        #endregion

        #region Data Structures

        [System.Serializable]
        public class TrackedTarget
        {
            public string id;
            public ARAnchor anchor; // Spatial anchor for persistence
            public Vector3 worldPosition;
            public Vector3 screenPosition;
            public float powerLevel;
            public float distance;
            public float height; // Target height in meters
            public float lastSeenTime;
            public bool isActive;
        }

        #endregion

        void OnDestroy()
        {
            ClearAllTargets();
        }
    }
}
