# DBZ SCOUTER XR: PROJECT BLUE
## Product Design Document (PDD)

**Version:** 1.0
**Date:** November 14, 2025
**Target Event:** Immerse the Bay Hackathon
**Primary Goal:** Hardware Feature Showcase + Internship/Prize Win

---

## Executive Summary

DBZ Scouter XR is an AR application that transforms XR hardware into an iconic Dragon Ball Z Scouter. The project prioritizes visual impact and hardware feature demonstration over deep functionality, positioning it as a compelling tech demo that showcases platform capabilities through a universally recognizable pop-culture lens.

**Core Value Proposition:** "Experience the Scouter IRL - A showcase of what XR hardware can do today"

---

## Project Objectives

### Primary Objectives (Must Achieve)
1. Create memorable, instantly recognizable demo experience
2. Showcase 3+ advanced XR hardware features
3. Demonstrate cross-platform development capability
4. Achieve smooth 60fps performance on target hardware

### Success Metrics
- **Demo Success Rate:** 95%+ successful runs without crashes
- **Wow Factor Score:** Judges/viewers audibly react ("That's cool!")
- **Technical Depth:** Demonstrate minimum 5 distinct CV/XR features
- **Platform Coverage:** Working build on 2+ platforms

---

## Feature Priority Matrix

### CORE FEATURES (High Priority - Must Have)
*These define the project. Without them, demo fails.*

#### 1. Real-Time Pose Detection & Power Level
- **Technical Showcase:** Computer vision, real-time ML inference
- **Implementation:** MediaPipe Pose → 33-point skeleton
- **User Experience:** Point at person → see power level instantly
- **Demo Time:** 45 seconds

**Acceptance Criteria:**
- Detects human within 1-5 meters
- Updates power level at 30+ fps
- Displays numeric value 1,000-10,000
- No noticeable lag between movement and response

#### 2. Depth-Based Halo/Outline Effect
- **Technical Showcase:** Depth sensing, custom shaders, background separation
- **Implementation:** LiDAR/Depth API → shader → colored outline around person
- **User Experience:** Iconic "Scouter vision" - person highlighted, background desaturated
- **Demo Time:** Continuous (always visible)

**Acceptance Criteria:**
- Clean edge detection around human subject
- Halo color changes with power level (blue → green → red)
- Background visibly separated (monochrome or blurred)
- Works in varying lighting conditions

#### 3. 3D Anchored HUD
- **Technical Showcase:** World-space UI, spatial anchors, depth-aware rendering
- **Implementation:** Canvas anchored to body keypoint, maintains fixed distance
- **User Experience:** Info panel "floats" above target's head, readable from any angle
- **Demo Time:** 30 seconds

**Acceptance Criteria:**
- UI stays anchored as user/target moves
- Maintains legible size (0.5m perceived distance)
- Displays: Power Level, Height, Distance from user
- No jitter or "swimming" effect

#### 4. "It's Over 9000!" Overload Effect
- **Technical Showcase:** Multi-sensory feedback
- **Implementation:** Threshold trigger → screen glitch + sound + haptic
- **User Experience:** When target reaches 9000+, Scouter "breaks" with shock
- **Demo Time:** 15 seconds (climax moment)

**Acceptance Criteria:**
- Triggers reliably at power level > 9000
- Screen glitch effect (shader distortion)
- Glass shatter sound effect
- Controller haptic feedback
- Single trigger per target (no spam)

---

### NICE-TO-HAVE FEATURES (Medium Priority)
*These elevate the demo but aren't deal-breakers.*

#### 5. Hand Pointing Gesture Recognition & XR Projection
- **Technical Showcase:** Real-time hand landmark tracking, gesture classification, spatial raycasting, XR interaction
- **Implementation:** MediaPipe hand landmarks → pointing pose detection → AR raycasting → power level calculation
- **User Experience:** Point with index finger to scan targets and reveal power levels with temporal ramp-up
- **Effort:** Medium (gesture recognition + XR integration)

**Acceptance Criteria:**
- Detects pointing gesture with index finger extended, other fingers curled
- Projects ray from hand position in pointing direction to detect objects/planes
- Displays scanning progress with temporal power level ramp-up (3-second scan)
- Plays scouter beep sounds that increase in frequency during scan
- Triggers "IT'S OVER 9000!" overload effect when threshold reached
- Works in XREAL AR environment with spatial audio positioning

**Why Nice-to-Have:**
- Demonstrates advanced XR interaction beyond gaze/head tracking
- Natural "Scouter IRL" interaction method
- Can fallback to controller input if hand tracking fails
- Elevates demo from passive viewing to active participation

