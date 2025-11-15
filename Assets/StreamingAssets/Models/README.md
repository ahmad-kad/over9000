# ML Model Files

## Required Files:
- pose_landmarker.tflite - MediaPipe Pose Landmarker model
- pose_landmarker.task - MediaPipe task file (if separate)

## Download from MediaPipe:
The MediaPipe Unity Plugin should include these files, or download from:
https://developers.google.com/mediapipe/solutions/vision/pose_landmarker

## Setup:
1. Ensure files are in StreamingAssets folder
2. Set import settings to "Streaming Asset" in Unity Inspector
3. MediaPipePoseEstimator.cs will load from this location
