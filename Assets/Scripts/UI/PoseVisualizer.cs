using UnityEngine;
using ScouterXR.AI;

namespace ScouterXR.UI
{
    public class PoseVisualizer : MonoBehaviour
    {
        [Header("AI Components")]
        public MediaPipePoseEstimator poseEstimator;
        public HandPointingRecognizer handRecognizer;
        public Camera mainCamera;

    [Header("Visualization Settings")]
    public bool showPoseSkeleton = true;
    public bool showHandLandmarks = true;
    public bool debugMode = true; // Enable debug boundaries
    public float landmarkSize = 0.01f;
    public float poseDistance = 2.0f; // Distance from camera to project pose into world space
    public Color poseColor = Color.green;
    public Color handColor = Color.blue;

    [Header("Camera Configuration")]
    public int cameraWidth = 1280;  // Webcam width
    public int cameraHeight = 720;  // Webcam height
    public WebCamTexture webcamTexture;

        // Pose connections (MediaPipe pose landmark connections)
        private readonly int[,] poseConnections = new int[,]
        {
            // Nose to shoulders
            {0, 1}, {0, 2},
            // Shoulders
            {1, 2}, {1, 3}, {2, 4},
            // Arms
            {3, 5}, {5, 7}, {7, 9},
            {4, 6}, {6, 8}, {8, 10},
            // Hips
            {3, 11}, {4, 12}, {11, 12},
            // Legs
            {11, 13}, {13, 15}, {15, 17},
            {12, 14}, {14, 16}, {16, 18},
            // Face (simplified)
            {0, 3}, {0, 4}
        };

        // Hand connections (MediaPipe hand landmark connections)
        private readonly int[,] handConnections = new int[,]
        {
            // Thumb
            {0, 1}, {1, 2}, {2, 3}, {3, 4},
            // Index finger
            {0, 5}, {5, 6}, {6, 7}, {7, 8},
            // Middle finger
            {0, 9}, {9, 10}, {10, 11}, {11, 12},
            // Ring finger
            {0, 13}, {13, 14}, {14, 15}, {15, 16},
            // Pinky
            {0, 17}, {17, 18}, {18, 19}, {19, 20}
        };

        void Update()
        {
            // Note: Debug.DrawLine only shows in Scene view, not Game view
            // For Game view visualization, we would need LineRenderer or OnGUI
        }

        void OnGUI()
        {
            if (!mainCamera) return;

            // Debug: Draw webcam display area boundaries
            if (debugMode)
            {
                DrawDebugBoundaries();
            }

            // Draw pose skeleton in 3D world space (proper for XR)
            if (showPoseSkeleton && poseEstimator != null && poseEstimator.LatestLandmarks != null && poseEstimator.LatestConfidence > 0.3f)
            {
                DrawPoseSkeletonInWorldSpace();
            }

            // Draw hand landmarks
            if (showHandLandmarks && handRecognizer != null)
            {
                DrawHandLandmarksOnGUI();
            }
        }

        private void DrawPoseSkeleton()
        {
            if (poseEstimator.LatestLandmarks == null) return;

            Vector3[] keypoints = poseEstimator.LatestLandmarks;

            // Draw connections between keypoints
            for (int i = 0; i < poseConnections.GetLength(0); i++)
            {
                int startIdx = poseConnections[i, 0];
                int endIdx = poseConnections[i, 1];

                if (startIdx < keypoints.Length && endIdx < keypoints.Length)
                {
                    Vector3 startPos = keypoints[startIdx];
                    Vector3 endPos = keypoints[endIdx];

                    // Convert to screen space for drawing
                    Vector3 startScreen = mainCamera.WorldToScreenPoint(startPos);
                    Vector3 endScreen = mainCamera.WorldToScreenPoint(endPos);

                    // Only draw if both points are in front of camera
                    if (startScreen.z > 0 && endScreen.z > 0)
                    {
                        Debug.DrawLine(startPos, endPos, poseColor, 0.1f);
                    }
                }
            }

            // Draw keypoints as spheres
            foreach (Vector3 keypoint in keypoints)
            {
                // Only draw if in front of camera
                Vector3 screenPos = mainCamera.WorldToScreenPoint(keypoint);
                if (screenPos.z > 0)
                {
                    DrawSphere(keypoint, landmarkSize, poseColor);
                }
            }
        }

