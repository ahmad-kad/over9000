# DBZ SCOUTER XR: PROJECT BLUE - Development Roadmap

## Project Overview

**Goal:** Create a compelling XR application that transforms modern AR hardware into the iconic Dragon Ball Z Scouter experience, showcasing advanced computer vision, depth sensing, and haptic technologies through a universally recognizable pop-culture lens.

**Primary Platform:** Meta Quest 3 (OpenXR)
**Secondary Platform:** Apple Vision Pro (PolySpatial)
**Target Event:** Immerse the Bay Hackathon

**Team Structure:** 4-person team with specialized workstreams (1 person lacks GPU)

---

## 👥 Team Workstreams & Task Distribution

### **Person A: GPU Specialist** 🎨 *(GPU Required)*
**Focus:** Visual Effects, Shaders, Rendering Pipeline
**Critical Tasks (Must-Have):**
- Task 3: Depth-Based Halo Shader (Foundation of visual identity)
**Medium Priority Tasks:**
- Task 10: Dynamic Background Effects
**Low Priority Tasks:**
- Task 18: Dynamic Lighting & Virtual Illumination
- Task 19: Cross-Reality Interactions & Particle Effects
- Task 20: Debug Overlay

### **Person B: AI/ML Engineer** 🧠 *(CPU-Only Compatible)*
**Focus:** Computer Vision, Algorithms, ML Integration
**Critical Tasks (Must-Have):**
- Task 1: MediaPipe Pose Detection Integration (Foundation of detection)
- Task 2: Power Level Calculation System (Core algorithm)
**Medium Priority Tasks:**
- Task 8: Hand Pointing Gesture Recognition & XR Projection
- Task 11: Multi-Target Support
- Task 12: 4D Tracking History Buffer
- Task 13: Face Recognition/Search
**Low Priority Tasks:**
- Task 16: Eye Tracking & Gaze Interaction
- Task 17: Spatial Awareness & Scene Semantics

### **Person C: XR Integration Specialist** 🔧 *(CPU-Only Compatible)*
**Focus:** Platform Abstraction, AR Foundation, Device Integration
**Critical Tasks (Must-Have):**
- Task 4: 3D Anchored HUD System (Core UI)
- Task 6: Platform Abstraction Layer (Cross-platform foundation)
**Medium Priority Tasks:**
- Task 9: Real-World Measurements
- Task 14: Custom Power Level Calibration
**Low Priority Tasks:**
- Task 21: Performance Monitoring
- Task 22: Error Handling & Graceful Degradation

### **Person D: Audio/UX Designer** 🎵 *(CPU-Only Compatible)*
**Focus:** User Experience, Audio Design, Interface Systems
**Critical Tasks (Must-Have):**
- Task 5: "It's Over 9000!" Overload Effect (Signature moment)
**Medium Priority Tasks:**
- Task 15: Voice Commands
**Low Priority Tasks:**
- Task 23: Localization Support

---

## 📋 Detailed Task Specifications

**Priority Legend:**
- 🔥 **Critical/Core:** Must-have for demo success
- 🟡 **Medium:** Elevates experience, not deal-breaker
- 🔵 **Low:** Nice-to-have, time permitting

---

### 🔥 1. MediaPipe Pose Detection Integration **[Person B]**
**Status:** Pending
**Technical Scope:** Computer vision, real-time ML inference
**Implementation:**
- Integrate MediaPipe Pose for 33-point body landmark detection
- Convert normalized coordinates to Unity world space
- Implement confidence thresholding (>0.5 for key joints)
- Cache last valid pose for frame drops
- Run inference on separate thread to avoid blocking main loop

**Acceptance Criteria:**
- Detects human within 1-5 meter range
- Updates pose data at 30+ fps
- Handles lighting variations and partial occlusions
- No noticeable lag between movement and pose updates

### 🔥 2. Power Level Calculation System **[Person B]**
**Status:** Pending
**Technical Scope:** Algorithm design, real-time computation
**Implementation:**
- **Stance Confidence (Cₛ)**: Stability, stance width, arm openness
- **Movement Spike (Sₘ)**: Joint velocity tracking with rolling history
- **Composite Score**: Weighted combination (stance × 0.4 + movement × 0.6) × 10000
- Configurable weights and thresholds via scriptable object

