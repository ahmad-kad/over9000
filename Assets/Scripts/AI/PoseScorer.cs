using UnityEngine;

namespace ScouterXR.AI
{
    /// <summary>
    /// Calculates a "power level" score based on pose quality and strength.
    /// Strong, confident poses = higher scores (like DBZ Scouter readings!)
    /// </summary>
    public class PoseScorer
    {
        // Landmark indices for key body points
        private const int LEFT_SHOULDER = 11;
        private const int RIGHT_SHOULDER = 12;
        private const int LEFT_ELBOW = 13;
        private const int RIGHT_ELBOW = 14;
        private const int LEFT_WRIST = 15;
        private const int RIGHT_WRIST = 16;
        private const int LEFT_HIP = 23;
        private const int RIGHT_HIP = 24;
        private const int LEFT_KNEE = 25;
        private const int RIGHT_KNEE = 26;
        private const int LEFT_ANKLE = 27;
        private const int RIGHT_ANKLE = 28;
        private const int NOSE = 0;

        /// <summary>
        /// Calculate pose quality score (0-9999+ like a Scouter reading)
        /// </summary>
        public static float CalculatePowerLevel(PoseData pose, float confidence)
        {
            if (!pose.IsValid || pose.landmarks.Length < 33)
            {
                return 0f;
            }

            float score = 0f;

            // Base score from detection confidence (0-1000)
            score += confidence * 1000f;

            // Visibility bonus - clear, well-lit poses score higher (0-500)
            float avgVisibility = CalculateAverageVisibility(pose);
            score += avgVisibility * 500f;

            // Pose symmetry - balanced poses score higher (0-1000)
            float symmetry = CalculatePoseSymmetry(pose);
            score += symmetry * 1000f;

            // Arm extension - arms spread wide = power pose (0-1500)
            float armExtension = CalculateArmExtension(pose);
            score += armExtension * 1500f;

            // Stance width - wide stance = strong pose (0-1000)
            float stanceWidth = CalculateStanceWidth(pose);
            score += stanceWidth * 1000f;

            // Height/posture - upright posture scores higher (0-800)
            float posture = CalculatePosture(pose);
            score += posture * 800f;

            // Action bonus - dynamic poses (punching, kicking) get multipliers
            float actionBonus = DetectActionPose(pose);
            score *= (1f + actionBonus);

            // Energy multiplier based on overall body engagement
            float energy = CalculateBodyEngagement(pose);
            score *= (0.5f + energy * 0.5f);

            return Mathf.Clamp(score, 0f, 9999f);
        }

        private static float CalculateAverageVisibility(PoseData pose)
        {
            if (pose.landmarkVisibility == null || pose.landmarkVisibility.Length == 0)
            {
                return 0.5f; // Default if no visibility data
            }

            float sum = 0f;
            int count = 0;
            
            // Focus on key body landmarks
            int[] keyLandmarks = { LEFT_SHOULDER, RIGHT_SHOULDER, LEFT_HIP, RIGHT_HIP, 
                                  LEFT_ELBOW, RIGHT_ELBOW, LEFT_KNEE, RIGHT_KNEE };
            
            foreach (int idx in keyLandmarks)
            {
                if (idx < pose.landmarkVisibility.Length)
                {
                    sum += pose.landmarkVisibility[idx];
                    count++;
                }
            }

            return count > 0 ? sum / count : 0.5f;
        }

        private static float CalculatePoseSymmetry(PoseData pose)
        {
            Vector3[] lm = pose.landmarks;
            
            // Compare left vs right sides
            float shoulderDiff = Vector3.Distance(
                lm[LEFT_SHOULDER] - lm[LEFT_HIP],
                lm[RIGHT_SHOULDER] - lm[RIGHT_HIP]
            );
            
            float armDiff = Vector3.Distance(
                lm[LEFT_ELBOW] - lm[LEFT_SHOULDER],
                lm[RIGHT_ELBOW] - lm[RIGHT_SHOULDER]
            );

            // Lower difference = more symmetrical = higher score
            float symmetry = 1f - Mathf.Clamp01((shoulderDiff + armDiff) * 2f);
            return symmetry;
        }

