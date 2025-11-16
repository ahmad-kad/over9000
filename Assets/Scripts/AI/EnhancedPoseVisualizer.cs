using UnityEngine;
using UnityEngine.UI;

namespace ScouterXR.AI
{
    /// <summary>
    /// Enhanced pose visualizer with:
    /// - Bounding box around target
    /// - Skeleton bones visualization
    /// - Power level scoring
    /// - Target vs User distinction
    /// </summary>
    public class EnhancedPoseVisualizer : MonoBehaviour
    {
        [Header("References")]
        public MediaPipePoseEstimator targetPoseEstimator; // TARGET person tracking
        public HandPointingRecognizer userHandTracker;      // USER hand tracking
        public Canvas overlayCanvas;
        public RawImage webcamDisplay;

        [Header("Target Visualization")]
        public bool showTargetPose = true;
        public Color targetLandmarkColor = Color.red;
        public Color targetBoneColor = Color.yellow;
        public Color targetBoxColor = Color.green;
        [Tooltip("Size of pose landmark dots in pixels")]
        public float targetLandmarkSize = 20f; // Increased for visibility
        public float targetBoneWidth = 3f;
        public float targetBoxWidth = 4f;

        [Header("User Visualization")]
        public bool showUserHands = true;
        public Color userHandColor = Color.cyan;
        public float userHandSize = 8f;

        [Header("Power Level Display")]
        public bool showPowerLevel = true;
        public Text powerLevelText;
        public Text powerDescriptionText;
        public float scoreUpdateInterval = 0.1f;

        [Header("Performance")]
        [Tooltip("Update visualization every N frames. 1 = real-time, higher = better performance but less responsive")]
        public int updateEveryNFrames = 2; // Update every other frame for better performance

        [Header("Debug")]
        [Tooltip("Enable debug logging to see pose coordinate calculations")]
        public bool debugMode = true; // Enable by default for troubleshooting
        public bool flipPoseVertically = true; // If webcam appears upside down - DEFAULT TRUE

        // Internal state
        private int frameCounter = 0;
        private float lastScoreUpdate = 0f;
        private float currentPowerLevel = 0f;

        // UI Elements for target
        private Image[] targetLandmarkDots;
        private LineRenderer[] targetBoneLines;
        private LineRenderer boundingBoxLine;
        private GameObject targetVisualsContainer;

        // UI Elements for user hands
        private Image[] userHandDots;
        private GameObject userVisualsContainer;

        // MediaPipe body connections (33 landmarks)
        private static readonly int[][] BodyConnections = new int[][]
        {
            // Face
            new int[] { 0, 1 }, new int[] { 1, 2 }, new int[] { 2, 3 }, new int[] { 3, 7 },
            new int[] { 0, 4 }, new int[] { 4, 5 }, new int[] { 5, 6 }, new int[] { 6, 8 },
            new int[] { 9, 10 },
            
            // Torso
            new int[] { 11, 12 }, new int[] { 11, 13 }, new int[] { 13, 15 },
            new int[] { 15, 17 }, new int[] { 15, 19 }, new int[] { 15, 21 },
            new int[] { 12, 14 }, new int[] { 14, 16 }, new int[] { 16, 18 },
            new int[] { 16, 20 }, new int[] { 16, 22 },
            new int[] { 11, 23 }, new int[] { 12, 24 }, new int[] { 23, 24 },
            
            // Left leg
            new int[] { 23, 25 }, new int[] { 25, 27 }, new int[] { 27, 29 },
            new int[] { 29, 31 }, new int[] { 27, 31 },
            
            // Right leg
            new int[] { 24, 26 }, new int[] { 26, 28 }, new int[] { 28, 30 },
            new int[] { 30, 32 }, new int[] { 28, 32 }
        };

        private void Start()
        {
            FindReferences();
            CreateTargetVisualization();
            CreateUserVisualization();
            CreatePowerLevelUI();

            Debug.Log("[EnhancedPoseVisualizer] Initialized - Target tracking + User hand tracking + Power level scoring");
        }

