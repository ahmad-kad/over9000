using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ScouterXR.AR
{
    public class ArFeatureManager : MonoBehaviour
    {
        [Header("AR Managers")]
        public ARHumanBodyManager humanBodyManager;     // For Human Stencil (Vision Pro, iOS)
        public AROcclusionManager occlusionManager;     // For Depth Fallback (Quest 3)

        [Header("Materials")]
        public Material haloMaterial;                    // Material for halo/UI rendering

        [Header("Debug")]
        public bool debugMode = true;

        void Start()
        {
            DetectAndConfigureFeatures();
        }

        private void DetectAndConfigureFeatures()
        {
            // Simplified: Always use AI depth estimation
            // This is reliable across all platforms and doesn't depend on hardware

            if (debugMode) Debug.Log("Using AI depth estimation for halo effects.");

            ConfigureAiDepthMode();
        }

        private void ConfigureAiDepthMode()
        {
            // Disable hardware AR components (not needed for AI depth)
            if (humanBodyManager != null) humanBodyManager.enabled = false;
            if (occlusionManager != null) occlusionManager.enabled = false;

            // Configure shader for AI depth mode
            if (haloMaterial != null)
            {
                haloMaterial.EnableKeyword("_AI_DEPTH");
                // Disable other modes
                haloMaterial.DisableKeyword("_STEREO_DEPTH");
                haloMaterial.DisableKeyword("_HUMAN_STENCIL");
                haloMaterial.DisableKeyword("_ENV_DEPTH");
                haloMaterial.DisableKeyword("_NO_DEPTH");
            }
        }

        // Public method to check current occlusion mode
        public OcclusionMode GetCurrentOcclusionMode()
        {
            // Simplified: Always AI depth estimation
            return OcclusionMode.AiDepthEstimation;
        }

        // Public method to force reconfiguration (useful for testing)
        public void ReconfigureFeatures()
        {
            DetectAndConfigureFeatures();
        }

    public enum OcclusionMode
    {
        AiDepthEstimation,  // AI pose-based depth estimation (primary)
        None                // No depth support
    }
    }
}
