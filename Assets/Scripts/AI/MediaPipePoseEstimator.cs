using System.Collections;
using UnityEngine;

namespace ScouterXR.AI
{
    /// <summary>
    /// High-level component that captures frames from a webcam (or supplied texture),
    /// runs the BlazePose detection/landmark pipeline, and exposes the latest pose data.
    /// NOTE: TFLiteModelRunner must exist in the scene (found via FindFirstObjectByType)
    /// </summary>
    // REMOVED: [RequireComponent(typeof(TFLiteModelRunner))] - causes duplicate instances
    public sealed class MediaPipePoseEstimator : MonoBehaviour
    {
        [Header("Pose Settings")]
        public bool useWebcamFeed = true;
        public string preferredWebcam;
        public int webcamWidth = 640;
        public int webcamHeight = 480;
        public int webcamFPS = 30;
        [Tooltip("Process every Nth frame (1 = every frame, 2 = every other frame for better performance)")]
        public int processEveryNFrames = 2; // Process every other frame for performance
        [Tooltip("Target inference rate in seconds (now frame-driven, used for reference/debugging).")]
        public float inferenceInterval = 0.033f; // ~30 FPS target rate
        public bool debugMode = false;

        [Header("References")]
        public Camera arCamera;
        public MediaPipeModelManager modelManager;

        [HideInInspector] public TFLiteModelRunner tfliteRunner;
        private WebCamTexture webcamTexture;
        private Texture2D frameTexture;
        private bool isProcessingFrame = false;
        private bool isPoseProcessingLoopRunning = false;
        private int frameCounter = 0;
        private int lastCopiedFrame = -1;

        private Vector3[] latestLandmarks = new Vector3[33];
        private float latestConfidence;
        private float lastUpdateTime;

        public Vector3[] LatestLandmarks => latestLandmarks;
        public float LatestConfidence => latestConfidence;
        public float LastUpdateTime => lastUpdateTime;
        public WebCamTexture SharedWebcamTexture => webcamTexture;

        private void Awake()
        {
            if (debugMode)
            {
                Debug.Log("[MediaPipePoseEstimator] 🔧 Awake: Initializing components...");
            }

            // Try to find TFLiteModelRunner on same GameObject first
            tfliteRunner = GetComponent<TFLiteModelRunner>();
            if (debugMode)
            {
                Debug.Log($"[MediaPipePoseEstimator] TFLiteModelRunner on same GO: {(tfliteRunner != null)}");
            }

            // If not found on same GameObject, search in scene
            if (tfliteRunner == null)
            {
                tfliteRunner = FindFirstObjectByType<TFLiteModelRunner>();
                if (debugMode)
                {
                    Debug.Log($"[MediaPipePoseEstimator] TFLiteModelRunner found in scene: {(tfliteRunner != null)}");
                }
            }

            if (modelManager == null)
            {
                modelManager = FindFirstObjectByType<MediaPipeModelManager>();
                if (debugMode)
                {
                    Debug.Log($"[MediaPipePoseEstimator] MediaPipeModelManager found: {(modelManager != null)}");
                }
            }

            if (arCamera == null)
            {
                arCamera = Camera.main;
                if (debugMode)
                {
                    Debug.Log($"[MediaPipePoseEstimator] AR Camera (Main): {(arCamera != null)}");
                }
            }

            if (debugMode)
            {
                Debug.Log("[MediaPipePoseEstimator] ✅ Awake complete");
            }
        }

        private void OnEnable()
        {
            if (useWebcamFeed)
            {
                StartCoroutine(InitializeWebcam());
            }
        }

        private void OnDisable()
        {
            Debug.Log($"[MediaPipePoseEstimator] 🛑 OnDisable called - destroying resources. webcamTexture={(webcamTexture != null)}, frameTexture={(frameTexture != null)}");

            if (webcamTexture != null)
            {
                webcamTexture.Stop();
                Debug.Log("[MediaPipePoseEstimator] Stopped webcam");
            }

            if (frameTexture != null)
            {
                Debug.Log("[MediaPipePoseEstimator] 🗑️ DESTROYING frameTexture in OnDisable!");
                Destroy(frameTexture);
                frameTexture = null;
            }

            isProcessingFrame = false; // Reset processing flag
            isPoseProcessingLoopRunning = false; // Reset loop flag

            Debug.Log("[MediaPipePoseEstimator] ✅ OnDisable complete");
        }

