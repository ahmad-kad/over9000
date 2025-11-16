using UnityEngine;
using UnityEditor;
using ScouterXR.Core;
using ScouterXR.AI;

namespace ScouterXR.Editor
{
    /// <summary>
    /// Editor tool to clean up conflicting scene components.
    /// Use: Tools → ScouterXR → Clean Scene Setup
    /// </summary>
    public static class SceneCleanupTool
    {
        [MenuItem("Tools/ScouterXR/Clean Scene Setup")]
        public static void CleanSceneSetup()
        {
            int removed = 0;

            // Find and remove old XRTestSceneSetup components (EVEN IF DISABLED)
            var oldSetups = Object.FindObjectsByType<XRTestSceneSetup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var setup in oldSetups)
            {
                Debug.Log($"[SceneCleanup] Removing XRTestSceneSetup from {setup.gameObject.name} (enabled={setup.enabled})");
                Object.DestroyImmediate(setup.gameObject);
                removed++;
            }

            // Find and remove old ScouterSceneSetup components (EVEN IF DISABLED)
            var scouterSetups = Object.FindObjectsByType<ScouterSceneSetup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var setup in scouterSetups)
            {
                Debug.Log($"[SceneCleanup] Removing ScouterSceneSetup from {setup.gameObject.name} (enabled={setup.enabled})");
                Object.DestroyImmediate(setup.gameObject);
                removed++;
            }

            // Find and remove ALL AI components (will be recreated by UnifiedSceneSetup)
            var tfliteRunners = Object.FindObjectsByType<TFLiteModelRunner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var runner in tfliteRunners)
            {
                Debug.Log($"[SceneCleanup] Removing TFLiteModelRunner from {runner.gameObject.name}");
                Object.DestroyImmediate(runner.gameObject);
                removed++;
            }

            var poseEstimators = Object.FindObjectsByType<MediaPipePoseEstimator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var estimator in poseEstimators)
            {
                Debug.Log($"[SceneCleanup] Removing MediaPipePoseEstimator from {estimator.gameObject.name}");
                Object.DestroyImmediate(estimator.gameObject);
                removed++;
            }

            var modelManagers = Object.FindObjectsByType<MediaPipeModelManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var manager in modelManagers)
            {
                Debug.Log($"[SceneCleanup] Removing MediaPipeModelManager from {manager.gameObject.name}");
                Object.DestroyImmediate(manager.gameObject);
                removed++;
            }

            var visualizers = Object.FindObjectsByType<EnhancedPoseVisualizer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var viz in visualizers)
            {
                Debug.Log($"[SceneCleanup] Removing EnhancedPoseVisualizer from {viz.gameObject.name}");
                Object.DestroyImmediate(viz.gameObject);
                removed++;
            }

            var handRecognizers = Object.FindObjectsByType<HandPointingRecognizer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var hand in handRecognizers)
            {
                Debug.Log($"[SceneCleanup] Removing HandPointingRecognizer from {hand.gameObject.name}");
                Object.DestroyImmediate(hand.gameObject);
                removed++;
            }

            if (removed > 0)
            {
                Debug.Log($"✅ [SceneCleanup] Removed {removed} conflicting components. Scene is now clean!");
                EditorUtility.DisplayDialog("Scene Cleanup Complete",
                    $"Removed {removed} conflicting components.\n\nNow add UnifiedSceneSetup to a GameObject and press Play!",
                    "OK");
            }
            else
            {
                Debug.Log("✅ [SceneCleanup] Scene is already clean!");
                EditorUtility.DisplayDialog("Scene Already Clean",
                    "No conflicting components found.\n\nYour scene is ready!",
                    "OK");
            }
        }

        [MenuItem("Tools/ScouterXR/Create Unified Setup")]
        public static void CreateUnifiedSetup()
        {
            // Check if one already exists
            var existing = Object.FindFirstObjectByType<UnifiedSceneSetup>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Setup Already Exists",
                    $"UnifiedSceneSetup already exists on GameObject: {existing.gameObject.name}",
                    "OK");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Create new setup
            GameObject setupObj = new GameObject("Scene Setup");
            var setup = setupObj.AddComponent<UnifiedSceneSetup>();
            setup.autoSetupOnAwake = true;
            setup.useWebcamFeed = true;
            setup.enablePoseEstimation = true;
            setup.enableHandRecognition = true;

            Selection.activeGameObject = setupObj;
            EditorUtility.DisplayDialog("Setup Created",
                "UnifiedSceneSetup created and configured!\n\nPress Play to start.",
                "OK");

            Debug.Log("✅ [SceneCleanup] Created UnifiedSceneSetup. Ready to play!");
        }

        [MenuItem("Tools/ScouterXR/Full Clean + Setup")]
        public static void FullCleanAndSetup()
        {
            CleanSceneSetup();
            CreateUnifiedSetup();
        }
    }
}

