# Framework Core

Building blocks for Unity guided experiences, shared by the Framework packages. Requires Unity 2022.3 or newer and the Universal Render Pipeline.

## Contents

| Assembly | Purpose |
|---|---|
| `Framework.CheckPoints` | `CheckPoint` and `CheckPointsParent`, scene objects that mark progress. |
| `Framework.Outline` | Object outlines with their shaders and materials. |
| `Framework.Tools` | Small scene tools such as `SnapObject`. |

## Installation

Add the package by git URL in the Package Manager, pinned to a release tag:

`https://github.com/BrusleyM/Framework-core.git#v1.0.0-preview.1`

`com.vishnu.framework.sequencer` depends on this package. Unity does not fetch git dependencies on its own, so install this package in the same project as the sequencer.

## Continuous integration and releases

Pull requests run workflow lint and package checks (manifest, meta files, GUIDs, assembly references, changelog). Releases follow the same flow as the sequencer package: `release-prepare` on `develop`, then `release-promote`, then a merge into `master` publishes the tag and GitHub release.

This package has no tests of its own yet. It is compiled and exercised by the sequencer package's Unity test jobs.