**Implementation Architecture:**

**Hand Landmark Structure (21 points):**
- Landmark 0: Wrist (reference point)
- Landmark 8: Index finger tip (pointing direction)
- Landmarks 5-7: Index finger joints (extension validation)
- Landmarks 9,13,17: Other finger MCP joints (curl validation)

**Gesture Detection Logic:**
```csharp
// Pointing gesture requires:
// 1. Index finger extended (tip furthest from wrist)
// 2. Other fingers curled (tips closer to wrist than MCP joints)
// 3. Hand oriented palm-forward
```

**XR Projection System:**
- Converts hand landmarks to world space coordinates
- Performs AR raycasting from hand position in pointing direction
- Projects onto detected planes/feature points
- Anchors scanning UI at intersection point

**Temporal Scanning Effects:**
- 3-second scanning duration with exponential power ramp-up
- Audio pitch increases during scan (scouter beeps get faster)
- Particle effects and visual feedback
- Triggers overload sequence at >9000 threshold

**Audio Integration:**
- `db-scouter-made-with-Voicemod.mp3` for scanning beeps
- `kamehameha-wave-sound-effect.mp3` for power ramp-up
- `9000 - AUDIO FROM JAYUZUMI.COM.mp3` for overload trigger
- Spatial audio positioning in XR space

**XR Hardware Showcase:**
- Hand landmark tracking via MediaPipe
- AR Foundation raycasting and plane detection
- XREAL passthrough camera integration
- Spatial audio and UI anchoring
- Controller haptic feedback for overload effects

#### 6. Real-World Measurements (Height/Arm Span)
- **Technical Showcase:** Spatial measurement, keypoint math
- **Implementation:** Calculate distance between head/feet keypoints
- **User Experience:** See actual height in meters
- **Effort:** Low (distance calculation)

**Why Nice-to-Have:**
- Adds "real app" credibility
- Simple math once pose works
- Easy to demo accuracy

#### 7. Dynamic Background Effects
- **Technical Showcase:** Advanced shader effects
- **Implementation:** Passthrough video manipulation
- **User Experience:** Background pulses/warps with power level changes
- **Effort:** Medium (shader complexity)

**Why Nice-to-Have:**
- Enhances immersion
- Not essential to core concept
- May cause performance issues

#### 8. Multi-Target Support
- **Technical Showcase:** Concurrent pose tracking
- **Implementation:** Track 2-3 people simultaneously
- **User Experience:** Scan crowd, see multiple power levels
- **Effort:** High (performance + UI complexity)

**Why Nice-to-Have:**
- Impressive technically
- Adds demo variety
- Risky for performance budget

---

### OPTIONAL FEATURES (Low Priority - Polish)
*Time permitting. Mention in pitch even if not built.*

#### 9. 4D Tracking History
- **Technical Showcase:** Temporal data structures
- **Implementation:** 5-second rolling buffer of power levels
- **User Experience:** Display "Last Known Power" when target re-acquired
- **Effort:** Medium

**Why Optional:**
- Cool for narrative ("Scouter memory")
- Not visible in 3-minute demo
- Adds code complexity

#### 10. Face Recognition/Search
- **Technical Showcase:** Face landmarks, optional ID
- **Implementation:** MediaPipe Face → name matching
- **User Experience:** "Identify" person by name
- **Effort:** High

**Why Optional:**
- Privacy concerns for hackathon
- Scope creep risk
- Requires test database

#### 11. Custom Power Level Calibration
- **Technical Showcase:** Settings UI, personalization
- **Implementation:** Sliders to adjust stance/movement weights
- **User Experience:** Tune what constitutes "high power"
- **Effort:** Low

**Why Optional:**
- Fun for extended play
- Not critical for 3-min demo
- Can be hardcoded initially

#### 12. Voice Commands
- **Technical Showcase:** Voice recognition
- **Implementation:** "What's the power level?" → audio readout
- **User Experience:** DBZ character voice responses
- **Effort:** Medium

**Why Optional:**
- Cool factor
- Unreliable in noisy demo environment
- Distracts from visual focus

#### 13. Eye Tracking & Gaze Interaction
- **Technical Showcase:** Eye tracking, gaze-based UI, foveated rendering, hands-free interaction
- **Implementation:** Platform eye tracking API → gaze cursor → target selection → foveated rendering
- **User Experience:** Look at someone to scan them - pure sci-fi interaction without hand gestures
- **Effort:** Medium (optional with fallback)

