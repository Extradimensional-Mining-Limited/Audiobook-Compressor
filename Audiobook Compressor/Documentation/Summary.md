Filename: Summary.md
Last Updated: 2025-07-21 01:37 CEST
Version: 1.1.4
State: Experimental
Signed: GitHub Copilot

Synopsis:
Output path logic in MainWindow and AudioProcessor fixed to prevent creation of subfolders named after files. Documentation conventions strictly followed.

---

## Project Summary: Audiobook Compressor

### 1. Project Goal
To create a standalone, distributable C# WPF desktop application named **Audiobook Compressor** that compresses a user's audiobook library.

### 2. Current Status
The UI/UX design phase is complete and core functionality is implemented. The project is now in the optimization phase, focusing on robustness and error handling. Recent updates have improved nullability support and tool path resolution.

### 3. Core Logic Source
The functional logic for file processing must be ported from the provided PowerShell script: `AudiobookCompressor-v6.2.ps1`. This script is the definitive "ground truth" for the core compression logic. All `ffmpeg` commands, file processing order, and logic (e.g., mono copy threshold) should be ported directly from this script.

### 4. UI/UX Philosophy
* **Minimalist Default View:** The application is designed to be clean and uncluttered on launch. All settings and logs are contained within collapsible `Expander` controls.
* **Dynamic Headers:** The `Expander` headers are not static labels. They should be updated programmatically to provide a dynamic summary of the current settings or the latest log message.
* **Functional Aesthetics:** The UI uses standard, functional WPF controls. Consistency in alignment, color, and size (e.g., ensuring buttons and `ComboBox`es have the same height) is a key principle.

### 5. Key Technical Decisions
* The six main compression settings are implemented as `ComboBox` drop-downs within a `WrapPanel` for a responsive layout.
* **Audio Channel Configuration:**
    * Defaults to Mono for audiobook optimization
    * Supports Stereo for maintaining original quality when desired
    * Channel selection affects mono copy threshold behavior
    * Channel state is tracked centrally in Settings class
* **ComboBox Input Handling:**
    * Editable ComboBoxes (`BitrateComboBox` and `ThresholdComboBox`) support:
        * Direct value selection from dropdown
        * Manual text input with Enter key confirmation
        * Value persistence on focus loss
        * Automatic unit formatting (k, Hz)
        * Immediate UI feedback
    * Non-editable ComboBoxes provide:
        * Instant state updates
        * Proper value persistence
        * UI synchronization
* **Settings State Management:**
    * All configuration options are centralized in the Settings class
    * Default values are provided for all settings
    * Options are exposed as read-only collections
    * Settings changes trigger UI updates automatically
    * Values are properly formatted before storage and display
    * State changes are immediately reflected in UI
* **UI Responsiveness:**
    * All user inputs trigger immediate UI updates
    * Value formatting is consistent across all inputs
    * Settings summary updates in real-time
    * Error handling prevents invalid inputs
    * Focus handling ensures value persistence
* The application's bitrate control logic is based on **ABR** (Average Bitrate) to align with the PowerShell script. VBR is a potential future feature.
* When **CBR** (Constant Bitrate) is selected, it must be a **1-pass** encode. The UI enforces this automatically.
* The application processes ten specific audio formats: `.m4b`, `.mp3`, `.aac`, `.m4a`, `.flac`, `.ogg`, `.wma`, `.wav`, `.webma`, and `.opus`. DRM-protected files are out of scope.
* **Error Handling:** The application implements comprehensive null checking and path validation:
    * Tool paths are resolved with proper error handling for missing directories
    * All nullable references are explicitly marked and handled
    * External tool dependencies are verified before startup
    * Path operations include safeguards against null references
* **UI Best Practices:**
    * ComboBox items are properly bound to data sources
    * Text formatting is consistent across the UI
    * UI elements handle overflow gracefully
    * Settings summary provides clear feedback
    * Input validation occurs in real-time
    * User feedback is immediate and clear
* **Async Pattern Implementation:**
    * File operations use async/await pattern consistently
    * Process handling follows Task-based Asynchronous Pattern (TAP)
    * IAsyncEnumerable used for streaming file processing
    * UI remains responsive during long-running operations
* **Type Safety:**
    * Explicit namespace usage prevents ambiguity
    * Proper type casting in event handlers
    * Null-safe access patterns
    * Clear separation of WPF and Windows Forms types

### 6. Project Workflow & Conventions
* **File Headers:** All project files must adhere to the structure defined in `Documentation/FileHeaderConvention.md`.
* **State Management:** Any modification to a file automatically sets its state to `Experimental`. The `Stable` tag is only to be used for deliberate releases **at the user's explicit request**.
* **Versioning & Branching:** Each experimental change iterates the minor version number (e.g., `1.0.0` -> `1.0.1`). Stable, milestone releases get a major version bump (e.g., `1.0.13` -> `1.1.0`), are tagged `Stable`, and are pushed to the `main` branch on GitHub, **only upon explicit instruction from the user**. Ongoing work occurs in the `experimental` branch.
* **Documentation:** A `Changelog.md` file will log all changes, and this executive `Summary.md` will be kept current as a handoff document.
* **Nullability:** The project uses C# nullable reference types. All nullable members must be explicitly marked with `?` and properly handled with null checks.

### 7. Next Immediate Steps
Complete the optimization phase by:
1.  Testing tool path resolution in various deployment scenarios
2.  Implementing proper error recovery for missing or inaccessible files
3.  Adding detailed error messages for common failure cases
4.  Conducting thorough testing of null handling and error paths
5.  Implementing progress reporting for async operations
6.  Adding cancellation support for long-running tasks

### 8. Directives for AI Collaboration
To ensure consistent and accurate contributions, any collaborating AI must adhere to the following directives:

1.  **Principle of Strict Adherence**: All rules and conventions outlined in this document are mandatory. Do not deviate from or make assumptions about these instructions.
2.  **Follow Document References**: When this document refers to other project documents (e.g., `Documentation/FileHeaderConvention.md`), you are required to read that document and apply its rules and conventions as if they were written here directly.
3.  **Verify Context**: Before making any changes, confirm you have the full and correct context of the current task and the latest versions of all relevant files.
4.  **Timestamp Procedure**: Use the PowerShell command `Get-Date -Format "yyyy-MM-dd HH:mm"` to generate timestamps for file headers and changelog entries.
5.  **Comprehensive Application**: Apply all relevant rules in a single, comprehensive action to maintain consistency and minimize iterative corrections.

### Recent Updates
- Output path logic in MainWindow and AudioProcessor now ensures files are placed in the correct output directory, not in subfolders named after the file.
- Documentation conventions strictly followed for all changes.

---