#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ScouterXR.Tests.Utilities;

namespace ScouterXR.Editor
{
    public class TestRunnerEditor : EditorWindow
    {
        [MenuItem("ScouterXR/Test Runner")]
        static void ShowWindow()
        {
            GetWindow<TestRunnerEditor>("ScouterXR Test Runner");
        }

        private Vector2 scrollPos;

        void OnGUI()
        {
            GUILayout.Label("ScouterXR Automated Test Runner", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Find test runner in scene
            var testRunner = FindObjectOfType<TestRunner>();

            if (testRunner == null)
            {
                EditorGUILayout.HelpBox("No TestRunner found in scene. Run ScouterSceneSetup first.", MessageType.Warning);

                if (GUILayout.Button("Create Test Runner"))
                {
                    var testObj = new GameObject("Test Runner");
                    testObj.AddComponent<TestRunner>();
                    Debug.Log("Created Test Runner in scene");
                }
            }
            else
            {
                // Display test status
                EditorGUILayout.LabelField("Test Status", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Total Tests Run:", testRunner.totalTestsRun.ToString());
                EditorGUILayout.LabelField("Tests Passed:", testRunner.testsPassed.ToString());
                EditorGUILayout.LabelField("Tests Failed:", testRunner.testsFailed.ToString());
                EditorGUILayout.LabelField("Last Test Time:", testRunner.lastTestTime);
                EditorGUILayout.LabelField("Last Results:", testRunner.lastTestResults);

                EditorGUILayout.Space();

                // Test configuration
                EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
                testRunner.runOnStart = EditorGUILayout.Toggle("Run on Start", testRunner.runOnStart);
                testRunner.runUnitTests = EditorGUILayout.Toggle("Run Unit Tests", testRunner.runUnitTests);
                testRunner.runIntegrationTests = EditorGUILayout.Toggle("Run Integration Tests", testRunner.runIntegrationTests);
                testRunner.testInterval = EditorGUILayout.FloatField("Test Interval (sec)", testRunner.testInterval);

                EditorGUILayout.Space();

                // Manual test controls
                EditorGUILayout.LabelField("Manual Controls", EditorStyles.boldLabel);

                if (GUILayout.Button("Run All Tests Now"))
                {
                    testRunner.RunTestsManually();
                    Debug.Log("Manual test run initiated");
                }

                if (GUILayout.Button("Reset Counters"))
                {
                    testRunner.ResetTestCounters();
                }

                EditorGUILayout.Space();

                // System health info
                EditorGUILayout.LabelField("System Health", EditorStyles.boldLabel);

                var perfMonitor = FindObjectOfType<ScouterXR.Core.PerformanceMonitor>();
                if (perfMonitor != null)
                {
                    EditorGUILayout.LabelField("Average FPS:", perfMonitor.GetAverageFPS().ToString("F1"));
                    EditorGUILayout.LabelField("Memory Usage:", perfMonitor.GetMemoryUsageMB() + " MB");
                }

                var poseEstimator = FindObjectOfType<ScouterXR.AI.MediaPipePoseEstimator>();
                if (poseEstimator != null)
                {
                    EditorGUILayout.LabelField("Estimated Depth:", poseEstimator.GetCurrentEstimatedDepth().ToString("F1") + "m");
                }

                var arManager = FindObjectOfType<ScouterXR.AR.ArFeatureManager>();
                if (arManager != null)
                {
                    EditorGUILayout.LabelField("AR Mode:", arManager.GetCurrentOcclusionMode().ToString());
                }
            }

            EditorGUILayout.EndScrollView();
        }

        void OnInspectorUpdate()
        {
            Repaint(); // Update the window every frame
        }
    }
}
#endif
