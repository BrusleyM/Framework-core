# Changelog

## [Unreleased]

### Added
- `EyeHeightPlacer`: puts a target (or its own object) at the height of the user's eyes, a set distance in front of them, optionally turned to face along their view direction. Call `Place()` or `TryPlace()` from code or a UnityEvent, or tick Place On Enable. It reads the eyes from an assigned transform or the main camera, so it works in VR and non-VR apps and can be called again for a seated or standing user. The math is in the pure `EyeHeightPlacement` class. Looking up or down does not tilt the placement: only the direction along the floor is used.

## [1.1.0] - 2026-11-18

### Added
- `SnapPoint`, `Snappable` and `SnapKey`: a snap point that accepts one key asset instead of an object name, animates the object into place over a set duration, can release it again, and raises `OnSnapped` and `OnReleased`. Triggering is the developer's choice: call `SnapPoint.TryOffer` and `SnapPoint.Release` from any code or UnityEvent, or add `SnapOnTriggerEnter` or `SnapOnCollisionEnter`.
- Edit mode and play mode tests for the snap point and `SnapBlend`.
- Snap behavior to know: an object snapped to one point must be released before another point accepts it. A snap triggers on enter only, so an object rejected while the point is occupied must leave and enter again (or be offered with `TryOffer`). Releasing during the animation stops the object where it is and raises only `OnReleased`. Disabling a point mid-animation, or destroying the object or the point, releases it. A point that moves during the animation is followed. Set `Ignore Time Scale` to animate while the game is paused. `Snappable` makes its Rigidbody kinematic and turns its interpolation off while snapped, and restores both on release. Only 3D Rigidbody is handled. A point with no key refuses everything.

### Fixed
- `Outline.OutlineActive` is created with the component, so enabling an outline no longer throws on an `Outline` added from code.

### Deprecated
- `SnapObject` (matches by name and teleports on collision). Use `SnapPoint` with `Snappable` and `SnapKey`.

## [1.0.0] - 2026-11-18

### Changed
- First stable release. The code is identical to 1.0.0-preview.1; the sequencer package was verified against it in clean projects on Unity 2022.3 and 6000.3.

## [1.0.0-preview.1] - 2026-11-18

### Added
- First release of the core package, split out of `com.vishnu.framework`: `Framework.CheckPoints`, `Framework.Outline` (with its shaders and materials) and `Framework.Tools`. Assembly names, namespaces and GUIDs are unchanged.
