using System.Collections;
using TensorFlowLite;
using UnityEngine;
using UnityEngine.UI;

namespace PoseDemo
{
    /// <summary>
    /// Minimal MediaPipe pose tracking experience that auto-configures the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoseCameraController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Vector2Int cameraResolution = new(1280, 720);
        [SerializeField] private int cameraFps = 30;
        [SerializeField] private bool preferFrontCamera = false;

        [Header("Models")]
        [SerializeField] private string poseDetectionModel = "Models/pose_detection.tflite";
        [SerializeField] private string poseLandmarkModel = "Models/pose_landmark_lite.tflite";
        [SerializeField, Range(0f, 1f)] private float detectionScoreThreshold = 0.55f;

        [Header("UI")]
        [SerializeField] private Vector2 referenceResolution = new(1920, 1080);
        [SerializeField] private Color backgroundColor = Color.black;

        private WebCamTexture webCamTexture;
        private PoseDetect poseDetect;
        private PoseLandmarkDetect poseLandmark;
        private Texture latestTexture;
        private bool isProcessingFrame;

        private GameObject uiRoot;
        private RawImage previewImage;
        private PoseOverlayUI overlay;

        private static readonly WaitForEndOfFrame endOfFrame = new();

// DISABLED: Auto-creation conflicts with UnifiedSceneSetup
// To re-enable, define POSE_DEMO_ENABLE_AUTOCREATE in Player Settings → Scripting Define Symbols
#if POSE_DEMO_ENABLE_AUTOCREATE
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (FindFirstObjectByType<PoseCameraController>() == null)
            {
                new GameObject(nameof(PoseCameraController)).AddComponent<PoseCameraController>();
            }
        }
#endif

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.name = nameof(PoseCameraController);

            BuildUi();
            InitializeCamera();
            InitializeModels();
        }

        private void OnDestroy()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                Destroy(webCamTexture);
            }

            poseDetect?.Dispose();
            poseLandmark?.Dispose();

            if (uiRoot != null)
            {
                Destroy(uiRoot);
            }
        }

        private void BuildUi()
        {
            uiRoot = new GameObject("Pose Demo UI");
            DontDestroyOnLoad(uiRoot);

            var canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;

            uiRoot.AddComponent<GraphicRaycaster>();

            previewImage = new GameObject("Camera Preview").AddComponent<RawImage>();
            previewImage.transform.SetParent(uiRoot.transform, false);
            previewImage.color = backgroundColor;

            RectTransform previewRect = previewImage.rectTransform;
            previewRect.anchorMin = Vector2.zero;
            previewRect.anchorMax = Vector2.one;
            previewRect.offsetMin = Vector2.zero;
            previewRect.offsetMax = Vector2.zero;

            overlay = new GameObject("Pose Overlay").AddComponent<PoseOverlayUI>();
            overlay.transform.SetParent(uiRoot.transform, false);
            RectTransform overlayRect = overlay.RectTransform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlay.Initialize(PoseLandmarkDetect.LandmarkCount);
        }

        private void InitializeCamera()
        {
            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogError("[PoseCameraController] No webcam devices found!");
                return;
            }

            string deviceName = WebCamTexture.devices[0].name;
            
            // Find front-facing camera if preferred
            if (preferFrontCamera)
            {
                foreach (var device in WebCamTexture.devices)
                {
                    if (device.isFrontFacing)
                    {
                        deviceName = device.name;
                        break;
                    }
                }
            }

            webCamTexture = new WebCamTexture(deviceName, cameraResolution.x, cameraResolution.y, cameraFps);
            webCamTexture.Play();
            
            StartCoroutine(WaitForCameraAndStart());
        }

        private IEnumerator WaitForCameraAndStart()
        {
            // Wait for webcam to initialize
            yield return new WaitUntil(() => webCamTexture.width > 16 && webCamTexture.height > 16);
            StartCoroutine(CameraUpdateLoop());
        }

        private IEnumerator CameraUpdateLoop()
        {
            while (webCamTexture != null && webCamTexture.isPlaying)
            {
                OnCameraTextureUpdated(webCamTexture);
                yield return null;
            }
        }

        private void InitializeModels()
        {
            poseDetect = new PoseDetect(new PoseDetect.Options
            {
                modelPath = poseDetectionModel,
                aspectMode = AspectMode.Fill,
                scoreThreshold = detectionScoreThreshold,
                useNonMaxSuppression = true,
                iouThreshold = 0.3f,
            });

            poseLandmark = new PoseLandmarkDetect(new PoseLandmarkDetect.Options
            {
                modelPath = poseLandmarkModel,
                useFilter = true,
                useWorldLandmarks = false,
            });

            poseLandmark.AspectMode = AspectMode.Fill;
        }

        private void OnCameraTextureUpdated(Texture texture)
        {
            latestTexture = texture;
            if (previewImage != null)
            {
                previewImage.texture = texture;
            }

            if (!isProcessingFrame && isActiveAndEnabled)
            {
                StartCoroutine(RunPosePipeline());
            }
        }

        private IEnumerator RunPosePipeline()
        {
            if (latestTexture == null)
            {
                yield break;
            }

            isProcessingFrame = true;
            yield return endOfFrame;

            poseDetect.Run(latestTexture);
            PoseDetect.Result detection = poseDetect.GetResults();

            if (detection.score < detectionScoreThreshold || detection.rect.width <= 0f)
            {
                overlay.HideAll();
                isProcessingFrame = false;
                yield break;
            }

            poseLandmark.Pose = detection;
            poseLandmark.Run(latestTexture);
            PoseLandmarkDetect.Result result = poseLandmark.GetResult();
            overlay.Render(result.viewportLandmarks);

            isProcessingFrame = false;
        }
    }
}

