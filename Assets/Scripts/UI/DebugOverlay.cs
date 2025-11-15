using UnityEngine;
using UnityEngine.UI;
using ScouterXR.Core;
using ScouterXR.AI;
using ScouterXR.AR;

namespace ScouterXR.UI
{
    public class DebugOverlay : MonoBehaviour
    {
        [Header("UI Components")]
        public Canvas debugCanvas;
        public Text debugText;
        public Button toggleButton;

        [Header("System References")]
        public PerformanceMonitor performanceMonitor;
        public MediaPipePoseEstimator poseEstimator;
        public ArFeatureManager arFeatureManager;
        public FallbackManager fallbackManager;
        public HandPointingRecognizer handRecognizer;

        [Header("Display Options")]
        public bool showPoseData = true;
        public bool showHandData = true;
        public bool showWebcamFeed = true;

        [Header("Display Settings")]
        public bool showByDefault = false;
        public float updateInterval = 0.5f;

        private bool isVisible = false;
        private float lastUpdateTime = 0f;

        void Start()
        {
            if (debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(showByDefault);
                isVisible = showByDefault;
            }

            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleVisibility);
            }

            // Create toggle button if it doesn't exist
            if (toggleButton == null)
            {
                CreateToggleButton();
            }
        }

        void Update()
        {
            if (!isVisible) return;

            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateDebugInfo();
                lastUpdateTime = Time.time;
            }
        }

        private void CreateToggleButton()
        {
            if (debugCanvas == null) return;

            // Create a simple toggle button in bottom-right corner
            GameObject buttonObj = new GameObject("Debug Toggle");
            buttonObj.transform.SetParent(debugCanvas.transform, false);

            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.95f, 0.05f);
            rectTransform.anchorMax = new Vector2(0.99f, 0.1f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image bgImage = buttonObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            Button button = buttonObj.AddComponent<Button>();
            toggleButton = button;

            // Add text to button
            GameObject textObj = new GameObject("Button Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = "DEBUG";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 12;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            button.onClick.AddListener(ToggleVisibility);
        }

        public void ToggleVisibility()
        {
            isVisible = !isVisible;
            if (debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(isVisible);
            }

            SystemLogger.LogInfo("DebugOverlay", $"Debug overlay {(isVisible ? "shown" : "hidden")}");
        }

        private void UpdateDebugInfo()
        {
            if (debugText == null) return;

            string debugInfo = BuildDebugInfo();
            debugText.text = debugInfo;
        }

        private string BuildDebugInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("=== SCOUTER XR DEBUG ===");
            sb.AppendLine($"Time: {Time.time:F1}s");
            sb.AppendLine($"Unity: {Application.unityVersion}");

            // Performance
            if (performanceMonitor != null)
            {
                var report = performanceMonitor.GetPerformanceReport();
                sb.AppendLine($"FPS: {report["averageFPS"]:F1}");
                sb.AppendLine($"Memory: {report["memoryUsageMB"]}MB");
                sb.AppendLine($"Quality: {report["qualityLevel"]}");
            }

            // Pose Detection
            if (poseEstimator != null && showPoseData)
            {
                sb.AppendLine($"Pose Depth: {poseEstimator.GetCurrentEstimatedDepth():F1}m");
                sb.AppendLine($"Power Level: {poseEstimator.GetPowerLevelFromCurrentPose():F0}");
                // Note: Pose validity would need to be exposed if needed
            }

            // Hand Detection
            if (handRecognizer != null && showHandData)
            {
                bool handDetected = handRecognizer.IsHandDetected();
                int handCount = handRecognizer.GetDetectedHandCount();
                bool pointing = handRecognizer.IsPointingGestureActive();
                float pointingConf = handRecognizer.GetPointingConfidence();

                sb.AppendLine($"Hands: {handCount} detected");
                sb.AppendLine($"Pointing: {(pointing ? "YES" : "NO")} ({pointingConf:F2})");

                if (pointing)
                {
                    Vector3 tipPos = handRecognizer.GetPointingTipPosition();
                    Vector3 direction = handRecognizer.GetPointingDirection();
                    sb.AppendLine($"Tip Pos: ({tipPos.x:F2}, {tipPos.y:F2}, {tipPos.z:F2})");
                    sb.AppendLine($"Direction: ({direction.x:F2}, {direction.y:F2}, {direction.z:F2})");
                }
            }

            // Webcam Feed
            if (showWebcamFeed)
            {
                var webcams = WebCamTexture.devices;
                sb.AppendLine($"Webcams: {webcams.Length} available");
                if (webcams.Length > 0)
                {
                    sb.AppendLine($"Primary: {webcams[0].name}");
                }
            }

            // AR Features
            if (arFeatureManager != null)
            {
                sb.AppendLine($"Depth Mode: {arFeatureManager.GetCurrentOcclusionMode()}");
            }

            // System Status
            if (fallbackManager != null)
            {
                var status = fallbackManager.GetSystemStatus();
                sb.AppendLine($"System Status: {status.GetSummary()}");
            }

            // Device Info
            sb.AppendLine($"Device: {SystemInfo.deviceModel}");
            sb.AppendLine($"Platform: {Application.platform}");
            sb.AppendLine($"Graphics: {SystemInfo.graphicsDeviceName}");

            // Log file info
            if (FindObjectOfType<SystemLogger>() != null)
            {
                var logger = FindObjectOfType<SystemLogger>();
                sb.AppendLine($"Log Size: {logger.GetLogFileSize() / 1024}KB");
            }

            return sb.ToString();
        }

        // Public methods for external control
        public void ShowDebugOverlay()
        {
            isVisible = true;
            if (debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(true);
            }
        }

        public void HideDebugOverlay()
        {
            isVisible = false;
            if (debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(false);
            }
        }

        public string GetCurrentDebugInfo()
        {
            return BuildDebugInfo();
        }

        // Cleanup
        void OnDestroy()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(ToggleVisibility);
            }
        }
    }
}