        private IEnumerator InitializeWebcam()
        {
            Debug.Log($"[MediaPipePoseEstimator] 📷 Available webcam devices: {WebCamTexture.devices.Length}");
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                var device = WebCamTexture.devices[i];
                Debug.Log($"[MediaPipePoseEstimator]   Device {i}: '{device.name}' frontFacing={device.isFrontFacing}");
            }

            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogError("[MediaPipePoseEstimator] ❌ No webcam devices detected at all!");
                yield break;
            }

            string deviceName = WebCamTexture.devices[0].name;
            Debug.Log($"[MediaPipePoseEstimator] 🎯 Selected default device: '{deviceName}'");

            if (!string.IsNullOrEmpty(preferredWebcam))
            {
                Debug.Log($"[MediaPipePoseEstimator] 🔍 Looking for preferred device containing: '{preferredWebcam}'");
                foreach (var device in WebCamTexture.devices)
                {
                    if (device.name.Contains(preferredWebcam))
                    {
                        deviceName = device.name;
                        Debug.Log($"[MediaPipePoseEstimator] ✅ Found preferred device: '{deviceName}'");
                        break;
                    }
                }
                if (deviceName != WebCamTexture.devices[0].name)
                {
                    Debug.Log($"[MediaPipePoseEstimator] 🔄 Using preferred device: '{deviceName}'");
                }
                else
                {
                    Debug.Log($"[MediaPipePoseEstimator] ⚠️ Preferred device '{preferredWebcam}' not found, using default: '{deviceName}'");
                }
            }

            webcamTexture = new WebCamTexture(deviceName, webcamWidth, webcamHeight, webcamFPS);
            Debug.Log($"[MediaPipePoseEstimator] 📹 CREATED WebCamTexture: device='{deviceName}', requested={webcamWidth}x{webcamHeight}@{webcamFPS}fps");
            webcamTexture.Play();
            Debug.Log($"[MediaPipePoseEstimator] ▶️ STARTED webcam playback");

            // Wait until webcam initializes with timeout and diagnostics
            Debug.Log($"[MediaPipePoseEstimator] Waiting for webcam to initialize...");
            float startTime = Time.time;
            float timeout = 10f; // 10 second timeout

            while (Time.time - startTime < timeout)
            {
                Debug.Log($"[MediaPipePoseEstimator] 🔍 WEBCAM STATUS: width={webcamTexture.width}, height={webcamTexture.height}, isPlaying={webcamTexture.isPlaying}, didUpdateThisFrame={webcamTexture.didUpdateThisFrame}, deviceName='{webcamTexture.deviceName}'");

                if (webcamTexture.width > 16 && webcamTexture.height > 16)
                {
            Debug.Log($"[MediaPipePoseEstimator] Webcam initialized: {webcamTexture.width}x{webcamTexture.height}");
            frameTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
            Debug.Log($"[MediaPipePoseEstimator] ✅ CREATED frameTexture: {frameTexture.width}x{frameTexture.height}, frameTexture != null: {frameTexture != null}");

                    // Assign webcam texture to RawImage for display
                    var rawImage = FindFirstObjectByType<UnityEngine.UI.RawImage>();
                    if (rawImage != null)
                    {
                        rawImage.texture = webcamTexture;
                        Debug.Log("[MediaPipePoseEstimator] Assigned webcam texture to RawImage display");
                    }
                    else
                    {
                        Debug.LogWarning("[MediaPipePoseEstimator] No RawImage found for webcam display");
                    }

            if (!isPoseProcessingLoopRunning)
            {
                isPoseProcessingLoopRunning = true;
                Debug.Log($"[MediaPipePoseEstimator] 🚀 STARTING PoseProcessingLoop - frameTexture ready: {frameTexture != null}");
                StartCoroutine(PoseProcessingLoop());
            }
            else
            {
                Debug.LogWarning("[MediaPipePoseEstimator] PoseProcessingLoop already running - not starting another instance");
            }
                    yield break;
                }

                yield return new WaitForSeconds(0.5f); // Check every 0.5 seconds
            }

