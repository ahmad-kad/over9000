using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using TensorFlowLite;

namespace ScouterXR.AI
{
    /// <summary>
    /// Executes MediaPipe BlazePose TensorFlow Lite models (detection + landmark) from Unity.
    /// Stage 1: pose_detection.tflite produces the person bounding box.
    /// Stage 2: pose_landmark_(lite|full).tflite produces the 33 pose landmarks.
    /// </summary>
    public sealed class TFLiteModelRunner : MonoBehaviour
    {
        private const string ModelsFolderName = "Models";
        private const string PoseDetectionModelFileName = "pose_detection.tflite";
        private const string PoseLandmarkLiteModelFileName = "pose_landmark_lite.tflite";
        private const string PoseLandmarkFullModelFileName = "pose_landmark_full.tflite";
        private const int PoseLandmarkCount = 33;

        [Header("Model Settings")]
        public MediaPipeModelManager modelManager;
        [Tooltip("Use the heavier pose_landmark_full model (better accuracy, slower). Keep false for better performance.")]
        public bool useFullLandmarkModel = false; // Keep false for performance
        [Range(0.01f, 0.9f)]
        [Tooltip("Minimum confidence for pose detection (Stage 1). Lower = detect more poses but more false positives.")]
        public float detectionScoreThreshold = 0.1f; // Lowered for testing to reduce fallback
        [Range(0.01f, 0.5f)]
        [Tooltip("Minimum confidence for landmark tracking (Stage 2). Lower = more tracking but less stable.")]
        public float landmarkScoreThreshold = 0.01f; // Even lower to allow more tracking
        [Tooltip("Attempt to use the GPU delegate for the landmark model if available.")]
        public bool enableGpuForLandmarks = true;
        public bool debugMode = true; // Enable debug logging for landmark detection diagnosis

        // MediaPipe BlazePose components
        private PoseDetect poseDetector;
        private PoseLandmarkDetect poseLandmarker;
        private Interpreter landmarkInterpreter;

        // Smoothing filters
        private RelativeVelocityFilter3D[] landmarkFilters;
        
        // Store last detection result for landmark processing
        private PoseDetect.Result lastDetectionResult;

        // Result types
        public class PoseDetectionResult
        {
            public bool success = false;
            public Rect boundingBox = Rect.zero;
            public float confidence = 0f;
            public string errorMessage = "";
            public Vector3[] landmarks = new Vector3[PoseLandmarkCount];
            public bool isMediaPipeResult = true; // True if from MediaPipe, false if from computer vision fallback
        }

        public class PoseLandmarkResult
        {
            public bool success = false;
            public Vector3[] landmarks = new Vector3[PoseLandmarkCount];
            public float[] landmarkConfidence = new float[PoseLandmarkCount];
            public float overallConfidence = 0f;
            public string errorMessage = "";
        }

        void Start()
        {
            if (modelManager == null)
            {
                Debug.LogWarning("[TFLiteModelRunner] ModelManager not assigned at Start - will retry...");
                StartCoroutine(WaitForModelManagerAndInitialize());
                return;
            }

            InitializeBlazePoseDetectors();
            InitializeMediaPipeFilters();
        }

        private System.Collections.IEnumerator WaitForModelManagerAndInitialize()
        {
            // Wait up to 5 seconds for ModelManager to be assigned
            float waitTime = 0f;
            float maxWait = 5f;
            
            while (modelManager == null && waitTime < maxWait)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }
            
            if (modelManager == null)
            {
                Debug.LogError("[TFLiteModelRunner] ModelManager still not assigned after 5 seconds - cannot initialize");
                yield break;
            }
            
            Debug.Log("[TFLiteModelRunner] ModelManager now available - initializing...");
            InitializeBlazePoseDetectors();
            InitializeMediaPipeFilters();
        }

        void OnDestroy()
        {
            poseDetector?.Dispose();
            poseLandmarker?.Dispose();
            landmarkInterpreter?.Dispose();
        }

