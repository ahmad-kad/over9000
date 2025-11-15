// This file contains documentation for the XR HUD Test Scene
// Attach XRTestSceneRunner to an empty GameObject to create the test scene

/*
=== XR HUD TEST SCENE SETUP GUIDE ===

This test scene demonstrates the complete MVP pipeline for the DBZ Scouter XR system:

1. WEBCAM INTEGRATION
   - Captures live webcam feed
   - Displays feed in bottom-right corner
   - Used for pose and hand estimation

2. POSE ESTIMATION
   - MediaPipe-based pose detection (currently using mock data)
   - Calculates power levels from pose stability
   - Displays real-time power level readings

3. HAND GESTURE RECOGNITION
   - MediaPipe-based hand landmark detection
   - Recognizes pointing gestures
   - Triggers scanning when pointing gesture is held

4. SPATIAL UI SYSTEM
   - World-space UI that follows targets
   - Screen-space overlay for HUD elements
   - Smooth animations and transitions

5. STATE-LINKED AUDIO
   - Audio feedback for power level changes
   - Scanning beeps during target acquisition
   - Overload effects for high power levels

6. STREAMING DATA PIPELINE
   - Webcam → Pose Estimation → Power Calculation → UI Update → Audio Feedback
   - Real-time processing at configurable frame rates
   - Comprehensive debugging and monitoring

=== USAGE INSTRUCTIONS ===

1. Create a new empty scene in Unity
2. Add an empty GameObject named "TestSceneRunner"
3. Attach the XRTestSceneRunner script to it
4. Press Play to start the test scene

=== KEYBOARD CONTROLS ===

SPACE - Start test scan (simulates pointing gesture)
ESC - Stop current scan
W - Toggle webcam display
D - Toggle debug overlay
U - Toggle UI visibility

=== SCENE COMPONENTS CREATED ===

- XRTestSceneSetup: Main scene configuration
- Camera: Main camera for scene viewing
- Canvas: UI canvas for overlays
- Pose Estimator: AI pose detection system
- Hand Pointing Recognizer: Gesture recognition
- Spatial UI Manager: 3D UI management
- Scouter UI: Screen-space HUD
- Debug Overlay: Real-time monitoring
- Spatial Audio Controller: 3D audio system
- XR Scouter Manager: Main XR interaction logic

=== DEBUGGING FEATURES ===

The debug overlay shows:
- Real-time FPS and memory usage
- Pose detection depth and power levels
- Hand detection status and pointing confidence
- Webcam device information
- System status and performance metrics

=== AUDIO ASSETS USED ===

- SCOUTER - AUDIO FROM JAYUZUMI.COM.mp3 (activation)
- db-scouter-made-with-Voicemod.mp3 (beeps)
- HIS POWER LEVEL IS 1200 - AUDIO FROM JAYUZUMI.COM.mp3 (readings)
- 9000 - AUDIO FROM JAYUZUMI.COM.mp3 (overload)
- over9000.swf.mp3 (overload alt)

=== CUSTOMIZATION ===

Modify XRTestSceneSetup.cs to:
- Enable/disable specific features
- Adjust webcam resolution and framerate
- Configure UI update rates
- Change audio settings
- Modify pose estimation parameters

=== TESTING PIPELINE ===

1. Webcam feed appears in bottom-right
2. Pose detection shows power levels in UI
3. Pointing gesture (simulated by SPACE) starts scanning
4. Progress indicator shows scanning progress
5. Audio feedback plays during scanning
6. Final power level displayed with appropriate audio
7. Over 9000 triggers special effects

=== NEXT STEPS ===

After verifying the test scene works:
1. Replace mock pose data with real MediaPipe integration
2. Add actual TensorFlow Lite model loading
3. Implement eye tracking for gaze-based interaction
4. Add more sophisticated gesture recognition
5. Integrate with actual XR hardware (HMD, controllers)

*/
