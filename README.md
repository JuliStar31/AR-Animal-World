# AR Animal World

A playful educational Augmented Reality (AR) mobile game for children ages 4–7, designed to teach animals through printed image cards, 3D animal models, sounds, narration, and simple interactive quizzes.

## Overview

AR Animal World lets children:

- scan printed animal cards with AR
- see a 3D animal appear in the real world
- listen to animal sounds and narration
- learn short facts in simple language
- complete a simple quiz with encouraging feedback

This repository is structured as a Unity-ready starter project for Android mobile development using Vuforia for image recognition and AR tracking.

## Target

- Platform: Android mobile
- Audience: children 4–7 years old
- Technology: Unity + Vuforia + AR Foundation-compatible approach
- Visual style: friendly, warm, bright, child-safe, soft educational UI

## Included in this repo

- project documentation for the complete 6-screen UI concept
- Unity-ready folder structure
- starter C# scripts for game flow, animal data, and quiz logic
- Vuforia implementation guidance for image targets and animal cards

## Recommended Unity setup

- Unity 2022 LTS or newer
- Android Build Support
- Vuforia Engine
- TextMeshPro
- Input System (optional but useful)
- AR camera support via Vuforia image targets

## Project structure

```text
AR-Animal-World/
├─ README.md
├─ docs/
│  ├─ UI-Concept.md
│  └─ Vuforia-Setup.md
├─ Assets/
│  ├─ Scenes/
│  ├─ Scripts/
│  ├─ Prefabs/
│  ├─ Images/
│  ├─ Audio/
│  └─ Models/
├─ ProjectSettings/
└─ Packages/
```

## Core gameplay flow

1. Open the main menu
2. Choose an animal
3. Scan the printed animal card with Vuforia
4. See the 3D animal anchored to the card in AR
5. Listen to animal sound and read a short fact
6. Answer a simple quiz and collect stars
7. Celebrate results with a friendly reward screen

## Design principles

- large buttons and read-friendly layout
- warm colors with sky blue, cream, green, and yellow accents
- soft shapes, rounded cards, no harsh red error states
- encouraging language only
- minimal clutter and simple navigation for early learners

## Example animal set

- Singa
- Gajah
- Kelinci

## Quick start

Full step-by-step guide (Indonesian): [docs/Panduan-Build-APK.md](docs/Panduan-Build-APK.md)

1. Open this repository in Unity 6 (tested with 6000.3) and import TMP Essential Resources
2. Install Vuforia Engine 11 and paste your license key in Vuforia Configuration
3. Add an AR Camera and three Image Targets named Singa, Gajah, Kelinci, using the printable cards
4. Optional: copy a 3D animal pack into `Assets/ThirdParty` and run **AR Animal World > Siapkan Model Hewan 3D**
5. Run **AR Animal World > Bangun Game di Scene Ini** to build every screen from the UI design and wire the cards
6. Press Play (the Scan screen has test buttons that fake a detected card)
7. Build for Android (minimum Android 10 / API 29)

Besides the printed cards, the Scan screen recognises any photo or realistic picture of a lion, elephant or rabbit with MobileNetV2 (ImageNet, `Assets/AI/mobilenetv2-12.onnx`) through Unity's Inference Engine package (`com.unity.ai.inference`, tested with 2.6.1), and shows that animal in front of the camera.

Placeholder 3D animals and synthesized sounds are generated automatically, so the game is playable before real models and audio exist. **Siapkan Model Hewan 3D** finds a lion, elephant and rabbit in any model under `Assets` and saves them as prefabs in `Assets/Resources/Models`, turned to face the player and sized to the card. Real sounds go in `Assets/Resources/Audio`.

## Next steps

This starter repository is designed so you can continue with:

- menu and screen flow implementation
- AR card detection and target mapping
- 3D animal placement behavior
- sound playback and narration
- quiz logic and star scoring

## License

This project is provided for educational and portfolio purposes.
