Filename: Changelog.md
Last Updated: 2025-07-22 03:56 CEST
Version: 1.1.10
State: Experimental
Signed: GitHub Copilot

Synopsis:
- UI and logic for ComboBox input handling refined: accepts and normalizes 'kb' (e.g., '67kb' -> '67k') for bitrate and threshold fields, updating the field and summary accordingly (1C2).
- User is notified if settings file is missing or corrupted and defaults are used (4C2).
- Updated filename sanitization logic to allow the centre dot (U+00B7) character (2C4).

# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

**AI Directives for Updating This Changelog**
**Rule 1: Locating the Edit Area**
* All new, unreleased changes **MUST** be added under the `### Added`, `### Changed`, or `### Fixed` sub-headings inside the `## [Unreleased]` section.

**Rule 2: The Most Important Rule - DO NOT CREATE NEW HEADERS**
* You **MUST NOT** create a new versioned header (e.g., `## [1.1.5]...`) for experimental, unreleased changes. The *only* time a new versioned header is created is for a formal, stable release, and this will be explicitly requested by the user.

**Example Workflow for a typical change:**
1. **User Request:** "Log a fix for the output path logic."
2. **Your Action:**
   * Locate the `## [Unreleased]` section.
   * Locate the `### Fixed` sub-section.
   * Add a new line: `- Fixed the output path logic.`
   * Update the main file header at the very top of *this file* with the new version, timestamp, etc.
   * **DO NOT** create a `## [1.1.5]` header.

**Rule 3: Formal Releases**
* A formal release is created by changing the `## [Unreleased]` heading to a version number (e.g., `## [1.2.0] - ...`) and adding a new, empty `## [Unreleased]` section above it. This action will **only** be performed upon explicit user instruction.

## [Unreleased]

### Added
- Review.md created: Comprehensive review of code issues, user pitfalls, and improvement suggestions, including remedies for each concern. Triaged all items by Type, Effort, and Priority for a structured roadmap.

### Changed
* Renamed Output Folder Defaults 'Reset' button to 'Restore'.
* Removed border from editable ComboBox values in Compression Settings for a cleaner look.
* Updated StartButton_Click to include null checks for `_cancellationSource`.
* Changed the default output directory to be a sibling of the source directory.
* Renamed Output Folder Defaults 'Reset' button to 'Restore'. 'Set' stores the current Output Folder as default; 'Restore' loads it into the Output Folder field.
* UI and logic are consistent. Removed border from editable ComboBox values in Compression Settings for a cleaner look.
* SampleRateComboBox now displays values with 'Hz' suffix, but only the numeric value is used for ffmpeg.

### Fixed
* Output path logic in MainWindow and AudioProcessor now ensures files are placed in the correct output directory, not in subfolders named after the file.
* Documentation conventions strictly followed for all changes.
* Output extension logic in AudioProcessor now matches PowerShell reference: all re-encoded files are output as .m4b, regardless of input extension.
* Copied mono files now preserve their original extension and use sanitized filenames.
* Output filenames are sanitized to avoid invalid characters.
* Fixed a bug where the application would crash if the source folder was empty.
* Fixed UI freeze when processing very large audiobook libraries.

## [1.1.0] - 2025-07-17 07:15 CEST - [Stable] - [Signed: User]

### Added
- Added comprehensive event handling for editable ComboBoxes
- Added LostFocus event handling for immediate value updates
- Added KeyDown event handling for Enter key support
- Added inline event handlers for better code organization
- Added proper unit formatting (k, Hz) across all inputs
- Added value validation and formatting on all input methods
- Added async/await pattern to file processing operations
- Added IAsyncEnumerable support for file scanning
- Added proper namespace organization
- Added async process handling for FFprobe operations
- Added channel configuration options to Settings class
- Added default channel setting (Mono)
- Added channel selection event handling
- Added CurrentChannel property to Settings class
- Added DefaultChannel constant for initialization
- Added XML documentation for channel settings
- Added state tracking for channel selection

### Changed
- Enhanced ComboBox value update behavior
- Improved Settings synchronization with UI
- Refined bitrate and threshold input handling
- Updated audio processing optimization logic
- Improved namespace organization and type safety
- Enhanced ProbeAudioFile to use async/await pattern
- Improved file scanning with streaming async enumeration
- Updated process handling with proper async patterns
- Enhanced error handling with async context
- Updated ComboBox initialization to support channel options
- Enhanced settings management for audio channels
- Improved settings synchronization with UI
- Improved Settings class organization
- Enhanced settings state management
- Reorganized settings constants for better grouping
- Updated settings documentation