            // Timeout reached - webcam failed to initialize
            Debug.LogError($"[MediaPipePoseEstimator] ❌ WEBCAM INITIALIZATION TIMEOUT after {timeout}s!");
            Debug.LogError($"[MediaPipePoseEstimator] 📊 FINAL WEBCAM STATE: width={webcamTexture.width}, height={webcamTexture.height}, isPlaying={webcamTexture.isPlaying}, didUpdateThisFrame={webcamTexture.didUpdateThisFrame}");
            Debug.LogError($"[MediaPipePoseEstimator] 📋 WEBCAM DEVICES: {WebCamTexture.devices.Length} total");
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                Debug.LogError($"[MediaPipePoseEstimator]   Device {i}: '{WebCamTexture.devices[i].name}' available={WebCamTexture.devices[i].isFrontFacing}");
            }
            Debug.LogError($"[MediaPipePoseEstimator] 🎯 REQUESTED: {webcamWidth}x{webcamHeight}@{webcamFPS}fps, device='{deviceName}'");

            // Clean up failed webcam
            if (webcamTexture != null)
            {
                webcamTexture.Stop();
                webcamTexture = null;
            }

            yield break;
        }

        private IEnumerator PoseProcessingLoop()
        {
            if (debugMode)
            {
                Debug.Log("[MediaPipePoseEstimator] PoseProcessingLoop started");
            }

            // Safety check: ensure webcam and frameTexture are ready
            if (webcamTexture == null || frameTexture == null)
            {
                Debug.LogError($"[MediaPipePoseEstimator] PoseProcessingLoop aborted - webcamTexture or frameTexture not ready! webcamTexture={(webcamTexture != null)}, frameTexture={(frameTexture != null)}");
                isPoseProcessingLoopRunning = false;
                yield break;
            }

            // For camera feed systems, respond to actual frames, not fixed timers
            // This ensures we process fresh frames as they arrive from the camera

            while (enabled)
            {
                if (webcamTexture != null)
                {
                    // Wait for the camera to actually provide a new frame
                    if (debugMode)
                        Debug.Log("[MediaPipePoseEstimator] Waiting for camera frame...");

                    yield return new WaitUntil(() => webcamTexture.didUpdateThisFrame);

                    if (debugMode)
                        Debug.Log("[MediaPipePoseEstimator] Camera frame received, processing...");

                    // Log webcam status
                    if (debugMode && Time.frameCount % 120 == 0) // Every 4 seconds
                    {
                        Debug.Log($"[MediaPipePoseEstimator] Webcam status: width={webcamTexture.width}, height={webcamTexture.height}, isPlaying={webcamTexture.isPlaying}, didUpdateThisFrame={webcamTexture.didUpdateThisFrame}");
                    }

                    // Only process if frameTexture is ready (webcam fully initialized)
                    if (frameTexture == null)
                    {
                        // Emergency recovery: try to recreate frameTexture if webcam is still working
                        if (webcamTexture != null && webcamTexture.width > 16 && webcamTexture.height > 16)
                        {
                            Debug.Log($"[MediaPipePoseEstimator] 🔄 EMERGENCY: Recreating frameTexture! webcamTexture dimensions: {webcamTexture.width}x{webcamTexture.height}");
                            frameTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
                            Debug.Log($"[MediaPipePoseEstimator] ✅ Recreated frameTexture: {frameTexture.width}x{frameTexture.height}");
                        }
                        else
                        {
                            Debug.LogWarning($"[MediaPipePoseEstimator] 🚨 CRITICAL: Skipping frame processing - frameTexture is null and cannot be recreated! webcamTexture={(webcamTexture != null)}, dimensions={(webcamTexture != null ? $"{webcamTexture.width}x{webcamTexture.height}" : "null")}, isPlaying={webcamTexture?.isPlaying}, isPoseProcessingLoopRunning={isPoseProcessingLoopRunning}");
                            continue;
                        }
                    }

                    // Skip frames for performance (process every Nth frame)
                    frameCounter++;
                    if (frameCounter % processEveryNFrames != 0)
                    {
                        if (debugMode && frameCounter % 60 == 0) // Log occasionally
                            Debug.Log($"[MediaPipePoseEstimator] Skipping frame {frameCounter} for performance (processEveryNFrames={processEveryNFrames})");
                        continue;
                    }

                    // Skip if already processing a frame (prevent overlapping inference)
                    if (isProcessingFrame)
                    {
                        if (debugMode && Time.frameCount % 60 == 0) // Log occasionally
                            Debug.Log("[MediaPipePoseEstimator] Skipping frame - already processing previous frame");
                        continue;
                    }
                    isProcessingFrame = true;

                    // Process the fresh frame immediately
                    UpdateFrameTexture();
                    yield return RunInference();
                    isProcessingFrame = false; // Reset flag after processing completes

                    if (debugMode && Time.frameCount % 60 == 0)
                        Debug.Log("[MediaPipePoseEstimator] Frame processing completed, waiting for next frame...");
                }
                else if (debugMode && Time.frameCount % 300 == 0)
                {
                    Debug.LogWarning("[MediaPipePoseEstimator] Webcam texture is null, waiting for camera...");
                }

                // Periodic health check
                if (Time.frameCount % 600 == 0) // Every 20 seconds
                {
                    Debug.Log($"[MediaPipePoseEstimator] 🔍 HEALTH CHECK: webcamTexture={(webcamTexture != null)}, frameTexture={(frameTexture != null)}, isProcessingFrame={isProcessingFrame}, isPoseProcessingLoopRunning={isPoseProcessingLoopRunning}");
                }

                // Small yield to prevent tight loop if webcam is null
                yield return null;
            }
        }

        private void UpdateFrameTexture()
        {
            if (frameTexture == null || webcamTexture == null)
            {
                return;
            }

            // Only copy if we haven't copied this frame (avoid redundant copies)
            if (lastCopiedFrame != Time.frameCount)
            {
                // Use Graphics.CopyTexture for GPU-to-GPU copy (much faster than CPU roundtrip)
                Graphics.CopyTexture(webcamTexture, frameTexture);
                lastCopiedFrame = Time.frameCount;

                if (debugMode && Time.frameCount % 60 == 0)
                    Debug.Log("[MediaPipePoseEstimator] Updated frame texture with Graphics.CopyTexture");
            }

            // Sample pixel for debugging (less frequent)
            if (debugMode && Time.frameCount % 120 == 0) // Every 4 seconds at 30fps
            {
                Color samplePixel = webcamTexture.GetPixel(10, 10); // Sample a corner pixel
                Debug.Log($"[MediaPipePoseEstimator] Webcam texture sample (10,10): R={samplePixel.r:F3}, G={samplePixel.g:F3}, B={samplePixel.b:F3}");
            }
        }

        private IEnumerator RunInference()
        {
            if (debugMode)
            {
                Debug.Log($"[MediaPipePoseEstimator] 🔍 RunInference: tfliteRunner={(tfliteRunner != null)}, frameTexture={(frameTexture != null)}");
            }

            if (tfliteRunner == null || frameTexture == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[MediaPipePoseEstimator] ❌ RunInference early exit: tfliteRunner null={tfliteRunner == null}, frameTexture null={frameTexture == null}");
                }
                yield break;
            }

            TFLiteModelRunner.PoseDetectionResult detection = null;
            if (debugMode)
            {
                Debug.Log($"[MediaPipePoseEstimator] 🚀 Starting pose detection on {frameTexture.width}x{frameTexture.height} frame...");
            }
            yield return StartCoroutine(tfliteRunner.RunPoseDetection(frameTexture, result => detection = result));

            if (debugMode)
            {
                Debug.Log($"[MediaPipePoseEstimator] 📊 Pose detection result: detection={(detection != null)}, success={detection?.success}, confidence={detection?.confidence:F3}");
            }

            if (detection == null || !detection.success)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[MediaPipePoseEstimator] ❌ Pose detection failed - no landmark processing. Result: {detection}");
                }
                yield break;
            }

            if (debugMode)
            {
                Debug.Log($"[MediaPipePoseEstimator] Pose detection succeeded - isMediaPipeResult={detection.isMediaPipeResult}, confidence={detection.confidence:F3}");
            }

            // Only run landmark detection if we have a real MediaPipe result
            // Computer vision fallback results can't be used with MediaPipe landmark detector
            if (!detection.isMediaPipeResult)
            {
                if (debugMode)
                {
                    Debug.Log("[MediaPipePoseEstimator] Computer vision detection only (isMediaPipeResult=false) - skipping MediaPipe landmarks.");
                }
                yield break;
            }

            if (debugMode)
            {
                Debug.Log($"[MediaPipePoseEstimator] Using MediaPipe detection result (isMediaPipeResult=true) - proceeding to landmarks.");
            }

            // IMPORTANT: Pass the FULL FRAME to RunPoseLandmark, not a cropped version!
            // PoseLandmarkDetect uses the detection result (poseLandmarker.Pose) to calculate
            // its own crop matrix internally. Manual cropping causes double-cropping and wrong results.
            TFLiteModelRunner.PoseLandmarkResult landmarkResult = null;
            yield return StartCoroutine(tfliteRunner.RunPoseLandmark(frameTexture, result => landmarkResult = result));

            if (landmarkResult != null && landmarkResult.success)
            {
                // Store previous pose for comparison
                Vector3[] previousPose = latestLandmarks != null ? (Vector3[])latestLandmarks.Clone() : null;
                float previousUpdateTime = lastUpdateTime;

                latestLandmarks = landmarkResult.landmarks;
                latestConfidence = landmarkResult.overallConfidence;
                lastUpdateTime = Time.time;

                if (debugMode)
                {
                    // Calculate actual frame rate
                    float timeSinceLastUpdate = lastUpdateTime - previousUpdateTime;
                    float actualFPS = timeSinceLastUpdate > 0 ? 1f / timeSinceLastUpdate : 0f;

                    // Check if pose actually changed
                    bool poseChanged = previousPose == null || previousPose.Length != latestLandmarks.Length;
                    if (!poseChanged && previousPose != null)
                    {
                        for (int i = 0; i < Mathf.Min(previousPose.Length, latestLandmarks.Length); i++)
                        {
                            if (Vector3.Distance(previousPose[i], latestLandmarks[i]) > 0.001f)
                            {
                                poseChanged = true;
                                break;
                            }
                        }
                    }

                    Debug.Log($"[MediaPipePoseEstimator] ✅ Pose UPDATED at {Time.time:F2}s - Confidence={latestConfidence:F3}, Nose={latestLandmarks[0]}, Changed={poseChanged}, FPS={actualFPS:F1}");
                }
            }
            else
            {
                if (debugMode)
                {
                    string reason = landmarkResult == null ? "null result" :
                                  !landmarkResult.success ? $"failed: {landmarkResult.errorMessage}" :
                                  "unknown";
                    Debug.LogWarning($"[MediaPipePoseEstimator] ❌ Pose NOT updated - Reason: {reason}");
                }
            }
        }

        private static Texture2D CropTextureToBoundingBox(Texture2D sourceTexture, Rect normalizedBBox)
        {
            if (sourceTexture == null)
            {
                return null;
            }

            int startX = Mathf.Clamp(Mathf.FloorToInt(normalizedBBox.x * sourceTexture.width), 0, sourceTexture.width - 1);
            int startY = Mathf.Clamp(Mathf.FloorToInt(normalizedBBox.y * sourceTexture.height), 0, sourceTexture.height - 1);
            int cropWidth = Mathf.Clamp(Mathf.FloorToInt(normalizedBBox.width * sourceTexture.width), 8, sourceTexture.width - startX);
            int cropHeight = Mathf.Clamp(Mathf.FloorToInt(normalizedBBox.height * sourceTexture.height), 8, sourceTexture.height - startY);

            try
            {
                Color[] pixels = sourceTexture.GetPixels(startX, startY, cropWidth, cropHeight);
                Texture2D cropped = new Texture2D(cropWidth, cropHeight, TextureFormat.RGB24, false);
                cropped.SetPixels(pixels);
                cropped.Apply();
                return cropped;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[MediaPipePoseEstimator] Crop failed: {e.Message}");
                return null;
            }
        }
    }
}
