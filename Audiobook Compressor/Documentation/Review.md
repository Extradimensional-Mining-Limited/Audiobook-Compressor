Filename: Review.md
Last Updated: 2025-07-21 16:56 CEST
Version: 1.1.8
State: Experimental
Signed: Gemini

Synopsis:
Triaged all items in the review, categorizing them by Type (Bug/Feature), Effort, and Priority to create a structured and actionable roadmap.

---

# Code Review & Risk Assessment

## 1. Input Validation & User Experience

**C. Concerns**

1.  **File/Folder Path Validation:**
    a. The UI checks for empty or invalid source/output paths, but does not validate for write permissions, path length, or special characters.
    b. No feedback if the output folder is not writable or if a file operation fails due to permissions.
    > **Type:** Bug | **Effort:** Medium | **Priority:** High

2.  **ComboBox Editable Inputs:**
    a. Manual entry in editable ComboBoxes may allow invalid or out-of-range values.
    b. No upper/lower bounds are enforced for bitrate or sample rate.
    > **Type:** Bug | **Effort:** Easy | **Priority:** Medium

3.  **File Overwrite Risk:**
    a. Existing files in the output directory may be silently overwritten.
    > **Type:** Bug | **Effort:** Medium | **Priority:** High

4.  **Cancellation Confirmation:**
    a. There is no "Are you sure?" confirmation dialog when the user clicks Cancel.
    > **Type:** Feature | **Effort:** Easy | **Priority:** Medium

5.  **Feedback Clarity:**
    a. `MessageBarText` should read: `Processing: [Filename]`.
    b. Log entries should be more specific (e.g., `Successfully converted: [Filename]`, `Copy failed: [Filename]`).
    c. The status bar should provide a more detailed summary (e.g., `X of Y completed. Z converted, A copied, B failed.`).
    > **Type:** Feature | **Effort:** Medium | **Priority:** Medium

6.  **Mono Source Upmixing:**
    a. When "Stereo" is selected, the application will convert mono source files to stereo, resulting in larger, inefficient files without providing any benefit to the user.
    > **Type:** Bug | **Effort:** Easy | **Priority:** High

7.  **2-pass encoding:**
    a. The application (through ffmpeg) is not actually performing a 2-pass encode when this is selected. This is because ffmpeg requires two separate runs to perform a 2-pass encode, and the current implementation does not handle this correctly.
    > **Type:** Bug | **Effort:** Hard | **Priority:** High

8.  **VBR and Multi-pass ABR Options
    a. The application does not currently support Variable Bit Rate (VBR) encoding or multi-pass Average Bit Rate (ABR) encoding.
    b. If VBR is selected, the application should default to a safe option if selected with CBR.
    > **Type:** Feature | **Effort:** Hard | **Priority:** Medium
9.  **Contextual UI for Advanced File Handling:**
    The UI must provide a clear, intuitive way to handle files that do not match the primary output type (e.g., mono files during a stereo encode).
    The current design, which lacks this, can lead to user confusion or unexpected behavior.
    > **Type:** Feature | **Effort:** Medium | **Priority:** High
10. **Inconsistent "Convert to Stereo" Logic:**
    a. When a user explicitly selects "Convert mono to stereo," the application lacks a clear rule for what to do if the estimated upmixed bitrate falls below the user-defined stereo copy threshold.
    b. This can lead to unexpected behavior where a file is re-encoded even though it meets the user's criteria for being "good enough" to copy.
    > **Type:** Bug | **Effort:** Medium | **Priority:** High

**R. Remedies**

1.  Add explicit checks for path validity, permissions, and length.
2.  Validate ComboBox input values against reasonable min/max ranges.
3.  Warn users before overwriting existing files or provide an option to skip/rename.
4.  Implement a confirmation dialog for the cancel action.
5.  Refine all user-facing status messages for better clarity and detail.
6.  Implement a new setting, "Don't convert mono files to stereo," enabled by default. The processing logic should check this setting and the source file's channel count to prevent unwanted upmixing.
7.  Implement a proper 2-pass encoding logic that runs ffmpeg twice with the appropriate parameters.
8.  Add VBR and multi-pass VBR.
    a. Include a VBR option in the bitrate control settings.
    b. Add logic to handle incompatibilities (e.g., VBR/multi-pass should default to a safe option if selected with CBR).
9.  The solution is a symmetrical, gated UI where advanced options are contextually revealed based on the user's primary "Mono" or "Stereo" selection. Details in Gemini AB (2025-07-22)


## 2. Error Handling & Robustness

**C. Concerns**

1.  **Exception Handling:**
    a. Many try/catch blocks silently ignore exceptions (empty catch blocks).
    b. Errors in async methods may not be surfaced to the user.
    > **Type:** Bug | **Effort:** Hard | **Priority:** High

2.  **External Tool Failures:**
    a. If ffmpeg/ffprobe is missing or fails, the user may not get a clear error message.
    > **Type:** Bug | **Effort:** Medium | **Priority:** High

3.  **File System Edge Cases:**
    a. Symbolic links, locked files, or network paths may cause unexpected failures.
    > **Type:** Bug | **Effort:** Hard | **Priority:** Medium

4.  **Filename Sanitization:**
    a. The sanitization routine should be reviewed to ensure no characters are removed unnecessarily. Specifically, the centre dot (`·`) should be removed from the scrub list.
    > **Type:** Bug | **Effort:** Easy | **Priority:** Low

**R. Remedies**

