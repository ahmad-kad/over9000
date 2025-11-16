using UnityEngine;
using UnityEngine.UI;
using ScouterXR.AI;

namespace ScouterXR.AI
{
    /// <summary>
    /// Data structure representing pose landmarks with visibility information.
    /// </summary>
    public struct PoseData
    {
        public Vector3[] landmarks;
        public float[] landmarkVisibility;
        public bool IsValid => landmarks != null && landmarks.Length > 0;
    }

    /// <summary>
    /// Simple pose visualizer that draws MediaPipe landmarks on a canvas overlay.
    /// Core feature: video stream → MediaPipe → pose visualization
    /// </summary>
    public class SimplePoseVisualizer : MonoBehaviour
    {
        [Header("References")]
        public MediaPipePoseEstimator poseEstimator;
        public Canvas overlayCanvas;
        public RawImage webcamDisplay;

        [Header("Visualization Settings")]
        public bool showLandmarks = true;
        public bool showConnections = true;
        public bool showHandLandmarks = true;
        public Color landmarkColor = Color.green;
        public Color connectionColor = Color.cyan;
        public Color handColor = Color.yellow;
        public float landmarkSize = 10f;
        public float connectionWidth = 2f;

        [Header("Performance")]
        public int updateEveryNFrames = 1;

        // Internal state
        private int frameCounter = 0;
        private GameObject landmarksContainer;
        private Image[] landmarkDots;
        private LineRenderer[] connectionLines;

        // MediaPipe pose connections (33 landmarks)
        private static readonly int[][] PoseConnections = new int[][]
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
            // Find components if not assigned
            if (poseEstimator == null)
            {
                poseEstimator = FindFirstObjectByType<MediaPipePoseEstimator>();
            }

            if (overlayCanvas == null)
            {
                overlayCanvas = FindFirstObjectByType<Canvas>();
            }

            if (webcamDisplay == null)
            {
                webcamDisplay = FindFirstObjectByType<RawImage>();
            }

            // Create landmarks container
            CreateVisualizationObjects();

            Debug.Log("[SimplePoseVisualizer] Pose visualizer initialized - ready to display landmarks");
        }

        private void CreateVisualizationObjects()
        {
            if (overlayCanvas == null)
            {
                Debug.LogError("[SimplePoseVisualizer] No canvas found for visualization overlay");
                return;
            }

            // Create container for all visualization objects
            landmarksContainer = new GameObject("Pose Landmarks Container");
            landmarksContainer.transform.SetParent(overlayCanvas.transform, false);
            
            RectTransform containerRect = landmarksContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Create landmark dots (33 MediaPipe pose landmarks)
            landmarkDots = new Image[33];
            for (int i = 0; i < 33; i++)
            {
                GameObject dotObj = new GameObject($"Landmark_{i}");
                dotObj.transform.SetParent(landmarksContainer.transform, false);

                Image dotImage = dotObj.AddComponent<Image>();
                dotImage.color = landmarkColor;

                RectTransform rectTransform = dotObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(landmarkSize, landmarkSize);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                dotObj.SetActive(false); // Hidden until we have pose data
                landmarkDots[i] = dotImage;
            }

            // Create connection lines
            connectionLines = new LineRenderer[PoseConnections.Length];
            for (int i = 0; i < PoseConnections.Length; i++)
            {
                GameObject lineObj = new GameObject($"Connection_{i}");
                lineObj.transform.SetParent(landmarksContainer.transform, false);

                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.startWidth = connectionWidth;
                line.endWidth = connectionWidth;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = connectionColor;
                line.endColor = connectionColor;
                line.positionCount = 2;
                line.useWorldSpace = false;

                lineObj.SetActive(false); // Hidden until we have pose data
                connectionLines[i] = line;
            }

            Debug.Log($"[SimplePoseVisualizer] Created {landmarkDots.Length} landmark dots and {connectionLines.Length} connection lines");
        }

        private void Update()
        {
            // Performance optimization: skip frames
            frameCounter++;
            if (frameCounter < updateEveryNFrames)
            {
                return;
            }
            frameCounter = 0;

            // Update visualization
            if (poseEstimator != null)
            {
                Vector3[] landmarks = poseEstimator.LatestLandmarks;
                if (landmarks != null && landmarks.Length > 0)
                {
                    PoseData pose = new PoseData
                    {
                        landmarks = landmarks,
                        landmarkVisibility = new float[landmarks.Length]  // Default visibility
                    };
                    // Set default visibility based on confidence
                    for (int i = 0; i < pose.landmarkVisibility.Length; i++)
                    {
                        pose.landmarkVisibility[i] = poseEstimator.LatestConfidence > 0.5f ? 1f : 0f;
                    }
                    UpdatePoseVisualization(pose);
                }
                else
                {
                    HideVisualization();
                }
            }
            else
            {
                HideVisualization();
            }
        }

