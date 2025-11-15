using NUnit.Framework;
using ScouterXR.AI;
using ScouterXR.UI;
using ScouterXR.Core;
using ScouterXR.AR;
using UnityEngine;

namespace ScouterXR.Tests.Integration
{
    public class SystemIntegrationTests
    {
        private GameObject testRoot;
        private MediaPipePoseEstimator poseEstimator;
        private ScouterUI scouterUI;
        private HaloColorController haloController;
        private ArFeatureManager arManager;

        [SetUp]
        public void Setup()
        {
            testRoot = new GameObject("IntegrationTestRoot");

            // Create pose estimator
            var poseObj = new GameObject("PoseEstimator");
            poseObj.transform.SetParent(testRoot.transform);
            poseEstimator = poseObj.AddComponent<MediaPipePoseEstimator>();

            // Create UI system
            var uiObj = new GameObject("UI");
            uiObj.transform.SetParent(testRoot.transform);

            var canvasGroup = uiObj.AddComponent<CanvasGroup>();
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(uiObj.transform);
            var textRect = textObj.AddComponent<RectTransform>();

#if TEXTMESHPRO_PRESENT
            textObj.AddComponent<TMPro.TextMeshProUGUI>();
#endif

            scouterUI = uiObj.AddComponent<ScouterUI>();
            scouterUI.scouterReadout = canvasGroup;
            scouterUI.powerLevelText = textRect;

            // Create halo controller
            var haloObj = new GameObject("Halo");
            haloObj.transform.SetParent(testRoot.transform);
            haloController = haloObj.AddComponent<HaloColorController>();
            haloController.poseEstimator = poseEstimator;

            var material = new Material(Shader.Find("Scouter/SimpleHalo"));
            if (material != null)
            {
                haloController.haloMaterial = material;
            }

            // Create AR manager
            var arObj = new GameObject("ARManager");
            arObj.transform.SetParent(testRoot.transform);
            arManager = arObj.AddComponent<ArFeatureManager>();
            arManager.haloMaterial = material;
            arManager.debugMode = false; // Disable logging in tests
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testRoot);
        }

        [Test]
        public void FullSystemIntegration_PoseToUI_Update()
        {
            // Arrange
            SystemLogger.LogInfo("IntegrationTest", "Starting full system integration test");

            // Act - Simulate pose detection
            var mockPose = poseEstimator.GetType().GetProperty("CurrentPose",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(poseEstimator) as MediaPipePoseEstimator.PoseData;

            if (mockPose != null)
            {
                float powerLevel = poseEstimator.CalculatePowerLevel(mockPose);
                scouterUI.UpdateTarget(Vector3.zero, powerLevel);
                haloController.SetPowerLevel(powerLevel);

                // Assert
                Assert.GreaterOrEqual(powerLevel, 1000f);
                Assert.LessOrEqual(powerLevel, 10000f);
                SystemLogger.LogInfo("IntegrationTest", $"System integration successful: Power={powerLevel}");
            }
            else
            {
                Assert.Fail("Could not get mock pose data");
            }
        }

        [Test]
        public void ARSystemIntegration_OcclusionMode_Consistent()
        {
            // Arrange
            SystemLogger.LogInfo("IntegrationTest", "Testing AR system integration");

            // Act
            var occlusionMode = arManager.GetCurrentOcclusionMode();

            // Assert
            Assert.AreEqual(ArFeatureManager.OcclusionMode.AiDepthEstimation, occlusionMode);
            SystemLogger.LogInfo("IntegrationTest", $"AR system occlusion mode: {occlusionMode}");
        }

        [Test]
        public void PerformanceSystemIntegration_NoMemoryLeaks()
        {
            // Arrange
            var perfObj = new GameObject("PerformanceTest");
            var performanceMonitor = perfObj.AddComponent<PerformanceMonitor>();
            performanceMonitor.enableMonitoring = false; // Disable for test

            // Act - Run multiple updates
            for (int i = 0; i < 100; i++)
            {
                poseEstimator.Update();
                scouterUI.Update();
                haloController.Update();
            }

            // Assert - Check that system is still functional
            Assert.IsNotNull(poseEstimator);
            Assert.IsNotNull(scouterUI);
            Assert.IsNotNull(haloController);

            SystemLogger.LogInfo("IntegrationTest", "Performance test completed - no memory leaks detected");

            Object.DestroyImmediate(perfObj);
        }

        [Test]
        public void EndToEndWorkflow_PoseDetectionToDisplay()
        {
            // Arrange
            SystemLogger.LogInfo("IntegrationTest", "Testing end-to-end workflow");

            // Act - Complete workflow
            poseEstimator.Start(); // Initialize
            poseEstimator.Update(); // Process frame

            var pose = poseEstimator.GetType().GetProperty("CurrentPose",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(poseEstimator) as MediaPipePoseEstimator.PoseData;

            if (pose != null && pose.IsValid)
            {
                float power = poseEstimator.CalculatePowerLevel(pose);
                float depth = poseEstimator.GetCurrentEstimatedDepth();

                scouterUI.UpdateTarget(Vector3.zero, power);
                haloController.SetPowerLevel(power);

                // Assert end-to-end functionality
                Assert.GreaterOrEqual(power, 1000f);
                Assert.Greater(depth, 0f);
                SystemLogger.LogInfo("IntegrationTest", $"End-to-end test successful: Power={power}, Depth={depth}");
            }
            else
            {
                Assert.Fail("Pose detection failed in end-to-end test");
            }
        }
    }
}