        private void FindReferences()
        {
            if (targetPoseEstimator == null)
            {
                targetPoseEstimator = FindFirstObjectByType<MediaPipePoseEstimator>();
            }

            if (userHandTracker == null)
            {
                userHandTracker = FindFirstObjectByType<HandPointingRecognizer>();
            }

            if (overlayCanvas == null)
            {
                overlayCanvas = FindFirstObjectByType<Canvas>();
                if (debugMode && overlayCanvas != null)
                {
                    Debug.Log($"[EnhancedPoseVisualizer] Found Canvas: {overlayCanvas.name}, RenderMode: {overlayCanvas.renderMode}");
                }
            }

            if (webcamDisplay == null)
            {
                webcamDisplay = FindFirstObjectByType<RawImage>();
            }

            if (debugMode)
            {
                Debug.Log($"[EnhancedPoseVisualizer] References found - PoseEstimator: {targetPoseEstimator != null}, HandTracker: {userHandTracker != null}, Canvas: {overlayCanvas != null}, Webcam: {webcamDisplay != null}");
            }
        }

        private void CreateTargetVisualization()
        {
            if (overlayCanvas == null) return;

            targetVisualsContainer = new GameObject("Target Visuals Container");
            targetVisualsContainer.transform.SetParent(overlayCanvas.transform, false);

            RectTransform containerRect = targetVisualsContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Make sure it renders on top of other UI elements
            targetVisualsContainer.transform.SetAsLastSibling();

            // Create landmark dots
            targetLandmarkDots = new Image[33];
            for (int i = 0; i < 33; i++)
            {
                GameObject dotObj = new GameObject($"Target_Landmark_{i}");
                dotObj.transform.SetParent(targetVisualsContainer.transform, false);

                Image dotImage = dotObj.AddComponent<Image>();
                dotImage.color = targetLandmarkColor;

                RectTransform rectTransform = dotObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(targetLandmarkSize, targetLandmarkSize);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                dotObj.SetActive(false);
                targetLandmarkDots[i] = dotImage;
            }

            // Create bone lines
            targetBoneLines = new LineRenderer[BodyConnections.Length];
            for (int i = 0; i < BodyConnections.Length; i++)
            {
                GameObject lineObj = new GameObject($"Target_Bone_{i}");
                lineObj.transform.SetParent(targetVisualsContainer.transform, false);

                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.startWidth = targetBoneWidth;
                line.endWidth = targetBoneWidth;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = targetBoneColor;
                line.endColor = targetBoneColor;
                line.positionCount = 2;
                line.useWorldSpace = false;

                lineObj.SetActive(false);
                targetBoneLines[i] = line;
            }

            // Create bounding box
            GameObject boxObj = new GameObject("Target_BoundingBox");
            boxObj.transform.SetParent(targetVisualsContainer.transform, false);
            boundingBoxLine = boxObj.AddComponent<LineRenderer>();
            boundingBoxLine.startWidth = targetBoxWidth;
            boundingBoxLine.endWidth = targetBoxWidth;
            boundingBoxLine.material = new Material(Shader.Find("Sprites/Default"));
            boundingBoxLine.startColor = targetBoxColor;
            boundingBoxLine.endColor = targetBoxColor;
            boundingBoxLine.positionCount = 5; // Rectangle
            boundingBoxLine.loop = true;
            boundingBoxLine.useWorldSpace = false;
            boxObj.SetActive(false);

            Debug.Log($"[EnhancedPoseVisualizer] Created target visualization: {targetLandmarkDots.Length} landmarks, {targetBoneLines.Length} bones");
        }

