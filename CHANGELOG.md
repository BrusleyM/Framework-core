# Changelog

## [Unreleased]

### Fixed
- `Outline.OutlineActive` is created with the component, so enabling an outline no longer throws on an `Outline` added from code.

## [1.0.0] - 2026-11-18

### Changed
- First stable release. The code is identical to 1.0.0-preview.1; the sequencer package was verified against it in clean projects on Unity 2022.3 and 6000.3.

## [1.0.0-preview.1] - 2026-11-18

### Added
- First release of the core package, split out of `com.vishnu.framework`: `Framework.CheckPoints`, `Framework.Outline` (with its shaders and materials) and `Framework.Tools`. Assembly names, namespaces and GUIDs are unchanged.