**Acceptance Criteria:**
- Gaze cursor appears at fixation point
- Target selection works without hand input
- Foveated rendering improves performance
- Falls back to hand tracking if eye tracking unavailable
- Calibration sequence for accurate tracking

**Why Optional:**
- Platform-dependent availability (Vision Pro, XREAL)
- Adds futuristic interaction method
- Complements hand tracking without replacing it
- Demonstrates advanced XR input modalities

#### 14. Spatial Awareness & Scene Semantics
- **Technical Showcase:** Scene classification, environmental understanding, contextual AR
- **Implementation:** Platform scene API → room layout detection → environmental effects → contextual power adjustments
- **User Experience:** Scouter adapts to surroundings - "Power levels higher in open spaces!"
- **Effort:** Medium (with fallback)

**Acceptance Criteria:**
- Detects indoor vs outdoor environments
- Identifies surface types and room acoustics
- Adjusts power calculations based on context
- Provides environmental audio cues
- Falls back to basic plane detection if unavailable

**Why Optional:**
- Enhances realism and immersion
- Demonstrates advanced AR understanding
- Provides natural environmental storytelling
- Works on all target platforms with graceful degradation

#### 15. Dynamic Lighting & Virtual Illumination
- **Technical Showcase:** AR lighting estimation, virtual shadows, photometric rendering, real-time illumination
- **Implementation:** Lighting estimation API → virtual aura lights → color-coded shadows → power-based illumination
- **User Experience:** Targets literally "glow" with power - visible even in low light
- **Effort:** Low (priority)

**Acceptance Criteria:**
- Virtual aura lighting around high-power targets
- Color progression: blue→green→red with power level
- Shadows cast by virtual illumination
- Photometric interaction with real lighting
- Performance optimized for mobile XR devices

**Why Optional:**
- Visually spectacular power visualization
- Works well with depth-based halo effects
- Low performance impact when implemented efficiently
- Enhances the "superhuman" visual metaphor

#### 16. Cross-Reality Interactions & Particle Effects
- **Technical Showcase:** Digital-physical object interaction, physics bridging, volumetric effects, particle systems in AR space
- **Implementation:** GPU particles → energy auras → shockwave effects → debris physics → cross-reality collisions
- **User Experience:** "Your energy field interacts with the virtual environment!" - tangible AR spectacle
- **Effort:** Low (priority)

**Acceptance Criteria:**
- Energy particle auras around targets
- Shockwave effects during overload moments
- Debris physics for destruction effects
- Cross-reality object interactions
- Performance-optimized particle systems

**Why Optional:**
- Creates visual spectacle and spectacle
- Demonstrates AR as tangible interaction
- Complements existing overload effects
- Easy to toggle on/off for performance

---

### EXCLUDED FEATURES (Out of Scope)
*Avoid these - they don't support hackathon goals.*

#### Persistent User Accounts
- **Why:** No backend needed for demo
- **Alternative:** Local session data only

#### Multiplayer/Networking
- **Why:** Adds complexity, not core to hardware showcase
- **Alternative:** Single-user experience

#### Power Level Combat/Game Mechanics
- **Why:** Turns showcase into game (scope explosion)
- **Alternative:** Pure scanner/analyzer tool

#### Content Creation Tools
- **Why:** Not aligned with "hardware showcase" goal
- **Alternative:** Fixed experience design

#### Analytics/Telemetry
- **Why:** No time for post-event analysis
- **Alternative:** Manual observation during demos

#### Tutorial System
- **Why:** Judges/users need instant gratification
- **Alternative:** Live guided demo by you

---

## Platform Priority

### Primary Platform: Meta Quest 3
- **Why:** Best depth sensor, accessible for demos, stable OpenXR
- **Features:** All Core + Most Nice-to-Have
- **Build Target:** Week 1 completion

### Secondary Platform: Apple Vision Pro
- **Why:** "Premium showcase," PolySpatial wow factor
- **Features:** Core only (depth halo + HUD)
- **Build Target:** Week 2 stretch goal

### Tertiary Platform: XREAL/Raven Glasses
- **Why:** Unique form factor, "actual Scouter" aesthetic
- **Features:** Modified (no depth effects, overlay-only UI)
- **Build Target:** Backup demo or video showcase

### Not Pursuing (Hackathon): PICO, Vive
- **Why:** Limited time, fewer units in venue
- **Alternative:** Mention in "Future Platform Support" slide

---

## Technical Architecture