        private void CreateUserVisualization()
        {
            if (overlayCanvas == null) return;

            userVisualsContainer = new GameObject("User Hand Visuals Container");
            userVisualsContainer.transform.SetParent(overlayCanvas.transform, false);
            
            RectTransform containerRect = userVisualsContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Create hand landmark dots (21 points per hand)
            userHandDots = new Image[21];
            for (int i = 0; i < 21; i++)
            {
                GameObject dotObj = new GameObject($"User_Hand_{i}");
                dotObj.transform.SetParent(userVisualsContainer.transform, false);

                Image dotImage = dotObj.AddComponent<Image>();
                dotImage.color = userHandColor;

                RectTransform rectTransform = dotObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(userHandSize, userHandSize);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                dotObj.SetActive(false);
                userHandDots[i] = dotImage;
            }

            Debug.Log($"[EnhancedPoseVisualizer] Created user hand visualization: {userHandDots.Length} hand landmarks");
        }

        private void CreatePowerLevelUI()
        {
            if (!showPowerLevel || overlayCanvas == null) return;

            // Create power level display if not assigned
            if (powerLevelText == null)
            {
                GameObject textObj = new GameObject("PowerLevelText");
                textObj.transform.SetParent(overlayCanvas.transform, false);

                RectTransform rectTransform = textObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.9f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.9f);
                rectTransform.sizeDelta = new Vector2(400, 100);

                powerLevelText = textObj.AddComponent<Text>();
                // Use default font instead of Arial.ttf which is deprecated
                powerLevelText.font = Font.CreateDynamicFontFromOSFont("Arial", 48);
                powerLevelText.fontSize = 48;
                powerLevelText.alignment = TextAnchor.MiddleCenter;
                powerLevelText.color = Color.green;
                powerLevelText.text = "POWER LEVEL: ---";
            }

            // Create description text
            if (powerDescriptionText == null)
            {
                GameObject descObj = new GameObject("PowerDescription");
                descObj.transform.SetParent(overlayCanvas.transform, false);

                RectTransform rectTransform = descObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.85f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.85f);
                rectTransform.sizeDelta = new Vector2(300, 50);

