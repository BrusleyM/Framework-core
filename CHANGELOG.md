# Changelog

## [Unreleased]

### Added
- `EyeHeightPlacer`: puts a target (or its own object) at the height of the user's eyes, a set distance in front of them, with an optional height offset. Facing is a choice: keep the rotation, point the target's forward along the view direction (the convention for UI canvases and quads), or turn it towards the user. Call `Place()` or `TryPlace()` from code or a UnityEvent, or tick Place On Enable with an optional delay in seconds. The eyes are an assigned transform or, when none is assigned, the main camera looked up at the moment of placement. It works in VR and non-VR apps, but an XR headset may not have tracked when the scene loads: use the delay, or call `Place()` from an event once tracking is live. Only the direction along the floor (world up) is used, so looking up or down does not tilt the placement. A target that is the eyes, one of their children or one of their ancestors (such as the rig, including the placer's own object when no target is assigned) is refused, since head movement would drag it away again or the user would be moved; `Place()` logs a warning that names the reason when it cannot place. The math is in the pure `EyeHeightPlacement` class.
- Look-at detection: `LookDetector` (on the camera or head transform) and `LookTarget` (on each thing to look at). `StartLooking()` begins a run and `StopLooking()` ends it. The detector samples about ten times a second, not every frame: a target counts as looked at when it is inside a view cone (angle limit, maximum distance) and a ray from the eyes reaches it with nothing from the blocking layers in front, so walls hide targets (a collider on the target or one of its children counts as the target; list any other collider that belongs to it, such as a wall the poster hangs on, in the target's Extra Colliders; the default blocking layers leave out Ignore Raycast; put the player's own colliders on a layer you remove from the blocking layers). A target can need a dwell time, so a glance does not count. Several targets are supported, and the run stops when any target is seen, when all are seen, or only on request. Events: on the detector `OnLookingStarted`, `OnLookingStopped` (with the reason), `OnTargetFound` and `OnAllTargetsFound`; on each target `OnLookStarted`, `OnLookEnded` and `OnSeen`, plus `LookProgress`. All also exist as C# events. Sampling allocates no managed memory, and a long stall (a paused app, a headset taken off) is limited to two sample intervals so it cannot complete a dwell at once. Use one detector per target. `ReportGaze` is public on purpose, so a different source such as eye tracking can drive a target. The cone, dwell and stop-rule logic is in pure, tested classes. To use a look as a checkpoint, call `CheckPoint.MarkAsChecked` from a target's `OnSeen`; the existing `CheckPointsParent` then raises `OnAllChecked` when every point is done. If the checkpoint and the target are the same object and the checkpoint has Disable When Complete, the object is deactivated from inside `OnSeen`.

### Fixed
- `CheckPoint.OnChecked` and `CheckPointsParent.OnAllChecked` and its point list are created with the component, so listening to them no longer throws on a check point built from code.
- Coroutines no longer fail silently or leave state stuck: a failure while a snap animates is logged and releases the object (and `TryOffer` reports the failure), a failure in the delayed eye-height placement is logged, and a listener that throws is logged without hiding the other listeners' events: the snap, placer and look-at events go through the shared `SafeEvents`, and the look-at sampling loop keeps sampling. The shared `GuardedCoroutine` helper reports a routine's exception once (including from routines it yields) instead of ending it unseen, and disposes the routine.
- No endless recursion from listeners: a placed listener that calls `Place()` again is refused with a warning, and a look target accepts a gaze report made from inside its own events (for example looking away from its seen listener) and raises the resulting events afterwards, in the order they happened, instead of recursing; if its events keep retriggering themselves it stops after eight rounds with an error.

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
