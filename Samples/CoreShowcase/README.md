# Core showcase

Open `Scenes/Core Showcase.unity` and press Play. It needs the Universal Render Pipeline (the glow uses HDR and bloom, so keep HDR on in the pipeline asset) and the legacy Input Manager for the walking keys.

A plaza with five glowing cylinder stands around a central hub. Each stand shows one feature of the core. Look at things to trigger them: a glowing ring appears on whatever you look at and fills while the dwell time runs, then turns green when it counts as seen.

| Stand | Shows | Try |
|---|---|---|
| Outline | `Outline` in three colours | Look at the sphere, cube or capsule: the outline switches on while you look and off when you look away. |
| Look at | `LookDetector` and `LookTarget` with a dwell time | Keep your eyes on the orb for about a second and a half: it flashes when seen. |
| Check points | `CheckPoint` and `CheckPointsParent` | Look at each of the three crystals: each one lights up, and the ring lights when all three are checked. |
| Snap | `SnapPoint`, `Snappable`, `SnapKey` | Two blocks drop onto the pad: the cyan one holds the accepted key and snaps in, the red one is refused. |
| Eye height | `EyeHeightPlacer` | Press P to bring the panel to eye height in front of you. |

## Controls

| Key | Action |
|---|---|
| W A S D and mouse | Walk and look |
| R | Reset the look orb and the check points |
| Space | Drop the snap blocks again |
| P | Place the panel at eye height in front of you |
| O | Send the panel back to its stand |
| H | Show or hide the help overlay |
| Esc | Free the cursor |

## Shaders

All in `Shaders/`, written for URP and SRP Batcher friendly:

- `Framework/Samples/Neon Glow`: HDR rim glow with a pulse and a flash for the bloom.
- `Framework/Samples/Hologram`: transparent additive look with fresnel, scanlines and flicker.
- `Framework/Samples/Iridescent`: colour that shifts with view angle, height and time.
- `Framework/Samples/Stand Glow`: glowing rims, rings and a stripe for the cylinder stands.
- `Framework/Samples/Gaze Ring`: the radial fill ring and centre dot drawn on the looked-at object (`GazeRing` drives it from the targets' events and `LookProgress`).
- `Framework/Samples/Grid Floor`: anti-aliased grid with fading rings.

Glow colour, strength and flash are changed at runtime with `GlowControl`, which uses a material property block so the materials are shared.

## Limits

- Keys and walking only work with the legacy Input Manager (Active Input Handling set to Input Manager or Both). With only the new Input System the camera rig warns and the key bindings do nothing.
- The post-processing profile needs post processing enabled on the URP renderer.
- `GlowControl` uses material property blocks, which opt those renderers out of the SRP Batcher. Fine for a demo; give each object its own material if you copy the pattern into a busy scene.
- The shaders have only a forward pass, so they do not appear in the depth or depth-normals textures.
