# Changelog

## [Unreleased]

### Added
- `SnapPoint`, `Snappable` and `SnapKey`: a snap point that accepts one key asset instead of an object name, animates the object into place over a set duration, can release it again, and raises `OnSnapped` and `OnReleased`. Triggering is the developer's choice: call `SnapPoint.TryOffer` and `SnapPoint.Release` from any code or UnityEvent, or add `SnapOnTriggerEnter` or `SnapOnCollisionEnter`.
- Edit mode tests for the snap point and `SnapBlend`.

### Deprecated
- `SnapObject` (matches by name and teleports on collision). Use `SnapPoint` with `Snappable` and `SnapKey`.

## [1.0.0] - 2026-11-18

### Changed
- First stable release. The code is identical to 1.0.0-preview.1; the sequencer package was verified against it in clean projects on Unity 2022.3 and 6000.3.

## [1.0.0-preview.1] - 2026-11-18

### Added
- First release of the core package, split out of `com.vishnu.framework`: `Framework.CheckPoints`, `Framework.Outline` (with its shaders and materials) and `Framework.Tools`. Assembly names, namespaces and GUIDs are unchanged.
