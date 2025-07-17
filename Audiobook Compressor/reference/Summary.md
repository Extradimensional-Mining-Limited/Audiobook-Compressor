[2025-07-17 01:41]
[1.0.2]
[Stable]
[Synopsis: Added 'Signed:' tag to the file header convention for clarity on authorship.]
[Signed: Gemini]

## Project Summary: Audiobook Compressor

### 1. Project Goal
To create a standalone, distributable C# WPF desktop application named **Audiobook Compressor** that compresses a user's audiobook library.

### 2. Current Status
The UI/UX design phase is complete. The final UI is implemented in the project's XAML files and has been visually verified and approved. The project is now in the C# code-behind implementation phase.

### 3. Core Logic Source
The functional logic for file processing must be ported from the provided PowerShell script: `AudiobookCompressor-v6.2.ps1`. This script is the definitive "ground truth" for the core compression logic. All `ffmpeg` commands, file processing order, and logic (e.g., mono copy threshold) should be ported directly from this script.

### 4. UI/UX Philosophy
* **Minimalist Default View:** The application is designed to be clean and uncluttered on launch. All settings and logs are contained within collapsible `Expander` controls.
* **Dynamic Headers:** The `Expander` headers are not static labels. They should be updated programmatically to provide a dynamic summary of the current settings or the latest log message.
* **Functional Aesthetics:** The UI uses standard, functional WPF controls. Consistency in alignment, color, and size (e.g., ensuring buttons and `ComboBox`es have the same height) is a key principle.

### 5. Key Technical Decisions
* The six main compression settings are implemented as `ComboBox` drop-downs within a `WrapPanel` for a responsive layout.
* The **Target Bitrate** and **Mono Copy Threshold** `ComboBox`es are editable (`IsEditable="True"`) to allow for advanced user input.
* The application's bitrate control logic is based on **ABR** (Average Bitrate) to align with the PowerShell script. VBR is a potential future feature.
* When **CBR** (Constant Bitrate) is selected, it must be a **1-pass** encode. The UI should enforce this.
* The application processes ten specific audio formats: `.m4b`, `.mp3`, `.aac`, `.m4a`, `.flac`, `.ogg`, `.wma`, `.wav`, `.webma`, and `.opus`. DRM-protected files are out of scope.

### 6. Project Workflow & Conventions
* **File Headers:** All project files must have a five-part header:
    1.  A **timestamp**, obtained by running the PowerShell command `Get-Date -Format "yyyy-MM-dd HH:mm"` in the terminal.
    2.  An incremented **file version**.
    3.  A **state tag**: `Stable` or `Experimental`.
    4.  A **synopsis** of the recent changes.
    5.  A **`Signed:`** tag indicating who authored or approved the changes (e.g., `Claude`, `User`).
* **State Management:** Any modification to a file automatically sets its state to `Experimental`. The `Stable` tag is only to be used for deliberate releases **at the user's explicit request**.
* **Versioning & Branching:** Each experimental change iterates the minor version number (e.g., `1.0.0` -> `1.0.1`). Stable, milestone releases get a major version bump (e.g., `1.0.13` -> `1.1.0`), are tagged `Stable`, and are pushed to the `main` branch on GitHub, **only upon explicit instruction from the user**. Ongoing work occurs in the `experimental` branch.
* **Documentation:** A `changelog.md` file will log all changes, and this executive `summary.md` will be kept current as a handoff document.

### 7. Next Immediate Steps
Begin the next phase of C# implementation. The first task is to ensure the `Click` event handlers for the `SourceBrowseButton` and `OutputBrowseButton` are fully implemented. This involves using a folder browser dialog to get the selected path and populate the corresponding `TextBox` controls.