        private void InitializeBlazePoseDetectors()
        {
            if (modelManager == null)
            {
                Debug.LogError("[BlazePose] Cannot initialize - ModelManager is null");
                return;
            }

            try
            {
                // Load pose detection model
                byte[] detectionModelData = modelManager.GetPoseDetectionModelData();
                Debug.Log($"[BlazePose] Got pose detection model: {(detectionModelData != null ? detectionModelData.Length + " bytes" : "NULL")}");
                if (detectionModelData != null && detectionModelData.Length > 0)
                {
                    // PoseDetect constructor loads from file path, so write to temp file
                    string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "temp_pose_detection.tflite");
                    System.IO.File.WriteAllBytes(tempPath, detectionModelData);
                    
                    if (debugMode) Debug.Log($"[BlazePose] Wrote temp model to: {tempPath}");
                    
                    var detectionOptions = new PoseDetect.Options
                    {
                        modelPath = tempPath,
                        scoreThreshold = detectionScoreThreshold,
                        useNonMaxSuppression = true,
                        iouThreshold = 0.3f,
                        aspectMode = AspectMode.Fit
                    };
                    
                    if (debugMode) Debug.Log("[BlazePose] Creating PoseDetect instance...");
                    poseDetector = new PoseDetect(detectionOptions);
                    if (debugMode) Debug.Log("[BlazePose] Pose detector loaded successfully");
                }
                else
                {
                    Debug.LogError("[BlazePose] Pose detection model data not available");
                }

                // Load pose landmark model
                byte[] landmarkModelData = modelManager.GetPoseLandmarkModelData();
                Debug.Log($"[BlazePose] Got pose landmark model: {(landmarkModelData != null ? landmarkModelData.Length + " bytes" : "NULL")}");
                if (landmarkModelData != null && landmarkModelData.Length > 0)
                {
                    // PoseLandmarkDetect constructor loads from file path, so write to temp file
                    string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "temp_pose_landmark.tflite");
                    System.IO.File.WriteAllBytes(tempPath, landmarkModelData);
                    
                    if (debugMode) Debug.Log($"[BlazePose] Wrote temp landmark model to: {tempPath}");
                    
                    var landmarkOptions = new PoseLandmarkDetect.Options
                    {
                        modelPath = tempPath,
                        useWorldLandmarks = false,
                        useFilter = true,
                        filterVelocityScale = new Vector3(10, 10, 2)
                    };
                    landmarkOptions.AspectMode = AspectMode.Fit;
                    
                    if (debugMode) Debug.Log("[BlazePose] Creating PoseLandmarkDetect instance...");
                    poseLandmarker = new PoseLandmarkDetect(landmarkOptions);
                    if (debugMode) Debug.Log("[BlazePose] Pose landmarker loaded successfully");
                }
                else
                {
                    Debug.LogError("[BlazePose] Pose landmark model data not available");
                }

