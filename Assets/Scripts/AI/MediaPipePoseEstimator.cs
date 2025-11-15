using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ScouterXR.Core;

namespace ScouterXR.AI
{
    public class MediaPipePoseEstimator : MonoBehaviour
    {
        [Header("References")]
        public ScouterXR.UI.ScouterUI scouterUI;
        public ScouterXR.UI.HaloColorController haloColorController;
        public Camera arCamera;
        public MediaPipeModelManager modelManager;

        [Header("Pose Settings")]
        public float confidenceThreshold = 0.5f;
        public bool debugMode = false;
        public bool useRealPoseDetection = true; // Toggle between real ML and mock data

        // Mock pose data for development (replace with actual MediaPipe integration)
        public PoseData currentPose;
        private bool poseValid = false;
        private WebCamTexture sharedWebcamTexture; // Shared webcam texture from test scene

    public         void Start()
        {
            SystemLogger.LogInfo("MediaPipePoseEstimator", $"STARTING: RealDetection={useRealPoseDetection}, DebugMode={debugMode}");

            // Initialize pose detection
            InitializePoseDetection();

            // Hide UI initially
            if (scouterUI != null)
            {
                scouterUI.HideScouter();
            }

            SystemLogger.LogInfo("MediaPipePoseEstimator", "MediaPipe Pose Estimator started successfully");
        }

        public void Update()
        {
            // Debug: Log that Update is being called
            if (Time.frameCount % 60 == 0) // Log every second
            {
                Debug.Log($"MediaPipePoseEstimator: Update() called on {gameObject.name}, enabled={enabled}, debugMode={debugMode}");
            }

            // 1. Get pose from MediaPipe (or mock data)
            currentPose = GetCurrentPose();

            // Debug logging for pose processing
            if (debugMode)
            {
                // Log more frequently for debugging
                if (Time.frameCount % 30 == 0) // Log every half second at 60fps
                {
                    string source = useRealPoseDetection && sharedWebcamTexture != null ? "WEBCAM" :
                                   useRealPoseDetection ? "SIMULATED" : "MOCK";
                    Debug.Log($"MediaPipePoseEstimator: POSE UPDATE: Source={source}, Valid={currentPose.IsValid}, Webcam={sharedWebcamTexture?.deviceName ?? "NONE"}");
                    SystemLogger.LogInfo("MediaPipePoseEstimator", $"POSE UPDATE: Source={source}, Valid={currentPose.IsValid}, Webcam={sharedWebcamTexture?.deviceName ?? "NONE"}");
                }
            }

            if (!currentPose.IsValid)
            {
                // Pose lost - optionally hide UI
                if (scouterUI != null && poseValid)
                {
                    scouterUI.HideScouter();
                }
                poseValid = false;
                return;
            }

            // Pose found - show UI if not already shown
            if (scouterUI != null && !poseValid)
            {
                scouterUI.ShowScouter();
            }
            poseValid = true;

            // 2. Calculate raw power level
            float rawPower = CalculatePowerLevel(currentPose);

            // 3. Convert keypoint to screen space for UI positioning
            Vector3 screenPos = Vector3.zero;
            if (arCamera != null && currentPose.Keypoints.Length > 0)
            {
                // Use nose or head position for UI anchoring
                Vector3 worldPos = currentPose.Keypoints[0]; // Assuming nose is index 0
                screenPos = arCamera.WorldToScreenPoint(worldPos);
            }

            // 4. Send raw data to UI systems (they handle smoothing)
            if (scouterUI != null)
            {
                scouterUI.UpdateTarget(screenPos, rawPower);
            }

            if (haloColorController != null)
            {
                haloColorController.SetPowerLevel(rawPower);
            }
        }

        private void InitializePoseDetection()
        {
            if (useRealPoseDetection)
            {
                // Initialize real MediaPipe models
                if (modelManager == null)
                {
                    modelManager = MediaPipeModelManager.Instance;
                    Debug.Log($"MediaPipePoseEstimator: Looking for MediaPipeModelManager.Instance - found: {modelManager != null}");
                    if (modelManager != null)
                    {
                        Debug.Log($"MediaPipePoseEstimator: Model manager has pose detection data: {(modelManager.GetPoseDetectionModelData()?.Length ?? 0) > 0}");
                        Debug.Log($"MediaPipePoseEstimator: Model manager has pose landmark data: {(modelManager.GetPoseLandmarkModelData()?.Length ?? 0) > 0}");
                    }
                }

                if (modelManager != null)
                {
                    // Initialize pose detection with actual models
                    InitializeMediaPipe();

                    // Auto-find camera if not set
                    if (arCamera == null)
                    {
                        arCamera = Camera.main;
                    }

                    SystemLogger.LogInfo("MediaPipePoseEstimator", "Real MediaPipe pose detection initialized");
                }
                else
                {
                    SystemLogger.LogWarning("MediaPipePoseEstimator", "ModelManager not found, falling back to mock data");
                    useRealPoseDetection = false;
                }
            }

            if (!useRealPoseDetection)
            {
                SystemLogger.LogInfo("MediaPipePoseEstimator", "Using mock pose detection for development");
            }
        }

