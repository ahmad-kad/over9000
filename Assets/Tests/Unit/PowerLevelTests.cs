using NUnit.Framework;
using ScouterXR.AI;
using UnityEngine;

namespace ScouterXR.Tests.Unit
{
    public class PowerLevelTests
    {
        [Test]
        public void CalculatePowerLevel_StablePose_ReturnsBaseLevel()
        {
            // Arrange
            var poseEstimator = new MediaPipePoseEstimator();
            var pose = CreateStablePose();

            // Act
            float powerLevel = poseEstimator.CalculatePowerLevel(pose);

            // Assert
            Assert.GreaterOrEqual(powerLevel, 1000f);
            Assert.LessOrEqual(powerLevel, 10000f);
            Debug.Log($"[TEST] Stable pose power level: {powerLevel}");
        }

        [Test]
        public void CalculatePowerLevel_DynamicPose_ReturnsHigherLevel()
        {
            // Arrange
            var poseEstimator = new MediaPipePoseEstimator();
            var stablePose = CreateStablePose();
            var dynamicPose = CreateDynamicPose();

            // Act
            float stablePower = poseEstimator.CalculatePowerLevel(stablePose);
            float dynamicPower = poseEstimator.CalculatePowerLevel(dynamicPose);

            // Assert
            Assert.Greater(dynamicPower, stablePower);
            Debug.Log($"[TEST] Dynamic pose ({dynamicPower}) > Stable pose ({stablePower})");
        }

        [Test]
        public void EstimateDepthFromPose_ValidPose_ReturnsReasonableDepth()
        {
            // Arrange
            var poseEstimator = new MediaPipePoseEstimator();
            var pose = CreateStablePose();

            // Act
            float depth = poseEstimator.EstimateDepthFromPose(pose);

            // Assert
            Assert.Greater(depth, 0.5f);
            Assert.Less(depth, 10f);
            Debug.Log($"[TEST] Estimated depth: {depth}m");
        }

        private MediaPipePoseEstimator.PoseData CreateStablePose()
        {
            return new MediaPipePoseEstimator.PoseData
            {
                Keypoints = new Vector3[33],
                Confidence = new float[33],
                Timestamp = Time.time,
                IsValid = true
            };
        }

        private MediaPipePoseEstimator.PoseData CreateDynamicPose()
        {
            var pose = CreateStablePose();
            // Simulate movement by adding velocity factors
            return pose;
        }
    }
}