                // Alternative: Direct interpreter for landmark stage (fallback)
                if (landmarkModelData != null && landmarkModelData.Length > 0 && enableGpuForLandmarks)
                {
                    var options = new InterpreterOptions();
                    options.AddGpuDelegate();
                    landmarkInterpreter = new Interpreter(landmarkModelData, options);
                    landmarkInterpreter.AllocateTensors();
                    if (debugMode) Debug.Log("[BlazePose] Direct landmark interpreter with GPU ready");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BlazePose] Initialization failed: {e.Message}");
                Debug.LogError($"[BlazePose] Stack trace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    Debug.LogError($"[BlazePose] Inner exception: {e.InnerException.Message}");
                }
            }
        }

        private void InitializeMediaPipeFilters()
        {
            landmarkFilters = new RelativeVelocityFilter3D[PoseLandmarkCount];
            for (int i = 0; i < landmarkFilters.Length; i++)
            {
                // Optimized for responsiveness: smaller window, lower velocity scale
                // windowSize: 3 = less lag, faster response to movements
                // velocityScale: 5 = less smoothing, more accurate to actual motion
                landmarkFilters[i] = new RelativeVelocityFilter3D(
                    windowSize: 3,
                    velocityScale: 5f,
                    distanceMode: RelativeVelocityFilter.DistanceEstimationMode.ForceCurrentScale
                );
            }
            if (debugMode) Debug.Log($"[MediaPipe] Initialized {PoseLandmarkCount} pose landmark filters (optimized for responsiveness)");
        }

        /// <summary>
        /// Stage 1: Run pose detection to find person bounding box
        /// </summary>
        public IEnumerator RunPoseDetection(Texture2D inputTexture, System.Action<PoseDetectionResult> onComplete)
        {
            var result = new PoseDetectionResult();

            if (inputTexture == null)
            {
                result.errorMessage = "Input texture is null";
                onComplete(result);
                yield break;
            }

            if (poseDetector == null)
            {
                result.errorMessage = "Pose detector not initialized";
                // Fallback to computer vision
                result = AnalyzeTextureForPerson(inputTexture);
                onComplete(result);
                yield break;
            }

            try
            {
                if (debugMode) Debug.Log($"[STAGE 1] Running pose detection on {inputTexture.width}x{inputTexture.height} frame");

                // Run detection directly on input texture
                poseDetector.Run(inputTexture);

                // Get results
                var detectionResult = poseDetector.GetResults();
                lastDetectionResult = detectionResult; // Store for landmark processing

                if (debugMode)
                {
                    Debug.Log($"[TFLiteModelRunner] Pose detection score: {detectionResult.score:F3}, threshold: {detectionScoreThreshold:F3}, success: {detectionResult.score >= detectionScoreThreshold}");
                }
                
                if (detectionResult.score > detectionScoreThreshold)
                {
                    result.success = true;
                    result.confidence = detectionResult.score;
                    result.boundingBox = detectionResult.rect;

                    // Extract keypoints from detection (4 keypoints: nose, left eye, right eye, left ear, right ear, left shoulder, right shoulder, left elbow, right elbow, left wrist, right wrist, left hip, right hip, left knee, right knee, left ankle, right ankle)
                    if (detectionResult.keypoints != null && detectionResult.keypoints.Length >= 4)
                    {
                        // Map keypoints to landmarks (simplified)
                        for (int i = 0; i < Mathf.Min(4, result.landmarks.Length); i++)
                        {
                            if (i < detectionResult.keypoints.Length)
                            {
                                Vector2 kp = detectionResult.keypoints[i];
                                result.landmarks[i] = new Vector3(kp.x, kp.y, 0f);
                            }
                        }
                    }

                    if (debugMode) Debug.Log($"[STAGE 1] Person detected: confidence={result.confidence:F3}, bbox={result.boundingBox}");
                }
                else
                {
                    // Fallback to computer vision if detection confidence is low
                    result = AnalyzeTextureForPerson(inputTexture);
                }
            }
            catch (System.Exception e)
            {
                result.errorMessage = $"Pose detection failed: {e.Message}";
                if (debugMode) Debug.LogError($"[STAGE 1] Error: {result.errorMessage}");
                // Fallback
                result = AnalyzeTextureForPerson(inputTexture);
            }

            onComplete(result);
        }

        /// <summary>
        /// Stage 2: Run pose landmark detection on full frame
        /// (PoseLandmarkDetect handles cropping internally using the detection result)
        /// </summary>
        public IEnumerator RunPoseLandmark(Texture2D inputTexture, System.Action<PoseLandmarkResult> onComplete)
        {
            var result = new PoseLandmarkResult();

            if (inputTexture == null)
            {
                result.errorMessage = "Input texture is null";
                onComplete(result);
                yield break;
            }

            if (poseLandmarker == null && landmarkInterpreter == null)
            {
                result.errorMessage = "Pose landmarker not initialized";
                // Fallback to analysis
                result = AnalyzeTextureForPoseLandmarks(inputTexture);
                onComplete(result);
                yield break;
            }

            try
            {
                if (debugMode) Debug.Log($"[STAGE 2] Running pose landmark detection on {inputTexture.width}x{inputTexture.height} full frame");

                // No preprocessing needed - PoseLandmarkDetect handles cropping internally
                Texture2D processedTexture = inputTexture;

                if (poseLandmarker != null)
                {
                    // Set the pose detection result so PoseLandmarkDetect can calculate crop matrix
                    if (lastDetectionResult.score > 0)
                    {
                        poseLandmarker.Pose = lastDetectionResult;
                        if (debugMode) Debug.Log($"[STAGE 2] Set pose detection result (score={lastDetectionResult.score:F3})");
                    }
                    else
                    {
                        Debug.LogWarning("[STAGE 2] No valid pose detection result available for landmark processing");
                    }
                    
                    // Use MediaPipe wrapper
                    if (debugMode) Debug.Log($"[STAGE 2] Calling poseLandmarker.Run()...");
                    poseLandmarker.Run(processedTexture);
                    
                    if (debugMode) Debug.Log($"[STAGE 2] Calling poseLandmarker.GetResult()...");
                    var landmarkResult = poseLandmarker.GetResult();

                    // Debug: Check what we got
                    if (debugMode)
                    {
                        Debug.Log($"[STAGE 2] Result score: {landmarkResult.score:F3}");
                        Debug.Log($"[STAGE 2] Viewport landmarks count: {(landmarkResult.viewportLandmarks != null ? landmarkResult.viewportLandmarks.Length : 0)}");
                    }

                    // Use configurable threshold for landmark quality
                    // MediaPipe landmark scores can be low even for good detections
                    // The actual quality is determined by individual landmark visibility scores
                    if (landmarkResult.score > landmarkScoreThreshold)
                    {
                        result.success = true;
                        result.overallConfidence = landmarkResult.score;

                        // Map landmarks from viewportLandmarks (Vector4[] where w = visibility)
                        var landmarks = landmarkResult.viewportLandmarks;
                        if (landmarks != null)
                        {
                            for (int i = 0; i < Mathf.Min(PoseLandmarkCount, landmarks.Length); i++)
                            {
                                if (i < landmarks.Length)
                                {
                                    Vector4 lm4 = landmarks[i];
                                    Vector3 lm = new Vector3(lm4.x, lm4.y, lm4.z);
                                    
                                    // Apply smoothing
                                    if (landmarkFilters != null && i < landmarkFilters.Length)
                                    {
                                        lm = landmarkFilters[i].Apply(Time.time, 1.0f, lm);
                                    }
                                    
                                    result.landmarks[i] = lm;
                                    result.landmarkConfidence[i] = lm4.w; // visibility in w component
                                }
                            }

                            if (debugMode) Debug.Log($"[STAGE 2] ✅ Landmarks detected: {landmarks.Length} points, confidence={result.overallConfidence:F3}");
                        }
                    }
                    else
                    {
                        if (debugMode) Debug.LogWarning($"[STAGE 2] ❌ Landmark score too low: {landmarkResult.score:F3} (threshold: {landmarkScoreThreshold:F2})");
                    }
                }
                else if (landmarkInterpreter != null)
                {
                    // Use direct interpreter
                    int[] inputShape = landmarkInterpreter.GetInputTensorInfo(0).shape;
                    int inputHeight = inputShape[1];
                    int inputWidth = inputShape[2];

                    // Resize if needed
                    Texture2D resizedInput = ResizeTexture(processedTexture, inputWidth, inputHeight);

                    // Get input/output tensors
                    float[] inputData = new float[inputWidth * inputHeight * 3];
                    Color[] pixels = resizedInput.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        Color p = pixels[i];
                        inputData[i * 3] = p.r;
                        inputData[i * 3 + 1] = p.g;
                        inputData[i * 3 + 2] = p.b;
                    }

                    landmarkInterpreter.SetInputTensorData(0, inputData);
                    landmarkInterpreter.Invoke();

                    // Get output
                    float[] outputData = new float[PoseLandmarkCount * 4]; // x, y, z, visibility
                    landmarkInterpreter.GetOutputTensorData(0, outputData);

                    result.success = true;
                    result.overallConfidence = 0.8f; // Default

                    for (int i = 0; i < PoseLandmarkCount; i++)
                    {
                        float x = outputData[i * 4];
                        float y = outputData[i * 4 + 1];
                        float z = outputData[i * 4 + 2];
                        float visibility = outputData[i * 4 + 3];

                        Vector3 rawLandmark = new Vector3(Mathf.Clamp01(x), Mathf.Clamp01(y), z);
                        Vector3 smoothed = landmarkFilters[i].Apply(Time.time, 1.0f, rawLandmark);
                        result.landmarks[i] = smoothed;
                        result.landmarkConfidence[i] = Mathf.Clamp01(visibility);
                    }

                    result.overallConfidence = result.landmarkConfidence.Average();
                }

                Destroy(processedTexture);
            }
            catch (System.Exception e)
            {
                result.errorMessage = $"Pose landmark failed: {e.Message}";
                Debug.LogError($"[STAGE 2] Error: {result.errorMessage}");
                Debug.LogError($"[STAGE 2] Stack trace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    Debug.LogError($"[STAGE 2] Inner exception: {e.InnerException.Message}");
                }
                // Fallback
                result = AnalyzeTextureForPoseLandmarks(inputTexture);
            }

            onComplete(result);
        }

        private Texture2D PreprocessTextureForPoseLandmark(Texture2D inputTexture)
        {
            // Return texture as-is, let the pose landmark detector handle preprocessing
            return inputTexture;
        }

        private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            if (source.width == targetWidth && source.height == targetHeight)
            {
                return source;
            }

            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            
            return result;
        }

        #region Computer Vision Fallbacks

        private PoseDetectionResult AnalyzeTextureForPerson(Texture2D texture)
        {
            var result = new PoseDetectionResult();
            result.isMediaPipeResult = false; // This is computer vision fallback, not MediaPipe

            if (debugMode) Debug.Log($"[FALLBACK] Analyzing {texture.width}x{texture.height} for person (isMediaPipeResult={result.isMediaPipeResult})");

            try
            {
                Color[] pixels = texture.GetPixels();
                int width = texture.width;
                int height = texture.height;

                // Skin detection
                int skinPixels = 0;
                Vector2 skinCenter = Vector2.zero;
                int sampleStep = 4;
                for (int y = 0; y < height; y += sampleStep)
                {
                    for (int x = 0; x < width; x += sampleStep)
                    {
                        Color pixel = pixels[y * width + x];
                        float r = pixel.r, g = pixel.g, b = pixel.b;
                        float max = Mathf.Max(r, g, b);
                        float min = Mathf.Min(r, g, b);

                        bool isSkin = (r > 0.25f && g > 0.15f && b > 0.05f) &&
                                      (r > g * 0.9f) &&
                                      ((max - min) < 0.5f) &&
                                      (r / (g + 0.1f) > 0.9f);

                        if (isSkin)
                        {
                            skinPixels++;
                            skinCenter += new Vector2(x, y);
                        }
                    }
                }

                float skinRatio = (float)skinPixels / ((width / sampleStep) * (height / sampleStep));
                if (skinRatio > 0.01f)
                {
                    result.success = true;
                    result.confidence = Mathf.Clamp01(skinRatio * 3f);

                    if (skinPixels > 0)
                    {
                        skinCenter /= skinPixels;
                        skinCenter.x /= width;
                        skinCenter.y /= height;

                        float personWidth = Mathf.Clamp(skinRatio * 2f, 0.2f, 0.8f);
                        float personHeight = Mathf.Clamp(personWidth * 1.8f, 0.2f, 1.0f);
                        float bboxX = Mathf.Clamp(skinCenter.x - personWidth / 2, 0f, 1f - personWidth);
                        float bboxY = Mathf.Clamp(skinCenter.y - personHeight / 2, 0f, 1f - personHeight);

                        result.boundingBox = new Rect(bboxX, bboxY, personWidth, personHeight);
                    }
                }
            }
            catch (System.Exception e)
            {
                result.errorMessage = e.Message;
            }

            return result;
        }

        private PoseLandmarkResult AnalyzeTextureForPoseLandmarks(Texture2D croppedTexture)
        {
            var result = new PoseLandmarkResult();

            if (debugMode) Debug.Log($"[FALLBACK] Analyzing cropped {croppedTexture.width}x{croppedTexture.height} for landmarks");

            try
            {
                Color[] pixels = croppedTexture.GetPixels();
                int width = croppedTexture.width;
                int height = croppedTexture.height;

                // Find head center (brightest area)
                Vector2 headCenter = FindHeadCenterInTexture(croppedTexture);
                float headSize = EstimateHeadSize(croppedTexture, headCenter);

                // Find shoulders
                Vector2[] shoulders = FindShoulderPositions(croppedTexture, headCenter, headSize);

                // Generate full pose
                GeneratePoseKeypointsFromAnalysis(result, headCenter, headSize, shoulders, width, height);

                result.success = result.overallConfidence > 0.4f;
            }
            catch (System.Exception e)
            {
                result.errorMessage = e.Message;
            }

            return result;
        }

        private Vector2 FindHeadCenterInTexture(Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();
            int width = texture.width;
            int height = texture.height;

            float maxBrightness = 0f;
            Vector2 brightestPos = new Vector2(width * 0.5f, height * 0.3f);

            int searchHeight = height / 3;
            for (int y = 0; y < searchHeight; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = pixels[y * width + x];
                    float brightness = pixel.grayscale;

                    if (IsSkinTone(pixel))
                    {
                        brightness *= 1.5f;
                    }

                    if (brightness > maxBrightness)
                    {
                        maxBrightness = brightness;
                        brightestPos = new Vector2(x, y);
                    }
                }
            }

            return new Vector2(brightestPos.x / width, brightestPos.y / height);
        }

        private float EstimateHeadSize(Texture2D texture, Vector2 headCenter)
        {
            Color[] pixels = texture.GetPixels();
            int width = texture.width;
            int height = texture.height;

            int centerX = (int)(headCenter.x * width);
            int centerY = (int)(headCenter.y * height);

            int skinCount = 0;
            int radius = Mathf.Min(width, height) / 8;

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;

                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        if (IsSkinTone(pixels[y * width + x]))
                        {
                            skinCount++;
                        }
                    }
                }
            }

            float headSizeRatio = Mathf.Sqrt((float)skinCount) / Mathf.Min(width, height);
            return Mathf.Clamp(headSizeRatio, 0.05f, 0.2f);
        }

        private Vector2[] FindShoulderPositions(Texture2D texture, Vector2 headCenter, float headSize)
        {
            Color[] pixels = texture.GetPixels();
            int width = texture.width;
            int height = texture.height;

            int shoulderY = (int)((headCenter.y + headSize * 2) * height);
            shoulderY = Mathf.Clamp(shoulderY, 0, height - 1);

            int leftShoulder = width;
            int rightShoulder = 0;

            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[shoulderY * width + x];
                if (pixel.grayscale > 0.2f || IsSkinTone(pixel))
                {
                    leftShoulder = Mathf.Min(leftShoulder, x);
                    rightShoulder = Mathf.Max(rightShoulder, x);
                }
            }

            if (leftShoulder >= rightShoulder)
            {
                float shoulderWidth = headSize * 2.5f;
                leftShoulder = (int)((headCenter.x - shoulderWidth) * width);
                rightShoulder = (int)((headCenter.x + shoulderWidth) * width);
            }

            Vector2 leftShoulderPos = new Vector2((float)leftShoulder / width, (float)shoulderY / height);
            Vector2 rightShoulderPos = new Vector2((float)rightShoulder / width, (float)shoulderY / height);

            return new Vector2[] { leftShoulderPos, rightShoulderPos };
        }

        private void GeneratePoseKeypointsFromAnalysis(PoseLandmarkResult result, Vector2 headCenter, float headSize, Vector2[] shoulderPositions, int width, int height)
        {
            Vector2 leftShoulder = shoulderPositions[0];
            Vector2 rightShoulder = shoulderPositions[1];

            float shoulderWidth = rightShoulder.x - leftShoulder.x;
            float shoulderCenterX = (leftShoulder.x + rightShoulder.x) / 2f;
            float shoulderCenterY = (leftShoulder.y + rightShoulder.y) / 2f;

            result.landmarks[0] = new Vector3(headCenter.x, headCenter.y, 0f); // nose
            result.landmarkConfidence[0] = 0.9f;

            float eyeOffsetX = headSize * 0.25f;
            float eyeOffsetY = headSize * 0.1f;
            result.landmarks[1] = new Vector3(headCenter.x - eyeOffsetX, headCenter.y - eyeOffsetY, 0f);
            result.landmarks[2] = new Vector3(headCenter.x + eyeOffsetX, headCenter.y - eyeOffsetY, 0f);
            result.landmarkConfidence[1] = result.landmarkConfidence[2] = 0.85f;

            float earOffsetX = headSize * 0.45f;
            result.landmarks[7] = new Vector3(headCenter.x - earOffsetX, headCenter.y, 0f);
            result.landmarks[8] = new Vector3(headCenter.x + earOffsetX, headCenter.y, 0f);
            result.landmarkConfidence[7] = result.landmarkConfidence[8] = 0.8f;

            result.landmarks[11] = new Vector3(leftShoulder.x, leftShoulder.y, 0f);
            result.landmarks[12] = new Vector3(rightShoulder.x, rightShoulder.y, 0f);
            result.landmarkConfidence[11] = result.landmarkConfidence[12] = 0.95f;

            float elbowOffsetX = shoulderWidth * 0.4f;
            float elbowOffsetY = shoulderWidth * 0.3f;
            result.landmarks[13] = new Vector3(leftShoulder.x - elbowOffsetX, shoulderCenterY + elbowOffsetY, 0f);
            result.landmarks[14] = new Vector3(rightShoulder.x + elbowOffsetX, shoulderCenterY + elbowOffsetY, 0f);
            result.landmarkConfidence[13] = result.landmarkConfidence[14] = 0.85f;

            float wristOffsetX = shoulderWidth * 0.6f;
            float wristOffsetY = shoulderWidth * 0.8f;
            result.landmarks[15] = new Vector3(leftShoulder.x - wristOffsetX, shoulderCenterY + wristOffsetY, 0f);
            result.landmarks[16] = new Vector3(rightShoulder.x + wristOffsetX, shoulderCenterY + wristOffsetY, 0f);
            result.landmarkConfidence[15] = result.landmarkConfidence[16] = 0.8f;

            float hipOffsetX = shoulderWidth * 0.15f;
            float hipOffsetY = shoulderWidth * 0.9f;
            result.landmarks[23] = new Vector3(leftShoulder.x + hipOffsetX, shoulderCenterY + hipOffsetY, 0f);
            result.landmarks[24] = new Vector3(rightShoulder.x - hipOffsetX, shoulderCenterY + hipOffsetY, 0f);
            result.landmarkConfidence[23] = result.landmarkConfidence[24] = 0.85f;

            float kneeOffsetY = shoulderWidth * 1.6f;
            result.landmarks[25] = new Vector3(leftShoulder.x + hipOffsetX, shoulderCenterY + kneeOffsetY, 0f);
            result.landmarks[26] = new Vector3(rightShoulder.x - hipOffsetX, shoulderCenterY + kneeOffsetY, 0f);
            result.landmarkConfidence[25] = result.landmarkConfidence[26] = 0.8f;

            float ankleOffsetY = shoulderWidth * 2.2f;
            result.landmarks[27] = new Vector3(leftShoulder.x + hipOffsetX, shoulderCenterY + ankleOffsetY, 0f);
            result.landmarks[28] = new Vector3(rightShoulder.x - hipOffsetX, shoulderCenterY + ankleOffsetY, 0f);
            result.landmarkConfidence[27] = result.landmarkConfidence[28] = 0.75f;

            // Fill remaining landmarks with interpolated values
            for (int i = 0; i < result.landmarks.Length; i++)
            {
                if (result.landmarks[i] == Vector3.zero && result.landmarkConfidence[i] == 0f)
                {
                    if (i >= 17 && i <= 22) // Left/Right hand
                    {
                        result.landmarks[i] = Vector3.Lerp(result.landmarks[15], result.landmarks[16], (i - 17) / 6f);
                        result.landmarkConfidence[i] = 0.7f;
                    }
                    else if (i >= 29 && i <= 32) // Left/Right foot index
                    {
                        result.landmarks[i] = Vector3.Lerp(result.landmarks[27], result.landmarks[28], (i - 29) / 4f);
                        result.landmarkConfidence[i] = 0.7f;
                    }
                    else
                    {
                        result.landmarks[i] = result.landmarks[0]; // Default to head position
                        result.landmarkConfidence[i] = 0.6f;
                    }
                }
            }

            result.overallConfidence = result.landmarkConfidence.Average();
        }

        private float CalculatePoseConfidence(Vector3[] landmarks, Vector2 headCenter, Vector2[] shoulderPositions)
        {
            float confidence = 0.5f;

            if (shoulderPositions.Length >= 2)
            {
                float shoulderYDiff = Mathf.Abs(shoulderPositions[0].y - shoulderPositions[1].y);
                if (shoulderYDiff < 0.1f)
                    confidence += 0.2f;
            }

            int validKeypoints = 0;
            foreach (Vector3 landmark in landmarks)
            {
                if (landmark.x >= 0f && landmark.x <= 1f && landmark.y >= 0f && landmark.y <= 1f)
                    validKeypoints++;
            }

            confidence += (validKeypoints / (float)landmarks.Length) * 0.3f;

            return Mathf.Clamp01(confidence);
        }

        private bool IsSkinTone(Color pixel)
        {
            float r = pixel.r, g = pixel.g, b = pixel.b;
            float max = Mathf.Max(r, g, b);
            float min = Mathf.Min(r, g, b);

            return (r > 0.3f && g > 0.2f && b > 0.1f) &&
                   (r > g && g > b) &&
                   ((max - min) < 0.4f) &&
                   (r / (g + 0.1f) > 1.0f);
        }

        #endregion

    }
}