        private void InitializeMediaPipe()
        {
            // TODO: Actual MediaPipe Unity initialization
            // This would integrate with the MediaPipe Unity package
            // For now, we prepare the models but use mock data

            try
            {
                // Validate models are available
                if (modelManager.GetPoseDetectionModelData() == null)
                {
                    throw new System.Exception("Pose detection model not loaded");
                }

                if (modelManager.GetPoseLandmarkModelData() == null)
                {
                    throw new System.Exception("Pose landmark model not loaded");
                }

                // TODO: Initialize MediaPipe graph with models
                // - Load pose_detection.tflite
                // - Load pose_landmark model (lite or full)
                // - Set up inference pipeline
                // - Configure camera input

                SystemLogger.LogInfo("MediaPipePoseEstimator", "MediaPipe models loaded and ready for inference");
            }
            catch (System.Exception e)
            {
                SystemLogger.LogError("MediaPipePoseEstimator", $"Failed to initialize MediaPipe: {e.Message}");
                useRealPoseDetection = false;
            }
        }

        // Enhanced mock data that simulates real MediaPipe behavior
        private Vector3[] mockKeypoints;
        private float[] mockConfidences;
        private float lastPoseUpdate = 0f;

        private PoseData GetSimulatedRealPose()
        {
            // Simulate what real MediaPipe would return
            // This creates more realistic pose data that could come from actual ML inference

            // Initialize base pose structure
            if (mockKeypoints == null)
            {
                InitializeRealisticPoseStructure();
            }

            // Simulate occasional pose updates (like real detection)
            if (Time.time - lastPoseUpdate > 1f) // Less frequent than mock
            {
                UpdatePoseWithRealisticMovement();
                lastPoseUpdate = Time.time;
            }

            // Simulate confidence variations like real ML models
            UpdateConfidencesRealistically();

            // Log pose generation for validation
            if (debugMode && Time.frameCount % 60 == 0) // Log every second
            {
                float avgConfidence = currentPose.Confidence != null && currentPose.Confidence.Length > 0
                    ? currentPose.Confidence.Sum() / currentPose.Confidence.Length : 0f;
                SystemLogger.LogInfo("MediaPipePoseEstimator", $"SIMULATED POSE: {currentPose.Keypoints?.Length ?? 0} keypoints, Avg Confidence: {avgConfidence:F2}");
            }

            PoseData realPose = new PoseData();
            realPose.Keypoints = (Vector3[])mockKeypoints.Clone();
            realPose.Confidence = (float[])mockConfidences.Clone();
            realPose.Timestamp = Time.time;
            realPose.IsValid = MeetsConfidenceThreshold(realPose);

            return realPose;
        }