        private void UpdatePoseVisualization(PoseData pose)
        {
            if (landmarkDots == null || webcamDisplay == null)
            {
                return;
            }

            // Get canvas dimensions for coordinate mapping
            RectTransform canvasRect = overlayCanvas.GetComponent<RectTransform>();
            RectTransform webcamRect = webcamDisplay.rectTransform;

            // Calculate webcam display bounds in canvas space
            Vector3[] webcamCorners = new Vector3[4];
            webcamRect.GetWorldCorners(webcamCorners);

            Vector2 webcamMin = overlayCanvas.transform.InverseTransformPoint(webcamCorners[0]);
            Vector2 webcamMax = overlayCanvas.transform.InverseTransformPoint(webcamCorners[2]);
            float webcamWidth = webcamMax.x - webcamMin.x;
            float webcamHeight = webcamMax.y - webcamMin.y;

            // Update landmark positions
            for (int i = 0; i < landmarkDots.Length && i < pose.landmarks.Length; i++)
            {
                Vector3 landmark = pose.landmarks[i];
                
                // MediaPipe landmarks are in normalized coordinates (0-1)
                // Map to webcam display area
                float screenX = webcamMin.x + (landmark.x * webcamWidth);
                float screenY = webcamMin.y + ((1f - landmark.y) * webcamHeight); // Flip Y axis

                RectTransform dotRect = landmarkDots[i].rectTransform;
                dotRect.anchoredPosition = new Vector2(screenX, screenY);

                // Show landmark if visibility is good
                bool isVisible = pose.landmarkVisibility != null && 
                                i < pose.landmarkVisibility.Length && 
                                pose.landmarkVisibility[i] > 0.5f;

                landmarkDots[i].gameObject.SetActive(showLandmarks && isVisible);
            }

            // Update connection lines
            if (showConnections)
            {
                for (int i = 0; i < connectionLines.Length && i < PoseConnections.Length; i++)
                {
                    int[] connection = PoseConnections[i];
                    int startIdx = connection[0];
                    int endIdx = connection[1];

                    if (startIdx < pose.landmarks.Length && endIdx < pose.landmarks.Length)
                    {
                        Vector3 startLandmark = pose.landmarks[startIdx];
                        Vector3 endLandmark = pose.landmarks[endIdx];

                        // Map to screen coordinates
                        Vector3 startPos = new Vector3(
                            webcamMin.x + (startLandmark.x * webcamWidth),
                            webcamMin.y + ((1f - startLandmark.y) * webcamHeight),
                            0
                        );

                        Vector3 endPos = new Vector3(
                            webcamMin.x + (endLandmark.x * webcamWidth),
                            webcamMin.y + ((1f - endLandmark.y) * webcamHeight),
                            0
                        );

                        LineRenderer line = connectionLines[i];
                        line.SetPosition(0, startPos);
                        line.SetPosition(1, endPos);

                        // Show line if both landmarks are visible
                        bool startVisible = pose.landmarkVisibility != null && 
                                          startIdx < pose.landmarkVisibility.Length && 
                                          pose.landmarkVisibility[startIdx] > 0.5f;
                        bool endVisible = pose.landmarkVisibility != null && 
                                        endIdx < pose.landmarkVisibility.Length && 
                                        pose.landmarkVisibility[endIdx] > 0.5f;

                        line.gameObject.SetActive(startVisible && endVisible);
                    }
                }
            }
            else
            {
                // Hide all connection lines
                foreach (var line in connectionLines)
                {
                    line.gameObject.SetActive(false);
                }
            }
        }

        private void HideVisualization()
        {
            if (landmarkDots != null)
            {
                foreach (var dot in landmarkDots)
                {
                    if (dot != null)
                    {
                        dot.gameObject.SetActive(false);
                    }
                }
            }

            if (connectionLines != null)
            {
                foreach (var line in connectionLines)
                {
                    if (line != null)
                    {
                        line.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Cleanup
            if (landmarksContainer != null)
            {
                Destroy(landmarksContainer);
            }
        }

        // Public API for runtime control
        public void SetVisualizationEnabled(bool enabled)
        {
            showLandmarks = enabled;
            showConnections = enabled;
        }

        public void SetLandmarkColor(Color color)
        {
            landmarkColor = color;
            if (landmarkDots != null)
            {
                foreach (var dot in landmarkDots)
                {
                    if (dot != null)
                    {
                        dot.color = color;
                    }
                }
            }
        }

        public void SetConnectionColor(Color color)
        {
            connectionColor = color;
            if (connectionLines != null)
            {
                foreach (var line in connectionLines)
                {
                    if (line != null)
                    {
                        line.startColor = color;
                        line.endColor = color;
                    }
                }
            }
        }
    }
}

