# 🚀 Over 9000! - Dragon Ball Z Pose Scouter

**"What does the Scouter say about his power level?"**

This is an augmented reality pose estimation system that uses MediaPipe and machine learning to analyze your body pose and give you a **power level reading** just like the Scouters from Dragon Ball Z! Strike powerful poses and watch your power level climb from "Weak" all the way to **"OVER 9000!!!"** 🔥

## 🎯 Features

- **Real-time Pose Tracking**: Uses MediaPipe BlazePose to detect 33 body landmarks
- **Power Level Scoring**: Analyzes pose strength, confidence, symmetry, and action poses
- **XR Ready**: Built for AR/VR experiences with Unity XR
- **Dragon Ball Z Theming**: Scouter-inspired UI with power level classifications
- **Multiple Visualizers**: Choose from simple dots or enhanced skeleton rendering
- **Performance Optimized**: Runs smoothly on mobile devices and desktops

## 🏃 Quick Start

1. **Open in Unity 6.0+** (URP required)
2. **Load the SampleScene**
3. **Press Play** and grant camera permissions
4. **Strike a pose!** - Spread your arms, stand tall, punch the air
5. **Watch your power level rise!**

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── AI/                    # ML inference and pose processing
│   │   ├── PoseScorer.cs      # Calculates power levels (the magic!)
│   │   ├── TFLiteModelRunner.cs # MediaPipe integration
│   │   └── EnhancedPoseVisualizer.cs # Fancy skeleton rendering
│   ├── Core/                  # Scene setup and XR management
│   │   ├── ScouterSceneSetup.cs # DBZ-themed scene bootstrap
│   │   └── XRTestSceneSetup.cs # XR configuration
│   └── UI/                    # User interface components
│       └── PoseVisualizer.cs  # Landmark visualization
├── StreamingAssets/
│   └── Models/                # MediaPipe BlazePose .tflite models
├── Scenes/
│   └── SampleScene.unity      # Main demo scene
└── Resources/
    └── Audio/SFX/             # Sound effects (glass shatter, etc.)
```

## ⚙️ How Power Levels Work

The scoring system analyzes multiple pose factors:

- **Confidence** (0-1000): How sure the AI is about pose detection
- **Visibility** (0-600): Clear view of body parts
- **Symmetry** (0-900): Balanced left/right pose
- **Arm Extension** (0-1500): Arms spread wide = power pose!
- **Stance Width** (0-1200): Wide stance = stronger presence
- **Posture** (0-900): Upright, confident posture
- **Action Bonus** (1.0x): Punching, kicking, arms raised = multipliers
- **Body Engagement** (0.4x): Overall pose dynamism

### Power Level Classifications:
- **0-999**: Weak
- **1000-2499**: Low
- **2500-4499**: Average
- **4500-6499**: Strong
- **6500-7999**: Powerful
- **8000-9199**: Elite
- **9200+**: OVER 9000!!!

## 🎮 Customization

### Adjust Sensitivity
Modify `Assets/Scripts/AI/PoseScorer.cs` to tune the scoring algorithm:
- Increase multipliers for more sensitive detection
- Decrease thresholds for harder-to-achieve high scores

### Visual Themes
- **Enhanced Visualizer**: Full skeleton with bones and joints
- **Simple Visualizer**: Just landmark dots
- **Scouter UI**: Dragon Ball Z themed power level display

### Camera Settings
- Front/back camera selection
- Resolution and FPS controls
- Detection confidence thresholds

## 🔧 Requirements

- **Unity 6.0+** (tested on 6000.0.39f1)
- **Universal Render Pipeline (URP)**
- **Webcam** (built-in or external)
- **Target Platforms**: Windows, macOS, Android, iOS

## 📱 Mobile Support

The project includes mobile-optimized settings and works on:
- **Android** (ARM64, with camera permissions)
- **iOS** (with camera permissions)
- Touch controls for VR/AR interactions

## 🐛 Troubleshooting

### No Camera Feed
- Check camera permissions in system settings
- Ensure no other apps are using the camera
- Try different camera resolution settings

### Low Power Levels
- Stand in well-lit areas
- Face the camera directly
- Try more dynamic poses (arms raised, wide stance)
- Adjust scoring sensitivity in `PoseScorer.cs`

### Performance Issues
- Lower camera resolution in `PoseCameraController`
- Increase detection score threshold
- Reduce landmark update frequency

### Build Issues
- Ensure all required packages are installed
- Check that MediaPipe models are in `StreamingAssets/Models/`
- Verify URP is properly configured

## 🎨 Extending the Project

### Add New Pose Types
1. Extend `PoseScorer.cs` with new detection methods
2. Add multipliers for specific pose combinations
3. Create custom power level categories

### Custom UI Themes
- Modify `EnhancedPoseVisualizer.cs` for new visual styles
- Add sound effects for power level changes
- Create VR/AR specific UI elements

### Multiplayer Features
- Add network synchronization for power level comparisons
- Create pose challenges and leaderboards
- Implement combo systems for pose sequences

## 📄 License

This project is open source and available under the MIT License. Feel free to use it for your own Dragon Ball Z fan projects!

## 🙏 Credits

- **MediaPipe**: Pose estimation models and Unity integration
- **Unity**: XR and rendering framework
- **Dragon Ball Z**: The inspiration for power levels and Scouters
- **TensorFlow Lite**: Machine learning inference

---

**"OVER 9000?!?! There\'s no way that can be right!"**

Start posing and see if you can break the scouter! 💪⚡


