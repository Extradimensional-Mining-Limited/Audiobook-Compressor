[2025-07-17 07:15 CEST]
[1.1.0]
[Stable]
[Synopsis: First stable release with complete UI functionality]
[Signed: User]

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---
**How to Update This File**
1.  Keep these instructions at the top of the file.
2.  Add all new changes under the appropriate sub-heading in the `[Unreleased]` section. All release entries should be added below this section.
3.  When creating a new release, the heading should follow the format: `## [Version] - YYYY-MM-DD HH:mm CEST - [State] - [Signed: Author]`
4.  For timestamps, use the local date and time. The PowerShell command `Get-Date -Format "yyyy-MM-dd HH:mm"` can be used to generate the base timestamp.
---

## [Unreleased]

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