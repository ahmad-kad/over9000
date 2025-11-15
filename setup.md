# DBZ SCOUTER XR: PROJECT BLUE
## Complete Development Environment Setup Guide

**Target Audience:** Developers setting up for Immerse the Bay Hackathon  
**Estimated Setup Time:** 3-4 hours  
**Prerequisite Knowledge:** Basic Unity experience

---

## Table of Contents
1. [Prerequisites & Hardware Requirements](#1-prerequisites--hardware-requirements)
2. [Software Installation](#2-software-installation)
3. [Unity Project Setup](#3-unity-project-setup)
4. [Platform-Specific Configuration](#4-platform-specific-configuration)
5. [Dependencies & SDKs](#5-dependencies--sdks)
6. [First Build & Test](#6-first-build--test)
7. [Troubleshooting](#7-troubleshooting)

---

## 1. Prerequisites & Hardware Requirements

### Development Machine Specs

**Minimum:**
- CPU: Intel i5 / AMD Ryzen 5 (quad-core)
- RAM: 16GB
- GPU: GTX 1060 / RTX 2060 (for Unity editor)
- Storage: 50GB free SSD space
- OS: Windows 10/11, macOS 12+, or Ubuntu 20.04+

**Recommended:**
- CPU: Intel i7/i9 / AMD Ryzen 7/9
- RAM: 32GB
- GPU: RTX 3060+
- Storage: 100GB NVMe SSD
- OS: Windows 11 or macOS 14+

### Target Hardware (Choose One or More)

#### Primary: XREAL Air 2 / Raven Glasses
- **Why:** Perfect "Scouter" form factor, hands-free XR experience
- **Cost:** $379-699
- **Setup Time:** 30 minutes
- **Recommended for:** Main demo platform - looks most like actual Scouter
- **UI Note:** You'll handle UI implementation in Unity

#### Secondary: PICO 4 / 4 Ultra
- **Why:** SecureMR for privacy-first CV, excellent depth sensing
- **Cost:** $400-600
- **Setup Time:** 45 minutes
- **Recommended for:** Technical showcase with SecureMR privacy features
- **UI Note:** You'll handle UI implementation in Unity

#### Backup Options (Lower Priority)

#### Tertiary: Meta Quest 3
- **Why:** Accessible, stable development
- **Cost:** ~$500
- **Setup Time:** 20 minutes
- **Use if:** XREAL/PICO unavailable

#### Optional: Apple Vision Pro
- **Why:** Premium showcase, PolySpatial
- **Cost:** $3,499
- **Setup Time:** 45 minutes
- **Use if:** Budget allows, want "wow factor"

### Peripheral Hardware

- **USB-C Cable (3m)**: For tethered debugging
- **Bluetooth Adapter**: If PC lacks BLE support
- **External Webcam**: For desktop MediaPipe testing

---

## 2. Software Installation

### Step 2.1: Install Unity Hub & Editor

1. **Download Unity Hub**
   ```
   URL: https://unity.com/download
   Version: Latest (2024.x)
   ```

2. **Install Unity Editor via Hub**
   ```
   Recommended Version: 2022.3.30f1 LTS
   
   Required Modules:
   ✅ Android Build Support
      ├── Android SDK & NDK Tools
      └── OpenJDK
   ✅ iOS Build Support (macOS only)
   ✅ Documentation
   
   Optional Modules:
   ⚪ Linux Build Support (for server builds)
   ⚪ WebGL Build Support (future web demos)
   ```

   **Why 2022.3 LTS?**
   - Stable AR Foundation support
   - Compatible with all target platforms
   - Best MediaPipe plugin compatibility

### Step 2.2: Install Platform SDKs

#### For Meta Quest (Android)

1. **Android Studio** (for SDK/NDK management)
   ```
   URL: https://developer.android.com/studio
   Version: Latest stable
   
   During install, select:
   ✅ Android SDK
   ✅ Android SDK Platform
   ✅ Android Virtual Device
   ```

2. **Configure Android SDK**
   - Open Android Studio → SDK Manager
   - Install SDK Platform 29 (Android 10.0) - minimum
   - Install SDK Platform 33 (Android 13.0) - recommended
   - Install NDK (Side by side) version 21.4.7075529
   - Note SDK path (e.g., `C:\Users\YourName\AppData\Local\Android\Sdk`)

3. **Set Unity Android Preferences**
   - Unity → Edit → Preferences → External Tools
   - Set Android SDK path
   - Set Android NDK path
   - Set JDK path (Unity's OpenJDK is fine)

#### For Apple Devices (iOS/visionOS)

1. **Xcode** (macOS only)
   ```
   URL: App Store → Search "Xcode"
   Version: 15.0 or later
   Size: ~15GB
   ```

2. **Apple Developer Account**
   - Free account sufficient for testing
   - Paid ($99/year) required for distribution
   - URL: https://developer.apple.com

3. **Command Line Tools**
   ```bash
   xcode-select --install
   ```

#### For PICO (OpenXR)

1. **PICO Developer Center**
   ```
   URL: https://developer.picoxr.com/resources/#pdc
   Download: PICO Developer Center (PDC)
   ```

2. **Install PDC**
   - Windows: Run installer
   - Enables device connection, log viewing, restart functions
   - Critical for debugging PICO builds

### Step 2.3: Install Development Tools

1. **Git** (version control)
   ```
   URL: https://git-scm.com/downloads
   Configure:
   git config --global user.name "Your Name"
   git config --global user.email "your.email@example.com"
   ```

2. **Visual Studio Code** or **Rider**
   ```
   VSCode: https://code.visualstudio.com/
   Rider: https://www.jetbrains.com/rider/ (paid, better Unity integration)
   
   Recommended Extensions (VSCode):
   - C# (Microsoft)
   - Unity Code Snippets
   - Shader languages support
   ```

3. **Android Logcat Package** (Unity)
   - Will install later via Package Manager
   - Essential for debugging on Quest/PICO

---

## 3. Unity Project Setup

### Step 3.1: Create New Project

1. **Open Unity Hub**
2. Click **New Project**
3. Select **3D Core** template
4. Set Project Name: `ScouterXR`
5. Set Location: `[Your workspace]/ScouterXR`
6. Click **Create Project**

**Wait for Unity to initialize (5-10 minutes on first launch)**

### Step 3.2: Configure Project Settings

#### Graphics Settings
```
Edit → Project Settings → Graphics

Graphics APIs (Android):
1. Remove "Vulkan" (if present)
2. Add "OpenGLES3" as first entry
3. Uncheck "Auto Graphics API"

Graphics APIs (iOS):
1. Metal (leave default)

Color Space: Linear (for better visuals)
```

#### Quality Settings
```
Edit → Project Settings → Quality

Default Quality Level: High
Anti Aliasing: 4x MSAA (Quest 3) / 2x (Quest 2)
Shadow Resolution: Medium
Texture Quality: Full Res
V Sync Count: Don't Sync (AR manages this)
```

#### Player Settings (Android - Quest/PICO)
```
Edit → Project Settings → Player → Android tab

Company Name: [Your company]
Product Name: DBZ Scouter XR

Resolution and Presentation:
- Default Orientation: Portrait
- Allowed Orientations: Auto Rotation (all checked)

Other Settings:
- Auto Graphics API: OFF
- Graphics APIs: OpenGLES3
- Multithreaded Rendering: ON
- Scripting Backend: IL2CPP
- API Compatibility Level: .NET Standard 2.1
- Target Architectures: ARM64 ONLY (uncheck ARMv7)
- Minimum API Level: Android 10.0 (API 29)
- Target API Level: Automatic (highest installed)

Identification:
- Package Name: com.yourcompany.scouterxr
  (must be unique, lowercase, no spaces)
```

#### Player Settings (iOS - XREAL/Vision Pro)
```
Edit → Project Settings → Player → iOS tab

Company Name: [Your company]
Product Name: DBZ Scouter XR

Resolution and Presentation:
- Default Orientation: Portrait

Other Settings:
- Auto Graphics API: ON (Metal is fine)
- Scripting Backend: IL2CPP
- API Compatibility Level: .NET Standard 2.1
- Target Minimum iOS Version: 16.0
- Architecture: ARM64

Identification:
- Bundle Identifier: com.yourcompany.scouterxr
  (must match Android package name format)

Camera Usage Description:
"This app uses the camera for augmented reality pose detection."
```

### Step 3.3: Install Core Unity Packages

1. **Open Package Manager**
   ```
   Window → Package Manager
   ```

2. **Switch to Unity Registry**
   - Dropdown at top-left: "Packages: Unity Registry"

3. **Install Required Packages** (in order)

   **AR Foundation Core:**
   ```
   Search: "AR Foundation"
   Version: 5.1.0 or later
   Click: Install
   ```

   **ARKit (iOS):**
   ```
   Search: "ARKit XR Plugin"
   Version: 5.1.0 or later
   Click: Install
   ```

   **ARCore (Android - Quest/PICO):**
   ```
   Search: "ARCore XR Plugin"
   Version: 5.1.0 or later
   Click: Install
   ```

   **OpenXR (Quest/PICO):**
   ```
   Search: "OpenXR Plugin"
   Version: 1.9.0 or later
   Click: Install
   ```

   **XR Plugin Management:**
   ```
   Search: "XR Plugin Management"
   Version: 4.4.0 or later
   Click: Install
   ```

   **Android Logcat:**
   ```
   Search: "Android Logcat"
   Version: Latest
   Click: Install
   ```

   **PolySpatial (Vision Pro only):**
   ```
   If targeting Vision Pro:
   Search: "Apple PolySpatial visionOS"
   Version: Latest
   Click: Install
   ```

4. **Verify Installation**
   - All packages should show green checkmark
   - No yellow warning triangles
   - Restart Unity if prompted

### Step 3.4: Configure XR Plugin Management

1. **Open XR Settings**
   ```
   Edit → Project Settings → XR Plug-in Management
   ```

2. **Android Tab (Quest/PICO)**
   ```
   ✅ OpenXR
   
   Click "OpenXR" to expand settings:
   - Interaction Profiles:
     ✅ Oculus Touch Controller Profile
     ✅ HTC Vive Controller Profile
   
   - Features:
     ✅ Meta Quest Support
     ✅ Hand Tracking (optional)
   ```

3. **iOS Tab (XREAL/iPhone)**
   ```
   ✅ ARKit
   
   Optional:
   ✅ Apple PolySpatial (if Vision Pro)
   ```

---

## 4. Platform-Specific Configuration

### Option A: XREAL Air 2 / Raven Glasses Setup (PRIMARY)

#### Step 4.1: Download XREAL SDK

```
URL: https://developer.xreal.com/download
File: com.xreal.xr.tar.gz (Unity SDK 3.0)
```

#### Step 4.2: Import XREAL Package

```
Window → Package Manager
"+" → Add package from tarball
Select: com.xreal.xr.tar.gz
Import all
```

#### Step 4.3: Enable XREAL Plugin

```
Edit → Project Settings → XR Plug-in Management
iOS tab:
✅ XREAL

Android tab (if using Beam Pro):
✅ XREAL
✅ ARCore
```

#### Step 4.4: Configure Player Settings

```
Edit → Project Settings → Player → iOS tab

Resolution and Presentation:
- Default Orientation: Portrait

Other Settings:
- Auto Graphics API: ON (Metal is fine)
- Scripting Backend: IL2CPP
- API Compatibility Level: .NET Standard 2.1
- Target Minimum iOS Version: 16.0
- Architecture: ARM64

Bundle Identifier: com.yourcompany.scouterxr

Camera Usage Description:
"This app uses the camera for augmented reality pose detection."
```

### Option B: PICO 4 / 4 Ultra Setup (PRIMARY)

#### Step 4.1: Download PICO SDK

```
URL: https://developer.picoxr.com/resources/#pdc
File: PICO Unity Integration SDK (latest)
```

#### Step 4.2: Import PICO Package

```
Window → Package Manager
"+" → Add package from disk
Navigate to: PICO SDK folder/package.json
Click: Open
```

#### Step 4.3: Enable PICO XR

```
Edit → Project Settings → XR Plug-in Management
Android tab:
✅ PICO XR

Click "PICO XR" to expand:
✅ Enable SecureMR (for on-device CV)
✅ Enable Video See-Through
```

#### Step 4.4: Configure Player Settings

```
Edit → Project Settings → Player → Android tab

Company Name: [Your company]
Product Name: DBZ Scouter XR

Resolution and Presentation:
- Default Orientation: Portrait

Other Settings:
- Auto Graphics API: OFF
- Graphics APIs: OpenGLES3
- Scripting Backend: IL2CPP
- API Compatibility Level: .NET Standard 2.1
- Target Architectures: ARM64 ONLY
- Minimum API Level: Android 10.0 (API 29)
- Target API Level: Automatic

Identification:
- Package Name: com.yourcompany.scouterxr
```

### Option C: Meta Quest 3 Setup (BACKUP)

#### Step 4.1: Install Quest SDK

1. **Download Oculus Integration**
   ```
   URL: https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022
   Method: Unity Asset Store (free)
   ```

2. **Import via Package Manager**
   ```
   Window → Package Manager → My Assets
   Find: Oculus Integration
   Click: Download → Import

   In import dialog:
   ✅ Oculus (folder)
   ✅ VR (folder)
   ⚪ Deselect: Sample scenes (not needed)
   ```

3. **Run Oculus Project Setup Tool**
   ```
   After import, popup appears:
   "Oculus Project Setup Tool"

   Click: "Fix All"
   Then: "Apply All"
   ```

#### Step 4.2: Enable Quest Features

```
Edit → Project Settings → Oculus

General:
✅ Enable Passthrough (essential!)
✅ Enable Guardian System
⚪ Skip Permission Requests (leave unchecked)

Quest 3 Features:
✅ Use Environment Depth
✅ Symmetric Projection
```

### Option D: Apple Vision Pro Setup (OPTIONAL)

#### Step 4.1: Enable visionOS Platform

```
File → Build Settings
Platform: visionOS (may need to install module)
Click: Switch Platform
```

#### Step 4.2: Configure PolySpatial

```
Edit → Project Settings → PolySpatial

Volume Camera:
✅ Enable Volume Camera Mode
Dimensions: 2m x 2m x 2m (default)

XR Origin:
✅ Track Device Pose
✅ Track User Presence
```

#### Step 4.3: Create visionOS Scheme

In Unity:
```
Edit → Project Settings → Player → visionOS

Other Settings:
- Requires visionOS: 1.0
- Device Capabilities:
  ✅ World Sensing
  ✅ Hand Tracking
```

---

## 5. Dependencies & SDKs

### Step 5.1: MediaPipe Integration

MediaPipe doesn't have an official Unity package. Use a community wrapper:

#### Option A: MediaPipeUnityPlugin (Recommended)

```
URL: https://github.com/homuler/MediaPipeUnityPlugin
Method: Clone or download ZIP
```

**Installation:**
```bash
cd Assets
git clone https://github.com/homuler/MediaPipeUnityPlugin.git MediaPipe
```

Or download ZIP and extract to `Assets/MediaPipe`

**Build Native Libraries:**
```bash
# Follow repo instructions for your platform
# Windows: Build with Visual Studio
# macOS: Build with Xcode
# Linux: Build with gcc

# This generates .so/.dylib/.dll files
```

#### Option B: Use TensorFlow Lite (Alternative)

If MediaPipe is too complex:

```
URL: https://github.com/tensorflow/tensorflow/tree/master/tensorflow/lite/examples/pose_estimation
Method: Download pose_landmarker.tflite model

Import TFLite package:
URL: https://github.com/asus4/tf-lite-unity-sample
```

### Step 5.2: Additional Assets

#### Shaders & Effects
```
Download: Free shader assets for glitch effect
URL: https://assetstore.unity.com/packages/vfx/shaders/

Recommended:
- Screen Glitch Shader Pack (free)
- Post Processing Stack v2 (Unity official)
```

#### UI Assets
```
Import TextMeshPro (if not already):
Window → TextMeshPro → Import TMP Essential Resources
```

#### Audio
```
Sounds needed:
- Glass shatter sound
- Electrical overload sound

Free sources:
- Freesound.org
- ZapSplat.com (requires free account)

Import:
1. Download WAV files
2. Drag into Assets/Audio/
3. Set Import Settings:
   - Force to Mono: Yes
   - Load Type: Compressed in Memory
   - Quality: 70% (Vorbis)
```

---

## 6. First Build & Test

### Step 6.1: Create Minimal Scene

1. **Delete Default Camera**
   - Right-click `Main Camera` → Delete

2. **Add AR Session Origin**
   - Right-click Hierarchy → XR → AR Session Origin
   - This creates camera rig compatible with all platforms

3. **Add AR Session**
   - Right-click Hierarchy → XR → AR Session
   - Manages AR lifecycle

4. **Save Scene**
   - File → Save Scene As
   - Name: `MainScene`
   - Location: Assets/Scenes/

### Step 6.2: Build for Quest (Test Build)

1. **Connect Quest via USB**
   - Enable Developer Mode on Quest:
     - Phone app → Settings → Developer Mode → ON
   - Connect USB-C cable
   - Put on headset, allow USB debugging

2. **Verify Device Connection**
   ```
   Open: PICO Developer Center or Android Studio
   Check: Device shows as connected
   ```

3. **Build and Run**
   ```
   File → Build Settings
   Platform: Android
   Click: Refresh (should show your Quest)
   
   Add Open Scenes (MainScene)
   
   Click: Build And Run
   Choose location: Builds/Quest_Test.apk
   Wait: 5-10 minutes for first build
   ```

4. **Test**
   - App should launch on Quest automatically
   - You should see passthrough (real world visible)
   - No functionality yet, but confirms setup works

### Step 6.3: Build for iOS (Test Build)

*macOS only*

1. **Connect iPhone/iPad**
   - USB-C or Lightning cable
   - Trust computer if prompted

2. **Build Project**
   ```
   File → Build Settings
   Platform: iOS
   Click: Build
   Choose folder: Builds/iOS/
   Wait: 3-5 minutes
   ```

3. **Open in Xcode**
   ```
   Navigate to: Builds/iOS/
   Open: Unity-iPhone.xcodeproj
   
   In Xcode:
   1. Select device from dropdown (your iPhone)
   2. Fix signing: Team → Select your Apple ID
   3. Click Play button (▶️)
   ```

4. **Test**
   - App launches on iPhone
   - Grant camera permission
   - Should see camera passthrough

---

## 7. Troubleshooting

### Common Issue #1: "No devices found"

**Quest:**
```
Solution:
1. Enable Developer Mode (Meta Quest app on phone)
2. Check USB cable (use USB 3.0 port, not 2.0)
3. Try different cable (data cables, not charge-only)
4. Install/update ADB:
   Open Android Studio → SDK Manager → SDK Tools
   ✅ Android SDK Platform-Tools
```

**iPhone:**
```
Solution:
1. Trust computer on iPhone
2. Update iTunes/Finder (provides iOS drivers)
3. Check cable (use official Apple cable)
4. Check Xcode → Window → Devices
   Should show device
```

### Common Issue #2: Build fails with "Android SDK not found"

```
Solution:
1. Open Unity → Edit → Preferences → External Tools
2. Click "Browse" next to Android SDK
3. Navigate to Android SDK location:
   Windows: C:\Users\[You]\AppData\Local\Android\Sdk
   Mac: ~/Library/Android/sdk
4. If folder doesn't exist:
   - Open Android Studio
   - Tools → SDK Manager
   - Note SDK Location path
   - Use that path in Unity
```

### Common Issue #3: "Unable to install APK"

```
Error: INSTALL_FAILED_UPDATE_INCOMPATIBLE

Solution:
1. On Quest:
   - Library → Unknown Sources
   - Find old version of app
   - Uninstall it
2. Or in Unity:
   File → Build Settings → Build (not Build and Run)
   Manually install via ADB:
   adb install -r path/to/your.apk
```

### Common Issue #4: App crashes immediately

**Quest:**
```
Check Logcat:
Window → Analysis → Android Logcat
Filter: "Unity" or "AndroidRuntime"

Common causes:
- Graphics API mismatch (must be OpenGLES3)
- Missing permissions in AndroidManifest.xml
- IL2CPP build error (try rebuild)
```

**iOS:**
```
Check Xcode console:
- Missing frameworks (add in Xcode project)
- Camera permission not requested
- Signing issue (re-sign in Xcode)
```

### Common Issue #5: MediaPipe not working

```
Symptoms:
- No pose detection
- Console error: "Failed to load model"

Solutions:
1. Check model file location:
   Assets/StreamingAssets/pose_landmarker.tflite
2. Verify model is set to "Streaming Asset":
   Select file → Inspector → Asset Labels
3. Test on device, not in editor (editor may lack native libs)
```

### Common Issue #6: Passthrough not showing

**Quest:**
```
Check:
1. Oculus → Tools → Project Setup Tool
   Should show green checkmarks
2. Edit → Project Settings → Oculus
   ✅ Enable Passthrough
3. In scene:
   Check OVRCameraRig has OVRPassthroughLayer component
```

### Common Issue #7: Performance issues (low FPS)

```
Solutions:
1. Lower quality settings:
   Edit → Project Settings → Quality
   Reduce shadows, textures, MSAA

2. Profile the app:
   Window → Analysis → Profiler
   Connect to device
   Check: CPU, GPU, Memory usage

3. Optimize MediaPipe:
   - Run inference every 2-3 frames (not every frame)
   - Reduce input resolution
   - Use GPU delegate if available
```

---

## Quick Reference: Build Commands

### Build for Quest 3
```bash
# Command line build (optional)
Unity.exe -quit -batchmode -projectPath "path/to/ScouterXR" \
  -buildTarget Android \
  -executeMethod BuildScript.BuildQuest
```

### Check ADB Devices
```bash
adb devices
# Should show:
# [serial number]    device
```

### Install APK Manually
```bash
adb install -r ScouterXR.apk
```

### View Logs
```bash
adb logcat | grep Unity
```

---

## Next Steps

After completing this setup:

1. ✅ You have a working Unity project
2. ✅ You can build to your target device
3. ✅ AR Foundation is configured
4. ✅ All SDKs are installed

**Now proceed to:**
- Implement `ScouterController.cs` (see SDD)
- Integrate MediaPipe pose estimation
- Build depth halo shader
- Test on device

**Time to first working demo:** ~2-3 days from this point

---

## Support Resources

### Official Documentation
- **AR Foundation:** https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1
- **Meta Quest:** https://developer.oculus.com/documentation/unity
- **PICO:** https://developer.picoxr.com/document/unity
- **XREAL:** https://developer.xreal.com/docs

### Community Help
- **Unity Forum:** forum.unity.com
- **r/Unity3D:** reddit.com/r/Unity3D
- **Stack Overflow:** Tag: [unity3d] [ar-foundation]

### Direct Support
- **Unity Support:** support.unity.com (requires Plus/Pro)
- **Meta Developer:** developer.oculus.com/resources

---

**Setup Complete! Ready to build the Scouter XR experience. 🚀**

*Last Updated: November 14, 2025*