1.  Log all exceptions and display user-friendly error messages.
2.  Add explicit error messages for missing or failed external tools.
3.  Handle file system edge cases with additional checks and error reporting.
4.  Update the filename sanitization logic.

## 3. UI Responsiveness & Threading

**C. Concerns**

1.  **UI Thread Blocking:**
    a. Some synchronous operations could block the UI if not properly awaited.
    > **Type:** Bug | **Effort:** Medium | **Priority:** High

2.  **Dispatcher Usage:**
    a. Excessive use of `Dispatcher.Invoke` may cause performance issues.
    > **Type:** Refactor | **Effort:** Medium | **Priority:** Low

3.  **Window Resizing:**
    a. There are some unresolved issues with how the window resizes, particularly when expanders are toggled.
    > **Type:** Bug | **Effort:** Medium | **Priority:** Low

**R. Remedies**

1.  Ensure all long-running operations are fully async and awaited.
2.  Minimize `Dispatcher.Invoke` calls; use `Dispatcher.BeginInvoke` for non-blocking updates.
3.  Investigate and fix window resizing and layout behavior.

## 4. Settings & State Management

**C. Concerns**

1.  **Settings Persistence:**
    a. User settings are saved on close, but errors during save/load are silently ignored.
    b. No migration logic for settings if the schema changes.
    > **Type:** Bug | **Effort:** Medium | **Priority:** Medium

2.  **Default Values:**
    a. If settings files are missing or corrupted, defaults are used without notifying the user.
    > **Type:** Bug | **Effort:** Easy | **Priority:** Medium

**R. Remedies**

1.  Log errors during settings save/load and notify the user if settings are lost or reset.
2.  Implement versioning/migration for settings files.

## 5. Cancellation & Resource Management

**C. Concerns**

1.  **CancellationToken Usage:**
    a. Cancellation is supported, but some async operations may not honor the token promptly.
    > **Type:** Bug | **Effort:** Medium | **Priority:** High

2.  **Resource Disposal:**
    a. Some resources (e.g., file handles) may not be disposed of if exceptions occur.
    > **Type:** Bug | **Effort:** Medium | **Priority:** High

**R. Remedies**

1.  Audit all async operations to ensure they check for cancellation.
2.  Use `using` statements or `try/finally` to guarantee resource cleanup.

## 6. Accessibility & Usability

**C. Concerns**

1.  **Keyboard Navigation:**
    a. Not all controls may be accessible via keyboard.
    > **Type:** Feature | **Effort:** Medium | **Priority:** Low

2.  **Screen Reader Support:**
    a. No explicit support for screen readers or accessibility tools.
    > **Type:** Feature | **Effort:** Hard | **Priority:** Low

**R. Remedies**

1.  Test and improve keyboard navigation and tab order.
2.  Add automation properties and labels for accessibility.

## 7. Documentation & Maintainability

**C. Concerns**

1.  **File Header Consistency:**
    a. Some files may have outdated or inconsistent headers.
    > **Type:** Process | **Effort:** Easy | **Priority:** Medium

2.  **Changelog Discipline:**
    a. Changelog updates may be missed for minor changes.
    > **Type:** Process | **Effort:** Easy | **Priority:** Medium

**R. Remedies**

1.  Automate header and changelog updates as part of the build or commit process.

## 8. Suggestions for Improvement

-   Add unit and integration tests for core logic.
    > **Type:** Feature | **Effort:** Hard | **Priority:** Medium
-   Provide a settings reset option in the UI.
    > **Type:** Feature | **Effort:** Easy | **Priority:** Medium
-   Offer more granular progress reporting (e.g., per-file, per-task).
    > **Type:** Feature | **Effort:** Medium | **Priority:** Medium
-   Allow user to select audio codec and advanced ffmpeg options.
    > **Type:** Feature | **Effort:** Hard | **Priority:** Low
-   Add localization support for non-English users.
    > **Type:** Feature | **Effort:** Hard | **Priority:** Low
-   Implement logging to a file for troubleshooting.
    > **Type:** Feature | **Effort:** Easy | **Priority:** High
-   Consider packaging as a single-file executable for easier distribution.
    > **Type:** Feature | **Effort:** Medium | **Priority:** Medium
-   **Add VBR and multi-pass ABR options:**
    a. Include a VBR option in the bitrate control settings.
    b. Add logic to handle incompatibilities (e.g., VBR/multi-pass should default to a safe option if selected with CBR).
    > **Type:** Feature | **Effort:** Hard | **Priority:** Medium
-   **Add Pause/Resume Functionality:**
    a. Implement a pause button to temporarily halt processing.
    b. Investigate the feasibility of saving state to allow the application to be closed and resumed later.
    > **Type:** Feature | **Effort:** Very Hard | **Priority:** Low
-   **Add Debug Logging Option:**
    a. Add a setting to enable more verbose, diagnostic logging for troubleshooting.
    > **Type:** Feature | **Effort:** Easy | **Priority:** High
-   **File Overwrite Behavior:**
    a. Add a setting to control what happens if a file already exists in the output directory.
    b. Options should include Skip, Overwrite, and Rename.
    > **Type:** Feature | **Effort:** Medium | **Priority:** Medium
-   **Preserve File Dates:**
    a. Add an option to carry over the original file's creation and modification timestamps to the new file.
    > **Type:** Feature | **Effort:** Easy | **Priority:** Low
-   **Shutdown When Complete:**
    a. Add a checkbox to automatically shut down the computer when the job is finished.
    > **Type:** Feature | **Effort:** Easy | **Priority:** Low
---

This review should be revisited after major changes or before each stable release.