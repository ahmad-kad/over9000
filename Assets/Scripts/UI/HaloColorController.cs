using UnityEngine;
using ScouterXR.AI;

namespace ScouterXR.UI
{
    public class HaloColorController : MonoBehaviour
    {
        [Header("Shader Settings")]
        public Material haloMaterial;

        [Header("AI Depth Fallback")]
        public MediaPipePoseEstimator poseEstimator;

        private float currentPowerForShader = 1000f;
        private float targetPower = 1000f;
        private float powerVelocity = 0.0f;

        // Shader property IDs for performance
        private int powerPropertyID;
        private int colorPropertyID;
        private int aiDepthPropertyID;

        void Start()
        {
            if (haloMaterial != null)
            {
                powerPropertyID = Shader.PropertyToID("_PowerLevel");
                colorPropertyID = Shader.PropertyToID("_HaloColor");
                aiDepthPropertyID = Shader.PropertyToID("_AiEstimatedDepth");
            }
        }

        public void Update()
        {
            if (haloMaterial == null) return;

            // Smooth the value that the shader will receive
            currentPowerForShader = Mathf.SmoothDamp(
                currentPowerForShader,
                targetPower,
                ref powerVelocity,
                0.25f
            );

            // Send the smoothed power value to the shader
            haloMaterial.SetFloat(powerPropertyID, currentPowerForShader);

            // Send AI estimated depth for fallback mode
            if (poseEstimator != null)
            {
                float estimatedDepth = poseEstimator.GetCurrentEstimatedDepth();
                haloMaterial.SetFloat(aiDepthPropertyID, estimatedDepth);
            }

            // Update color based on power level (for non-shader fallback)
            Color haloColor = GetHaloColorForPower(currentPowerForShader);
            haloMaterial.SetColor(colorPropertyID, haloColor);
        }

        // Public method for AI backend to call
        public void SetPowerLevel(float newPowerLevel)
        {
            targetPower = Mathf.Clamp(newPowerLevel, 1000f, 10000f);
        }

        // Color mapping based on power level
        private Color GetHaloColorForPower(float power)
        {
            if (power >= 9000f)
            {
                return Color.red; // Over 9000 - red
            }
            else if (power >= 7000f)
            {
                return Color.yellow; // High power - yellow
            }
            else if (power >= 5000f)
            {
                return new Color(1f, 0.5f, 0f); // Orange
            }
            else if (power >= 3000f)
            {
                return Color.green; // Medium power - green
            }
            else
            {
                return Color.blue; // Low power - blue
            }
        }
    }
}
