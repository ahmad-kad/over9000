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
            // Load binary model files from Resources/Models/
            LoadModelData();

            // Validate models are loaded
            ValidateModelLoading();

            SystemLogger.LogInfo("MediaPipeModelManager", "Models initialized successfully");
        }

        private void LoadModelData()
        {
            try
            {
                // Load pose detection model
                var poseDetectionTextAsset = Resources.Load<TextAsset>("Models/pose_detection");
                if (poseDetectionTextAsset != null)
                {
                    poseDetectionModelData = poseDetectionTextAsset.bytes;
                }

                // Load pose landmark models
                var poseLandmarkLiteTextAsset = Resources.Load<TextAsset>("Models/pose_landmark_lite");
                if (poseLandmarkLiteTextAsset != null)
                {
                    poseLandmarkLiteModelData = poseLandmarkLiteTextAsset.bytes;
                }

                var poseLandmarkFullTextAsset = Resources.Load<TextAsset>("Models/pose_landmark_full");
                if (poseLandmarkFullTextAsset != null)
                {
                    poseLandmarkFullModelData = poseLandmarkFullTextAsset.bytes;
                }

                // Load hand models if enabled
                if (enableHandTracking)
                {
                    var handLandmarkLiteTextAsset = Resources.Load<TextAsset>("Models/hand_landmark_lite");
                    if (handLandmarkLiteTextAsset != null)
                    {
                        handLandmarkLiteModelData = handLandmarkLiteTextAsset.bytes;
                    }

                    var handLandmarkFullTextAsset = Resources.Load<TextAsset>("Models/hand_landmark_full");
                    if (handLandmarkFullTextAsset != null)
                    {
                        handLandmarkFullModelData = handLandmarkFullTextAsset.bytes;
                    }
                }
            }
            catch (System.Exception e)
            {
                SystemLogger.LogError("MediaPipeModelManager", $"Failed to load model data: {e.Message}");
            }
        }

        private void ValidateModelLoading()
        {
            bool allModelsValid = true;

            if (poseDetectionModelData == null || poseDetectionModelData.Length == 0)
            {
                SystemLogger.LogError("MediaPipeModelManager", "Pose detection model not found or empty");
                allModelsValid = false;
            }

            byte[] poseModelData = useFullModels ? poseLandmarkFullModelData : poseLandmarkLiteModelData;
            if (poseModelData == null || poseModelData.Length == 0)
            {
                SystemLogger.LogError("MediaPipeModelManager", $"Pose landmark model ({(useFullModels ? "full" : "lite")}) not found or empty");
                allModelsValid = false;
            }

            if (enableHandTracking)
            {
                byte[] handModelData = useFullModels ? handLandmarkFullModelData : handLandmarkLiteModelData;
                if (handModelData == null || handModelData.Length == 0)
                {
                    SystemLogger.LogError("MediaPipeModelManager", $"Hand landmark model ({(useFullModels ? "full" : "lite")}) not found or empty");
                    allModelsValid = false;
                }
            }

            if (allModelsValid)
            {
                SystemLogger.LogInfo("MediaPipeModelManager", $"All MediaPipe models loaded successfully (Mode: {(useFullModels ? "Full" : "Lite")}, Hands: {(enableHandTracking ? "Enabled" : "Disabled")})");
            }
            else
            {
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
                    instance = FindObjectOfType<MediaPipeModelManager>();
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