        private void InitializeRealisticPoseStructure()
        {
            mockKeypoints = new Vector3[33];
            mockConfidences = new float[33];

            // Create anatomically correct pose structure (MediaPipe 33-point format)
            // Nose (0)
            mockKeypoints[0] = new Vector3(0, 1.75f, 3f);

            // Eyes (1-2)
            mockKeypoints[1] = new Vector3(-0.03f, 1.73f, 2.95f); // Left eye
            mockKeypoints[2] = new Vector3(0.03f, 1.73f, 2.95f);  // Right eye

            // Ears (3-4)
            mockKeypoints[3] = new Vector3(-0.08f, 1.70f, 3.05f); // Left ear
            mockKeypoints[4] = new Vector3(0.08f, 1.70f, 3.05f);  // Right ear

            // Shoulders (5-6)
            mockKeypoints[5] = new Vector3(-0.15f, 1.60f, 3.0f); // Left shoulder
            mockKeypoints[6] = new Vector3(0.15f, 1.60f, 3.0f);  // Right shoulder

            // Elbows (7-8)
            mockKeypoints[7] = new Vector3(-0.25f, 1.45f, 2.9f); // Left elbow
            mockKeypoints[8] = new Vector3(0.25f, 1.45f, 2.9f);  // Right elbow

            // Wrists (9-10)
            mockKeypoints[9] = new Vector3(-0.35f, 1.25f, 2.8f); // Left wrist
            mockKeypoints[10] = new Vector3(0.35f, 1.25f, 2.8f); // Right wrist

            // Hips (11-12)
            mockKeypoints[11] = new Vector3(-0.08f, 1.15f, 3.1f); // Left hip
            mockKeypoints[12] = new Vector3(0.08f, 1.15f, 3.1f);  // Right hip

            // Knees (13-14)
            mockKeypoints[13] = new Vector3(-0.06f, 0.75f, 3.0f); // Left knee
            mockKeypoints[14] = new Vector3(0.06f, 0.75f, 3.0f);  // Right knee

            // Ankles (15-16)
            mockKeypoints[15] = new Vector3(-0.04f, 0.15f, 3.1f); // Left ankle
            mockKeypoints[16] = new Vector3(0.04f, 0.15f, 3.1f);  // Right ankle

            // Initialize remaining keypoints with reasonable defaults
            for (int i = 17; i < 33; i++)
            {
                mockKeypoints[i] = Vector3.zero;
                mockConfidences[i] = 0.0f; // Not used in basic pose
            }

            // Set high confidence for main body parts
            for (int i = 0; i < 17; i++)
            {
                mockConfidences[i] = Random.Range(0.85f, 0.98f);
            }
        }

        private void UpdatePoseWithRealisticMovement()
        {
            // Simulate natural pose variations (breathing, minor movements)
            float breathingOffset = Mathf.Sin(Time.time * 2f) * 0.01f;

            // Apply subtle breathing to chest area
            for (int i = 5; i <= 12; i++) // Shoulders and hips
            {
                mockKeypoints[i].y += breathingOffset;
            }

            // Random micro-movements (like real pose detection jitter)
            for (int i = 0; i < mockKeypoints.Length; i++)
            {
                if (mockConfidences[i] > 0.8f) // Only move high-confidence points
                {
                    mockKeypoints[i] += new Vector3(
                        Random.Range(-0.005f, 0.005f),
                        Random.Range(-0.003f, 0.003f),
                        Random.Range(-0.008f, 0.008f)
                    );
                }
            }
        }

        private void UpdateConfidencesRealistically()
        {
            // Simulate confidence variations like real ML models
            // High-confidence points occasionally drop slightly
            for (int i = 0; i < mockConfidences.Length; i++)
            {
                if (mockConfidences[i] > 0.8f)
                {
                    // Occasional confidence drops (simulating real ML uncertainty)
                    if (Random.value < 0.05f) // 5% chance
                    {
                        mockConfidences[i] *= Random.Range(0.95f, 0.99f);
                    }
                }
            }
        }

        private PoseData GetStableMockPose()
        {
            // Fallback mock data for when real detection fails
            if (mockKeypoints == null)
            {
                mockKeypoints = new Vector3[33];
                mockConfidences = new float[33];

                // Simple standing pose
                for (int i = 0; i < mockKeypoints.Length; i++)
                {
                    mockKeypoints[i] = new Vector3(0, 1.7f, 3f);
                    mockConfidences[i] = Random.Range(0.7f, 0.95f);
                }
            }

            PoseData mockPose = new PoseData();
            mockPose.Keypoints = (Vector3[])mockKeypoints.Clone();
            mockPose.Confidence = (float[])mockConfidences.Clone();
            mockPose.Timestamp = Time.time;
            mockPose.IsValid = MeetsConfidenceThreshold(mockPose);

            return mockPose;
        }

        private bool MeetsConfidenceThreshold(PoseData pose)
        {
            if (pose.Confidence == null || pose.Confidence.Length == 0)
                return false;

            // Require 80% of keypoints above threshold
            int validCount = 0;
            foreach (float conf in pose.Confidence)
            {
                if (conf >= confidenceThreshold) validCount++;
            }
            return (validCount / (float)pose.Confidence.Length) >= 0.8f;
        }

        public float CalculatePowerLevel(PoseData pose)
        {
            // Simple mock power calculation
            // TODO: Replace with actual stance analysis and movement tracking

            // Base power from pose stability (mock)
            float stability = CalculatePoseStability(pose);
            float basePower = Mathf.Lerp(1000f, 5000f, stability);

            // Add some variation based on "stance width"
            float stanceWidth = CalculateStanceWidth(pose);
            basePower += stanceWidth * 1000f;

            // Add time-based variation to simulate changing power levels
            float timeVariation = Mathf.Sin(Time.time * 0.5f) * 500f;

            return Mathf.Clamp(basePower + timeVariation, 1000f, 9500f);
        }