        private void DrawHandLandmarks()
        {
            // Get hand landmarks from recognizer
            var landmarks = handRecognizer.LatestHandLandmarks;
            
            // Draw hand if detected (check if any landmark is non-zero)
            if (landmarks != null && landmarks.Length > 0)
            {
                bool hasValidLandmarks = false;
                for (int i = 0; i < landmarks.Length; i++)
                {
                    if (landmarks[i] != Vector3.zero)
                    {
                        hasValidLandmarks = true;
                        break;
                    }
                }
                
                if (hasValidLandmarks)
                {
                    DrawHandSkeleton(landmarks, handColor);
                }
            }
        }

        private void DrawHandSkeleton(Vector3[] landmarks, Color color)
        {
            // Draw connections between hand landmarks
            for (int i = 0; i < handConnections.GetLength(0); i++)
            {
                int startIdx = handConnections[i, 0];
                int endIdx = handConnections[i, 1];

                if (startIdx < landmarks.Length && endIdx < landmarks.Length)
                {
                    Vector3 startPos = landmarks[startIdx];
                    Vector3 endPos = landmarks[endIdx];

                    // Convert to screen space for drawing
                    Vector3 startScreen = mainCamera.WorldToScreenPoint(startPos);
                    Vector3 endScreen = mainCamera.WorldToScreenPoint(endPos);

                    // Only draw if both points are in front of camera
                    if (startScreen.z > 0 && endScreen.z > 0)
                    {
                        Debug.DrawLine(startPos, endPos, color, 0.1f);
                    }
                }
            }

            // Draw landmarks as spheres
            foreach (Vector3 landmark in landmarks)
            {
                // Only draw if in front of camera
                Vector3 screenPos = mainCamera.WorldToScreenPoint(landmark);
                if (screenPos.z > 0)
                {
                    DrawSphere(landmark, landmarkSize * 0.5f, color);
                }
            }
        }

        private void DrawSphere(Vector3 position, float radius, Color color)
        {
            // Simple sphere drawing using Gizmos (only works in Scene view)
            // For game view, we'd need a different approach

            // Create a temporary game object for visualization
            GameObject sphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereObj.transform.position = position;
            sphereObj.transform.localScale = Vector3.one * radius * 2;
            sphereObj.GetComponent<Renderer>().material.color = color;

            // Auto-destroy after a frame
            Destroy(sphereObj, 0.1f);
        }

        // Alternative: Use LineRenderer for persistent visualization
        public void CreatePersistentVisualization()
        {
            // Create LineRenderer for pose skeleton
            if (showPoseSkeleton)
            {
                GameObject poseLinesObj = new GameObject("Pose Skeleton Lines");
                poseLinesObj.transform.SetParent(transform);

                LineRenderer poseRenderer = poseLinesObj.AddComponent<LineRenderer>();
                poseRenderer.material = new Material(Shader.Find("Sprites/Default"));
                poseRenderer.startColor = poseColor;
                poseRenderer.endColor = poseColor;
                poseRenderer.widthMultiplier = 0.005f;
                poseRenderer.positionCount = poseConnections.GetLength(0) * 2; // 2 points per connection
            }

            // Create LineRenderer for hand landmarks
            if (showHandLandmarks)
            {
                GameObject handLinesObj = new GameObject("Hand Landmark Lines");
                handLinesObj.transform.SetParent(transform);

                LineRenderer handRenderer = handLinesObj.AddComponent<LineRenderer>();
                handRenderer.material = new Material(Shader.Find("Sprites/Default"));
                handRenderer.startColor = handColor;
                handRenderer.endColor = handColor;
                handRenderer.widthMultiplier = 0.003f;
                handRenderer.positionCount = handConnections.GetLength(0) * 2; // 2 points per connection
            }
        }

        // Materials for GL drawing
        private Material poseMaterial;
        private Material handMaterial;
        private Material debugMaterial;

