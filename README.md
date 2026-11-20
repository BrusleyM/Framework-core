# Framework Core

Building blocks for Unity guided experiences, shared by the Framework packages. Requires Unity 2022.3 or newer and the Universal Render Pipeline.

## Contents

| Assembly | Purpose |
|---|---|
| `Framework.CheckPoints` | `CheckPoint` and `CheckPointsParent`, scene objects that mark progress. |
| `Framework.Outline` | Object outlines with their shaders and materials. |
| `Framework.Timing` | `ElapsedTimer` (how long something took) and `CountdownTimer` (a time limit or idle wait), on an `ITimeSource`. |
| `Framework.Tools` | Small scene tools such as `SnapObject`. |
| `Framework.Core.Editor` | Editor only. Imports the package samples (Tools > Framework > Import core samples) and compares package versions (`PackageVersion`, `PackageCompatibility`); both are also used by the sequencer package. |

## Sample

The core showcase is an optional sample: a plaza with five glowing stands that show every feature. It does not install with the package. Import it in the Package Manager (select Framework Core, open the Samples tab, press Import), and it is copied to `Assets/Samples/Framework Core/<version>/Core showcase` with its tests. When the package is a folder inside Assets (a submodule, for example) Unity does not list its samples, so use Tools > Framework > Import core samples, which copies them to `Assets/Samples/Framework Core/Core showcase`. Open `Scenes/Core Showcase.unity` from there; controls and details are in the README next to it.

## Installation

Add the package by git URL in the Package Manager, pinned to a release tag:

`https://github.com/BrusleyM/Framework-core.git#v1.0.0-preview.1`

`com.vishnu.framework.sequencer` depends on this package. Unity does not fetch git dependencies on its own, so install this package in the same project as the sequencer.

## Continuous integration and releases

Pull requests run workflow lint and package checks (manifest, meta files, GUIDs, assembly references, changelog). Releases follow the same flow as the sequencer package: `release-prepare` on `develop`, then `release-promote`, then a merge into `master` publishes the tag and GitHub release.

This package has no tests of its own yet. It is compiled and exercised by the sequencer package's Unity test jobs.
