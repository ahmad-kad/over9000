using System.Collections;
using UnityEngine;

namespace ScouterXR.AI
{
    /// <summary>
    /// Captures webcam frames, runs the MediaPipe hand landmark model, and exposes the latest hand pose.
    /// </summary>
    [RequireComponent(typeof(TensorFlowLiteInference))]
    public sealed class HandPointingRecognizer : MonoBehaviour
    {
        [Header("Hand Tracking")]
        public bool enableHandTracking = true;
        [Tooltip("Time between hand inferences in seconds. Lower = more responsive but higher CPU usage.")]
        public float inferenceInterval = 0.05f; // ~20 FPS hand tracking (faster than pose)
        public bool useSharedPoseWebcam = true;
        public MediaPipePoseEstimator poseEstimator;
        public bool debugMode = false;

        private TensorFlowLiteInference inferenceEngine;
        private WebCamTexture ownWebcamTexture;
        private Texture2D handFrameTexture;

        private Vector3[] latestHandLandmarks = new Vector3[21];
        private float lastHandUpdateTime;

        public Vector3[] LatestHandLandmarks => latestHandLandmarks;
        public float LastHandUpdateTime => lastHandUpdateTime;

        private void Awake()
        {
            inferenceEngine = GetComponent<TensorFlowLiteInference>();
        }

        private void OnEnable()
        {
            StartCoroutine(HandTrackingLoop());
        }

        private void OnDisable()
        {
            if (ownWebcamTexture != null)
            {
                ownWebcamTexture.Stop();
            }

            if (handFrameTexture != null)
            {
                Destroy(handFrameTexture);
                handFrameTexture = null;
            }
        }

        private IEnumerator HandTrackingLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.05f, inferenceInterval));

            while (enabled)
            {
                if (!enableHandTracking || inferenceEngine == null)
                {
                    yield return wait;
                    continue;
                }

                Texture2D frame = AcquireFrameTexture();
                if (frame != null)
                {
                    TensorFlowLiteInference.InferenceResult result = null;
                    yield return StartCoroutine(inferenceEngine.RunHandInference(frame, r => result = r));

                    if (result != null && result.success && result.landmarks != null)
                    {
                        latestHandLandmarks = result.landmarks;
                        lastHandUpdateTime = Time.time;

                        if (debugMode)
                        {
                            Debug.Log($"[HandPointingRecognizer] Hand landmarks updated (confidence {result.confidence:F3})");
                        }
                    }
                }

                yield return wait;
            }
        }

        private Texture2D AcquireFrameTexture()
        {
            WebCamTexture sourceTexture = null;

            if (useSharedPoseWebcam && poseEstimator != null && poseEstimator.SharedWebcamTexture != null)
            {
                sourceTexture = poseEstimator.SharedWebcamTexture;
            }
            else
            {
                if (ownWebcamTexture == null)
                {
                    if (WebCamTexture.devices.Length == 0)
                    {
                        if (debugMode) Debug.LogWarning("[HandPointingRecognizer] No webcam devices found.");
                        return null;
                    }
                    ownWebcamTexture = new WebCamTexture(WebCamTexture.devices[0].name);
                    ownWebcamTexture.Play();
                }

                sourceTexture = ownWebcamTexture;
            }

            if (sourceTexture == null || !sourceTexture.isPlaying || sourceTexture.width < 16)
            {
                return null;
            }

            if (handFrameTexture == null || handFrameTexture.width != sourceTexture.width || handFrameTexture.height != sourceTexture.height)
            {
                handFrameTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGB24, false);
            }

            handFrameTexture.SetPixels32(sourceTexture.GetPixels32());
            handFrameTexture.Apply();

            return handFrameTexture;
        }
    }
}
