# MediaPipe Camera Feed Demo

This repository now focuses on a single goal: running MediaPipe pose tracking
directly on the local camera feed with the fewest moving parts possible. All XR,
spatial audio, and Scouter-specific scripts were removed so the project boots
straight into a clean pose-overlay experience.

## Quick Start

1. Open the project in Unity 2022.3+ (any URP/LTS build works).
2. Press Play in an empty scene – `PoseCameraController` bootstraps itself if it
   does not already exist in the hierarchy.
3. Grant camera permissions. The game view shows the webcam texture plus
   MediaPipe landmarks rendered on top.

### Disable auto-bootstrap

If you prefer manual control, add the scripting define symbol
`POSE_DEMO_DISABLE_AUTOCREATE` (Project Settings → Player) and drop
`PoseCameraController` into any scene to manage it yourself.

## Key Files

| Path | Purpose |
| ---- | ------- |
| `Assets/Scripts/PoseDemo/PoseCameraController.cs` | Starts the webcam, runs the TensorFlow Lite pose detection + landmark models, and feeds results to the overlay UI. |
| `Assets/Scripts/PoseDemo/PoseOverlayUI.cs` | Draws 33 landmark markers in normalized screen space. |
| `Assets/Scripts/AI/PoseDetect.cs` & `PoseLandmarkDetect.cs` | Upstream TensorFlow Lite tasks sourced from the MediaPipe Unity sample; kept verbatim for clarity. |
| `Assets/StreamingAssets/Models/*.tflite` | BlazePose models used at runtime. |

## Customisation Tips

- **Camera selection**: Toggle `preferFrontCamera` on the controller to prefer a
  front-facing lens. Resolution/FPS can also be changed via serialized fields.
- **Landmark visuals**: Update the marker color, size, or threshold on
  `PoseOverlayUI` to match your UI style.
- **Performance**: Lower `cameraResolution` or the detection score threshold if
  you need faster updates on low-powered hardware.
- **Extending output**: Subscribe to the resulting `PoseLandmarkDetect.Result`
  inside `PoseCameraController.RunPosePipeline()` to drive custom gameplay or
  analytics.

## Folder Layout

```
Assets/
  Scripts/
    AI/                # TensorFlow Lite pose tasks (minimal, no app logic)
    PoseDemo/          # Runtime controller + overlay UI
  StreamingAssets/
    Models/            # BlazePose .tflite files
```

Everything else is standard Unity boilerplate (Packages, ProjectSettings, etc.)
and can remain untouched.

## Troubleshooting

- **Black screen**: Ensure the webcam is not used by another app. Stop Play mode
  and try again; `WebCamInput` will iterate through available devices.
- **No landmarks**: Confirm the `.tflite` model files exist inside
  `Assets/StreamingAssets/Models`. You can swap in custom MediaPipe models by
  replacing those files.
- **Orientation issues**: `WebCamInput` normalises orientation, but some laptop
  webcams misreport rotations. Adjust `cameraResolution` to a square aspect to
  verify alignment, then tweak as needed.

Happy hacking!