### Core Stack (All Platforms)
- Unity 2022.3 LTS (C#)
- AR Foundation (unified AR API)
- MediaPipe Pose (vision processing)
- Custom Shaders (halo, glitch effects)

### Platform-Specific Layers
- Quest: OpenXR + OVRPassthrough + Quest Depth
- Vision: PolySpatial + ARKit + LiDAR
- XREAL: ARKit (iOS companion) + Optical ST

---

## User Flow (3-Minute Demo)

### Act 1: Setup (0:00-0:30)
1. Put on headset (Quest 3)
2. App auto-starts in passthrough
3. Calibration prompt (look around briefly)

### Act 2: Discovery (0:30-1:30)
1. Point at volunteer/judge
2. Halo appears around them
3. HUD shows power level ~2,000
4. Move around - UI follows target
5. Target does "power up" pose (arms raised)
6. Power level climbs to 5,000 → 7,000

### Act 3: Climax (1:30-2:00)
1. Target does dramatic movement (punch motion)
2. Power level spikes to 9,500
3. **"IT'S OVER 9000!"** screen glitch
4. Glass shatter sound
5. Controller haptic feedback
6. Power level resets to "????"

### Act 4: Explanation (2:00-3:00)
1. Remove headset
2. Show technical breakdown slide
3. Explain cross-platform architecture
4. Mention future applications (fitness, PT, sports)

---

## Success Criteria by Role

### For Judges (Technical Evaluation)
- 5+ distinct technologies integrated
- Real-time performance (60fps)
- Cross-platform architecture evident
- Novel XR interaction techniques

### For Audience (Visceral Reaction)
- Immediate recognition ("That's a Scouter!")
- Audible reaction to visual effects
- Desire to try it themselves

### For Recruiters (Internship Pipeline)
- Clean, documented codebase
- System design thinking (abstraction layers)
- Problem-solving narrative (depth API fallbacks)
- Professional presentation materials

---

## Risk Mitigation

### High-Risk Items
1. **MediaPipe Performance:** Pre-optimize, test on device early
2. **Depth API Inconsistency:** Build software fallback (edge detection)
3. **Haptic Feedback:** Controller vibration as primary feedback method
4. **Demo Environment Lighting:** Test in bright/dark conditions

### Backup Plans
- **Plan A:** Live Quest 3 demo with full haptics
- **Plan B:** Live Quest 3 demo with basic haptics
- **Plan C:** Video demo on loop, live code walkthrough

---

## Development Timeline

### Week 1: Core MVP (Quest 3)
- **Mon-Tue:** MediaPipe integration + power level math
- **Wed-Thu:** Depth halo shader + HUD anchoring
- **Fri:** Overload effect + ring integration
- **Weekend:** Testing + polish

### Week 2: Expansion + Backup
- **Mon-Tue:** Vision Pro build attempt
- **Wed:** Nice-to-have features (gesture, measurements)
- **Thu:** Presentation materials + video backup
- **Fri:** Rehearsal + contingency testing

### Week 3: Final Polish
- **Mon-Wed:** Bug fixes, performance tuning
- **Thu:** Dry run with judges/peers
- **Fri:** Hackathon Day - setup + demos

---

## Presentation Materials Needed

1. **Title Slide:** "DBZ Scouter XR: Project Blue" logo
2. **Problem Statement:** "XR hardware is powerful but demos are boring"
3. **Solution:** Live demo (2 minutes)
4. **Technical Deep Dive:** Architecture diagram (30 seconds)
5. **Applications:** Beyond entertainment (fitness, safety, sports)
6. **Platform Roadmap:** Future device support
7. **Team/Contact:** GitHub, resume links

---

## Key Metrics to Highlight

### Performance
- **Framerate:** 60fps sustained
- **Latency:** <50ms pose detection
- **Range:** 1-5 meter tracking distance

### Technical Complexity
- **5** Hardware APIs integrated
- **2+** Platforms supported
- **33** Body landmarks tracked
- **1** Multi-sensory XR feedback

### Innovation Score
- First known Scouter XR implementation
- Fingertip haptics in XR (unique)
- Real-time pose → gamified output
- Cross-platform AR Foundation showcase

---

## Final Recommendation

**Focus ruthlessly on Core Features.** A flawless demo of 4 features beats a buggy demo of 10. The multi-sensory overload effect is your differentiator - make the "Over 9000" moment unforgettable.

Build for Quest 3 first. Add Vision Pro only if Quest is stable by Day 10. Everything else is for the pitch deck, not the live demo.

**Remember:** This is a hardware showcase disguised as a meme. Judges want to see what their platforms can do. Make them feel like they're holding a real Scouter.