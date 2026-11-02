# GUI Components and Level Design — Floor 01

This folder contains the GUI components and level-design work:

- Start, pause, settings, win, and lose menus.
- A single 3D labyrinth/platforming floor with four visual puzzle-piece placement markers.
- Static trap and exit triggers that demonstrate the lose/win menu states.

The puzzle markers are intentionally visual-only. Collection, orbiting, depositing, inventory,
enemy AI, checkpoints, and persistent death consequences remain owned by the other project tasks.

## Running the level

Press Play from `Assets/Scenes/SampleScene.unity`. The runtime bootstrap constructs Floor 01 so
the work is immediately playable without generated assets.

When using a working Unity editor, `RTX Penguins > Build Labyrinth Level` creates the standalone
`Assets/Scenes/LabyrinthFloor01.unity` scene and adds it to Build Settings.
