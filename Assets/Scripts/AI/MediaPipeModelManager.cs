using System.IO;
using UnityEngine;

namespace ScouterXR.AI
{
    /// <summary>
    /// Loads MediaPipe TensorFlow Lite models from StreamingAssets/Models.
    /// Provides lightweight accessors for pose/hand model byte arrays.
    /// </summary>
    public class MediaPipeModelManager : MonoBehaviour
    {
        private const string ModelsFolderName = "Models";
        private const string PoseDetectionFile = "pose_detection.tflite";
        private const string PoseLandmarkLiteFile = "pose_landmark_lite.tflite";
        private const string PoseLandmarkFullFile = "pose_landmark_full.tflite";
        private const string HandLandmarkLiteFile = "hand_landmark_lite.tflite";
        private const string HandLandmarkFullFile = "hand_landmark_full.tflite";

        [Header("Model Options")]
        public bool useFullModels = false;
        public bool enableHandTracking = true;

        private byte[] poseDetectionModelData;
        private byte[] poseLandmarkLiteModelData;
        private byte[] poseLandmarkFullModelData;
        private byte[] handLandmarkLiteModelData;
        private byte[] handLandmarkFullModelData;

        private bool modelsLoaded = false;
        public bool ModelsLoaded => modelsLoaded;

        private void Awake()
        {
            LoadModelData();
        }

        private void LoadModelData()
        {
            Debug.Log("[MediaPipeModelManager] Loading models from: " + Application.streamingAssetsPath);
            string modelsPath = Path.Combine(Application.streamingAssetsPath, ModelsFolderName);

            // Try StreamingAssets first
            poseDetectionModelData = LoadBinary(Path.Combine(modelsPath, PoseDetectionFile));
            poseLandmarkLiteModelData = LoadBinary(Path.Combine(modelsPath, PoseLandmarkLiteFile));
            poseLandmarkFullModelData = LoadBinary(Path.Combine(modelsPath, PoseLandmarkFullFile));

            if (enableHandTracking)
            {
                handLandmarkLiteModelData = LoadBinary(Path.Combine(modelsPath, HandLandmarkLiteFile));
                handLandmarkFullModelData = LoadBinary(Path.Combine(modelsPath, HandLandmarkFullFile));
            }

            // Fallback: Try Resources folder if StreamingAssets failed
            if (poseDetectionModelData == null)
            {
                Debug.Log("[MediaPipeModelManager] Trying Resources/Models as fallback");
                poseDetectionModelData = LoadFromResources("Models/" + PoseDetectionFile.Replace(".tflite", ""));
                poseLandmarkLiteModelData = LoadFromResources("Models/" + PoseLandmarkLiteFile.Replace(".tflite", ""));
                poseLandmarkFullModelData = LoadFromResources("Models/" + PoseLandmarkFullFile.Replace(".tflite", ""));

                if (enableHandTracking)
                {
                    handLandmarkLiteModelData = LoadFromResources("Models/" + HandLandmarkLiteFile.Replace(".tflite", ""));
                    handLandmarkFullModelData = LoadFromResources("Models/" + HandLandmarkFullFile.Replace(".tflite", ""));
                }
            }

            // Log loading status
            Debug.Log($"[MediaPipeModelManager] Pose Detection: {(poseDetectionModelData != null ? poseDetectionModelData.Length : 0)} bytes");
            Debug.Log($"[MediaPipeModelManager] Pose Landmark Lite: {(poseLandmarkLiteModelData != null ? poseLandmarkLiteModelData.Length : 0)} bytes");
            Debug.Log($"[MediaPipeModelManager] Pose Landmark Full: {(poseLandmarkFullModelData != null ? poseLandmarkFullModelData.Length : 0)} bytes");

            if (poseDetectionModelData == null)
            {
                Debug.LogError("[MediaPipeModelManager] Pose detection model missing from " + Path.Combine(modelsPath, PoseDetectionFile));
            }

            if (useFullModels && poseLandmarkFullModelData == null)
            {
                Debug.LogWarning("[MediaPipeModelManager] Full pose landmark model missing, falling back to lite");
                useFullModels = false;
            }

            if (enableHandTracking && useFullModels && handLandmarkFullModelData == null)
            {
                Debug.LogWarning("[MediaPipeModelManager] Full hand landmark model missing, falling back to lite");
            }

            // CRITICAL: Only set modelsLoaded = true if REQUIRED models actually loaded
            bool hasPoseDetection = poseDetectionModelData != null && poseDetectionModelData.Length > 0;
            bool hasPoseLandmark = (poseLandmarkLiteModelData != null && poseLandmarkLiteModelData.Length > 0) ||
                                   (poseLandmarkFullModelData != null && poseLandmarkFullModelData.Length > 0);

            if (hasPoseDetection && hasPoseLandmark)
            {
                modelsLoaded = true;
                Debug.Log("[MediaPipeModelManager] Model loading complete - all required models loaded");
            }
            else
            {
                modelsLoaded = false;
                Debug.LogError("[MediaPipeModelManager] Model loading FAILED - required models missing!");
                Debug.LogError($"  - Pose Detection: {(hasPoseDetection ? poseDetectionModelData.Length + " bytes" : "NULL")}");
                Debug.LogError($"  - Pose Landmark: {(hasPoseLandmark ? "OK" : "NULL")}");
            }
        }

        private static byte[] LoadBinary(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                return null;
            }

            if (!File.Exists(absolutePath))
            {
                Debug.LogWarning($"[MediaPipeModelManager] File not found: {absolutePath}");
                return null;
            }

            try
            {
                return File.ReadAllBytes(absolutePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MediaPipeModelManager] Failed to load {absolutePath}: {e.Message}");
                return null;
            }
        }

        private static byte[] LoadFromResources(string resourcePath)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset != null)
            {
                Debug.Log($"[MediaPipeModelManager] Loaded {resourcePath} from Resources: {textAsset.bytes.Length} bytes");
                return textAsset.bytes;
            }
            else
            {
                Debug.LogWarning($"[MediaPipeModelManager] Resource not found: {resourcePath}");
                return null;
            }
        }

        public byte[] GetPoseDetectionModelData()
        {
            return poseDetectionModelData;
        }

        public byte[] GetPoseLandmarkModelData()
        {
            return useFullModels && poseLandmarkFullModelData != null
                ? poseLandmarkFullModelData
                : poseLandmarkLiteModelData;
        }

        public byte[] GetHandLandmarkModelData(bool requestFullModel = false)
        {
            if (!enableHandTracking)
            {
                return null;
            }

            if (requestFullModel && handLandmarkFullModelData != null)
            {
                return handLandmarkFullModelData;
            }

            return handLandmarkLiteModelData;
        }
    }
}

