# XREAL Air 2 Ultra Kit Setup Guide for Unity

**Project:** DBZ Scouter XR (Project Blue)  
**Target Platform:** XREAL Air 2 Ultra Glasses  
**Framework:** Unity AR Foundation  
**Last Updated:** November 15, 2025

---

## Table of Contents

1. [Hardware Requirements](#1-hardware-requirements)
2. [Software Prerequisites](#2-software-prerequisites)
3. [XREAL SDK Installation](#3-xreal-sdk-installation)
4. [Unity Project Configuration](#4-unity-project-configuration)
5. [Platform-Specific Settings](#5-platform-specific-settings)
6. [AR Foundation Integration](#6-ar-foundation-integration)
7. [Build Configuration](#7-build-configuration)
8. [Testing & Deployment](#8-testing--deployment)
9. [Troubleshooting](#9-troubleshooting)
10. [Performance Optimization](#10-performance-optimization)

---

## 1. Hardware Requirements

### XREAL Air 2 Ultra Kit Contents
- **XREAL Air 2 Ultra Glasses** (AR glasses with 23° FOV)
- **USB-C Connection Cable** (3m braided cable recommended)
- **Power Adapter** (USB-C PD compatible)
- **Beam Pro Controller** (optional, for enhanced interaction)

### Development Hardware
- **Host Device:** iPhone 13 Pro or later (iOS 16.0+)
- **Alternative:** Android phone with USB-C (for Beam Pro mode)
- **Computer:** Mac (primary) or Windows with iOS/Android build support
- **Cable:** USB-C to USB-C (for iPhone) or USB-C to USB-A/C (for Android)

### System Requirements
- **RAM:** 16GB minimum, 32GB recommended
- **Storage:** 50GB free space for Unity + builds
- **Graphics:** Dedicated GPU (Metal/OpenGL compatible)
- **Network:** Stable internet for SDK downloads

---

## 2. Software Prerequisites

### 2.1 Unity Hub & Editor

**Download Unity Hub:**
```
URL: https://unity.com/download
Platform: Mac/Windows
```

**Install Unity Editor:**
- Open Unity Hub
- Go to "Installs" tab
- Click "Install Editor"
- Select version: **2022.3.30f1 LTS** (recommended for AR Foundation stability)
- **Required Modules:**
  - ✅ iOS Build Support
  - ✅ Android Build Support (IL2CPP)
  - ✅ macOS Build Support (if on Mac)

**Why 2022.3 LTS?**
- Best AR Foundation compatibility
- Stable XR Plugin Management
- Proven with XREAL SDK

### 2.2 Xcode (macOS Required)

**Install Xcode:**
```bash
# Via App Store
Search: "Xcode"
Version: 15.0 or later
```

**Command Line Tools:**
```bash
xcode-select --install
```

**Verify Installation:**
```bash
xcodebuild -version
# Should show: Xcode 15.x.x
```

### 2.3 iOS Development Setup

**Apple Developer Account:**
- Free account sufficient for development
- Paid account ($99/year) required for distribution
- URL: https://developer.apple.com/programs/

**iOS Device Setup:**
- Enable Developer Mode on iPhone:
  - Settings → Privacy & Security → Developer Mode → Enable
- Trust computer connection when prompted
- Install iOS 16.0+ on device

---

## 3. XREAL SDK Installation

### 3.1 Download XREAL SDK

**Official Download:**
```
URL: https://developer.xreal.com/download
File: com.xreal.xr.tar.gz (Unity SDK 3.0)
Size: ~50MB
```

**Alternative Sources:**
- Unity Asset Store (search "XREAL")
- GitHub releases (if available)

### 3.2 SDK Contents

After extraction, the SDK contains:
```
XREAL_SDK/
├── package.json          # Unity package manifest
├── Runtime/              # Core runtime components
│   ├── XREAL XR Plugin
│   ├── AR Features
│   └── Interaction Basics
├── Samples/              # Example scenes
└── Documentation/        # API reference
```

### 3.3 Import into Unity Project

**Method 1: Package Manager (Recommended)**
1. Open Unity project
2. Window → Package Manager
3. Click "+" → "Add package from tarball..."
4. Select: `com.xreal.xr.tar.gz`
5. Wait for import (2-3 minutes)

**Method 2: Manual Import**
1. Extract tar.gz to temporary folder
2. Copy contents to `Assets/XREAL/`
3. Restart Unity if prompted

**Verification:**
- Check Package Manager for "XREAL XR Plugin"
- Should show version 3.0.x
- No import errors in console

---

## 4. Unity Project Configuration

### 4.1 Create/Open Project

**New Project Setup:**
1. Unity Hub → New Project
2. Template: "3D Core" (not URP/HDRP for AR compatibility)
3. Project Name: `ScouterXR_XREAL`
4. Location: Choose workspace directory
5. Click "Create"

**Existing Project:**
- Open existing ScouterXR project
- Ensure Unity version compatibility

### 4.2 Install Required Unity Packages

**Open Package Manager:**
```
Window → Package Manager
Set: "Unity Registry"
```

**Required Packages (Install in Order):**

1. **AR Foundation** (Core AR framework)
   ```
   Search: "AR Foundation"
   Version: 5.1.0 or later
   Status: Install
   ```

2. **ARKit XR Plugin** (iOS AR provider)
   ```
   Search: "ARKit XR Plugin"
   Version: 5.1.0 or later
   Status: Install
   ```

3. **XR Plugin Management** (XR provider management)
   ```
   Search: "XR Plugin Management"
   Version: 4.4.0 or later
   Status: Install
   ```

4. **Android Logcat** (Debugging tool)
   ```
   Search: "Android Logcat"
   Version: Latest
   Status: Install (optional, for Beam Pro debugging)
   ```

**Verify Installation:**
- All packages show green checkmarks
- No yellow warning triangles
- Restart Unity if prompted

### 4.3 Configure XR Plugin Management

**Access Settings:**
```
Edit → Project Settings → XR Plug-in Management
```

**iOS Tab Configuration (XREAL Air):**
```
✅ ARKit (enable for camera access)
✅ XREAL (enable XREAL provider)

XREAL Settings:
├── Interaction Profiles
│   ├── ✅ XREAL Controller Profile
│   └── ✅ Hand Tracking (if using hand gestures)
├── Features
│   ├── ✅ Spatial Anchors
│   ├── ✅ Plane Detection
│   └── ✅ Image Tracking (optional)
```

**Android Tab Configuration (Beam Pro - Optional):**
```
✅ ARCore (enable for Android AR)
✅ XREAL (enable XREAL provider)

XREAL Settings: Same as iOS tab
```

---

## 5. Platform-Specific Settings

### 5.1 iOS Player Settings (Primary)

**Access Settings:**
```
File → Build Settings → iOS
Edit → Project Settings → Player → iOS tab
```

**Identification:**
```
Bundle Identifier: com.yourcompany.scouterxr
Version: 1.0.0
Build: 1
Display Name: DBZ Scouter XR
```

**Resolution and Presentation:**
```
Default Orientation: Portrait
Allowed Orientations: Portrait only
Use Animated Autorotation: No
```

**Other Settings:**
```
Auto Graphics API: On (Metal)
Graphics APIs: Metal (default)
Scripting Backend: IL2CPP
API Compatibility Level: .NET Standard 2.1
Target Minimum iOS Version: 16.0
Architecture: ARM64
```

**Camera Usage Description:**
```
"This app uses the camera for augmented reality pose detection and spatial computing."
```

**Location Usage Description:**
```
"This app uses location services for spatial anchors and world tracking."
```

### 5.2 Android Player Settings (Beam Pro)

**Access Settings:**
```
File → Build Settings → Android
Edit → Project Settings → Player → Android tab
```

**Identification:**
```
Package Name: com.yourcompany.scouterxr
Version: 1.0.0
Minimum API Level: Android 10.0 (API 29)
Target API Level: Automatic (highest installed)
```

**Resolution and Presentation:**
```
Default Orientation: Portrait
Allowed Orientations: Portrait only
```

**Other Settings:**
```
Auto Graphics API: Off
Graphics APIs: OpenGLES3 (only)
Scripting Backend: IL2CPP
API Compatibility Level: .NET Standard 2.1
Target Architectures: ARM64
```

**Camera Usage Description:**
```
Same as iOS
```

### 5.3 Graphics Settings

**Access Settings:**
```
Edit → Project Settings → Graphics
```

**Graphics APIs (Android):**
```
Remove: Vulkan (if present)
Add: OpenGLES3 (as first entry)
Uncheck: Auto Graphics API
```

**Graphics APIs (iOS):**
```
Metal (leave default)
```

**Additional Settings:**
```
Color Space: Linear
Shader Precision Model: Unified
```

### 5.4 Quality Settings

**Access Settings:**
```
Edit → Project Settings → Quality
```

**Recommended Settings:**
```
Default Quality Level: High
Pixel Light Count: 1
Texture Quality: Full Res
Anisotropic Textures: Enabled
Anti Aliasing: 2x Multi Sampling
Soft Particles: No
Realtime Reflection Probes: No
Billboards Face Camera Position: No
```

---

## 6. AR Foundation Integration

### 6.1 Create AR Scene

**Delete Default Camera:**
- Select `Main Camera` in Hierarchy
- Delete (Right-click → Delete)

**Add AR Session Origin:**
```
Right-click Hierarchy → XR → AR Session Origin
```

**Add AR Session:**
```
Right-click Hierarchy → XR → AR Session
```

**Configure AR Session Origin:**
- Select `AR Session Origin` in Hierarchy
- Inspector → Add Component → `AR Pose Driver` (for device tracking)

### 6.2 Add XREAL Components

**Add XREAL Manager:**
```
Select AR Session Origin → Add Component → XREAL → XREAL Manager
```

**Configure XREAL Manager:**
```
XREAL Manager (Script):
├── Enable Passthrough: Yes
├── Enable Spatial Anchors: Yes
├── Enable Hand Tracking: Optional
└── Enable Eye Tracking: Optional
```

**Add XREAL Camera:**
```
Select Main Camera (under AR Session Origin) → Add Component → XREAL → XREAL Camera
```

### 6.3 Camera Configuration

**AR Camera Settings:**
```
Inspector → AR Camera (Script):
├── Focus Mode: Auto
├── Light Estimation: Ambient Intensity
└── Face Tracking: Disabled (not needed for Scouter)
```

**XREAL Camera Settings:**
```
Inspector → XREAL Camera (Script):
├── Render Mode: Passthrough
├── FOV: 23° (matches XREAL Air 2)
└── Stabilization: Enabled
```

---

## 7. Build Configuration

### 7.1 iOS Build Setup

**Switch Platform:**
```
File → Build Settings
Platform: iOS
Click: Switch Platform
```

**Add Scenes:**
- Click "Add Open Scenes"
- Ensure main scene is included

**Build Settings:**
```
Build System: New Build System (Preview)
Development Build: Yes (for debugging)
Autoconnect Profiler: Yes
Deep Profiling Support: Yes
Script Debugging: Yes
```

**Build Project:**
```
Click: Build
Save Location: Builds/iOS/
File Name: ScouterXR (creates ScouterXR.xcodeproj)
```

### 7.2 Xcode Configuration

**Open Project:**
- Navigate to `Builds/iOS/`
- Open `Unity-iPhone.xcodeproj`

**Project Settings:**
```
Project Navigator → Unity-iPhone
├── General Tab
│   ├── Bundle Identifier: com.yourcompany.scouterxr
│   ├── Signing: Select your Apple ID
│   └── Deployment Target: 16.0
├── Build Settings
│   ├── Enable Bitcode: No
│   ├── Swift Language Version: 5.0
│   └── Other C Flags: Add "-Wno-nullability-completeness"
```

**Signing & Capabilities:**
```
Signing & Capabilities Tab:
├── Team: Select your Apple Developer account
├── Bundle Identifier: com.yourcompany.scouterxr
├── Automatically manage signing: Yes
```

### 7.3 Android Build Setup (Beam Pro)

**Switch Platform:**
```
File → Build Settings
Platform: Android
Click: Switch Platform
```

**Player Settings Verification:**
- Ensure Android settings configured (Section 5.2)

**Build APK:**
```
Build Settings:
├── Build System: Gradle
├── Export Project: No (build APK directly)
├── Development Build: Yes
├── Autoconnect Profiler: Yes

Click: Build And Run
Save Location: Builds/Android/
File Name: ScouterXR.apk
```

---

## 8. Testing & Deployment

### 8.1 iOS Testing (XREAL Air)

**Connect iPhone:**
- Connect iPhone to Mac via USB-C
- Trust computer if prompted
- Ensure iPhone unlocked

**Xcode Deployment:**
```
Xcode Toolbar:
├── Device: Select your iPhone
├── Click ▶️ (Run button)

Wait: 2-3 minutes for build + deploy
```

**XREAL Connection:**
1. Put on XREAL Air 2 Ultra glasses
2. Ensure glasses charged (check LED indicator)
3. Connect glasses to iPhone:
   - iPhone Settings → XREAL → Connect Device
   - Follow pairing instructions

**Test App:**
- App launches automatically on glasses
- Should show passthrough camera view
- Test basic AR functionality

### 8.2 Android Testing (Beam Pro)

**Connect Android Device:**
- Enable Developer Options on Android
- Enable USB Debugging
- Connect via USB

**Deploy APK:**
```
Method 1: Build And Run (Unity)
Method 2: Manual ADB install
   adb install -r ScouterXR.apk
```

**Beam Pro Connection:**
- Pair Beam Pro with Android device
- Launch app on Beam Pro
- Test AR functionality

### 8.3 Basic Functionality Tests

**AR Foundation Tests:**
- ✅ Camera passthrough working
- ✅ Device tracking stable
- ✅ No AR session errors in logs

**XREAL-Specific Tests:**
- ✅ Glasses display shows Unity content
- ✅ Field of view matches 23° specification
- ✅ Passthrough rendering enabled
- ✅ No lens distortion artifacts

---

## 9. Troubleshooting

### 9.1 Common iOS Issues

**"Device not found" Error:**
```
Solutions:
1. Trust computer on iPhone
2. Check USB-C cable (use Apple certified)
3. Restart iPhone and Mac
4. Update Xcode to latest version
5. Check iOS version compatibility
```

**Build Fails with Code Signing:**
```
Solutions:
1. Xcode → Preferences → Accounts → Add Apple ID
2. Project → Signing & Capabilities → Select Team
3. Clean build folder: Xcode → Product → Clean Build Folder
4. Delete derived data: ~/Library/Developer/Xcode/DerivedData/
```

**XREAL Glasses Not Connecting:**
```
Solutions:
1. Ensure XREAL app installed on iPhone
2. Check glasses battery level
3. Restart glasses (hold power button 5s)
4. Re-pair in XREAL app
5. Check iOS location permissions
```

### 9.2 Common Android Issues

**ADB Device Not Found:**
```
Solutions:
1. Enable Developer Options → USB Debugging
2. Try different USB ports/cables
3. Install OEM USB drivers
4. Restart Android device
5. Check adb devices command
```

**APK Install Fails:**
```
Error: INSTALL_FAILED_UPDATE_INCOMPATIBLE

Solutions:
1. Uninstall existing app from device
2. Change package name if needed
3. Check Android version compatibility
4. Clean project and rebuild
```

### 9.3 Unity-Specific Issues

**XR Plugin Not Loading:**
```
Check:
1. Package Manager → XREAL XR Plugin installed
2. XR Plug-in Management → XREAL enabled
3. Restart Unity after package install
4. Check console for plugin errors
```

**AR Session Not Starting:**
```
Check:
1. AR Session component attached to scene
2. Camera permissions requested
3. AR Session Origin configured
4. No conflicting XR providers enabled
```

**Performance Issues:**
```
Check:
1. Frame rate (target 60fps)
2. CPU/GPU usage in Profiler
3. Shader compilation errors
4. Texture compression settings
```

### 9.4 XREAL-Specific Issues

**Poor Passthrough Quality:**
```
Solutions:
1. Clean glasses lenses
2. Adjust interpupillary distance
3. Check lighting conditions
4. Update XREAL firmware
```

**Tracking Drift:**
```
Solutions:
1. Ensure good lighting
2. Avoid reflective surfaces
3. Reset tracking: AR Session → Reset
4. Check for magnetic interference
```

---

## 10. Performance Optimization

### 10.1 Unity Optimization

**Quality Settings:**
```
Edit → Project Settings → Quality
- Reduce Anti Aliasing to 2x
- Lower Texture Quality if needed
- Disable unnecessary effects
```

**Player Settings:**
```
Other Settings:
- Strip Engine Code: Yes
- Optimize Mesh Data: Yes
- Prebake Collision Meshes: Yes
```

### 10.2 AR Foundation Optimization

**AR Session Configuration:**
```
AR Session (Script):
├── Match Frame Rate: Yes
├── Tracking Mode: Position and Rotation
└── Plane Detection: Disabled (if not needed)
```

**AR Camera Optimization:**
```
AR Camera (Script):
├── Light Estimation: Disabled (if not needed)
├── Face Tracking: Disabled
└── Environment Textures: Disabled
```

### 10.3 XREAL-Specific Optimization

**XREAL Manager Settings:**
```
XREAL Manager (Script):
├── Enable Hand Tracking: Only if needed
├── Enable Eye Tracking: Only if needed
├── Spatial Anchor Update Rate: 30Hz
└── Plane Detection Mode: Horizontal only
```

**Rendering Optimization:**
```
XREAL Camera (Script):
├── Render Scale: 0.8 (reduce if performance issues)
├── Stabilization: Enabled (improves comfort)
└── FOV Adaptation: Enabled
```

### 10.4 Profiling

**Unity Profiler:**
```
Window → Analysis → Profiler
- Connect to device
- Monitor CPU, GPU, Memory
- Check AR Foundation subsystems
```

**Xcode Instruments (iOS):**
```
Xcode → Product → Profile
- Select "Time Profiler"
- Look for AR/VR bottlenecks
```

### 10.5 Build Optimization

**IL2CPP Settings:**
```
Player Settings → Other Settings:
├── IL2CPP Code Generation: Faster (smaller) builds
├── C++ Compiler Configuration: Release
└── Strip Engine Code: Yes
```

**Asset Optimization:**
```
- Use ASTC texture compression
- Enable Crunch texture compression
- Use Asset Bundles for large assets
- Minimize shader variants
```

---

## Success Checklist

### Pre-Build Verification
- ✅ Unity 2022.3 LTS installed
- ✅ XREAL SDK 3.0 imported
- ✅ All required packages installed
- ✅ XR Plugin Management configured
- ✅ Player settings correct for target platform
- ✅ AR Foundation components added to scene

### Build Verification
- ✅ Project builds without errors
- ✅ No console warnings (red errors only)
- ✅ Build size reasonable (<100MB for iOS)
- ✅ Code signing configured (iOS)

### Runtime Verification
- ✅ App launches on device
- ✅ XREAL glasses connect properly
- ✅ AR session starts without errors
- ✅ Passthrough rendering works
- ✅ Basic tracking functional

### Performance Verification
- ✅ 60fps sustained performance
- ✅ No overheating issues
- ✅ Battery life acceptable (>30 minutes)
- ✅ No crashes during testing

---

## Next Steps

After completing XREAL setup:

1. **Integrate MediaPipe** for pose detection
2. **Implement Scouter UI** and HUD system
3. **Add Depth Halo Shader** for visual effects
4. **Implement Controller Haptics** for feedback
5. **Test Multi-Platform** compatibility

---

## Resources & Support

### Official Documentation
- **XREAL Developer Portal:** https://developer.xreal.com
- **Unity AR Foundation:** https://docs.unity3d.com/Packages/com.unity.xr.arfoundation
- **ARKit Documentation:** https://developer.apple.com/documentation/arkit

### Community Support
- **Unity Forums:** AR Foundation section
- **XREAL Developer Community:** https://forum.xreal.com
- **Stack Overflow:** Tag `[unity3d] [xreal] [ar-foundation]`

### Debug Tools
- **Unity Profiler:** Performance analysis
- **Xcode Instruments:** iOS performance
- **Android Logcat:** Android debugging
- **XREAL Debug Tools:** SDK logging

---

*This guide is specific to XREAL Air 2 Ultra with Unity AR Foundation. For other XREAL models or Unity versions, consult the official documentation.*