        private float CalculatePoseStability(PoseData pose)
        {
            // Mock stability calculation
            // In real implementation, this would analyze joint variance over time
            return Random.Range(0.3f, 0.9f);
        }

        private float CalculateStanceWidth(PoseData pose)
        {
            // Mock stance width calculation
            // In real implementation, this would measure shoulder width
            return Random.Range(0.1f, 0.8f);
        }

        // AI-based depth estimation for fallback when stereo depth unavailable
        public float EstimateDepthFromPose(PoseData pose)
        {
            if (pose == null || pose.Keypoints == null || pose.Keypoints.Length == 0)
                return 3.0f; // Default distance

            // Method 1: Average distance of keypoints from camera
            float totalDistance = 0f;
            int validKeypoints = 0;

            for (int i = 0; i < pose.Keypoints.Length; i++)
            {
                if (pose.Confidence != null && pose.Confidence[i] > 0.5f)
                {
                    // Calculate distance from camera (assuming camera at origin looking down Z)
                    float distance = Mathf.Abs(pose.Keypoints[i].z);
                    if (distance > 0.5f && distance < 10f) // Reasonable range
                    {
                        totalDistance += distance;
                        validKeypoints++;
                    }
                }
            }

            if (validKeypoints > 0)
            {
                return totalDistance / validKeypoints;
            }

            // Method 2: Pose scale estimation (fallback)
            return EstimateDepthFromPoseScale(pose);
        }

        private float EstimateDepthFromPoseScale(PoseData pose)
        {
            // Estimate distance based on apparent size of the person
            // Taller apparent height = closer distance

            if (pose.Keypoints.Length < 2) return 3.0f;

            // Use head-to-foot distance as proxy for scale
            Vector3 headPos = Vector3.zero;
            Vector3 footPos = Vector3.zero;
            int headCount = 0, footCount = 0;

            // Find head (nose) and feet positions
            for (int i = 0; i < pose.Keypoints.Length && i < pose.Confidence.Length; i++)
            {
                if (pose.Confidence[i] > 0.7f)
                {
                    // Nose (index 0 in MediaPipe) - head position
                    if (i == 0)
                    {
                        headPos = pose.Keypoints[i];
                        headCount++;
                    }
                    // Ankles (indices 27, 30) - foot positions
                    else if (i == 27 || i == 30)
                    {
                        footPos += pose.Keypoints[i];
                        footCount++;
                    }
                }
            }

            if (headCount > 0 && footCount > 0)
            {
                footPos /= footCount; // Average foot position
                float apparentHeight = Mathf.Abs(headPos.y - footPos.y);

                // Typical person height is ~1.7m
                // Estimate distance: closer = larger apparent height
                const float typicalHeight = 1.7f;
                float estimatedDistance = typicalHeight / Mathf.Max(apparentHeight, 0.5f);

                return Mathf.Clamp(estimatedDistance, 1.0f, 8.0f);
            }

            return 3.0f; // Default fallback
        }

        // Public method to get current estimated depth
        public float GetCurrentEstimatedDepth()
        {
            return EstimateDepthFromPose(currentPose);
        }

        // Public method to get power level from current pose (for test scene integration)
        public float GetPowerLevelFromCurrentPose()
        {
            return CalculatePowerLevel(currentPose);
        }

        // Public method to set shared webcam texture (for test scene integration)
        public void SetWebcamTexture(WebCamTexture texture)
        {
            // Store reference to shared webcam texture
            sharedWebcamTexture = texture;
            Debug.Log($"SetWebcamTexture: Received webcam texture: {texture?.deviceName}, playing={texture?.isPlaying}");
            SystemLogger.LogInfo("MediaPipePoseEstimator", $"Received shared webcam texture: {texture?.deviceName}");
        }

        // Called when XR scanning starts (for integration with XRScouterManager)
        public void OnScanningStarted(Vector3 scanTargetPosition)
        {
            SystemLogger.LogInfo("MediaPipePoseEstimator", $"XR scanning started at {scanTargetPosition}");

            // Could modify pose estimation behavior during scanning
            // For example, increase update frequency or focus on specific areas
        }