**Acceptance Criteria:**
- Power level ranges from 1,000-10,000
- Responsive to both static poses and dynamic movement
- Smooth transitions without jitter
- Calibrates for different body types/sizes

### 🔥 3. Depth-Based Halo Shader **[Person A]**
**Status:** Pending
**Technical Scope:** Custom shaders, depth processing, background separation
**Implementation:**
- Sample depth texture to separate foreground/background
- Edge detection using neighboring pixel depth comparison
- Colored outline (green → yellow → red based on power level)
- Background desaturation/masking
- LOD system for performance optimization

**Acceptance Criteria:**
- Clean edge detection around human subjects
- No halo artifacts on background objects
- Color changes smoothly with power level
- Works in varying lighting conditions
- Performance: <5ms render time

### 🔥 4. 3D Anchored HUD System **[Person C]**
**Status:** Pending
**Technical Scope:** World-space UI, spatial anchors, depth-aware rendering
**Implementation:**
- Canvas anchored to body keypoints (head position)
- Billboard behavior (always faces camera)
- Dynamic scaling based on distance (0.5m-1.5m perceived distance)
- Displays: Power Level, Height (meters), Distance from camera
- Color-coded feedback (blue → green → yellow → red)

**Acceptance Criteria:**
- UI stays anchored during target/camera movement
- Text remains legible at all viewing angles
- No jitter or "swimming" effects
- Scales appropriately with distance
- Updates at 30+ fps

### 🔥 5. "It's Over 9000!" Overload Effect **[Person D]**
**Status:** Pending
**Technical Scope:** Multi-sensory feedback, shader effects, audio integration
**Implementation:**
- Threshold trigger at power level > 9000
- Screen glitch shader (distortion + color aberration)
- Glass shatter sound effect
- Controller haptic feedback (escalating intensity)
- Single trigger per target (prevents spam)

**Acceptance Criteria:**
- Triggers reliably and immediately at threshold
- Visual glitch lasts 0.5-1.0 seconds
- Audio plays clearly in XR environment
- Haptic feedback provides "shock" sensation
- Resets power level display to "????"

### 🔥 6. Platform Abstraction Layer **[Person C]**
**Status:** Pending
**Technical Scope:** Cross-platform architecture, dependency injection
**Implementation:**
- Factory pattern for depth/haptic providers
- **Quest 3:** OpenXR + OVRPassthrough + Environment Depth
- **Vision Pro:** PolySpatial + ARKit LiDAR
- **Fallback:** Edge-detection pseudo-depth
- Interface-based design for easy testing/switching

**Acceptance Criteria:**
- Same codebase works across all target platforms
- Automatic provider detection and initialization
- Graceful fallback when hardware features unavailable
- No platform-specific code in core business logic

---

## Nice-to-Have Features (Medium Priority - Enhancement)

These features elevate the experience but aren't critical for the core demo. Implement after core features are solid.

### 🟡 8. Hand Pointing Gesture Recognition & XR Projection **[Person B]**
**Status:** ✅ Documented & Implemented
**Technical Scope:** Real-time hand landmark tracking, gesture classification, spatial raycasting, XR interaction
**Implementation:**
- ✅ MediaPipe hand landmark detection (21-point tracking)
- ✅ Pointing gesture classification (index finger extended, others curled)
- ✅ XR raycasting from hand position to detect objects/planes
- ✅ Temporal power level scanning with audio ramp-up (3-second duration)
- ✅ Laser pointer visualization during pointing
- ✅ Spatial UI anchoring at raycast hit points
- ✅ Audio integration with scouter beeps and overload effects
- ✅ XREAL AR glasses integration for passthrough and spatial audio

**Files Created:**
- `HandPointingRecognizer.cs` - Gesture detection logic
- `PowerLevelScanner.cs` - Scanning system with temporal effects
- `XRScouterManager.cs` - Integration with AR Foundation and XREAL
- Updated PDD and SDD with complete implementation details