        private static float CalculateArmExtension(PoseData pose)
        {
            Vector3[] lm = pose.landmarks;
            
            // Measure how far arms are extended from body
            float shoulderWidth = Vector3.Distance(lm[LEFT_SHOULDER], lm[RIGHT_SHOULDER]);
            float bodyCenter = (lm[LEFT_SHOULDER].x + lm[RIGHT_SHOULDER].x) / 2f;
            
            float leftArmExtension = Mathf.Abs(lm[LEFT_WRIST].x - bodyCenter) / shoulderWidth;
            float rightArmExtension = Mathf.Abs(lm[RIGHT_WRIST].x - bodyCenter) / shoulderWidth;
            
            // Arms spread wide = higher score
            float extension = (leftArmExtension + rightArmExtension) / 2f;
            return Mathf.Clamp01(extension);
        }

        private static float CalculateStanceWidth(PoseData pose)
        {
            Vector3[] lm = pose.landmarks;
            
            // Measure leg separation
            float ankleDistance = Vector3.Distance(lm[LEFT_ANKLE], lm[RIGHT_ANKLE]);
            float shoulderWidth = Vector3.Distance(lm[LEFT_SHOULDER], lm[RIGHT_SHOULDER]);
            
            // Wider stance relative to shoulders = stronger pose
            float stanceRatio = ankleDistance / (shoulderWidth + 0.01f);
            return Mathf.Clamp01(stanceRatio / 2f); // Normalize
        }

        private static float CalculatePosture(PoseData pose)
        {
            Vector3[] lm = pose.landmarks;
            
            // Measure vertical alignment (nose over hips)
            float noseX = lm[NOSE].x;
            float hipCenterX = (lm[LEFT_HIP].x + lm[RIGHT_HIP].x) / 2f;
            float alignment = 1f - Mathf.Abs(noseX - hipCenterX) * 5f;
            
            // Measure vertical extension (head high above hips)
            float noseY = lm[NOSE].y;
            float hipCenterY = (lm[LEFT_HIP].y + lm[RIGHT_HIP].y) / 2f;
            float height = Mathf.Clamp01((noseY - hipCenterY) * 2f);
            
            return Mathf.Clamp01((alignment + height) / 2f);
        }

        private static float DetectActionPose(PoseData pose)
        {
            Vector3[] lm = pose.landmarks;
            float bonus = 0f;

            // Detect punching pose (arm extended forward)
            float leftArmForward = lm[LEFT_WRIST].z;
            float rightArmForward = lm[RIGHT_WRIST].z;
            if (leftArmForward > 0.2f || rightArmForward > 0.2f)
            {
                bonus += 0.3f; // 30% bonus for punching
            }

            // Detect arms raised (victory/power pose)
            float leftWristHeight = lm[LEFT_WRIST].y;
            float rightWristHeight = lm[RIGHT_WRIST].y;
            float shoulderHeight = (lm[LEFT_SHOULDER].y + lm[RIGHT_SHOULDER].y) / 2f;
            
            if (leftWristHeight > shoulderHeight && rightWristHeight > shoulderHeight)
            {
                bonus += 0.5f; // 50% bonus for arms raised
            }

            // Detect kicking (one leg raised significantly)
            float leftKneeHeight = lm[LEFT_KNEE].y;
            float rightKneeHeight = lm[RIGHT_KNEE].y;
            float hipHeight = (lm[LEFT_HIP].y + lm[RIGHT_HIP].y) / 2f;
            
            if (Mathf.Abs(leftKneeHeight - rightKneeHeight) > 0.15f)
            {
                bonus += 0.4f; // 40% bonus for kicking
            }

            return bonus;
        }

        private static float CalculateBodyEngagement(PoseData pose)
        {
            Vector3[] lm = pose.landmarks;
            
            // Measure overall body spread/activation
            float bodySpan = 0f;
            
            // Arm span
            bodySpan += Vector3.Distance(lm[LEFT_WRIST], lm[RIGHT_WRIST]);
            
            // Leg span
            bodySpan += Vector3.Distance(lm[LEFT_ANKLE], lm[RIGHT_ANKLE]);
            
            // Torso height
            bodySpan += Vector3.Distance(lm[NOSE], (lm[LEFT_HIP] + lm[RIGHT_HIP]) / 2f);
            
            // Normalize to 0-1 range (larger values = more engaged body)
            return Mathf.Clamp01(bodySpan / 3f);
        }

        /// <summary>
        /// Get a qualitative description of the pose strength
        /// </summary>
        public static string GetPowerLevelDescription(float score)
        {
            if (score < 500) return "Weak";
            if (score < 1500) return "Low";
            if (score < 3000) return "Average";
            if (score < 5000) return "Strong";
            if (score < 7000) return "Powerful";
            if (score < 8500) return "Elite";
            return "OVER 9000!!!";
        }
    }
}


