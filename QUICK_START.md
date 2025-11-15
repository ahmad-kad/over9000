# DBZ Scouter XR - Quick Start Guide

## 🎯 **PRIMARY PLATFORMS: XREAL + PICO4**

## 🚀 One-Click Setup (Automated)

### For macOS/Linux Users:
```bash
./setup_assets.sh
```

### For Windows Users:
```cmd
setup_assets.bat
```

**What the script does:**
- ✅ Downloads MediaPipe Unity Plugin (~50MB)
- ✅ Downloads audio effect files
- ✅ Creates complete project directory structure
- ✅ Generates Unity package manifest
- ⚠️ Creates placeholders for manual downloads

**Time saved:** ~2 hours of manual downloading and folder creation

## 🥇 **RECOMMENDED: Start with XREAL Air 2**
- Perfect "Scouter" form factor
- Hands-free XR experience
- Most authentic DBZ aesthetic

## 📋 What Gets Downloaded Automatically

### MediaPipe Unity Plugin
- **Source:** GitHub (homuler/MediaPipeUnityPlugin)
- **Location:** `Assets/ThirdParty/MediaPipe/`
- **Purpose:** Real-time pose detection with 33-point skeleton tracking

### Audio Files
- **Glass Shatter:** For "over 9000" effect
- **Electrical Overload:** For Scouter breaking animation
- **Location:** `Assets/Audio/SFX/`
- **Format:** MP3, ready for Unity import

### Project Structure
```
Assets/
├── ThirdParty/MediaPipe/     ← Downloaded
├── Shaders/GlitchShaders/    ← Placeholder
├── Audio/SFX/                ← Downloaded
└── StreamingAssets/Models/   ← Instructions
```

## 🛠️ Manual Downloads Required

### 1. Unity Asset Store (Free)
**Open Unity → Window → Package Manager → My Assets**

- **Oculus Integration** (Required for Quest)
  - URL: https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022
  - Purpose: Quest passthrough, depth sensing, haptic feedback

- **Screen Glitch Shader Pack** (Required for overload effect)
  - URL: https://assetstore.unity.com/packages/vfx/shaders/screen-glitch-effect-184112
  - Purpose: Visual "Scouter breaking" effect

### 2. ML Model Files (Required for pose detection)
**Download from Google:**
- URL: https://developers.google.com/mediapipe/solutions/vision/pose_landmarker
- Files: `pose_landmarker.tflite`, `pose_landmarker.task`
- Place in: `Assets/StreamingAssets/Models/`

## ⚙️ Next Steps After Setup

### 1. Complete Unity Setup
Follow `setup.md` for:
- Unity Hub installation
- Package Manager setup
- Platform-specific configuration
- Build settings

### 2. Build MediaPipe Libraries
```bash
cd Assets/ThirdParty/MediaPipe
# Follow README.md for your platform
# Windows: Open .sln in Visual Studio, build
# macOS: Open .xcodeproj in Xcode, build
# Linux: Use provided build scripts
```

### 3. First Test Build
- Connect Quest 3 or Vision Pro
- File → Build Settings → Build and Run
- Should show passthrough with basic AR setup

### 4. Implement Core Features
Follow `SDD.md` to implement:
- ScouterController.cs
- Pose estimation pipeline
- Depth-based halo shader
- Power level calculation

## 🔧 Troubleshooting

### Script Won't Run
**macOS/Linux:**
```bash
chmod +x setup_assets.sh
./setup_assets.sh
```

**Windows:**
- Ensure you're running as Administrator
- Or right-click → "Run as administrator"

### Downloads Fail
- Check internet connection
- Some audio downloads may fail due to URL changes
- Manual alternatives provided in README files

### Unity Package Errors
- Ensure Unity 2022.3 LTS
- Check Package Manager for conflicts
- Restart Unity after asset imports

## 📚 Documentation Reference

- **[setup.md](setup.md)** - Complete development environment setup
- **[SDD.md](SDD.md)** - Software design and implementation details
- **[PDD.md](PDD.md)** - Product requirements and feature priorities
- **[intro.md](intro.md)** - Project overview and technical breakdown

## 🎯 Success Checklist

After setup completion:
- [ ] Script ran successfully
- [ ] All directories created
- [ ] MediaPipe plugin downloaded
- [ ] Unity project opens without errors
- [ ] Asset Store packages imported
- [ ] ML models placed in StreamingAssets
- [ ] First build succeeds on device

**Ready to build the Scouter!** 🥷⚡

---
*Last updated: November 15, 2025*
