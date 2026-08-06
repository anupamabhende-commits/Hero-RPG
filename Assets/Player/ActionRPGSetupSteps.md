# Action RPG Prototype Setup

## One-click setup

1. Open the project in Unity.
2. Wait for scripts to compile.
3. Run `Magic Adventure > Setup Action RPG Prototype Scene`.
4. Open `Assets/Scenes/ActionRPGPrototype.unity`.
5. Press Play.

The setup creates a combat test arena, instantiates `Assets/Character/character.prefab`, disables the old embedded camera, adds the modular player controller, creates an action RPG follow camera, places a lock-on test target, and wires the portal destination.

## Controls

- `WASD`: Move
- `Left Shift`: Sprint
- `Space`: Jump
- `Q`: Dash/Roll
- `Left Mouse`: Attack
- `Right Mouse`: Cast fireball
- `F`: Cast portal
- `Tab`: Toggle lock-on
- Mouse movement: Orbit camera

## Manual setup

1. Add `PlayerController` to the player root.
2. Make sure the player root has a `CharacterController`.
3. Add or let `PlayerController` auto-add these modules: `PlayerStats`, `PlayerInput`, `PlayerMovement`, `PlayerRotation`, `PlayerJump`, `PlayerDash`, `PlayerCombat`, `PlayerAnimator`, and `GroundChecker`.
4. Create a child object named `CameraTarget` at about `(0, 1.45, 0)` and assign it to `PlayerController.cameraTarget`.
5. Create a scene camera, tag it `MainCamera`, add `ActionRPGCamera`, and assign the `CameraTarget`.
6. Assign the scene camera to `PlayerController.playerCamera`.
7. Assign an enemy or dummy transform to `PlayerController.lockOnTarget` and `ActionRPGCamera.lockOnTarget`.
8. If using spells, keep `PlayerSpellController` on the player and assign its fireball prefab, fire point, and portal destination.
9. Add Animator parameters as needed: `Speed` float, `Grounded` bool, `Jump` trigger, `Attack` trigger, `Roll` trigger, `Cast` trigger, `Sprint` bool, and `LockOn` bool.

## What is still needed for a finished game feel

- Final walk/run/jump/roll/attack animation clips and transitions.
- Actual enemy lock-on target selection instead of the single test dummy.
- Hitboxes/damage windows for attacks.
- Ability cooldown UI and mana/stamina if desired.
- Camera tuning after testing your final character scale and level geometry.
