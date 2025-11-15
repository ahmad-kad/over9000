using UnityEngine;

namespace ScouterXR.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class SpatialAudioController : MonoBehaviour
    {
    [Header("Core Audio Clips")]
    public AudioClip scouterActivationClip;    // SCOUTER - AUDIO FROM JAYUZUMI.COM.mp3
    public AudioClip scouterBeepClip;          // db-scouter-made-with-Voicemod.mp3
    public AudioClip powerLevelReadingClip;   // HIS POWER LEVEL IS 1200 - AUDIO FROM JAYUZUMI.COM.mp3
    public AudioClip overloadClip;             // 9000 - AUDIO FROM JAYUZUMI.COM.mp3
    public AudioClip overloadAltClip;          // over9000.swf.mp3
    public AudioClip battleMusicClip;          // dragonball_battle.mp3
    public AudioClip glassShatterClip;         // glass_shatter.mp3
    public AudioClip defeatScreamClip;         // NOOOOOO - AUDIO FROM JAYUZUMI.COM.mp3

    [Header("Scanning Audio")]
    public AudioClip[] scanningBeeps;          // Array for ramp-up beeping effect
    private int currentBeepIndex = 0;

        [Header("Spatial Settings")]
        public float minDistance = 1f;      // Distance where sound is at full volume
        public float maxDistance = 15f;    // Distance where sound fades to silence
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

        private AudioSource audioSource;
        private Transform targetTransform;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            ConfigureSpatialAudio();
        }

        void Start()
        {
            // Play initial scouter activation sound
            if (scouterActivationClip != null)
            {
                audioSource.clip = scouterActivationClip;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        private void ConfigureSpatialAudio()
        {
            // Essential spatial audio settings
            audioSource.spatialBlend = 1f;          // Full 3D spatialization
            audioSource.rolloffMode = rolloffMode;  // Distance-based volume falloff
            audioSource.minDistance = minDistance;  // Full volume range
            audioSource.maxDistance = maxDistance;  // Fade out distance

            // Additional spatial settings
            audioSource.dopplerLevel = 0.1f;       // Minimal doppler effect
            audioSource.spread = 0f;               // No spreading (focused sound)

            // Performance optimizations
            audioSource.priority = 128;            // Normal priority
            audioSource.volume = 0.7f;             // Reasonable default volume
        }

        // Public methods for controlling audio
        public void PlayScouterActivation()
        {
            if (scouterActivationClip != null)
            {
                audioSource.clip = scouterActivationClip;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        public void PlayScouterBeep()
        {
            if (scouterBeepClip != null)
            {
                audioSource.PlayOneShot(scouterBeepClip);
            }
        }

        public void PlayPowerLevelReading()
        {
            if (powerLevelReadingClip != null)
            {
                audioSource.PlayOneShot(powerLevelReadingClip);
            }
        }

        public void StartScanningBeeps()
        {
            if (scanningBeeps != null && scanningBeeps.Length > 0)
            {
                currentBeepIndex = 0;
                PlayNextScanningBeep();
            }
        }

        private void PlayNextScanningBeep()
        {
            if (scanningBeeps == null || scanningBeeps.Length == 0) return;

            if (currentBeepIndex < scanningBeeps.Length)
            {
                audioSource.PlayOneShot(scanningBeeps[currentBeepIndex]);
                currentBeepIndex++;

                // Schedule next beep (faster as we approach overload)
                float delay = Mathf.Lerp(0.5f, 0.1f, (float)currentBeepIndex / scanningBeeps.Length);
                Invoke("PlayNextScanningBeep", delay);
            }
        }

        public void PlayOverload()
        {
            if (overloadClip != null)
            {
                audioSource.Stop(); // Stop any current audio
                audioSource.PlayOneShot(overloadClip);

                // Also play glass shatter for extra effect
                if (glassShatterClip != null)
                {
                    Invoke("PlayGlassShatter", 0.5f); // Delay for dramatic effect
                }

            }
        }

        private void PlayGlassShatter()
        {
            if (glassShatterClip != null)
            {
                audioSource.PlayOneShot(glassShatterClip);
            }
        }

        public void PlayBattleMusic()
        {
            if (battleMusicClip != null)
            {
                audioSource.clip = battleMusicClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        public void PlayDefeatScream()
        {
            if (defeatScreamClip != null)
            {
                audioSource.PlayOneShot(defeatScreamClip);
            }
        }

        public void StopAudio()
        {
            audioSource.Stop();
        }

        // Method to update audio based on power level
        public void UpdatePowerLevel(float powerLevel)
        {
            // Clamp to valid range
            powerLevel = Mathf.Clamp(powerLevel, 1000f, 9500f);

            // Adjust pitch based on power level (subtle range to avoid artifacts)
            float normalizedPower = Mathf.InverseLerp(1000f, 9500f, powerLevel);
            audioSource.pitch = Mathf.Lerp(0.9f, 1.2f, normalizedPower);

            // Adjust volume based on power level (maintain base volume)
            float volumeMultiplier = Mathf.Lerp(0.5f, 1.0f, normalizedPower);
            audioSource.volume = volumeMultiplier; // Don't multiply by constant
        }

        // Method to attach to a moving target
        public void AttachToTarget(Transform target)
        {
            targetTransform = target;
            if (target != null)
            {
                transform.SetParent(target);
                transform.localPosition = Vector3.zero;
            }
        }

        // Method to detach from target
        public void DetachFromTarget()
        {
            if (targetTransform != null)
            {
                transform.SetParent(null);
                targetTransform = null;
            }
        }

        // Debug method to visualize audio range
        void OnDrawGizmosSelected()
        {
            if (audioSource != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, audioSource.minDistance);

                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, audioSource.maxDistance);
            }
        }
    }
}
