# DBZ SCOUTER XR: PROJECT BLUE
*"It's Over 9000!!!"* [Original Video and Audio]

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Copyright Notice for Assets
**⚠️ IMPORTANT: Audio assets contain copyrighted Dragon Ball Z content**

This project includes audio assets that are protected by copyright. The Dragon Ball Z franchise, including all character voices, sound effects, and music, is owned by Toei Animation, Funimation, and Bandai Namco Entertainment.

- **Personal/educational use only**: This project is intended for personal development and learning purposes
- **No commercial distribution**: Do not distribute, sell, or monetize this application
- **No public hosting**: Do not upload to app stores or make publicly available
- **Replace for production**: Use only original or properly licensed audio assets for any public release

See [Assets/README.md](Assets/README.md) for detailed asset licensing information.

## Project Mission
To create a compelling Extended Reality (XR) application that simulates the iconic Dragon Ball Z Scouter experience, leveraging real-time computer vision and spatial computing features for a high-impact hackathon demo.

## Target Platforms
Universal XR Application targeting:
- **iOS Devices (ARKit)**: iPhone, iPad, Ray-Ban Meta Smart Glasses (via companion app)
- **Meta Devices (OpenXR)**: Meta Quest 3, Meta Quest 2
- **Apple Vision Pro (visionOS)**: Leveraging the PolySpatial package (for advanced showcase)

## I. Core Features & Technical Breakdown

The application uses real-time video, depth sensing, and pose estimation to create a dynamic AR overlay on a target individual.

### 1. Spatial & Visual Features (AR/Depth)

| Feature | Technical Implementation | Value Proposition |
|---------|------------------------|------------------|
| **Depth Culling & Halo** | Uses the device's Depth API/LiDAR (ARKit) or equivalent depth data (Quest/Vision Pro) to separate the foreground (person) from the background. A custom shader renders a vibrant, color-changing Halo/Outline around the subject while fading or applying a monochrome effect to the environment (pass-through mode). | High Visual Impact. Creates the immediate, iconic Scouter view. |
| **Holographic Depth-Following UI** | The HUD elements (Power Level, Height, etc.) are rendered as 3D world-space canvases anchored to the target. The UI dynamically adjusts its size/position so it always appears legible and anchored at a fixed distance (e.g., 0.5m) from the subject, maintaining depth fidelity. | Authentic Immersion. Ensures the data "follows" the subject in 3D space, not just on the screen. |
| **Real-World Measurements** | Calculates the absolute distance between key 3D keypoints (e.g., head/feet, fingertip/fingertip). | Technical Accuracy. Displays quantifiable metrics like Height and Arm Span in meters. |
| **Gesture: Point-to-Select** | Detects a simple pointing gesture (straight arm and hand) using pose keypoints (shoulder, elbow, wrist). This gesture is used to confirm the target and freeze the current Power Level reading for a closer look. | Intuitive Interaction. Enables hands-free control, a crucial XR feature. |
| **Face Search** | Optional feature to identify individuals using face landmarks. | High Tech Integration. Adds a real-world utility layer. |

### 2. Power Level System (Computer Vision Metrics)

The Power Level is a Composite Score that reacts to both the person's pose and movement.

| Metric Component | Calculation Logic | Sensitivity Control |
|------------------|------------------|-------------------|
| **4D Tracking & History Buffer** | Implements a rolling data structure to store the target's Power Level, spatial location (ARAnchor), and last known movement vector for the previous 5 seconds. If the target is lost and reacquired, the "Last Power Level" is momentarily flashed. | Persistence & Awareness. Uses the time dimension to simulate "memory." |
| **$C_{Static}$ (Stance Confidence)** | Measures the confidence and dominance of the posture (Baseline Factor, 0-1.0). Metrics include:<br>1. Stability: Low hip-sway/COG drift<br>2. Wide Stance: Ankle distance relative to height<br>3. Openness: Arms held away from the body (not crossed). | Tunable weights applied to each pose metric to define what constitutes a "high-power" stance. |
| **$S_{Dynamic}$ (Movement Spike)** | Measures the intensity and speed of movement (Spike Factor). Metrics include:<br>1. Joint Velocity: Speed of wrist/elbow keypoints between frames<br>2. Total Displacement: Area swept by limbs (proxy for force). | Keypoint Confidence Filter: Only use keypoint data with a confidence score > 0.5. Raising this threshold lowers the sensitivity to subtle/blurry movement. |
| **Scouter Overload FX** | Triggered when Power Level exceeds a hard threshold (e.g., 8,000). Activates screen glitch, shatter sound, and haptic feedback. | Aesthetic Polish. Creates the necessary "wow factor." |

## II. Technical Stack & Justification

