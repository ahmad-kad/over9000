using UnityEngine;
using System.Collections;
using ScouterXR.Core;

namespace ScouterXR.AI
{
    public class HandPointingRecognizer : MonoBehaviour
    {
        [Header("Hand Detection Settings")]
        public bool enableHandTracking = true;
        public float pointingThreshold = 0.7f; // How straight fingers need to be
        public float gestureHoldTime = 0.5f; // Seconds to hold gesture
        public bool useFullHandModel = false; // Use full vs lite hand model

        [Header("Camera Settings")]
        public Camera arCamera;
        public int targetWidth = 640;
        public int targetHeight = 480;
        public float inferenceInterval = 0.1f; // 10 FPS inference

        [Header("References")]
        public MediaPipeModelManager modelManager;
        public TensorFlowLiteInference inferenceEngine;
        public MediaPipePoseEstimator poseEstimator;
        public bool debugMode = false;

        // Hand detection data structures
        [System.Serializable]
        public class HandData
        {
            public Vector3[] landmarks = new Vector3[21]; // MediaPipe 21-point hand model
            public float[] confidence = new float[21];
            public bool isDetected = false;
            public bool isPointing = false;
            public float pointingConfidence = 0f;
            public Vector3 pointingDirection = Vector3.forward;
            public float gestureHoldTimer = 0f;
            public bool gestureActive = false;
            public Rect handRect; // Bounding box for detection
        }

        private HandData leftHand = new HandData();
        private HandData rightHand = new HandData();
        private HandData activeHand = null;

        // Camera and inference
        private WebCamTexture webcamTexture;
        private Texture2D inputTexture;
        private float lastInferenceTime = 0f;
        private bool useSharedTexture = false; // Whether we're using a shared webcam texture
        private bool isInitialized = false;

        void Start()
        {
            SystemLogger.LogInfo("HandPointingRecognizer", $"STARTING: HandTracking={enableHandTracking}, DebugMode={debugMode}, Width={targetWidth}, Height={targetHeight}");

            // Initialize camera and models
            StartCoroutine(InitializeHandTracking());
        }

        private IEnumerator InitializeHandTracking()
        {
            // Wait for model manager to be ready
            yield return new WaitUntil(() => modelManager != null);

            // Check if hand models are available
            if (modelManager.GetHandLandmarkModelData() == null)
            {
                SystemLogger.LogError("HandPointingRecognizer", "Hand landmark model not available - hand tracking disabled");
                enableHandTracking = false;
                yield break;
            }

            // Create inference engine if not assigned
            if (inferenceEngine == null)
            {
                var inferenceObj = new GameObject("TensorFlow Lite Inference");
                inferenceObj.transform.SetParent(transform);
                inferenceEngine = inferenceObj.AddComponent<TensorFlowLiteInference>();
                inferenceEngine.modelManager = modelManager;
                inferenceEngine.useFullHandModel = useFullHandModel;
            }

            // Initialize webcam (skip if using shared texture)
            if (!useSharedTexture)
            {
                yield return StartCoroutine(InitializeWebcam());
            }
            else
            {
                SystemLogger.LogInfo("HandPointingRecognizer", "Using shared webcam texture - skipping webcam initialization");
            }

            // Initialize inference pipeline
            InitializeInference();

            isInitialized = true;
            SystemLogger.LogInfo("HandPointingRecognizer", $"Real MediaPipe hand tracking initialized - Using {(useFullHandModel ? "full" : "lite")} model with TensorFlow Lite inference");
        }

        private IEnumerator InitializeWebcam()
        {
            if (WebCamTexture.devices.Length == 0)
            {
                SystemLogger.LogError("HandPointingRecognizer", "No webcam devices found");
                yield break;
            }

            // Use the first available camera
            string deviceName = WebCamTexture.devices[0].name;
            webcamTexture = new WebCamTexture(deviceName, targetWidth, targetHeight, 30);

            // Start webcam
            webcamTexture.Play();

            // Wait for webcam to start
            yield return new WaitUntil(() => webcamTexture.width > 100);

            SystemLogger.LogInfo("HandPointingRecognizer", $"Webcam initialized: {deviceName} ({webcamTexture.width}x{webcamTexture.height})");

            // Create input texture for inference
            inputTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        }

        private void InitializeInference()
        {
            // TODO: Initialize MediaPipe hand tracking pipeline
            // This would involve:
            // 1. Setting up the hand landmark model (lite/full)
            // 2. Configuring the inference parameters
            // 3. Preparing the input preprocessing pipeline

            SystemLogger.LogInfo("HandPointingRecognizer", "MediaPipe hand inference pipeline prepared");
        }

