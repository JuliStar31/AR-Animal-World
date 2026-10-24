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
- add simple animation or idle movement after detection

## Example logic

- Lion card detected -> spawn lion model
- Elephant card detected -> spawn elephant model
- Rabbit card detected -> spawn rabbit model

## Wiring the card handler

`Assets/Scripts/AR/AnimalImageTargetHandler.cs` does the detection work for each card. Requires Vuforia Engine 10 or newer (it uses `ObserverBehaviour.OnTargetStatusChanged`).

1. Put one `ARAnimalWorldManager` in the scene (for example on an empty `GameManager` object).
2. Add an Image Target for each card (lion, elephant, rabbit) under the AR Camera's scene.
3. On each Image Target, add `AnimalImageTargetHandler` and set:
   - **App Manager**: the `ARAnimalWorldManager` object
   - **Animal Id**: `Lion`, `Elephant`, or `Rabbit` to match the card
   - **Model Prefab**: the animal prefab (optional, see below)
   - **Sound Clip**: the animal sound (optional, see below)
   - **Narration Clip**: a recorded voice reading `narrationText` (optional)
   - **Model Local Position / Euler / Scale**: tune so the animal sits nicely on the card
4. Remove the `DefaultObserverEventHandler` component Vuforia adds to each Image Target. It toggles child renderers on its own and will fight with the handler.

If Model Prefab or Sound Clip are left empty, the handler loads them from `Resources` using the names in `AnimalData`:

| Animal | Model | Sound |
| --- | --- | --- |
| Lion | `Resources/Models/LionModel` | `Resources/Audio/lion_roar` |
| Elephant | `Resources/Models/ElephantModel` | `Resources/Audio/elephant_trumpet` |
| Rabbit | `Resources/Models/RabbitModel` | `Resources/Audio/rabbit_squeak` |

### What happens at runtime

- **Card found** (`TRACKED` or `EXTENDED_TRACKED`): the model is spawned as a child of the Image Target the first time and shown again on later scans. The animal sound plays, then the narration after a short pause. The handler calls `ARAnimalWorldManager.HandleAnimalDetected`, which selects that animal (so the quiz uses it) and raises `OnAnimalDetected`.
- **Card lost**: the model is hidden, audio stops, and `ARAnimalWorldManager.HandleAnimalLost` raises `OnAnimalLost`.

UI scripts can subscribe to `OnAnimalDetected` to show the fact card (`fact`) and narration subtitles (`narrationText`), and to `OnAnimalLost` to show a friendly "point the camera at the card" hint. `IsAnimalInView` tells you whether a card is currently tracked.

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
