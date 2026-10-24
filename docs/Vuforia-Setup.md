# Vuforia Setup Guide

This project is designed around Vuforia image targets for printed animal cards. Vuforia is a good fit for a child-friendly AR learning app because it allows easy detection of a card and placement of a 3D animal model in the real world.

## Recommended setup

1. Open Unity Hub and create a new 3D project or use this repository as a starting point.
2. Install the Vuforia Engine package.
3. Add Android Build Support.
4. Create a Vuforia license key in the Vuforia Developer portal.
5. In Unity, enable Vuforia and configure the project for Android.

## Recommended image target workflow

- Create three target images:
  - lion card
  - elephant card
  - rabbit card
- Use clear contrast and minimal clutter in the printed card design
- Add a safe border and recognizable animal silhouette
- Ensure good input image quality and strong contrast for reliable detection

## Suggested target configuration

- Target type: Image
- Target size: ideally 5–10 cm printed card
- Good lighting and clear camera focus
- Avoid reflective surfaces and blurry print

## AR object setup

For each animal card:

- add a Vuforia Image Target
- assign a corresponding 3D animal prefab
- anchor the animal model on the detected target
- add simple animation or idle movement
after detection

## Example logic

- Lion card detected -> spawn lion model
- Elephant card detected -> spawn elephant model
- Rabbit card detected -> spawn rabbit model

## Tips for child-safe AR

- keep the 3D model well-anchored to the card
- use gentle motion and not too much realism that can be frightening
- ensure buttons remain visible over the camera view without covering too much of the world
- keep text instructions short and friendly

## Testing

- test on a real Android device
- test under bright daylight and indoor lighting
- verify target recognition distance and camera focus
- optimize target images if detection is unreliable