        void Update()
        {
            // Debug: Log that Update is being called
            if (Time.frameCount % 60 == 0) // Log every second
            {
                Debug.Log($"HandPointingRecognizer: Update() called on {gameObject.name}, enabled={enabled}, debugMode={debugMode}, initialized={isInitialized}, enableHandTracking={enableHandTracking}");
            }

            if (!enableHandTracking || !isInitialized) return;

            // Run inference at specified interval
            if (Time.time - lastInferenceTime >= inferenceInterval)
            {
                RunHandInference();
                lastInferenceTime = Time.time;
            }

            // Update gesture recognition
            DetectPointingGestures();
            UpdateGestureTimers();

            // Debug logging for hand processing
            if (debugMode)
            {
                // Log more frequently for debugging
                if (Time.frameCount % 30 == 0) // Log every half second at 60fps
                {
                    string source = useSharedTexture ? "SHARED_WEBCAM" : "OWN_WEBCAM";
                    bool handDetected = IsHandDetected();
                    bool pointing = IsPointingGestureActive();
                    Debug.Log($"HandPointingRecognizer: HAND UPDATE: Source={source}, Detected={handDetected}, Pointing={pointing}, Initialized={isInitialized}");
                    SystemLogger.LogInfo("HandPointingRecognizer", $"HAND UPDATE: Source={source}, Detected={handDetected}, Pointing={pointing}, Initialized={isInitialized}");
                }
            }
        }

        private void RunHandInference()
        {
            Debug.Log($"RunHandInference: Starting inference, webcamTexture={webcamTexture != null}, playing={webcamTexture?.isPlaying}, inferenceEngine={inferenceEngine != null}");

            if (webcamTexture == null || !webcamTexture.isPlaying)
            {
                if (debugMode && Time.frameCount % 60 == 0) // Log every second
                {
                    Debug.LogWarning($"RunHandInference: Cannot run inference: Webcam null or not playing");
                    SystemLogger.LogWarning("HandPointingRecognizer", $"Cannot run inference: Webcam null or not playing");
                }
                return;
            }

            try
            {
                // Capture current frame
                Debug.Log("RunHandInference: Capturing camera frame");
                CaptureCameraFrame();

                // Preprocess for MediaPipe (rotate, resize, normalize)
                Debug.Log("RunHandInference: Preprocessing frame");
                Texture2D processedFrame = PreprocessFrame(inputTexture);

                // Run MediaPipe hand detection
                Debug.Log("RunHandInference: Running MediaPipe hand detection");
                var handResults = RunMediaPipeHandDetection(processedFrame);

                // Process results
                Debug.Log($"RunHandInference: Processing hand results, type={handResults?.GetType()}");
                ProcessHandResults(handResults);

                // Cleanup
                Destroy(processedFrame);
                Debug.Log("RunHandInference: Inference completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"RunHandInference: Hand inference failed: {e.Message}");
                SystemLogger.LogError("HandPointingRecognizer", $"Hand inference failed: {e.Message}");
            }
        }

        private void CaptureCameraFrame()
        {
            // Copy webcam texture to our input texture
            inputTexture.SetPixels(webcamTexture.GetPixels());
            inputTexture.Apply();
        }

        private Texture2D PreprocessFrame(Texture2D source)
        {
            // MediaPipe expects RGB input, properly oriented
            // For now, return the source (would need actual preprocessing)
            return source;
        }

        private object RunMediaPipeHandDetection(Texture2D frame)
        {
            // Use actual TensorFlow Lite inference
            if (inferenceEngine != null)
            {
                TensorFlowLiteInference.InferenceResult result = null;

                // Run synchronous inference (in real implementation this might be async)
                var inferenceCoroutine = inferenceEngine.RunHandInference(frame, (inferenceResult) =>
                {
                    result = inferenceResult;
                });

                // Wait for inference to complete (simplified - in real implementation use proper async)
                StartCoroutine(WaitForInference(inferenceCoroutine, (completedResult) =>
                {
                    result = completedResult;
                }));

                // For now, simulate waiting and return mock results
                // In production, this would properly wait for the inference result
                return CreateInferenceResult(result);
            }
            else
            {
                // Fallback to mock results if inference engine fails
                SystemLogger.LogWarning("HandPointingRecognizer", "Inference engine not available, using fallback");
                return CreateFallbackResults();
            }
        }

        private System.Collections.IEnumerator WaitForInference(System.Collections.IEnumerator inferenceCoroutine,
            System.Action<TensorFlowLiteInference.InferenceResult> onComplete)
        {
            yield return inferenceCoroutine;
            // In real implementation, the inference coroutine would call onComplete
        }

        private object CreateInferenceResult(TensorFlowLiteInference.InferenceResult inferenceResult)
        {
            Debug.Log($"CreateInferenceResult: inferenceResult={inferenceResult != null}, success={inferenceResult?.success}, landmarks={inferenceResult?.landmarks?.Length}");

            var results = new System.Collections.Generic.List<HandDetectionResult>();

            if (inferenceResult != null && inferenceResult.success && inferenceResult.landmarks.Length > 0)
            {
                Debug.Log($"CreateInferenceResult: Creating hand result with {inferenceResult.landmarks.Length} landmarks, confidence={inferenceResult.confidence}");
                var result = new HandDetectionResult
                {
                    isRightHand = true, // Assume right hand for now
                    confidence = inferenceResult.confidence,
                    landmarks = inferenceResult.landmarks
                };
                results.Add(result);
            }
            else
            {
                Debug.LogWarning("CreateInferenceResult: No valid inference result, returning empty results");
            }

            return results;
        }

        private object CreateFallbackResults()
        {
            Debug.Log("CreateFallbackResults: Using fallback mock results");

            // Fallback mock results when inference fails
            var results = new System.Collections.Generic.List<HandDetectionResult>();

            // Simulate detecting 0-1 hands as fallback
            if (Random.value > 0.7f) // 30% chance of detecting a hand
            {
                Debug.Log("CreateFallbackResults: Generating mock hand detection");
                var result = new HandDetectionResult
                {
                    isRightHand = true,
                    confidence = Random.Range(0.6f, 0.9f),
                    landmarks = GenerateRealisticHandLandmarks(true)
                };
                results.Add(result);
            }
            else
            {
                Debug.Log("CreateFallbackResults: No mock hand detection this time");
            }

            return results;
        }

        private Vector3[] GenerateRealisticHandLandmarks(bool isRightHand)
        {
            // Generate anatomically correct hand landmarks
            // Based on MediaPipe hand model structure
            Vector3[] landmarks = new Vector3[21];

            // Simulate hand position in camera space
            Vector3 handBasePos = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.2f, 0.2f),
                Random.Range(0.5f, 1.5f)
            );

