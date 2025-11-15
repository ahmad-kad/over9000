# DBZ Scouter XR Scripts

This folder contains the core C# scripts for the DBZ Scouter XR application.

## 📁 Folder Structure

```
Scripts/
├── Core/           # Main orchestration scripts
│   ├── ScouterManager.cs         # Main controller
│   ├── ScouterSceneSetup.cs      # Auto scene setup
│   └── SpatialAudioController.cs # 3D audio system
├── UI/             # User interface systems
│   ├── ScouterUI.cs             # UI fading & transitions
│   └── HaloColorController.cs   # Shader color smoothing
├── AI/             # Computer vision & ML
│   └── MediaPipePoseEstimator.cs # Pose detection & power calc
└── AR/             # Cross-platform AR features
    └── ArFeatureManager.cs      # Occlusion mode detection
```

## 🚀 Quick Start

### 1. Auto Scene Setup
Add the `ScouterSceneSetup` component to any GameObject in your scene and enable `autoSetupScene`. It will automatically create:
- AR Session & Session Origin
- AR Camera with proper settings
- All required manager components
- Basic UI canvas with scouter readout

### 2. Manual Setup
If you prefer manual setup:

1. **AR Foundation Setup:**
   - Add `AR Session` GameObject
   - Add `AR Session Origin` GameObject with `AR Camera` child

2. **Add Managers:**
   - `ScouterManager` (main controller)
   - `ArFeatureManager` (cross-platform features)
   - `MediaPipePoseEstimator` (AI backend)

3. **UI Setup:**
   - Create Canvas with `ScouterUI` component
   - Add `HaloColorController` for shader effects
   - Add `SpatialAudioController` for 3D sound

## 🎯 Key Components

### ScouterManager
**Purpose:** Central orchestration of all systems
- Coordinates AI, UI, AR, and Audio systems
- Handles system initialization and cross-communication
- Provides public API for external control

### MediaPipePoseEstimator
**Purpose:** Computer vision and power level calculation
- Interfaces with MediaPipe for pose detection
- Calculates power levels from pose stability and movement
- Sends raw data to UI systems (they handle smoothing)

### ScouterUI
**Purpose:** Smooth UI transitions and animations
- Uses DOTween for professional transitions
- SmoothDamp for value interpolation
- CanvasGroup for panel fading

### ArFeatureManager
**Purpose:** Cross-platform AR feature detection
- Detects available occlusion methods (stencil vs depth)
- Configures shader modes automatically
- Handles fallback scenarios

### SpatialAudioController
**Purpose:** 3D positional audio
- `Spatial Blend = 1` for full 3D spatialization
- Automatic positioning (attach to moving UI)
- Distance-based volume falloff

## 🔧 Dependencies

- **DOTween** (free from Asset Store) - Required for UI animations
- **TextMeshPro** - For better text rendering (optional)
- **AR Foundation 5.x** - For cross-platform AR
- **MediaPipe Unity Plugin** - For pose detection

## 🎮 Usage Examples

### Basic Setup:
```csharp
// Attach ScouterSceneSetup to any GameObject
// Enable autoSetupScene = true
// Scene will be ready to run!
```

### Manual Control:
```csharp
// Get references
var scouter = FindObjectOfType<ScouterManager>();
var ui = FindObjectOfType<ScouterUI>();

// Start scanning
scouter.StartScanning();

// Update target position and power
ui.UpdateTarget(screenPosition, powerLevel);

// Trigger overload
scouter.TriggerOverload();
```

## 🎨 Shader Setup

The `ScouterHalo.shader` supports three modes:
- **Stencil Mode:** Human segmentation (Vision Pro/iOS)
- **Depth Mode:** Environment depth (Quest 3)
- **Overlay Mode:** Simple rendering (fallback)

The `ArFeatureManager` automatically configures the shader based on platform capabilities.

## 🔊 Audio Setup

1. Add `SpatialAudioController` component
2. Assign your audio clips (scouter hum, power up, overload)
3. Attach to your moving UI element for automatic 3D positioning
4. Call `UpdatePowerLevel(power)` to adjust audio based on power

## 🐛 Debugging

- Enable `debugMode` on components for console logging
- Use `ScouterManager.GetPlatformInfo()` for platform details
- Check Unity Profiler for performance bottlenecks
- Use Frame Debugger for shader issues

## 📈 Performance Tips

- **UI:** CanvasGroup fading is GPU-accelerated
- **Audio:** Spatial audio has minimal CPU impact
- **Shaders:** Use shader keywords for platform-specific optimizations
- **AI:** Run pose detection on background thread

Happy coding! May your power level reach over 9000! 💥
