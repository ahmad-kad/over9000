using System;
using System.Collections;
using System.IO;
using UnityEngine;
using TensorFlowLite;

namespace ScouterXR.AI
{
    /// <summary>
    /// Lightweight runner for MediaPipe hand landmark TensorFlow Lite models.
    /// </summary>
    public sealed class TensorFlowLiteInference : MonoBehaviour
    {
        private const string ModelsFolderName = "Models";
        private const string HandLandmarkLiteFile = "hand_landmark_lite.tflite";
        private const string HandLandmarkFullFile = "hand_landmark_full.tflite";
        private const int HandLandmarkCount = 21;

        [Header("Model Settings")]
        public MediaPipeModelManager modelManager;
        public bool useFullHandModel = false;
        public bool enableGpuDelegate = true;
        public bool debugMode = false;

        public class InferenceResult
        {
            public bool success;
            public Vector3[] landmarks;
            public float confidence;
            public string errorMessage = "";
        }

        private Interpreter handInterpreter;
        private InterpreterOptions handOptions;
        private int inputWidth = 224;
        private int inputHeight = 224;
        private float[] inputBuffer;
        private float[] outputBuffer;
        private RelativeVelocityFilter3D[] handFilters;

        private void Awake()
        {
            InitializeHandFilters();
            InitializeInterpreter();
        }

        private void OnDestroy()
        {
            handInterpreter?.Dispose();
            handInterpreter = null;
            handOptions?.Dispose();
            handOptions = null;
        }

        private void InitializeHandFilters()
        {
            handFilters = new RelativeVelocityFilter3D[HandLandmarkCount];
            for (int i = 0; i < handFilters.Length; i++)
            {
                handFilters[i] = new RelativeVelocityFilter3D(
                    windowSize: 3,
                    velocityScale: 8f,
                    RelativeVelocityFilter.DistanceEstimationMode.ForceCurrentScale);
            }
        }

        private void InitializeInterpreter()
        {
            if (modelManager == null)
            {
                modelManager = FindFirstObjectByType<MediaPipeModelManager>();
            }

            byte[] modelData = modelManager != null
                ? modelManager.GetHandLandmarkModelData(useFullHandModel)
                : LoadHandModelFromStreamingAssets();

            if (modelData == null || modelData.Length == 0)
            {
                Debug.LogError("[TensorFlowLiteInference] Hand landmark model data missing");
                return;
            }

            handOptions = new InterpreterOptions();
            if (enableGpuDelegate)
            {
                try
                {
                    handOptions.AddGpuDelegate();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[TensorFlowLiteInference] GPU delegate unavailable, falling back to CPU: {e.Message}");
                }
            }

            handInterpreter = new Interpreter(modelData, handOptions);
            var inputInfo = handInterpreter.GetInputTensorInfo(0);
            inputHeight = inputInfo.shape[1];
            inputWidth = inputInfo.shape[2];

            handInterpreter.AllocateTensors();
            inputBuffer = new float[inputWidth * inputHeight * 3];

            var outputInfo = handInterpreter.GetOutputTensorInfo(0);
            int outputLength = 1;
            foreach (var dim in outputInfo.shape)
            {
                outputLength *= dim;
            }

            outputBuffer = new float[outputLength];

            if (debugMode)
            {
                Debug.Log($"[TensorFlowLiteInference] Hand interpreter ready (input {inputWidth}x{inputHeight}, output {outputLength})");
            }
        }

        private static byte[] LoadHandModelFromStreamingAssets()
        {
            string basePath = Path.Combine(Application.streamingAssetsPath, ModelsFolderName);
            string litePath = Path.Combine(basePath, HandLandmarkLiteFile);
            if (!File.Exists(litePath))
            {
                Debug.LogWarning($"[TensorFlowLiteInference] Could not find {litePath}");
                return null;
            }

            return File.ReadAllBytes(litePath);
        }

        public IEnumerator RunHandInference(Texture2D inputTexture, Action<InferenceResult> onComplete)
        {
            var result = new InferenceResult();

            if (inputTexture == null)
            {
                result.errorMessage = "Input texture is null";
                onComplete?.Invoke(result);
                yield break;
            }

            if (handInterpreter == null)
            {
                result.errorMessage = "Hand interpreter not initialized";
                onComplete?.Invoke(result);
                yield break;
            }

            Texture2D processed = null;
            Texture2D resized = null;

            try
            {
                processed = EnsureRGBTexture(inputTexture);
                resized = ResizeTexture(processed, inputWidth, inputHeight);

                Color[] pixels = resized.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    int offset = i * 3;
                    inputBuffer[offset] = pixels[i].r;
                    inputBuffer[offset + 1] = pixels[i].g;
                    inputBuffer[offset + 2] = pixels[i].b;
                }

                handInterpreter.SetInputTensorData(0, inputBuffer);
                handInterpreter.Invoke();
                handInterpreter.GetOutputTensorData(0, outputBuffer);

                ParseHandOutput(result, outputBuffer);
                result.success = true;
            }
            catch (Exception e)
            {
                result.success = false;
                result.errorMessage = $"Hand inference failed: {e.Message}";
                Debug.LogError($"[TensorFlowLiteInference] {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                if (processed != null && processed != inputTexture)
                {
                    Destroy(processed);
                }
                if (resized != null && resized != processed)
                {
                    Destroy(resized);
                }
            }

            onComplete?.Invoke(result);
            yield break;
        }

        private void ParseHandOutput(InferenceResult result, float[] output)
        {
            int valuesPerLandmark = Mathf.Max(1, output.Length / HandLandmarkCount);
            result.landmarks = new Vector3[HandLandmarkCount];
            result.confidence = 0.9f;

            for (int i = 0; i < HandLandmarkCount; i++)
            {
                int offset = i * valuesPerLandmark;
                float x = valuesPerLandmark > 0 ? output[offset] : 0f;
                float y = valuesPerLandmark > 1 ? output[offset + 1] : 0f;
                float z = valuesPerLandmark > 2 ? output[offset + 2] : 0f;

                Vector3 raw = new Vector3(Mathf.Clamp01(x), Mathf.Clamp01(y), z);
                Vector3 smoothed = handFilters[i].Apply(Time.time, 1f, raw);
                result.landmarks[i] = smoothed;
            }
        }

        private static Texture2D EnsureRGBTexture(Texture2D texture)
        {
            if (texture.format == TextureFormat.RGB24)
            {
                return texture;
            }

            Texture2D rgb = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            rgb.SetPixels(texture.GetPixels());
            rgb.Apply();
            return rgb;
        }

        private static Texture2D ResizeTexture(Texture2D inputTexture, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(inputTexture, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D resizedTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
            resizedTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            resizedTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return resizedTexture;
        }
    }
}