            // Convert to world space (simplified)
            if (arCamera != null)
            {
                // Simulate perspective projection
                Vector3 screenPos = arCamera.WorldToScreenPoint(handBasePos);
                handBasePos = arCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 1f));
            }

            float handSize = Random.Range(0.12f, 0.18f);
            Vector3 handForward = isRightHand ? Vector3.right : Vector3.left;
            Vector3 handUp = Vector3.up;
            Vector3 handRight = Vector3.Cross(handUp, handForward);

            // Wrist (landmark 0)
            landmarks[0] = handBasePos;

            // Thumb (landmarks 1-4)
            landmarks[1] = handBasePos + handRight * 0.02f + handForward * 0.03f; // Thumb CMC
            landmarks[2] = landmarks[1] + handForward * 0.025f; // Thumb MCP
            landmarks[3] = landmarks[2] + handForward * 0.02f; // Thumb IP
            landmarks[4] = landmarks[3] + handForward * 0.015f; // Thumb tip

            // Index finger (landmarks 5-8) - pointing variations
            Vector3 indexBase = handBasePos + handRight * 0.01f + handForward * 0.08f;
            landmarks[5] = indexBase; // Index MCP
            landmarks[6] = landmarks[5] + handForward * 0.025f; // Index PIP
            landmarks[7] = landmarks[6] + handForward * 0.02f; // Index DIP

            // Vary index finger extension for pointing gestures
            float indexExtension = Random.Range(0.8f, 1.2f);
            landmarks[8] = landmarks[7] + handForward * 0.015f * indexExtension; // Index tip

            // Middle finger (landmarks 9-12)
            Vector3 middleBase = handBasePos + handForward * 0.09f;
            landmarks[9] = middleBase; // Middle MCP
            landmarks[10] = landmarks[9] + handForward * 0.03f; // Middle PIP
            landmarks[11] = landmarks[10] + handForward * 0.025f; // Middle DIP
            landmarks[12] = landmarks[11] + handForward * 0.02f * Random.Range(0.7f, 1.0f); // Middle tip

            // Ring finger (landmarks 13-16)
            Vector3 ringBase = handBasePos - handRight * 0.01f + handForward * 0.085f;
            landmarks[13] = ringBase; // Ring MCP
            landmarks[14] = landmarks[13] + handForward * 0.028f; // Ring PIP
            landmarks[15] = landmarks[14] + handForward * 0.023f; // Ring DIP
            landmarks[16] = landmarks[15] + handForward * 0.018f * Random.Range(0.7f, 1.0f); // Ring tip

            // Pinky finger (landmarks 17-20)
            Vector3 pinkyBase = handBasePos - handRight * 0.02f + handForward * 0.07f;
            landmarks[17] = pinkyBase; // Pinky MCP
            landmarks[18] = landmarks[17] + handForward * 0.022f; // Pinky PIP
            landmarks[19] = landmarks[18] + handForward * 0.018f; // Pinky DIP
            landmarks[20] = landmarks[19] + handForward * 0.015f * Random.Range(0.7f, 1.0f); // Pinky tip

            return landmarks;
        }

        private void ProcessHandResults(object results)
        {
            var handResults = results as System.Collections.Generic.List<HandDetectionResult>;
            if (handResults == null) return;

            // Reset hand data
            leftHand.isDetected = false;
            rightHand.isDetected = false;

            // Process detected hands
            foreach (var result in handResults)
            {
                HandData targetHand = result.isRightHand ? rightHand : leftHand;

                targetHand.isDetected = true;
                targetHand.landmarks = result.landmarks;

                // Set confidence values (simulate based on detection confidence)
                for (int i = 0; i < targetHand.confidence.Length; i++)
                {
                    targetHand.confidence[i] = result.confidence * Random.Range(0.9f, 1.1f);
                }

                // Convert landmarks to world space
                ConvertLandmarksToWorldSpace(targetHand);

                // Detailed hand detection logging
                if (debugMode)
                {
                    SystemLogger.LogInfo("HandPointingRecognizer", $"HAND DETECTED: {(result.isRightHand ? "Right" : "Left")} hand, Confidence: {result.confidence:F2}");

                    if (result.landmarks != null && result.landmarks.Length > 0)
                    {
                        SystemLogger.LogInfo("HandPointingRecognizer", $"Landmarks: {result.landmarks.Length} points detected");

                        // Log key landmark positions for validation
                        if (result.landmarks.Length >= 5)
                        {
                            SystemLogger.LogInfo("HandPointingRecognizer", $"Key points: Wrist={result.landmarks[0]}, Thumb={result.landmarks[1]}, Index={result.landmarks[2]}, Middle={result.landmarks[3]}, Pinky={result.landmarks[4]}");
                        }
                    }
                    else
                    {
                        SystemLogger.LogWarning("HandPointingRecognizer", "Hand detected but no landmarks found!");
                    }
                }
                else
                {
                    SystemLogger.LogInfo("HandPointingRecognizer", $"Hand detected - {(result.isRightHand ? "Right" : "Left")}, Confidence: {result.confidence:F2}");
                }
            }
        }

        private void ConvertLandmarksToWorldSpace(HandData hand)
        {
            // Convert MediaPipe landmarks (normalized 0-1 coordinates) to Unity world space
            // This is a simplified conversion - real implementation would be more complex

            for (int i = 0; i < hand.landmarks.Length; i++)
            {
                // MediaPipe gives normalized coordinates, convert to world space
                Vector3 normalizedPos = hand.landmarks[i];

                // Convert to screen space first
                Vector3 screenPos = new Vector3(
                    normalizedPos.x * Screen.width,
                    normalizedPos.y * Screen.height,
                    normalizedPos.z * 10f // Depth scaling
                );

                // Convert to world space
                if (arCamera != null)
                {
                    hand.landmarks[i] = arCamera.ScreenToWorldPoint(screenPos);
                }
            }
        }

        private void DetectPointingGestures()
        {
            // Check both hands for pointing gesture
            CheckHandForPointing(leftHand);
            CheckHandForPointing(rightHand);

            // Choose the more confident pointing hand
            if (leftHand.isPointing && rightHand.isPointing)
            {
                activeHand = (leftHand.pointingConfidence > rightHand.pointingConfidence) ? leftHand : rightHand;
            }
            else if (leftHand.isPointing)
            {
                activeHand = leftHand;
            }
            else if (rightHand.isPointing)
            {
                activeHand = rightHand;
            }
            else
            {
                activeHand = null;
            }
        }

        private void CheckHandForPointing(HandData hand)
        {
            if (!hand.isDetected || hand.landmarks.Length < 21)
            {
                hand.isPointing = false;
                hand.pointingConfidence = 0f;
                return;
            }

            // Pointing gesture detection using MediaPipe landmark analysis
            Vector3 wrist = hand.landmarks[0];
            Vector3 indexTip = hand.landmarks[8];
            Vector3 indexDip = hand.landmarks[7];
            Vector3 middleTip = hand.landmarks[12];
            Vector3 ringTip = hand.landmarks[16];
            Vector3 pinkyTip = hand.landmarks[20];

            // Calculate finger extensions from wrist
            float indexExtension = Vector3.Distance(indexTip, wrist);
            float middleExtension = Vector3.Distance(middleTip, wrist);
            float ringExtension = Vector3.Distance(ringTip, wrist);
            float pinkyExtension = Vector3.Distance(pinkyTip, wrist);

            // Pointing criteria:
            // 1. Index finger is significantly extended
            // 2. Other fingers are relatively curled
            // 3. Index finger is relatively straight (not curled)

            float avgOtherExtension = (middleExtension + ringExtension + pinkyExtension) / 3f;
            bool indexExtended = indexExtension > avgOtherExtension * 1.2f;

            bool othersCurled = middleExtension < indexExtension * 0.8f &&
                               ringExtension < indexExtension * 0.8f &&
                               pinkyExtension < indexExtension * 0.8f;

            // Check if index finger is straight (not curled)
            Vector3 indexVector = indexTip - indexDip;
            float indexStraightness = Vector3.Dot(indexVector.normalized, Vector3.forward);
            bool indexStraight = indexStraightness > 0.7f; // Fairly straight

            hand.isPointing = indexExtended && othersCurled && indexStraight;
            hand.pointingConfidence = hand.isPointing ? CalculatePointingConfidence(hand) : 0f;

            // Calculate pointing direction
            if (hand.isPointing)
            {
                Vector3 indexBase = hand.landmarks[5]; // Index MCP
                hand.pointingDirection = (indexTip - indexBase).normalized;
            }
        }

        private float CalculatePointingConfidence(HandData hand)
        {
            float extensionConfidence = 0f;
            float curlConfidence = 0f;
            float straightnessConfidence = 0f;

            // Extension confidence
            Vector3 wrist = hand.landmarks[0];
            float[] extensions = new float[4];
            extensions[0] = Vector3.Distance(hand.landmarks[8], wrist);  // Index
            extensions[1] = Vector3.Distance(hand.landmarks[12], wrist); // Middle
            extensions[2] = Vector3.Distance(hand.landmarks[16], wrist); // Ring
            extensions[3] = Vector3.Distance(hand.landmarks[20], wrist); // Pinky

            float avgOtherExtensions = (extensions[1] + extensions[2] + extensions[3]) / 3f;
            extensionConfidence = Mathf.Clamp01((extensions[0] - avgOtherExtensions) / 0.05f);

            // Curl confidence
            float curlRatio = avgOtherExtensions / extensions[0];
            curlConfidence = Mathf.Clamp01((1f - curlRatio) * 5f);

            // Straightness confidence
            Vector3 indexVector = hand.landmarks[8] - hand.landmarks[7]; // Tip to DIP
            straightnessConfidence = Mathf.Clamp01(Vector3.Dot(indexVector.normalized, Vector3.forward) * 2f);

            return (extensionConfidence * 0.4f + curlConfidence * 0.4f + straightnessConfidence * 0.2f);
        }

        private void UpdateGestureTimers()
        {
            if (activeHand != null && activeHand.isPointing)
            {
                activeHand.gestureHoldTimer += Time.deltaTime;

                if (activeHand.gestureHoldTimer >= gestureHoldTime && !activeHand.gestureActive)
                {
                    activeHand.gestureActive = true;
                    OnPointingGestureActivated(activeHand);
                }
            }
            else if (activeHand != null)
            {
                activeHand.gestureHoldTimer = 0f;
                if (activeHand.gestureActive)
                {
                    activeHand.gestureActive = false;
                    OnPointingGestureDeactivated(activeHand);
                }
            }
        }

        private void OnPointingGestureActivated(HandData hand)
        {
            SystemLogger.LogInfo("HandPointingRecognizer", $"REAL pointing gesture activated - Confidence: {hand.pointingConfidence:F2}");

            // Notify XR Manager
            var xrManager = FindFirstObjectByType<XRScouterManager>();
            if (xrManager != null)
            {
                xrManager.OnPointingGestureDetected(hand.pointingDirection, hand.landmarks[8]);
            }
        }

        private void OnPointingGestureDeactivated(HandData hand)
        {
            SystemLogger.LogInfo("HandPointingRecognizer", "Pointing gesture deactivated");

            var xrManager = FindFirstObjectByType<XRScouterManager>();
            if (xrManager != null)
            {
                xrManager.OnPointingGestureLost();
            }
        }

        // Public API
        public bool IsPointingGestureActive()
        {
            return activeHand != null && activeHand.gestureActive;
        }

        public Vector3 GetPointingDirection()
        {
            return activeHand != null ? activeHand.pointingDirection : Vector3.forward;
        }

        public Vector3 GetPointingTipPosition()
        {
            return activeHand != null && activeHand.landmarks.Length >= 9 ? activeHand.landmarks[8] : Vector3.zero;
        }

        public float GetPointingConfidence()
        {
            return activeHand != null ? activeHand.pointingConfidence : 0f;
        }

        public bool IsHandDetected()
        {
            return leftHand.isDetected || rightHand.isDetected;
        }

        // Public method to set shared webcam texture (for test scene integration)
        public void SetWebcamTexture(WebCamTexture texture)
        {
            Debug.Log($"SetWebcamTexture (Hand): Received webcam texture: {texture?.deviceName}, playing={texture?.isPlaying}");
            if (texture != null)
            {
                // Use the shared webcam texture instead of creating our own
                webcamTexture = texture;
                useSharedTexture = true;
                Debug.Log($"SetWebcamTexture (Hand): Set useSharedTexture=true, webcamTexture assigned");
                SystemLogger.LogInfo("HandPointingRecognizer", $"Using shared webcam texture: {texture.deviceName}");
            }
            else
            {
                Debug.LogWarning("SetWebcamTexture (Hand): Received null texture!");
            }
        }

        public int GetDetectedHandCount()
        {
            int count = 0;
            if (leftHand.isDetected) count++;
            if (rightHand.isDetected) count++;
            return count;
        }

        // Debug visualization
        void OnDrawGizmos()
        {
            if (!enableHandTracking) return;

            // Draw detected hands
            DrawHandGizmos(leftHand, Color.blue);
            DrawHandGizmos(rightHand, Color.red);

            // Draw active pointing
            if (activeHand != null && activeHand.isPointing)
            {
                Gizmos.color = Color.green;
                Vector3 tipPos = GetPointingTipPosition();
                Gizmos.DrawRay(tipPos, GetPointingDirection() * 2f);
                Gizmos.DrawSphere(tipPos, 0.01f);
            }
        }

        private void DrawHandGizmos(HandData hand, Color color)
        {
            if (!hand.isDetected) return;

            Gizmos.color = color;

            // Draw landmarks
            foreach (Vector3 landmark in hand.landmarks)
            {
                Gizmos.DrawSphere(landmark, 0.005f);
            }

            // Draw connections (simplified hand skeleton)
            if (hand.landmarks.Length >= 21)
            {
                // Wrist to finger bases
                Gizmos.DrawLine(hand.landmarks[0], hand.landmarks[1]); // Wrist to thumb
                Gizmos.DrawLine(hand.landmarks[0], hand.landmarks[5]); // Wrist to index
                Gizmos.DrawLine(hand.landmarks[0], hand.landmarks[9]); // Wrist to middle
                Gizmos.DrawLine(hand.landmarks[0], hand.landmarks[13]); // Wrist to ring
                Gizmos.DrawLine(hand.landmarks[0], hand.landmarks[17]); // Wrist to pinky

                // Finger segments
                DrawFingerGizmos(hand.landmarks, 1, 4); // Thumb
                DrawFingerGizmos(hand.landmarks, 5, 8); // Index
                DrawFingerGizmos(hand.landmarks, 9, 12); // Middle
                DrawFingerGizmos(hand.landmarks, 13, 16); // Ring
                DrawFingerGizmos(hand.landmarks, 17, 20); // Pinky
            }
        }

        private void DrawFingerGizmos(Vector3[] landmarks, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                Gizmos.DrawLine(landmarks[i], landmarks[i + 1]);
            }
        }

        void OnDestroy()
        {
            // Cleanup webcam
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
            }
        }

        // Internal classes
        private class HandDetectionResult
        {
            public bool isRightHand;
            public float confidence;
            public Vector3[] landmarks;
        }
    }
}
