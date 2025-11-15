using UnityEngine;
using System.IO;
using ScouterXR.Core;

namespace ScouterXR.AI
{
    public class MediaPipeModelManager : MonoBehaviour
    {
        [Header("Model Data (Runtime Loaded)")]
        // Models are loaded from Resources/Models/ at runtime
        private byte[] poseDetectionModelData;
    private byte[] poseLandmarkLiteModelData;
    private byte[] poseLandmarkFullModelData;
    private byte[] handLandmarkLiteModelData;
    private byte[] handLandmarkFullModelData;

        [Header("Model Settings")]
        public bool useFullModels = false;        // Use full models vs lite (better accuracy vs performance)
        public bool enableHandTracking = true;

        private string modelsPath;

        void Awake()
        {
            modelsPath = Path.Combine(Application.streamingAssetsPath, "Models");
            InitializeModels();
        }

        private void InitializeModels()
        {
            // Load binary model files from StreamingAssets/Models/
            LoadModelData();

            // Validate models are loaded
            ValidateModelLoading();

            SystemLogger.LogInfo("MediaPipeModelManager", "Models initialized successfully");
        }

        private void LoadModelData()
        {
            try
            {
                // Load binary model files from StreamingAssets/Models/ (more reliable for binary data)
                string modelsPath = Path.Combine(Application.streamingAssetsPath, "Models");

                // Load pose detection model
                string poseDetectionPath = Path.Combine(modelsPath, "pose_detection.tflite");
                if (File.Exists(poseDetectionPath))
                {
                    poseDetectionModelData = File.ReadAllBytes(poseDetectionPath);
                    Debug.Log($"Loaded pose detection model: {poseDetectionModelData.Length} bytes");
                }
                else
                {
                    Debug.LogWarning($"Pose detection model not found at: {poseDetectionPath}");
                }

                // Load pose landmark models
                string poseLandmarkLitePath = Path.Combine(modelsPath, "pose_landmark_lite.tflite");
                if (File.Exists(poseLandmarkLitePath))
                {
                    poseLandmarkLiteModelData = File.ReadAllBytes(poseLandmarkLitePath);
                    Debug.Log($"Loaded pose landmark lite model: {poseLandmarkLiteModelData.Length} bytes");
                }
                else
                {
                    Debug.LogWarning($"Pose landmark lite model not found at: {poseLandmarkLitePath}");
                }

                string poseLandmarkFullPath = Path.Combine(modelsPath, "pose_landmark_full.tflite");
                if (File.Exists(poseLandmarkFullPath))
                {
                    poseLandmarkFullModelData = File.ReadAllBytes(poseLandmarkFullPath);
                    Debug.Log($"Loaded pose landmark full model: {poseLandmarkFullModelData.Length} bytes");
                }
                else
                {
                    Debug.LogWarning($"Pose landmark full model not found at: {poseLandmarkFullPath}");
                }

                // Load hand models if enabled
                if (enableHandTracking)
                {
                    string handLandmarkLitePath = Path.Combine(modelsPath, "hand_landmark_lite.tflite");
                    if (File.Exists(handLandmarkLitePath))
                    {
                        handLandmarkLiteModelData = File.ReadAllBytes(handLandmarkLitePath);
                        Debug.Log($"Loaded hand landmark lite model: {handLandmarkLiteModelData.Length} bytes");
                    }
                    else
                    {
                        Debug.LogWarning($"Hand landmark lite model not found at: {handLandmarkLitePath}");
                    }

                    string handLandmarkFullPath = Path.Combine(modelsPath, "hand_landmark_full.tflite");
                    if (File.Exists(handLandmarkFullPath))
                    {
                        handLandmarkFullModelData = File.ReadAllBytes(handLandmarkFullPath);
                        Debug.Log($"Loaded hand landmark full model: {handLandmarkFullModelData.Length} bytes");
                    }
                    else
                    {
                        Debug.LogWarning($"Hand landmark full model not found at: {handLandmarkFullPath}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load model data: {e.Message}");
                SystemLogger.LogError("MediaPipeModelManager", $"Failed to load model data: {e.Message}");
            }
        }

        private void ValidateModelLoading()
        {
            bool allModelsValid = true;

            Debug.Log($"MediaPipeModelManager: Validating models - poseDetection: {poseDetectionModelData?.Length ?? 0} bytes");

            if (poseDetectionModelData == null || poseDetectionModelData.Length == 0)
            {
                Debug.LogError("MediaPipeModelManager: Pose detection model not found or empty");
                SystemLogger.LogError("MediaPipeModelManager", "Pose detection model not found or empty");
                allModelsValid = false;
            }

            byte[] poseModelData = useFullModels ? poseLandmarkFullModelData : poseLandmarkLiteModelData;
            Debug.Log($"MediaPipeModelManager: Pose landmark model ({(useFullModels ? "full" : "lite")}): {poseModelData?.Length ?? 0} bytes");

            if (poseModelData == null || poseModelData.Length == 0)
            {
                Debug.LogError($"MediaPipeModelManager: Pose landmark model ({(useFullModels ? "full" : "lite")}) not found or empty");
                SystemLogger.LogError("MediaPipeModelManager", $"Pose landmark model ({(useFullModels ? "full" : "lite")}) not found or empty");
                allModelsValid = false;
            }

            if (enableHandTracking)
            {
                byte[] handModelData = useFullModels ? handLandmarkFullModelData : handLandmarkLiteModelData;
                Debug.Log($"MediaPipeModelManager: Hand landmark model ({(useFullModels ? "full" : "lite")}): {handModelData?.Length ?? 0} bytes");

                if (handModelData == null || handModelData.Length == 0)
                {
                    Debug.LogError($"MediaPipeModelManager: Hand landmark model ({(useFullModels ? "full" : "lite")}) not found or empty");
                    SystemLogger.LogError("MediaPipeModelManager", $"Hand landmark model ({(useFullModels ? "full" : "lite")}) not found or empty");
                    allModelsValid = false;
                }
            }

            if (allModelsValid)
            {
                Debug.Log($"MediaPipeModelManager: All MediaPipe models loaded successfully (Mode: {(useFullModels ? "Full" : "Lite")}, Hands: {(enableHandTracking ? "Enabled" : "Disabled")})");
                SystemLogger.LogInfo("MediaPipeModelManager", $"All MediaPipe models loaded successfully (Mode: {(useFullModels ? "Full" : "Lite")}, Hands: {(enableHandTracking ? "Enabled" : "Disabled")})");
            }
            else
            {
                Debug.LogWarning("MediaPipeModelManager: Some models failed to load - falling back to mock data");
                SystemLogger.LogWarning("MediaPipeModelManager", "Some models failed to load - falling back to mock data");
            }
        }

        // Public accessors for model data
        public byte[] GetPoseDetectionModelData()
        {
            return poseDetectionModelData;
        }

        public byte[] GetPoseLandmarkModelData()
        {
            return useFullModels ? poseLandmarkFullModelData : poseLandmarkLiteModelData;
        }

        public byte[] GetHandLandmarkModelData()
        {
            if (!enableHandTracking) return null;
            return useFullModels ? handLandmarkFullModelData : handLandmarkLiteModelData;
        }

        // Model info for debugging
        public string GetModelInfo()
        {
            return $"Pose Detection: {(poseDetectionModelData != null ? poseDetectionModelData.Length + " bytes" : "Not loaded")}\n" +
                   $"Pose Landmark: {(GetPoseLandmarkModelData() != null ? GetPoseLandmarkModelData().Length + " bytes" : "Not loaded")}\n" +
                   $"Hand Landmark: {(GetHandLandmarkModelData() != null ? GetHandLandmarkModelData().Length + " bytes" : "Not loaded")}\n" +
                   $"Mode: {(useFullModels ? "Full" : "Lite")}, Hands: {(enableHandTracking ? "Enabled" : "Disabled")}";
        }

        // Performance mode switching
        public void SetPerformanceMode(bool highPerformance)
        {
            useFullModels = highPerformance;
            SystemLogger.LogInfo("MediaPipeModelManager", $"Switched to {(useFullModels ? "high performance" : "low performance")} mode");

            // Re-validate models
            ValidateModelLoading();
        }

        public void SetHandTracking(bool enabled)
        {
            enableHandTracking = enabled;
            SystemLogger.LogInfo("MediaPipeModelManager", $"Hand tracking {(enabled ? "enabled" : "disabled")}");

            // Re-validate models
            ValidateModelLoading();
        }

        // Singleton pattern for easy access
        private static MediaPipeModelManager instance;
        public static MediaPipeModelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<MediaPipeModelManager>();
                    Debug.Log($"MediaPipeModelManager.Instance: Found instance: {instance != null}");
                }
                return instance;
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
