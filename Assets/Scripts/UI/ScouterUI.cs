using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ScouterXR.Core;

namespace ScouterXR.UI
{
    public class ScouterUI : MonoBehaviour
    {
        [Header("UI Components")]
        public CanvasGroup scouterReadout;
        public RectTransform powerLevelText;
        public Transform targetLockOnUI;

        [Header("Audio Feedback")]
        public SpatialAudioController audioController;
        public AudioClip powerLevelChangeClip;
        public AudioClip targetLockedClip;
        public AudioClip scanningBeepClip;

        // --- Private values for smoothing ---
        private float currentPowerLevel = 1000f;
        private float targetPowerLevel = 1000f;
        private float powerVelocity = 0.0f;

        private Vector3 targetHaloPosition;
        private Vector3 haloVelocity = Vector3.zero;


        void Update()
        {
            // 1. SMOOTH HALO POSITION
            if (targetLockOnUI != null)
            {
                targetLockOnUI.position = Vector3.SmoothDamp(
                    targetLockOnUI.position,
                    targetHaloPosition,
                    ref haloVelocity,
                    0.15f
                );
            }

            // 2. SMOOTH POWER LEVEL VALUE
            currentPowerLevel = Mathf.SmoothDamp(
                currentPowerLevel,
                targetPowerLevel,
                ref powerVelocity,
                0.2f
            );

            // Update UI text if available
            if (powerLevelText != null)
            {
                // Safer component lookup with fallback
                var tmpText = powerLevelText.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = $"Power Level: {currentPowerLevel:F0}";
                }
                else
                {
                    // Fallback to Unity UI Text
                    var uiText = powerLevelText.GetComponentInChildren<UnityEngine.UI.Text>();
                    if (uiText != null)
                    {
                        uiText.text = $"Power Level: {currentPowerLevel:F0}";
                    }
                }
            }
        }

        // Call this to show the UI
        public void ShowScouter()
        {
            if (scouterReadout != null)
            {
                scouterReadout.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
            }

            if (powerLevelText != null)
            {
                powerLevelText.DOAnchorPosX(0, 0.7f).SetEase(Ease.OutBack);
            }
        }

        // Call this to hide
        public void HideScouter()
        {
            if (scouterReadout != null)
            {
                scouterReadout.DOFade(0f, 0.3f).SetEase(Ease.InQuad);
            }
        }

        // Public method for AI backend to send data
        public void UpdateTarget(Vector3 screenPosition, float power)
        {
            targetHaloPosition = screenPosition;

            // Play audio feedback when power level changes significantly
            float powerDifference = Mathf.Abs(power - targetPowerLevel);
            if (powerDifference > 500f && targetPowerLevel > 1000f) // Only play for meaningful changes
            {
                PlayPowerLevelChangeAudio(power);
            }

            targetPowerLevel = power;
        }

        // Public method to set power level directly
        public void SetPowerLevel(float power)
        {
            // Play audio feedback when power level changes significantly
            float powerDifference = Mathf.Abs(power - targetPowerLevel);
            if (powerDifference > 500f && targetPowerLevel > 1000f)
            {
                PlayPowerLevelChangeAudio(power);
            }

            targetPowerLevel = power;
        }

        // Audio feedback methods
        private void PlayPowerLevelChangeAudio(float newPowerLevel)
        {
            if (audioController != null)
            {
                // Use the spatial audio controller for power level readings
                audioController.PlayPowerLevelReading();
            }
            else if (powerLevelChangeClip != null)
            {
                // Fallback to direct audio clip
                AudioSource.PlayClipAtPoint(powerLevelChangeClip, Camera.main.transform.position);
            }
        }

        public void PlayTargetLockedAudio()
        {
            if (audioController != null)
            {
                audioController.PlayScouterBeep();
            }
            else if (targetLockedClip != null)
            {
                AudioSource.PlayClipAtPoint(targetLockedClip, Camera.main.transform.position);
            }
        }

        public void PlayScanningBeepAudio()
        {
            if (audioController != null)
            {
                audioController.PlayScouterBeep();
            }
            else if (scanningBeepClip != null)
            {
                AudioSource.PlayClipAtPoint(scanningBeepClip, Camera.main.transform.position);
            }
        }

        // Auto-assign audio controller if not set
        void Start()
        {
            // Start hidden
            if (scouterReadout != null)
            {
                scouterReadout.alpha = 0f;
            }

            // Auto-assign audio controller if not set
            if (audioController == null)
            {
                audioController = FindObjectOfType<SpatialAudioController>();
            }
        }
    }
}