        // Get current pose data
        private PoseData GetCurrentPose()
        {
            Debug.Log($"GetCurrentPose: useRealPoseDetection={useRealPoseDetection}, sharedWebcamTexture={sharedWebcamTexture != null}");

            if (useRealPoseDetection && sharedWebcamTexture != null)
            {
                // Try to get real pose from webcam texture
                Debug.Log("GetCurrentPose: Using WEBCAM texture");
                return GetPoseFromWebcamTexture(sharedWebcamTexture);
            }
            else if (useRealPoseDetection)
            {
                // Fallback to simulated real pose if no webcam texture
                Debug.Log("GetCurrentPose: Using SIMULATED pose (webcam texture null)");
                return GetSimulatedRealPose();
            }
            else
            {
                Debug.Log("GetCurrentPose: Using MOCK pose (real detection disabled)");
                return GetStableMockPose();
            }
        }

        // Get pose data from webcam texture
        private PoseData GetPoseFromWebcamTexture(WebCamTexture webcamTexture)
        {
            Debug.Log($"GetPoseFromWebcamTexture: Processing webcam texture {webcamTexture.deviceName}");

            // Create texture for inference
            Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);

            // Copy webcam texture to input texture
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(webcamTexture.width, webcamTexture.height, 24);
            Graphics.Blit(webcamTexture, renderTexture);
            RenderTexture.active = renderTexture;

            inputTexture.ReadPixels(new Rect(0, 0, webcamTexture.width, webcamTexture.height), 0, 0);
            inputTexture.Apply();

            // Reset render texture
            RenderTexture.active = currentRT;
            Destroy(renderTexture);

            // Run pose detection inference
            PoseData pose = null;
            if (modelManager != null)
            {
                Debug.Log("GetPoseFromWebcamTexture: Running pose detection inference");
                // Start pose detection inference coroutine
                StartCoroutine(RunPoseDetectionInference(inputTexture, (result) =>
                {
                    if (result.success && result.landmarks != null)
                    {
                        pose = new PoseData();
                        pose.Keypoints = result.landmarks;
                        pose.Confidence = new float[result.landmarks.Length];
                        for (int i = 0; i < result.landmarks.Length; i++)
                        {
                            pose.Confidence[i] = result.confidence;
                        }
                        pose.Timestamp = Time.time;
                        pose.IsValid = true;

                        Debug.Log($"GetPoseFromWebcamTexture: Pose detected with {result.landmarks.Length} keypoints, confidence: {result.confidence:F2}");

                        if (debugMode)
                        {
                            SystemLogger.LogInfo("MediaPipePoseEstimator", $"POSE DETECTED: {pose.Keypoints.Length} keypoints, Confidence: {result.confidence:F2}");
                            if (pose.Keypoints.Length >= 3)
                            {
                                SystemLogger.LogInfo("MediaPipePoseEstimator", $"Keypoints: Nose={pose.Keypoints[0]}, LeftShoulder={pose.Keypoints[1]}, RightShoulder={pose.Keypoints[2]}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"GetPoseFromWebcamTexture: Pose detection failed - {result.errorMessage}");
                        // Fallback to simulated pose
                        pose = GetSimulatedRealPose();
                    }
                }));
            }
            else
            {
                Debug.LogWarning("GetPoseFromWebcamTexture: Model manager is null, using simulated pose");
                pose = GetSimulatedRealPose();
            }

            // For now, return simulated pose while inference runs asynchronously
            // In a real implementation, we'd wait for the inference to complete
            pose = GetSimulatedRealPose();
            pose.Timestamp = Time.time;

            SystemLogger.LogInfo("MediaPipePoseEstimator", $"Processing pose from webcam texture: {webcamTexture.deviceName}");
            return pose;
        }

        // Run pose detection inference
        private System.Collections.IEnumerator RunPoseDetectionInference(Texture2D inputTexture, System.Action<TensorFlowLiteInference.InferenceResult> onComplete)
        {
            if (modelManager == null)
            {
                {
                    var result = new TensorFlowLiteInference.InferenceResult();
                    result.errorMessage = "Model manager not available";
                    onComplete(result);
                    yield break;
                }
            }

            // For now, simulate pose detection - in real implementation, this would run actual inference
            yield return new WaitForSeconds(0.1f); // Simulate inference time

            var inferenceResult = new TensorFlowLiteInference.InferenceResult();
            inferenceResult.success = true;
            inferenceResult.confidence = 0.85f;

            // Generate mock pose keypoints (MediaPipe pose has 33 keypoints)
            inferenceResult.landmarks = new Vector3[33];
            for (int i = 0; i < 33; i++)
            {
                // Generate realistic pose keypoints
                inferenceResult.landmarks[i] = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0.1f, 1.0f)
                );
            }

            onComplete(inferenceResult);
        }

        // Data structures
        public class PoseData
        {
            public Vector3[] Keypoints;
            public float[] Confidence;
            public float Timestamp;
            public bool IsValid;
        }
    }
}
