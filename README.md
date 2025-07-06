# PunchPulse Accessible VR Boxing Game

![A boxing glove with motion lines and the word "PunchPulse”" across the glove](./PunchPulse_logo.JPEG)

A cross-platform accessible virtual reality boxing application designed to provide inclusive physical activity experiences for blind and low vision users. This project implements a research-informed approach to VR accessibility, leveraging spatial audio, haptic feedback systems, and adaptive interaction paradigms.

**Developed by:** xAbility Design Lab, University of Illinois Urbana-Champaign

## Abstract

This repository contains the complete implementation of an accessible VR boxing game that eliminates visual dependencies through systematic application of non-visual interaction modalities. The system architecture emphasizes modular design patterns, allowing for extensible accessibility features and cross-platform deployment across OpenXR-compatible VR devices.

## Technical Overview

### Core Technologies

- **Unity Engine**: 2022.3 LTS with Universal Render Pipeline
- **XR Framework**: OpenXR 1.0+ with provider-agnostic implementation
- **Audio Engine**: Unity's 3D Audio system with custom spatial processing
- **Haptic System**: Native XR controller haptics with extensible feedback patterns
- **Build Targets**: Meta Quest 2/3, PC VR (SteamVR), Android VR

### System Requirements

#### Development Environment

```
Unity Editor: 2022.3.x LTS or newer
.NET Framework: 4.8+
OpenXR Plugin: 1.4.0+
XR Interaction Toolkit: 2.3.0+
TextMeshPro: 3.0.6+
```

#### Runtime Dependencies

```
OpenXR Runtime: 1.0+
Minimum RAM: 4GB
Storage: 2GB available space
VR Headset: OpenXR compatible device
```

## Project Architecture

### Core Module System

The application follows a modular architecture with clear separation of concerns:

```
Assets/Scripts/
├── AttackLogic/          # Combat mechanics and physics
├── BackgroundAudio/      # Ambient audio and spatial soundscapes
├── Gameplay/            # Game state management and progression
├── Menu/                # UI accessibility and navigation
├── PlayerCues/          # Non-visual feedback systems
└── Tutorial/            # Guided onboarding sequences
```

### Key Components

#### 1. Game Module Manager (`GameModuleManager.cs`)

Singleton pattern implementation managing global game state with three distinct modes:

- **LevelProgression**: Adaptive difficulty scaling
- **Manual**: User-controlled parameter adjustment
- **HardSurvival**: Fixed high-difficulty challenges

#### 2. Accessibility Systems

**Spatial Audio Engine**

- 3D positional audio with HRTF processing
- Dynamic range compression for hearing aid compatibility
- Directional audio cues with configurable intensity

**Haptic Feedback Framework**

- Pattern-based vibration sequences
- Velocity-sensitive impact feedback
- Contextual haptic notifications

**Non-Visual Navigation**

- Audio-first menu systems
- Spatial UI with consistent positioning
- Voice-guided interactions

#### 3. Combat System

**Physics-Based Interaction**

- Velocity estimation for punch detection (`VelocityEstimator.cs`)
- Collision-based impact calculation
- Realistic boxing mechanics with accessibility adaptations

**Enemy AI System**

- Predictable attack patterns for accessibility
- Audio-telegraphed movements
- Adjustable reaction times

### Scene Architecture

#### Primary Scenes

- `BoxingRing.unity`: Main gameplay environment
- `TutorialScene.unity`: Onboarding and training module

#### Asset Organization

```
Assets/
├── Animations/          # Character and UI animations
├── Materials/           # PBR materials and shaders
├── Models/             # 3D assets and meshes
├── Prefabs/            # Reusable game objects
├── Sounds/             # Audio assets and spatial clips
└── Settings/           # Configuration profiles
```

## Development Setup

### Environment Configuration

1. **Clone Repository**

```bash
git clone https://github.com/xability/punch-pulse.git
cd punch-pulse
```

2. **Unity Project Setup**

```bash
# Open Unity Hub
# Add project from disk
# Select Unity 2022.3.x LTS
```

3. **Package Dependencies**
   Ensure the following packages are installed via Package Manager:

- OpenXR Plugin
- XR Interaction Toolkit
- XR Plugin Management
- TextMeshPro

4. **Build Configuration**

```
Platform: Android (Meta Quest) or PC (Windows/Linux)
Rendering: Universal Render Pipeline
Color Space: Linear
Graphics API: Vulkan (Android) / DirectX 11 (PC)
```

### Testing Framework

#### Unit Testing

- Unity Test Framework integration
- Component-level test coverage
- Accessibility feature validation

#### VR Testing Protocol

1. **Headset Verification**: Multi-device compatibility testing
2. **Accessibility Validation**: Screen reader simulation
3. **Performance Profiling**: Frame rate and latency analysis
4. **User Experience Testing**: Blind and low vision user feedback

## API Reference

### Core Interfaces

#### IAccessibleComponent

```csharp
public interface IAccessibleComponent
{
    void OnAccessibilityModeChanged(AccessibilityMode mode);
    string GetAccessibilityDescription();
    void TriggerAccessibilityFeedback();
}
```

#### IDifficultyScalable

```csharp
public interface IDifficultyScalable
{
    void SetDifficultyLevel(float level);
    float GetCurrentDifficulty();
    void ResetToDefault();
}
```

### Event System

The application implements a centralized event system for loose coupling:

```csharp
// Combat Events
public static event System.Action<float> OnPunchLanded;
public static event System.Action<Vector3> OnEnemyAttack;

// Accessibility Events
public static event System.Action<string> OnAudioCueTriggered;
public static event System.Action<HapticPattern> OnHapticFeedback;
```

## Contributing

We welcome contributions to improve the game's accessibility and features. Please see our [Contributing Guidelines](CONTRIBUTING.md) for detailed information on:

- Development workflow and branch structure
- Code standards and conventions
- Pull request process
- Issue reporting templates
- Testing requirements

## Deployment

### Build Pipeline

#### Meta Quest Deployment

Those wishing to deploy the game on Meta Quest devices should follow these steps:

- navigate to the Unity Build Settings and configure the project for Android with OpenXR support.
- ensure the OpenXR Feature Groups include Meta Quest Support.
  OR
- run test1.apk on a connected device via Meta Quest developer mode.

```bash
# Configure build settings
Unity Build Settings > Android
XR Plug-in Management > OpenXR
OpenXR Feature Groups > Meta Quest Support

# Generate APK
Build and Run > Deploy to connected device
```

#### PC VR Deployment

```bash
# Windows build
Unity Build Settings > PC, Mac & Linux Standalone
Target Platform: Windows x86_64
XR Plug-in Management > OpenXR

# Build executable
Build > Generate standalone application
```

### Distribution

- **Development Builds**: Internal testing and iteration
- **Release Builds**: Stable versions for end users
- **Community Builds**: Open source accessibility demonstrations

## License

MIT License

This project encourages adaptation and extension for accessibility research and inclusive VR development.

## Contact

**Technical Issues**: Create a GitHub issue with detailed reproduction steps

**General Questions**: Contact xAbility Design Lab  

**Accessibility Feedback**: xability-lab@illinois.edu

---

_This repository supports inclusive VR development. Contributions from the accessibility community are welcomed and valued._