### Fixed
- Fixed delayed summary updates in editable ComboBoxes
- Fixed ComboBox value persistence issues
- Fixed namespace conflicts between WPF and Windows Forms
- Fixed type ambiguity in event handlers
- Fixed audio optimization detection logic
- Fixed value formatting consistency
- Improved error handling in audio processing
- Fixed ComboBoxItem namespace reference
- Fixed process handling null checks
- Fixed async method signatures and return types
- Fixed build errors related to async operations
- Fixed DefaultChannel reference in MainWindow
- Fixed channel initialization in ComboBox
- Fixed settings state persistence
- Improved settings documentation clarity

### Technical Debt
- Removed redundant method declarations
- Consolidated duplicate code in event handlers
- Improved code organization with inline handlers
- Enhanced type safety with explicit namespaces

## [1.0.5] - 2025-07-17 06:15 CEST - [Experimental] - [Signed: Claude]

### Added
- Added MaxWidth constraint to settings summary text
- Added consistent unit formatting for bitrates and frequencies

### Changed
- Improved ComboBox value display in settings summary
- Enhanced text formatting in settings summary
- Updated ComboBox bindings to use proper data sources

### Fixed
- Fixed ComboBox items collection conflict
- Fixed settings summary text truncation
- Fixed unit display consistency in UI

## [1.0.4] - 2025-07-17 06:00 CEST - [Experimental] - [Signed: Claude]

### Added
- Added explicit nullability annotations throughout the codebase
- Added null validation in Constants.cs tool path resolution
- Added proper error messages for tool path resolution failures
- Added safeguards against null references in path operations

### Changed
- Enhanced error handling in Constants.cs and related classes
- Improved tool path resolution logic for development environments
- Updated AudioFileInfo to use required properties
- Modified event handlers to properly handle null parameters

### Fixed
- Fixed potential null reference issues in AppDirectory resolution
- Fixed nullability warnings across the codebase
- Fixed tool path validation during startup
- Improved error handling for file operations

## [1.0.3] - 2025-07-17 03:48 CEST - [Stable] - [Signed: Gemini]

### Added
- Added XML documentation to ProgressWidthConverter for better IntelliSense support
- Added conditional debug logging in ProgressWidthConverter (enabled only in DEBUG builds)
- Added detailed progress value validation with warning logs
- Created AudioFileInfo class for managing audio file metadata
- Created Settings class for managing compression parameters
- Created AudioProcessor service for handling FFmpeg operations
- Added support for all required audio formats
- Added proper file sanitization for output paths
- Added event-based progress reporting
- Implemented folder browsing for source and output paths
- Implemented dynamic settings summary in UI
- Added file scanning and processing functionality
- Added progress tracking and status updates
- Added logging system with UI updates

### Changed
- Enhanced ProgressWidthConverter with improved type handling and error reporting
- Improved code organization in ProgressWidthConverter for better maintainability
- Structured core functionality into Models and Services namespaces
- Enhanced MainWindow with full audio processing implementation
- Updated UI controls to properly reflect processing state

### Fixed
- Enforced CBR to use 1-pass encoding only
- Properly handle file path sanitization for output files
- Improved error handling and user feedback

## [1.0.2] - 2025-07-17 01:41 CEST - [Stable] - [Signed: Gemini]

### Added
- `Signed:` tag to the file header convention for better authorship tracking.
- `Changelog.md` file to track project history.
- `Summary.md` file to serve as a handoff document for collaborators.

## [1.0.0] - 2025-07-14 05:33 CEST - [Stable] - [Signed: User]

### Added
- Initial project setup in Visual Studio.
- Ported UI design from PowerShell script to C# WPF (`MainWindow.xaml`).
- Implemented `Expander` controls for a collapsible UI.
- Implemented a unified status and progress bar.
- Implemented `ComboBox` controls for all compression settings.
- Added editable `ComboBox`es for "Target Bitrate" and "Mono Copy Threshold".
- Added `CheckBox` controls for file type selection.

### Changed
- Refined UI layout and control alignment for a consistent visual appearance.

### Fixed
- Corrected various XAML layout issues, including button stretching and `StatusBar` sizing.
- Updated StartButton_Click to include null checks for `_cancellationSource`.