**Acceptance Criteria Met:**
- ✅ Detects pointing gesture with proper finger pose validation
- ✅ Projects ray from hand to objects with AR raycasting
- ✅ Displays scanning progress with exponential power ramp-up
- ✅ Plays scouter audio effects with pitch modulation
- ✅ Triggers "IT'S OVER 9000!" sequence at threshold
- ✅ Works in XREAL AR environment with spatial positioning

### 🟡 9. Real-World Measurements **[Person C]**
**Status:** Pending
**Technical Scope:** Spatial mathematics, keypoint calculations
**Implementation:**
- Height: Distance between head/foot keypoints
- Arm span: Distance between left/right wrist keypoints
- Distance: Camera-to-target distance calculation
- Unit conversion and display formatting

### 🟡 10. Dynamic Background Effects **[Person A]**
**Status:** Pending
**Technical Scope:** Advanced shader effects, post-processing
**Implementation:**
- Background pulsing synchronized with power level changes
- Color shifting based on power level intensity
- Subtle distortion effects during high power spikes
- Performance-optimized (LOD-based rendering)

### 🟡 11. Multi-Target Support **[Person B]**
**Status:** Pending
**Technical Scope:** Concurrent pose tracking, UI management
**Implementation:**
- Track 2-3 targets simultaneously
- Separate HUD instances per target
- Priority system for closest/most confident targets
- Performance monitoring to prevent frame drops

### 🟡 12. 4D Tracking History Buffer **[Person B]**
**Status:** Pending
**Technical Scope:** Temporal data structures, memory management
**Implementation:**
- 5-second rolling buffer of power levels and positions
- "Last Known Power" flash for reacquired targets
- Temporal visualization options (debug mode)
- Memory-efficient circular buffer implementation

### 🟡 13. Face Recognition/Search **[Person B]**
**Status:** Pending
**Technical Scope:** Face landmarks, privacy-conscious design
**Implementation:**
- MediaPipe Face detection integration
- Explicit user consent requirement
- Local-only processing (no server calls)
- Optional name matching from local database

### 🟡 14. Custom Power Level Calibration **[Person C]**
**Status:** Pending
**Technical Scope:** Settings UI, runtime configuration
**Implementation:**
- Sliders for stance/movement weight adjustment
- Preset profiles for different activity types
- Real-time calibration feedback
- Save/load calibration settings

### 🟡 15. Voice Commands **[Person D]**
**Status:** Pending
**Technical Scope:** Voice recognition, text-to-speech
**Implementation:**
- "What's the power level?" trigger phrase
- Audio readout of current target power level
- Character voice synthesis (optional DBZ-style)
- Noise filtering for demo environments

### 🔵 16. Eye Tracking & Gaze Interaction **[Person B]**
**Status:** Pending
**Technical Scope:** Eye tracking, gaze-based UI, foveated rendering, hands-free interaction
**Implementation:**
- Platform eye tracking API integration (Vision Pro, XREAL)
- Gaze cursor for target selection without hand input
- Foveated rendering for performance optimization
- Calibration sequence for accurate tracking
- Fallback to hand tracking if eye tracking unavailable

### 🔵 17. Spatial Awareness & Scene Semantics **[Person B]**
**Status:** Pending
**Technical Scope:** Scene classification, environmental understanding, contextual AR
**Implementation:**
- Platform scene API integration (Quest 3 Scene API, Vision Pro)
- Room layout detection (indoor/outdoor, surface types)
- Environmental audio cues based on acoustics
- Contextual power level adjustments
- Graceful fallback to basic plane detection

### 🔵 18. Dynamic Lighting & Virtual Illumination **[Person A]**
**Status:** Pending
**Technical Scope:** AR lighting estimation, virtual shadows, photometric rendering, real-time illumination
**Implementation:**
- AR lighting estimation integration
- Virtual aura lights around high-power targets
- Color-coded shadows (blue→green→red progression)
- Photometric interaction with real-world lighting
- Performance optimization for mobile XR devices

### 🔵 19. Cross-Reality Interactions & Particle Effects **[Person A]**
**Status:** Pending
**Technical Scope:** Digital-physical object interaction, physics bridging, volumetric effects, particle systems in AR space
**Implementation:**
- GPU-accelerated particle systems for energy auras
- Shockwave effects during overload moments
- Debris physics for destruction sequences
- Cross-reality object collisions and interactions
- Performance toggle for battery-constrained devices

