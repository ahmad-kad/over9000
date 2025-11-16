using Unity.XR.CoreUtils;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif

namespace ScouterXR.Core
{
    /// <summary>
    /// XROrigin wrapper that ensures a Camera Floor Offset object and tracked pose driver
    /// are configured before the base Awake executes. This prevents warning spam and keeps
    /// the AR camera parented correctly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AutoXROrigin : XROrigin
    {
        [SerializeField]
        private string offsetObjectName = "Camera Floor Offset";

        private new void Awake()
        {
            EnsureOffsetHierarchy();
            EnsureCameraParented();
            
            // Add TrackedPoseDriver before calling base.Awake() to suppress warning
#if ENABLE_INPUT_SYSTEM
            EnsureTrackedPoseDriver();
#endif
            
            // Call base.Awake() after TrackedPoseDriver is set up
            base.Awake();
        }
        
        private new void Start()
        {
            // Double-check TrackedPoseDriver in Start as well
#if ENABLE_INPUT_SYSTEM
            EnsureTrackedPoseDriver();
#endif
        }

        private void EnsureOffsetHierarchy()
        {
            if (CameraFloorOffsetObject != null)
            {
                return;
            }

            Transform offsetTransform = transform.Find(offsetObjectName);
            if (offsetTransform == null)
            {
                GameObject offset = new GameObject(offsetObjectName);
                offsetTransform = offset.transform;
                offsetTransform.SetParent(transform, false);
            }

            CameraFloorOffsetObject = offsetTransform.gameObject;
        }

        private void EnsureCameraParented()
        {
            if (Camera == null || CameraFloorOffsetObject == null)
            {
                return;
            }

            if (Camera.transform.parent != CameraFloorOffsetObject.transform)
            {
                Camera.transform.SetParent(CameraFloorOffsetObject.transform, false);
            }
        }

#if ENABLE_INPUT_SYSTEM
        private void EnsureTrackedPoseDriver()
        {
            if (Camera == null)
            {
                return;
            }

            var driver = Camera.GetComponent<TrackedPoseDriver>();
            if (driver != null)
            {
                return;
            }

            driver = Camera.gameObject.AddComponent<TrackedPoseDriver>();
            driver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            driver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

            // Note: TrackedPoseDriver automatically sets up XR HMD tracking
            // No manual InputAction configuration needed
        }
#endif
    }
}

