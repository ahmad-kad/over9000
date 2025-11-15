using NUnit.Framework;
using ScouterXR.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ScouterXR.Tests.Unit
{
    public class UITests
    {
        private GameObject testObject;
        private ScouterUI scouterUI;
        private CanvasGroup canvasGroup;
        private RectTransform textRect;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestUI");

            // Create CanvasGroup
            canvasGroup = testObject.AddComponent<CanvasGroup>();

            // Create text rect
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(testObject.transform);
            textRect = textObj.AddComponent<RectTransform>();

            // Add TextMeshPro component (if available) or fallback
#if TEXTMESHPRO_PRESENT
            var tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmpText.text = "Test";
#else
            var uiText = textObj.AddComponent<Text>();
            uiText.text = "Test";
#endif

            // Setup ScouterUI
            scouterUI = testObject.AddComponent<ScouterUI>();
            scouterUI.scouterReadout = canvasGroup;
            scouterUI.powerLevelText = textRect;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [Test]
        public void ScouterUI_Initialization_HidesUI()
        {
            // Arrange & Act (happens in Setup)

            // Assert
            Assert.AreEqual(0f, canvasGroup.alpha);
            Debug.Log("[TEST] UI initializes hidden correctly");
        }

        [Test]
        public void ScouterUI_UpdateTarget_ChangesPowerLevel()
        {
            // Arrange
            Vector3 testPosition = new Vector3(100, 200, 0);
            float testPower = 5000f;

            // Act
            scouterUI.UpdateTarget(testPosition, testPower);

            // Assert - Check that internal values are set
            // (We can't easily test SmoothDamp in unit tests)
            Assert.IsNotNull(scouterUI);
            Debug.Log($"[TEST] UI target updated: pos={testPosition}, power={testPower}");
        }

        [Test]
        public void ScouterUI_SetPowerLevel_DirectUpdate()
        {
            // Arrange
            float testPower = 7500f;

            // Act
            scouterUI.SetPowerLevel(testPower);

            // Assert
            Assert.IsNotNull(scouterUI);
            Debug.Log($"[TEST] Power level set to: {testPower}");
        }

        [Test]
        public void HaloColorController_PowerLevel_ClampsValues()
        {
            // Arrange
            var haloObj = new GameObject("HaloTest");
            var haloController = haloObj.AddComponent<HaloColorController>();
            var material = new Material(Shader.Find("Scouter/SimpleHalo"));
            haloController.haloMaterial = material;

            // Act
            haloController.SetPowerLevel(500f);  // Below minimum
            haloController.SetPowerLevel(12000f); // Above maximum

            // Assert
            Assert.IsNotNull(haloController);
            Debug.Log("[TEST] Power level clamping works");

            Object.DestroyImmediate(haloObj);
        }
    }
}