| Category | Component | Justification |
|----------|-----------|---------------|
| **Engine** | Unity (C#) | Superior framework (AR Foundation) for AR mobile/headset deployment; essential for cross-platform compatibility across ARKit, OpenXR, and visionOS via a single codebase. |
| **AR Framework** | AR Foundation, ARKit XR Plugin, ARCore XR Plugin, Meta XR/OpenXR, PolySpatial | The unified interface to access camera, depth, and tracking across all targeted platforms with minimal code changes. |
| **Computer Vision** | MediaPipe Pose (or TFLite equivalent) | Necessary for real-time, lightweight 33-point 3D body landmark detection on resource-constrained XR hardware. |
| **Assets** | Custom Shaders, UI/HUD Prefabs, Sound FX | Used for the Scouter aesthetic, halo visualization, and overload effects. |


## III. Setup Process: Unity Cross-Platform Initialization

This expanded process prepares your Unity project for deployment across all major XR ecosystems using AR Foundation's plug-in architecture.

### Phase 1: Environment Setup
- **Install Unity**: Install a Unity LTS Editor version via Unity Hub (e.g., 2022 LTS)
- **Crucial**: Ensure the iOS Build Support and Android Build Support modules are checked during installation
- **Create Project**: Create a new 3D Core project in Unity
- **Switch Platform**: Go to File > Build Settings, select iOS, and click Switch Platform (We start with iOS, but the setup handles Android/Quest later)

### Phase 2: Core Package Installation (Package Manager)
- **Open Package Manager**: Navigate to Window > Package Manager
- **Select Unity Registry**: Change the package source dropdown from "In Project" to Unity Registry
- **Install Packages** (Search and Install):
  - AR Foundation (Core AR framework)
  - ARKit XR Plugin (iOS provider package for iPhone/Ray-Ban)
  - ARCore XR Plugin (Android provider package for generic devices)
  - XR Plugin Management (Manages device setup)
  - OpenXR Plugin (Required for Meta Quest/OpenXR standard)

### Phase 3: Project Configuration (Multi-Platform Settings)
- **Open Settings**: Go to Edit > Project Settings
- **XR Plug-in Management**:
  - iOS Tab (iPhone icon): Check ARKit and PolySpatial (if targeting Vision Pro)
  - Android Tab (Android icon): Check ARCore and OpenXR
- **Player Settings (General)**:
  - iOS Tab: Set Bundle Identifier and Camera Usage Description. Ensure ARM64 is selected for Architecture
  - Android Tab: Set Bundle Identifier. Set Minimum API Level to at least 24 (Android 7.0) for ARCore support

### Phase 4: Scene Preparation (AR Session)
- **Delete Main Camera**: Delete the default Main Camera GameObject from the Hierarchy
- **Add AR Session Origin**: Right-click in the Hierarchy > XR > AR Session Origin
  - This acts as the origin point for virtual content, compatible with all providers
- **Add AR Session**: Right-click in the Hierarchy > XR > AR Session
  - This controls the AR experience lifecycle across all platforms

### Next Steps
Your unified scene is now set up. The primary development challenge is integrating the MediaPipe/TFLite vision pipeline to extract the pose data, which is done independently of the AR provider, allowing your C# code to work universally.

**Recommended Tutorial**: [Beginners guide to UNITY AR Foundation (ARKit & ARCore) - Build your first AR app from scratch!](tutorial-link)

### UI Reference
*"Scouter its NINETY THOUSAND!!!"*



## IV. Platform-Specific Setup Guides

Setting up your development environment for XREAL (formerly Nreal) devices involves two distinct processes: one for web-based development using the WebSpatial SDK and one for native development using the XREAL SDK for Unity.

### 1. WebSpatial SDK Setup
The WebSpatial SDK is designed for creating web projects with spatial capabilities, often used for deployment on web-enabled spatial computing platforms.

#### Prerequisites
- Node.js installed on your system
- A modern web project framework (e.g., React, Vite, TypeScript)

#### Setup Steps (Using a React/Vite Example)

**Create a Web Project:**
Use a package manager to initialize a new project, such as a React/Vite/TypeScript application:

```bash
npx create-vite --template react-ts
# or
pnpm dlx create-vite --template react-ts
```

**Install the WebSpatial SDK:**
Install the necessary core and framework-specific packages, along with build tools and dependencies like three and @google/model-viewer:

```bash
npm install --save @webspatial/react-sdk @webspatial/core-sdk @google/model-viewer three
npm install --save-dev @webspatial/builder @webspatial/platform-visionos @webspatial/vite-plugin vite-plugin-html
```

**Integrate into the Build Tool:**
- **Configure the Compiler**: Modify your `tsconfig.*.json` file to configure JSX compilation for the SDK:

```json
{ "compilerOptions": { "jsxImportSource": "@webspatial/react-sdk" } }
```

- **Add the Vite Plugin**: Integrate the WebSpatial Vite plugin into your `vite.config.ts` or equivalent build file

**Run for WebSpatial:**
Execute a dedicated command to generate web code optimized for the WebSpatial environment:

```bash
XR_ENV=avp npm run dev
```




### 2. Unity XR/VR Support (XREAL SDK 3.0)
The XREAL SDK 3.0 is the latest development framework for creating AR applications in Unity, leveraging the engine's standard XR Interaction Toolkit (XRI) and AR Foundation packages for a unified development experience.

#### Prerequisites
- **Unity**: Recommended LTS versions (e.g., 2021.3.x LTS or 2022.3.x LTS) with Android Build Support enabled
- **Android SDK**: API Level 29 (Android 10.0) or later
- **XREAL SDK**: Unity package (e.g., `com.xreal.xr.tar.gz`), available from the XREAL developer portal

#### Setup Steps

**Create and Configure Unity Project:**
- Open Unity Hub and create a new 3D project
- Go to File > Build Settings and Switch Platform to Android

**Import Unity XR Packages:**
- Open the Package Manager (Window > Package Manager)
- Install the XR Interaction Toolkit from the Unity Registry. Import the Starter Assets from the Samples tab
- Optionally, install AR Foundation if your application requires AR features like Plane Detection, Image Tracking, or Spatial Anchors

**Import XREAL SDK:**
- In the Package Manager, click the + button and select "Add package from tarball..."
- Select the downloaded XREAL SDK tarball file (e.g., `com.xreal.xr.tar.gz`)
- **Note**: The SDK includes modular components like Interaction Basics (essential) and AR Features (optional)

**Enable XREAL XR Plug-in:**
- Go to Edit > Project Settings > XR Plug-in Management
- Under the Android tab, check the box next to the XREAL plug-in to activate support for the glasses

**Configure Player Settings:**
- Go to File > Build Settings > Player Settings and set the following under the Android tab:
  - **Resolution and Presentation > Default Orientation**: Portrait
  - **Other Settings > Auto Graphics API**: false
  - **Other Settings > Graphics APIs**: OpenGL ES3
  - **Other Settings > Minimum API Level**: Android 10.0 or higher
  - **Other Settings > Target API Level**: Automatic (highest installed)

**Deploy the App:**
- Build the Android APK
- Connect your supported device (e.g., Android phone or XREAL Beam Pro) to your computer
- Install the app using Android Debug Bridge (adb) and then connect the device to your XREAL glasses to run the application




## V. SecureMR Resources

### SecureMR Overview
▶️ **Why SecureMR?**

SecureMR is PICO's privacy-first on-device Mixed Reality AI engine, letting you run computer-vision pipelines (object detection, pose, glTF rendering, depth processing) without exposing raw camera frames to your Unity app. It's secure, fast, and optimized for running ML models fully on the headset.

🔗 **Learn more:** [Introducing SecureMR](https://developer.picoxr.com/news/introducing-securemr/)

▶️ **🧠 Understand Core Concepts of SecureMR**  
🔗 **SecureMR Key Concepts (Official Docs):** [SecureMR Key Concepts](https://developer.picoxr.com/document/unity/securemr-key-concepts/)

### SecureMR Development Setup

🎯 **Overview**  
This guide walks you through how to install and configure everything you need to run the SecureMR Unity Sample Projects — including UFO, MNIST, and others — on your PICO headset. By the end, you'll be able to build and deploy SecureMR apps on your PICO device.

🧰 **Prerequisites**
- 🧠 Basic Unity familiarity
- 💻 Unity Hub installed
- 🥽 PICO 4 / PICO 4 Ultra headset
- 🔌 USB cable
- ⚙️ Developer Mode enabled on headset

🔗 **Download Links**

| Tool | Link | Description |
|------|------|-------------|
| 🧩 Unity Hub | [https://unity.com/download](https://unity.com/download) | Manage Unity versions & projects |
| 💾 SecureMR Unity Sample | [https://github.com/Pico-Developer/SecureMR-Unity-Sample](https://github.com/Pico-Developer/SecureMR-Unity-Sample) | Open-source demo repository |
| 🧱 PICO Unity Integration SDK | [https://developer.picoxr.com/resources/#pdc](https://developer.picoxr.com/resources/#pdc) | XR & SecureMR plugin for Unity |
| 🧰 PICO Developer Center | [https://developer.picoxr.com/resources/#pdc](https://developer.picoxr.com/resources/#pdc) | Manage headset connection, restart, & logs |


#### Step-by-Step Setup Guide

**1. Install Unity Hub:**
- Go to [https://unity.com/download](https://unity.com/download)
- Download & install Unity Hub
- Unity Hub helps manage Unity versions and sample projects
- We'll use it to open the SecureMR sample repo

**2. Get the SecureMR Sample Project:**
- Visit [https://github.com/Pico-Developer/SecureMR-Unity-Sample](https://github.com/Pico-Developer/SecureMR-Unity-Sample)
- Click Code → Download ZIP or clone via Git
- In Unity Hub → click "Add project from disk" → select your cloned folder

**3. Install Correct Unity Version:**
- When you open the project, Unity Hub may ask for a matching editor version
- Install Unity 6000.0.39f1 (listed in the repo README)
- This version is officially tested for SecureMR

**4. Download PICO SDK:**
- Go to [https://developer.picoxr.com/resources/#pdc](https://developer.picoxr.com/resources/#pdc)
- Download:
  - PICO Unity Integration SDK
  - PICO Developer Center (PDC)
- 🧠 **PDC Tip**: lets you check device status, restart headset, and view logs easily

**5. Install PICO SDK in Unity:**
- Unzip the SDK → open the folder
- Find package.json
- In Unity → Window → Package Manager → "+" → "Add package from disk"
- Select package.json → click Apply
- ✅ This installs all PICO XR & SecureMR components

**6. Configure XR Plug-ins:**
- Go to Edit → Project Settings → XR Plugin Management
- Enable ✅ PICO XR
- Add OpenXR manually:
  - Window → Package Manager → "+" → "Add package by name"
  - Enter `com.unity.xr.openxr`
- Confirm both PICO XR and OpenXR appear in XR Plugin Management

**7. Configure Player Settings:**

| Setting | Value |
|---------|-------|
| Company / Product Name | Any |
| Minimum API Level | Android 10 (API 29) |
| Target API Level | Automatic |
| Scripting Backend | IL2CPP |
| Target Architectures | ARM64 only |

⚙️ These ensure compatibility with the PICO headset.

**8. Set Up App ID:**
- Go to [https://developer.picoxr.com/#/](https://developer.picoxr.com/#/) and log in
- Navigate: My Ticket Console → My Apps → API Test
- Copy your App ID
- Back in Unity → PICO → Platform Settings → paste your App ID

**9. Build and Deploy:**
- In Unity → open Assets/Samples/Scenes/UFO Scene
- In Inspector, ensure:
  - ✅ SecureMR enabled
  - ✅ Video SeeThrough enabled (under PXR_Manager)
- Go to File → Build Settings:
  - Add the UFO Scene
  - Platform → Android
  - Connect headset & check PDC app — ensure connected
  - In Unity → Run Device → select headset → Build and Run
- 🚀 The app will build and launch automatically on your headset!

#### Troubleshooting Tips
- Make sure your headset is in Developer Mode
- Always use IL2CPP + ARM64 for builds
- Restart device via PICO Developer Center if it's not detected
- Verify you're on Unity 6000.0.39f1
- If the scene is black:
  - Check SecureMR & Video SeeThrough boxes
  - Ensure you're running on the device (not in editor)

#### Android Logcat Integration
The Android Logcat package lets you view real-time logs from your connected PICO headset directly inside Unity — no need to open ADB manually. This is extremely useful for debugging SecureMR pipelines, tensor mapping errors, and operator exceptions.

**How to install:**
- In Unity Editor, go to Window → Package Manager
- Click the + icon → "Add package by name"
- Enter: `com.unity.mobile.android-logcat`
- Press Add
- Once installed, open it anytime from: Window → Analysis → Android Logcat

🔗 **Docs:** [Android Logcat Package Documentation](https://docs.unity3d.com/Packages/com.unity.mobile.android-logcat@1.4/manual/index.html)

### SecureMR Learning Resources

▶️ **🛠️ Building with SecureMR**
1. [SecureMR Basics Explained](https://youtu.be/-jiY08OBGSk)
2. [SecureMR + Unity: Setup Guide and Installations](https://youtu.be/_i2eBMt2Lhs)
3. [Building a Simple SecureMR App with GLTF Rendering](https://youtu.be/za8S1oOWAUg)
4. [Explaining the SecureMR UFO Demo](https://youtu.be/ClhiTVZKFK4)

▶️ **💡 Hackathon Inspiration**
- **SecureMR Use Cases:** [https://developer.picoxr.com/document/unity/securemr-use-cases/](https://developer.picoxr.com/document/unity/securemr-use-cases/)
- **SecureMR Samples:** [https://developer.picoxr.com/document/unity/securemr-samples/](https://developer.picoxr.com/document/unity/securemr-samples/)


 