### 🔵 20. Debug Overlay **[Person A]**
**Status:** Pending
**Technical Scope:** Development tools, runtime visualization
**Implementation:**
- 3D keypoint visualization (spheres at landmark positions)
- Skeleton bone connections
- Bounding box display
- Confidence value overlays
- Performance metrics panel

### 🔵 21. Performance Monitoring **[Person C]**
**Status:** Pending
**Technical Scope:** Profiling, optimization
**Implementation:**
- Frame time tracking (target: 60fps sustained)
- MediaPipe inference timing (<12ms per frame)
- Memory usage monitoring
- GPU/CPU utilization graphs
- Automated performance regression detection

### 🔵 22. Error Handling & Graceful Degradation **[Person C]**
**Status:** Pending
**Technical Scope:** Exception management, user experience
**Implementation:**
- Specific exception types for different failure modes
- User-facing warning messages for recoverable errors
- Automatic fallback to cached/safe states
- Connection status indicators for external hardware

### 🔵 23. Localization Support **[Person D]**
**Status:** Pending
**Technical Scope:** Internationalization, string management
**Implementation:**
- English/Japanese language packs for DBZ theme
- Dynamic string loading from JSON
- RTL language support preparation
- Voice synthesis language switching

---

## Development Timeline

### Week 1: Core MVP (Quest 3 Focus)
- **Mon-Tue:** MediaPipe integration + basic power calculation
- **Wed-Thu:** Depth halo shader + HUD anchoring
- **Fri:** Overload effect + controller haptics
- **Weekend:** Testing + platform abstraction layer

### Week 2: Expansion + Polish
- **Mon-Tue:** Vision Pro build attempt + gesture recognition
- **Wed:** Nice-to-have features (measurements, background effects)
- **Thu:** Debug tools + performance monitoring
- **Fri:** Error handling + demo rehearsal

### Week 3: Final Polish + Backup Plans
- **Mon-Wed:** Bug fixes, performance tuning, localization
- **Thu:** Dry run with external testers
- **Fri:** Hackathon Day - setup + live demos

---

## Success Metrics

### Technical Excellence
- ✅ **60fps sustained** performance
- ✅ **<50ms latency** from movement to power level update
- ✅ **95%+ demo success rate** (no crashes)
- ✅ **Cross-platform compatibility** (2+ platforms working)

### User Experience
- ✅ **Instant recognition** ("That's a Scouter!")
- ✅ **Wow factor** (audible reactions from judges/audience)
- ✅ **Hardware showcase** (5+ XR features demonstrated)
- ✅ **Novel integration** (multi-sensory XR feedback)

### Project Quality
- ✅ **Clean architecture** (separation of concerns, interfaces)
- ✅ **Comprehensive testing** (unit + integration coverage)
- ✅ **Documentation** (inline comments, API references)
- ✅ **Error resilience** (graceful degradation)

---

## Implementation Guidelines

### Code Quality Standards
- SOLID principles application
- Interface-based design for testability
- Comprehensive error handling with specific exceptions
- Structured logging with correlation IDs
- Performance-conscious algorithms (Big O awareness)

### Testing Strategy
- Unit tests for core business logic (90%+ coverage)
- Integration tests for component interactions
- Performance tests with automated thresholds
- Cross-platform compatibility testing

### Risk Mitigation
- **Hardware Dependencies:** Build software fallbacks early
- **Performance Issues:** Profile continuously, optimize MediaPipe usage
- **Demo Environment:** Test in bright/dark conditions, noisy spaces
- **Platform Differences:** Abstract platform-specific code behind interfaces

---

## Related Documentation

- **[PDD (Product Design Document)](pdd.md)** - Feature priorities and success criteria
- **[SDD (Software Design Document)](sdd.md)** - Technical architecture and implementation details
- **[Setup Guide](setup.md)** - Development environment configuration
- **[Intro](intro.md)** - Project overview and technical background

---

## Contact & Resources

**Development Team:** Ahmad Kaddoura
**Repository:** [GitHub Link]
**Documentation:** [Notion/Confluence Link]
**Support:** Discord server invite

---

*Last Updated: November 15, 2025*
*Next Review: Post-Implementation Retrospective*