        private void DrawDebugBoundaries()
        {
            GL.PushMatrix();
            GL.LoadPixelMatrix();

            if (debugMaterial == null)
            {
                debugMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                debugMaterial.hideFlags = HideFlags.HideAndDontSave;
                debugMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                debugMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                debugMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                debugMaterial.SetInt("_ZWrite", 0);
            }

            debugMaterial.SetPass(0);
            
            // Calculate actual display boundaries based on aspect ratio
            float cameraAspectRatio = (float)cameraWidth / cameraHeight;
            float screenAspectRatio = (float)Screen.width / Screen.height;
            
            float displayWidth, displayHeight, offsetX, offsetY;
            if (cameraAspectRatio > screenAspectRatio)
            {
                displayWidth = Screen.width * 0.9f;
                displayHeight = displayWidth / cameraAspectRatio;
            }
            else
            {
                displayHeight = Screen.height * 0.9f;
                displayWidth = displayHeight * cameraAspectRatio;
            }
            
            offsetX = (Screen.width - displayWidth) * 0.5f;
            offsetY = (Screen.height - displayHeight) * 0.5f;
            
            // Draw webcam display area boundaries (red rectangle)
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            float left = offsetX;
            float right = offsetX + displayWidth;
            float bottom = offsetY;
            float top = offsetY + displayHeight;

            // Bottom edge
            GL.Vertex3(left, bottom, 0);
            GL.Vertex3(right, bottom, 0);

            // Top edge
            GL.Vertex3(left, top, 0);
            GL.Vertex3(right, top, 0);

            // Left edge
            GL.Vertex3(left, bottom, 0);
            GL.Vertex3(left, top, 0);

            // Right edge
            GL.Vertex3(right, bottom, 0);
            GL.Vertex3(right, top, 0);

            GL.End();

            // Draw center crosshairs
            GL.Begin(GL.LINES);
            GL.Color(Color.yellow);

            float centerX = left + displayWidth * 0.5f;
            float centerY = bottom + displayHeight * 0.5f;

            // Horizontal line through center
            GL.Vertex3(left, centerY, 0);
            GL.Vertex3(right, centerY, 0);

            // Vertical line through center
            GL.Vertex3(centerX, bottom, 0);
            GL.Vertex3(centerX, top, 0);

            GL.End();

            GL.PopMatrix();
        }

        void Start()
        {
            Debug.Log($"PoseVisualizer: Started with poseEstimator={poseEstimator != null}, handRecognizer={handRecognizer != null}, mainCamera={mainCamera != null}");

            // Create persistent visualization if needed
            CreatePersistentVisualization();
        }

        private void DrawPoseSkeletonInWorldSpace()
        {
            if (poseEstimator == null)
            {
                if (debugMode && Time.frameCount % 300 == 0)
                    Debug.LogWarning("PoseVisualizer: poseEstimator is NULL");
                return;
            }

            if (poseEstimator.LatestLandmarks == null)
            {
                if (debugMode && Time.frameCount % 300 == 0)
                    Debug.LogWarning($"PoseVisualizer: LatestLandmarks is NULL (confidence={poseEstimator.LatestConfidence:F3}, lastUpdate={poseEstimator.LastUpdateTime:F2}s)");
                return;
            }

            Vector3[] keypoints = poseEstimator.LatestLandmarks;
            
            // Check if landmarks are valid (not all zero)
            bool hasValidData = false;
            for (int i = 0; i < keypoints.Length; i++)
            {
                if (keypoints[i] != Vector3.zero)
                {
                    hasValidData = true;
                    break;
                }
            }
            
            if (!hasValidData)
            {
                if (debugMode && Time.frameCount % 300 == 0)
                    Debug.LogWarning("PoseVisualizer: All landmarks are ZERO");
                return;
            }

            // Debug: Log coordinate system
            if (debugMode && Time.frameCount % 60 == 0 && keypoints.Length > 0)
            {
                Debug.Log($"PoseVisualizer: ✅ Drawing pose - Nose: {keypoints[0]}, Confidence: {poseEstimator.LatestConfidence:F3}, UpdateAge: {Time.time - poseEstimator.LastUpdateTime:F2}s");
            }

            // Convert 2D normalized image coordinates to 3D world space
            Vector3[] worldKeypoints = ConvertImageCoordsToWorldSpace(keypoints);

            // Draw the pose skeleton in 3D world space
            DrawPoseSkeletonIn3D(worldKeypoints);
        }

        private Vector3[] ConvertImageCoordsToWorldSpace(Vector3[] imageKeypoints)
        {
            Vector3[] worldKeypoints = new Vector3[imageKeypoints.Length];

            for (int i = 0; i < imageKeypoints.Length; i++)
            {
                // MediaPipe landmarks come in normalized coordinates (0-1)
                // Y is already flipped in PoseLandmarkDetect output (line 227: 1f - y)
                // So Y=0 is TOP, Y=1 is BOTTOM (matches screen coordinates)
                
                // Convert normalized image coordinates (0-1) to screen coordinates (pixels)
                float screenX = imageKeypoints[i].x * Screen.width;
                // Y is already in screen space orientation (0=top, 1=bottom)
                // Unity screen coordinates have (0,0) at bottom-left, so we need to flip
                float screenY = (1f - imageKeypoints[i].y) * Screen.height;

                // Project screen coordinates to 3D world space at a fixed distance from camera
                // This creates a ray from camera through the screen point
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenX, screenY, poseDistance));

                worldKeypoints[i] = worldPos;
            }

