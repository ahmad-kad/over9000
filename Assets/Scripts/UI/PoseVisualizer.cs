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
        public float landmarkSize = 0.01f;
        public Color poseColor = Color.green;
        public Color handColor = Color.blue;

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

            // Draw pose skeleton
            if (showPoseSkeleton && poseEstimator != null && poseEstimator.currentPose != null && poseEstimator.currentPose.IsValid)
            {
                DrawPoseSkeletonOnGUI();
            }

            // Draw hand landmarks
            if (showHandLandmarks && handRecognizer != null)
            {
                DrawHandLandmarksOnGUI();
            }
        }

        private void DrawPoseSkeleton()
        {
            if (poseEstimator.currentPose.Keypoints == null) return;

            Vector3[] keypoints = poseEstimator.currentPose.Keypoints;

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
            // Get hand data from recognizer
            var leftHand = handRecognizer.GetType().GetField("leftHand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(handRecognizer) as HandPointingRecognizer.HandData;
            var rightHand = handRecognizer.GetType().GetField("rightHand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(handRecognizer) as HandPointingRecognizer.HandData;

            // Draw left hand
            if (leftHand != null && leftHand.isDetected && leftHand.landmarks != null)
            {
                DrawHandSkeleton(leftHand.landmarks, Color.cyan);
            }

            // Draw right hand
            if (rightHand != null && rightHand.isDetected && rightHand.landmarks != null)
            {
                DrawHandSkeleton(rightHand.landmarks, handColor);
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

        void Start()
        {
            Debug.Log($"PoseVisualizer: Started with poseEstimator={poseEstimator != null}, handRecognizer={handRecognizer != null}, mainCamera={mainCamera != null}");

            // Create persistent visualization if needed
            CreatePersistentVisualization();
        }

        private void DrawPoseSkeletonOnGUI()
        {
            if (poseEstimator.currentPose.Keypoints == null) return;

            Vector3[] keypoints = poseEstimator.currentPose.Keypoints;

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
            GL.Begin(GL.LINES);
            GL.Color(poseColor);

            // Draw connections between keypoints
            for (int i = 0; i < poseConnections.GetLength(0); i++)
            {
                int startIdx = poseConnections[i, 0];
                int endIdx = poseConnections[i, 1];

                if (startIdx < keypoints.Length && endIdx < keypoints.Length)
                {
                    Vector3 startPos = keypoints[startIdx];
                    Vector3 endPos = keypoints[endIdx];

                    // Convert world to screen space
                    Vector3 startScreen = mainCamera.WorldToScreenPoint(startPos);
                    Vector3 endScreen = mainCamera.WorldToScreenPoint(endPos);

                    // Only draw if both points are in front of camera and on screen
                    if (startScreen.z > 0 && endScreen.z > 0 &&
                        startScreen.x >= 0 && startScreen.x <= Screen.width &&
                        startScreen.y >= 0 && startScreen.y <= Screen.height &&
                        endScreen.x >= 0 && endScreen.x <= Screen.width &&
                        endScreen.y >= 0 && endScreen.y <= Screen.height)
                    {
                        GL.Vertex3(startScreen.x, Screen.height - startScreen.y, 0);
                        GL.Vertex3(endScreen.x, Screen.height - endScreen.y, 0);
                    }
                }
            }

            GL.End();

            // Draw keypoints as small circles
            GL.Begin(GL.QUADS);
            GL.Color(poseColor);

            foreach (Vector3 keypoint in keypoints)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(keypoint);
                if (screenPos.z > 0 &&
                    screenPos.x >= 0 && screenPos.x <= Screen.width &&
                    screenPos.y >= 0 && screenPos.y <= Screen.height)
                {
                    float size = landmarkSize * 50; // Scale for screen
                    float x = screenPos.x;
                    float y = Screen.height - screenPos.y;

                    GL.Vertex3(x - size, y - size, 0);
                    GL.Vertex3(x + size, y - size, 0);
                    GL.Vertex3(x + size, y + size, 0);
                    GL.Vertex3(x - size, y + size, 0);
                }
            }

            GL.End();
            GL.PopMatrix();
        }

        private void DrawHandLandmarksOnGUI()
        {
            // Get hand data
            var leftHand = handRecognizer.GetType().GetField("leftHand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(handRecognizer) as HandPointingRecognizer.HandData;
            var rightHand = handRecognizer.GetType().GetField("rightHand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(handRecognizer) as HandPointingRecognizer.HandData;

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

            // Draw left hand
            if (leftHand != null && leftHand.isDetected && leftHand.landmarks != null)
            {
                DrawHandSkeletonGL(leftHand.landmarks);
            }

            // Draw right hand
            if (rightHand != null && rightHand.isDetected && rightHand.landmarks != null)
            {
                DrawHandSkeletonGL(rightHand.landmarks);
            }

            GL.End();
            GL.PopMatrix();
        }

        private void DrawHandSkeletonGL(Vector3[] landmarks)
        {
            for (int i = 0; i < handConnections.GetLength(0); i++)
            {
                int startIdx = handConnections[i, 0];
                int endIdx = handConnections[i, 1];

                if (startIdx < landmarks.Length && endIdx < landmarks.Length)
                {
                    Vector3 startPos = landmarks[startIdx];
                    Vector3 endPos = landmarks[endIdx];

                    Vector3 startScreen = mainCamera.WorldToScreenPoint(startPos);
                    Vector3 endScreen = mainCamera.WorldToScreenPoint(endPos);

                    if (startScreen.z > 0 && endScreen.z > 0 &&
                        startScreen.x >= 0 && startScreen.x <= Screen.width &&
                        startScreen.y >= 0 && startScreen.y <= Screen.height &&
                        endScreen.x >= 0 && endScreen.x <= Screen.width &&
                        endScreen.y >= 0 && endScreen.y <= Screen.height)
                    {
                        GL.Vertex3(startScreen.x, Screen.height - startScreen.y, 0);
                        GL.Vertex3(endScreen.x, Screen.height - endScreen.y, 0);
                    }
                }
            }
        }
    }
}