                powerDescriptionText = descObj.AddComponent<Text>();
                // Use default font instead of Arial.ttf which is deprecated
                powerDescriptionText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
                powerDescriptionText.fontSize = 24;
                powerDescriptionText.alignment = TextAnchor.MiddleCenter;
                powerDescriptionText.color = Color.yellow;
                powerDescriptionText.text = "";
            }
        }

        private void Update()
        {
            // Force real-time updates for pose tracking - no frame skipping
            // Performance optimization - but ensure pose tracking is real-time
            frameCounter++;
            if (frameCounter < updateEveryNFrames)
            {
                return;
            }
            frameCounter = 0;

            if (debugMode && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[EnhancedPoseVisualizer] Update() called, enabled={enabled}, gameObject.active={gameObject.activeSelf}, frameCounter={frameCounter}, updateEveryNFrames={updateEveryNFrames}");
            }

            // Update target pose visualization
            if (showTargetPose && targetPoseEstimator != null)
            {
                UpdateTargetVisualization();
            }
            else
            {
                if (debugMode && Time.frameCount % 300 == 0)
                {
                    Debug.LogWarning($"[EnhancedPoseVisualizer] Target pose visualization disabled or estimator null: showTargetPose={showTargetPose}, estimator={targetPoseEstimator != null}");
                }
                HideTargetVisualization();
            }

            // Update user hand visualization
            if (showUserHands && userHandTracker != null)
            {
                UpdateUserHandVisualization();
            }
            else
            {
                HideUserVisualization();
            }

            // Update power level (less frequently)
            if (showPowerLevel && Time.time - lastScoreUpdate > scoreUpdateInterval)
            {
                UpdatePowerLevel();
                lastScoreUpdate = Time.time;
            }
        }

        // Store previous pose for comparison
        private Vector3[] previousLandmarks;
        private float previousConfidence;

        private void UpdateTargetVisualization()
        {
            if (debugMode && Time.frameCount % 30 == 0)
            {
                Debug.Log("[EnhancedPoseVisualizer] UpdateTargetVisualization() called");
            }

            if (targetPoseEstimator == null)
            {
                if (debugMode && Time.frameCount % 300 == 0)
                    Debug.LogWarning("[EnhancedPoseVisualizer] targetPoseEstimator is NULL");
                HideTargetVisualization();
                return;
            }

            Vector3[] landmarks = targetPoseEstimator.LatestLandmarks;
            float confidence = targetPoseEstimator.LatestConfidence;

            if (debugMode && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[EnhancedPoseVisualizer] Got pose data: landmarks={landmarks?.Length ?? 0}, confidence={confidence:F3}, nose={(landmarks != null && landmarks.Length > 0 ? landmarks[0] : Vector3.zero)}");
            }

            if (landmarks == null || landmarks.Length < 33 || confidence < 0.3f)
            {
                if (debugMode && Time.frameCount % 300 == 0)
                    Debug.LogWarning($"[EnhancedPoseVisualizer] Invalid pose data: landmarks={landmarks?.Length ?? 0}, confidence={confidence:F3}");
                HideTargetVisualization();
                return;
            }

            // Check if pose data actually changed
            bool poseChanged = previousLandmarks == null || previousLandmarks.Length != landmarks.Length ||
                              Mathf.Abs(previousConfidence - confidence) > 0.001f;
            if (!poseChanged && previousLandmarks != null)
            {
                for (int i = 0; i < Mathf.Min(previousLandmarks.Length, landmarks.Length); i++)
                {
                    if (Vector3.Distance(previousLandmarks[i], landmarks[i]) > 0.01f) // Increased threshold for sensitivity
                    {
                        poseChanged = true;
                        break;
                    }
                }
            }

            // Store for next comparison
            previousLandmarks = (Vector3[])landmarks.Clone();
            previousConfidence = confidence;

            if (debugMode && (poseChanged || Time.frameCount % 60 == 0)) // Log when pose changes or every 60 frames
            {
                // Show all keypoints in one array for easy debugging
                string keypointsStr = "[";
                for (int i = 0; i < Mathf.Min(landmarks.Length, 17); i++) // Show first 17 keypoints (body)
                {
                    keypointsStr += $"({landmarks[i].x:F1},{landmarks[i].y:F1},{landmarks[i].z:F1})";
                    if (i < 16) keypointsStr += ",";
                }
                keypointsStr += "]";

                Debug.Log($"[EnhancedPoseVisualizer] 🎯 ALL KEYPOINTS: {keypointsStr} | confidence={confidence:F3} | changed={poseChanged} | nose=({landmarks[0].x:F2}, {landmarks[0].y:F2})");
            }

            // Create pose data
            PoseData pose = new PoseData
            {
                landmarks = landmarks,
                landmarkVisibility = new float[landmarks.Length]
            };
            for (int i = 0; i < pose.landmarkVisibility.Length; i++)
            {
                pose.landmarkVisibility[i] = confidence;
            }

            DrawTargetPose(pose);
        }

        private void DrawTargetPose(PoseData pose)
        {
            if (webcamDisplay == null) return;

            RectTransform webcamRect = webcamDisplay.rectTransform;
            Vector3[] webcamCorners = new Vector3[4];
            webcamRect.GetWorldCorners(webcamCorners);

            Vector2 webcamMin = overlayCanvas.transform.InverseTransformPoint(webcamCorners[0]);
            Vector2 webcamMax = overlayCanvas.transform.InverseTransformPoint(webcamCorners[2]);
            float webcamWidth = webcamMax.x - webcamMin.x;
            float webcamHeight = webcamMax.y - webcamMin.y;

            if (debugMode && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[EnhancedPoseVisualizer] Webcam bounds: {webcamMin} to {webcamMax}, Size: {webcamWidth}x{webcamHeight}");
                Debug.Log($"[EnhancedPoseVisualizer] Nose landmark: {pose.landmarks[0]}, flipPoseVertically: {flipPoseVertically}");
                float noseScreenX = webcamMin.x + (pose.landmarks[0].x * webcamWidth);
                float yCoord = flipPoseVertically ? pose.landmarks[0].y : (1f - pose.landmarks[0].y);
                float noseScreenY = webcamMin.y + (yCoord * webcamHeight);
                Debug.Log($"[EnhancedPoseVisualizer] Nose screen pos: ({noseScreenX:F1}, {noseScreenY:F1})");
            }

            // Calculate bounding box
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            // Draw landmarks and update bounding box
            for (int i = 0; i < targetLandmarkDots.Length && i < pose.landmarks.Length; i++)
            {
                Vector3 landmark = pose.landmarks[i];

                float screenX = webcamMin.x + (landmark.x * webcamWidth);
                // Apply vertical flip if webcam is upside down
                float yCoord = flipPoseVertically ? landmark.y : (1f - landmark.y);
                float screenY = webcamMin.y + (yCoord * webcamHeight);

                minX = Mathf.Min(minX, screenX);
                minY = Mathf.Min(minY, screenY);
                maxX = Mathf.Max(maxX, screenX);
                maxY = Mathf.Max(maxY, screenY);

                RectTransform dotRect = targetLandmarkDots[i].rectTransform;
                Vector2 oldPosition = dotRect.anchoredPosition;
                dotRect.anchoredPosition = new Vector2(screenX, screenY);

                bool isVisible = pose.landmarkVisibility[i] > 0.5f;
                targetLandmarkDots[i].gameObject.SetActive(isVisible);

                // Force UI update
                LayoutRebuilder.MarkLayoutForRebuild(dotRect);

                // Debug: Log position updates for key landmarks
                if (debugMode && (i == 0 || i == 11 || i == 12)) // Nose, left hip, right hip
                {
                    Vector2 newPosition = dotRect.anchoredPosition;
                    bool positionChanged = Vector2.Distance(oldPosition, newPosition) > 0.1f;
                    Debug.Log($"[EnhancedPoseVisualizer] Landmark {i} ({GetLandmarkName(i)}): calculated=({screenX:F1}, {screenY:F1}), set=({newPosition.x:F1}, {newPosition.y:F1}), visible={isVisible}, changed={positionChanged}");
                }
            }

            // Draw bones
            for (int i = 0; i < targetBoneLines.Length && i < BodyConnections.Length; i++)
            {
                int[] connection = BodyConnections[i];
                int startIdx = connection[0];
                int endIdx = connection[1];

                if (startIdx < pose.landmarks.Length && endIdx < pose.landmarks.Length)
                {
                    Vector3 startLandmark = pose.landmarks[startIdx];
                    Vector3 endLandmark = pose.landmarks[endIdx];

                    // Apply vertical flip if webcam is upside down
                    float startYCoord = flipPoseVertically ? startLandmark.y : (1f - startLandmark.y);
                    float endYCoord = flipPoseVertically ? endLandmark.y : (1f - endLandmark.y);

                    Vector3 startPos = new Vector3(
                        webcamMin.x + (startLandmark.x * webcamWidth),
                        webcamMin.y + (startYCoord * webcamHeight),
                        0
                    );

                    Vector3 endPos = new Vector3(
                        webcamMin.x + (endLandmark.x * webcamWidth),
                        webcamMin.y + (endYCoord * webcamHeight),
                        0
                    );

                    LineRenderer line = targetBoneLines[i];
                    line.SetPosition(0, startPos);
                    line.SetPosition(1, endPos);
                    line.gameObject.SetActive(true);
                }
            }

            // Draw bounding box with padding
            float padding = 20f;
            boundingBoxLine.SetPosition(0, new Vector3(minX - padding, minY - padding, 0));
            boundingBoxLine.SetPosition(1, new Vector3(maxX + padding, minY - padding, 0));
            boundingBoxLine.SetPosition(2, new Vector3(maxX + padding, maxY + padding, 0));
            boundingBoxLine.SetPosition(3, new Vector3(minX - padding, maxY + padding, 0));
            boundingBoxLine.SetPosition(4, new Vector3(minX - padding, minY - padding, 0));
            boundingBoxLine.gameObject.SetActive(true);

            // Force canvas update to ensure UI elements are redrawn
            if (overlayCanvas != null)
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        private void UpdateUserHandVisualization()
        {
            Vector3[] handLandmarks = userHandTracker.LatestHandLandmarks;
            
            if (handLandmarks == null || handLandmarks.Length < 21)
            {
                HideUserVisualization();
                return;
            }

            // Draw hand landmarks (simplified - would need proper coordinate mapping)
            for (int i = 0; i < userHandDots.Length && i < handLandmarks.Length; i++)
            {
                // TODO: Proper coordinate mapping for hand landmarks
                userHandDots[i].gameObject.SetActive(false); // Disabled for now
            }
        }

        private void UpdatePowerLevel()
        {
            if (targetPoseEstimator == null) return;

            Vector3[] landmarks = targetPoseEstimator.LatestLandmarks;
            float confidence = targetPoseEstimator.LatestConfidence;

            if (landmarks == null || landmarks.Length < 33)
            {
                if (powerLevelText != null) powerLevelText.text = "POWER LEVEL: ---";
                if (powerDescriptionText != null) powerDescriptionText.text = "No target detected";
                return;
            }

            PoseData pose = new PoseData
            {
                landmarks = landmarks,
                landmarkVisibility = new float[landmarks.Length]
            };
            for (int i = 0; i < pose.landmarkVisibility.Length; i++)
            {
                pose.landmarkVisibility[i] = confidence;
            }

            // Calculate power level
            currentPowerLevel = PoseScorer.CalculatePowerLevel(pose, confidence);
            string description = PoseScorer.GetPowerLevelDescription(currentPowerLevel);

            // Update UI
            if (powerLevelText != null)
            {
                powerLevelText.text = $"POWER LEVEL: {Mathf.RoundToInt(currentPowerLevel)}";
                
                // Color based on power level
                if (currentPowerLevel > 8500)
                {
                    powerLevelText.color = Color.red; // OVER 9000!!!
                }
                else if (currentPowerLevel > 5000)
                {
                    powerLevelText.color = Color.yellow;
                }
                else
                {
                    powerLevelText.color = Color.green;
                }
            }

            if (powerDescriptionText != null)
            {
                powerDescriptionText.text = description;
            }

            // Force canvas update to ensure visual changes are applied
            if (overlayCanvas != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(overlayCanvas.GetComponent<RectTransform>());
            }
        }

        private void HideTargetVisualization()
        {
            if (targetLandmarkDots != null)
            {
                foreach (var dot in targetLandmarkDots)
                {
                    if (dot != null) dot.gameObject.SetActive(false);
                }
            }

            if (targetBoneLines != null)
            {
                foreach (var line in targetBoneLines)
                {
                    if (line != null) line.gameObject.SetActive(false);
                }
            }

            if (boundingBoxLine != null)
            {
                boundingBoxLine.gameObject.SetActive(false);
            }
        }

        private void HideUserVisualization()
        {
            if (userHandDots != null)
            {
                foreach (var dot in userHandDots)
                {
                    if (dot != null) dot.gameObject.SetActive(false);
                }
            }
        }

        public float GetCurrentPowerLevel()
        {
            return currentPowerLevel;
        }

        // Helper method for debugging
        private string GetLandmarkName(int index)
        {
            string[] names = {
                "nose", "left_eye", "right_eye", "left_ear", "right_ear",
                "left_shoulder", "right_shoulder", "left_elbow", "right_elbow",
                "left_wrist", "right_wrist", "left_hip", "right_hip",
                "left_knee", "right_knee", "left_ankle", "right_ankle"
            };
            return index < names.Length ? names[index] : $"landmark_{index}";
        }
    }
}