            return worldKeypoints;
        }

        private void DrawPoseSkeletonIn3D(Vector3[] worldKeypoints)
        {
            // Draw connections between keypoints in 3D world space
            for (int i = 0; i < poseConnections.GetLength(0); i++)
            {
                int startIdx = poseConnections[i, 0];
                int endIdx = poseConnections[i, 1];

                if (startIdx < worldKeypoints.Length && endIdx < worldKeypoints.Length)
                {
                    Vector3 startPos = worldKeypoints[startIdx];
                    Vector3 endPos = worldKeypoints[endIdx];

                    // Draw line in world space
                    Debug.DrawLine(startPos, endPos, poseColor, 0.1f);
                }
            }

            // Draw keypoints as small spheres in 3D world space
            foreach (Vector3 keypoint in worldKeypoints)
            {
                DrawSphere(keypoint, landmarkSize, poseColor);
            }
        }

        private void DrawPoseSkeletonOnGUI()
        {
            if (poseEstimator.LatestLandmarks == null) return;

            Vector3[] keypoints = poseEstimator.LatestLandmarks;

            GL.PushMatrix();
            GL.LoadPixelMatrix();

            // Set material for drawing
            if (poseMaterial == null)
            {
                poseMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                poseMaterial.hideFlags = HideFlags.HideAndDontSave;
                poseMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                poseMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                poseMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                poseMaterial.SetInt("_ZWrite", 0);
            }

            poseMaterial.SetPass(0);

            // Calculate aspect-ratio aware display area with proper Y-flip transform
            Matrix4x4 transform = CalculateImageToScreenTransform();

            // Draw connections between keypoints
            GL.Begin(GL.LINES);
            GL.Color(poseColor);

            for (int i = 0; i < poseConnections.GetLength(0); i++)
            {
                int startIdx = poseConnections[i, 0];
                int endIdx = poseConnections[i, 1];

                if (startIdx < keypoints.Length && endIdx < keypoints.Length)
                {
                    Vector3 startPos = transform.MultiplyPoint(keypoints[startIdx]);
                    Vector3 endPos = transform.MultiplyPoint(keypoints[endIdx]);

                    GL.Vertex3(startPos.x, startPos.y, 0);
                    GL.Vertex3(endPos.x, endPos.y, 0);
                }
            }

            GL.End();

            // Draw keypoints as small quads
            GL.Begin(GL.QUADS);
            GL.Color(poseColor);

            // MediaPipe keypoint labels
            string[] poseLabels = new string[] {
                "nose", "l_eye", "r_eye", "l_ear", "r_ear", "l_shoulder", "r_shoulder",
                "l_elbow", "r_elbow", "l_wrist", "r_wrist", "l_hip", "r_hip",
                "l_knee", "r_knee", "l_ankle", "r_ankle", "neck", "head", "l_eye_inner",
                "r_eye_inner", "l_ear_inner", "r_ear_inner", "mouth_left", "mouth_right",
                "l_hand", "r_hand", "l_foot_idx", "r_foot_idx", "l_foot", "r_foot",
                "l_foot_idx2", "r_foot_idx2"
            };

            for (int i = 0; i < keypoints.Length; i++)
            {
                Vector3 screenPos = transform.MultiplyPoint(keypoints[i]);
                float size = landmarkSize * 20;

                GL.Vertex3(screenPos.x - size, screenPos.y - size, 0);
                GL.Vertex3(screenPos.x + size, screenPos.y - size, 0);
                GL.Vertex3(screenPos.x + size, screenPos.y + size, 0);
                GL.Vertex3(screenPos.x - size, screenPos.y + size, 0);
            }

            GL.End();

            // Draw text labels for keypoints (using OnGUI overlay)
            if (Time.frameCount % 10 == 0)  // Draw labels less frequently for performance
            {
                GUI.color = poseColor;
                for (int i = 0; i < Mathf.Min(keypoints.Length, poseLabels.Length); i++)
                {
                    Vector3 screenPos = transform.MultiplyPoint(keypoints[i]);
                    if (screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
                    {
                        GUI.Label(new Rect(screenPos.x + 5, screenPos.y, 100, 20), poseLabels[i], GetLabelStyle());
                    }
                }
                GUI.color = Color.white;
            }

            // Debug logging
            if (debugMode && keypoints.Length > 0 && Time.frameCount % 60 == 0)
            {
                Vector3 noseScreen = transform.MultiplyPoint(keypoints[0]);
                Debug.Log($"PoseVisualizer: Nose normalized {keypoints[0]} -> screen ({noseScreen.x:F0}, {noseScreen.y:F0})");
                Debug.Log($"PoseVisualizer: Transform active - Aspect ratio aware display configured");
            }

            GL.PopMatrix();
        }

        private Matrix4x4 CalculateImageToScreenTransform()
        {
            // Get camera aspect ratio (from webcam dimensions)
            float cameraAspectRatio = (float)cameraWidth / cameraHeight;
            float screenAspectRatio = (float)Screen.width / Screen.height;

            // Calculate display area that preserves aspect ratio
            float displayWidth, displayHeight, offsetX, offsetY;

            if (cameraAspectRatio > screenAspectRatio)
            {
                // Camera is wider than screen - fit to width
                displayWidth = Screen.width * 0.9f;
                displayHeight = displayWidth / cameraAspectRatio;
            }
            else
            {
                // Camera is taller than screen - fit to height
                displayHeight = Screen.height * 0.9f;
                displayWidth = displayHeight * cameraAspectRatio;
            }

            // Center the display area
            offsetX = (Screen.width - displayWidth) * 0.5f;
            offsetY = (Screen.height - displayHeight) * 0.5f;

            // Build transform matrix
            // Normalized [0,1] coords -> Screen pixel coords
            Matrix4x4 transform = Matrix4x4.identity;

            // Scale from [0,1] to display dimensions
            transform.m00 = displayWidth;
            // MediaPipe Y is already flipped (Y=0 is top, Y=1 is bottom)
            // GL.LoadPixelMatrix has (0,0) at bottom-left
            // So we need to flip Y: negate the scale and offset from top
            transform.m11 = -displayHeight;  // Negative to flip Y

            // Translate to display position
            transform.m03 = offsetX;
            // Start from top of display area and go down (because of negative scale)
            transform.m13 = offsetY + displayHeight;

            if (debugMode && Time.frameCount % 300 == 0)
            {
                Debug.Log($"PoseVisualizer: Transform config - Camera aspect: {cameraAspectRatio:F2}, Screen aspect: {screenAspectRatio:F2}");
                Debug.Log($"PoseVisualizer: Display {displayWidth:F0}x{displayHeight:F0} at ({offsetX:F0}, {offsetY:F0})");
            }

            return transform;
        }

        private GUIStyle GetLabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.white;
            style.fontSize = 10;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        private void DrawHandLandmarksOnGUI()
        {
            // Get hand landmarks
            var landmarks = handRecognizer.LatestHandLandmarks;
            
            // Check if hand is detected (any non-zero landmark)
            bool hasValidLandmarks = false;
            if (landmarks != null && landmarks.Length > 0)
            {
                for (int i = 0; i < landmarks.Length; i++)
                {
                    if (landmarks[i] != Vector3.zero)
                    {
                        hasValidLandmarks = true;
                        break;
                    }
                }
            }
            
            if (!hasValidLandmarks) return;

            GL.PushMatrix();
            GL.LoadPixelMatrix();

            if (handMaterial == null)
            {
                handMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                handMaterial.hideFlags = HideFlags.HideAndDontSave;
                handMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                handMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                handMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                handMaterial.SetInt("_ZWrite", 0);
            }

            handMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(handColor);

            // Draw hand
            DrawHandSkeletonGL(landmarks);

            GL.End();
            GL.PopMatrix();
        }

        private void DrawHandSkeletonGL(Vector3[] landmarks)
        {
            // Webcam display covers 90% of screen (5% margin on each side)
            float marginX = Screen.width * 0.05f;  // 5% margin
            float marginY = Screen.height * 0.05f; // 5% margin
            float displayWidth = Screen.width * 0.9f;   // 90% of screen width
            float displayHeight = Screen.height * 0.9f; // 90% of screen height

            for (int i = 0; i < handConnections.GetLength(0); i++)
            {
                int startIdx = handConnections[i, 0];
                int endIdx = handConnections[i, 1];

                if (startIdx < landmarks.Length && endIdx < landmarks.Length)
                {
                    Vector3 startPos = landmarks[startIdx];
                    Vector3 endPos = landmarks[endIdx];

                    // Convert normalized coordinates to webcam display area
                    float startX = marginX + (startPos.x * displayWidth);
                    float startY = marginY + (startPos.y * displayHeight);
                    float endX = marginX + (endPos.x * displayWidth);
                    float endY = marginY + (endPos.y * displayHeight);

                    // Note: Y coordinate handling may need adjustment based on webcam texture orientation

                    GL.Vertex3(startX, startY, 0);
                    GL.Vertex3(endX, endY, 0);
                }
            }
        }
    }
}
