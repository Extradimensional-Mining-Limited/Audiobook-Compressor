Filename: ToDo.md
Last Updated: 2025-07-21 17:01 CEST
Version: 1.1.0
State: Experimental
Signed: Gemini

Synopsis:
A prioritized to-do list generated from the Code Review & Risk Assessment document, ordered by effort, priority, and type.

---

# Project To-Do List

This list is prioritized to address the easiest and most critical items first.

## Effort: Easy

### Priority: High
- [ ] **(Feature)** 8.6: Implement logging to a file for troubleshooting.
- [ ] **(Feature)** 8.10: Add a setting to enable verbose, diagnostic logging.
- [ ] **(Bug)** 1C6: Implement a new setting, "Don't convert mono files to stereo," and relevant processing logic.

### Priority: Medium
- [X] **(Bug)** 1C2: Validate and handle manual entry in editable ComboBoxes.
- [X] **(Bug)** 4C2: Notify the user when a corrupted settings file forces the use of default values.
- [ ] **(Process)** 7C1: Ensure file header consistency across all project files.
- [ ] **(Process)** 7C2: Maintain discipline in updating the changelog for all changes.
- [ ] **(Feature)** 1C4: Implement an "Are you sure?" confirmation dialog for the cancel action.
- [ ] **(Feature)** 8.2: Provide a settings reset option in the UI.

### Priority: Low
- [X] **(Bug)** 2C4: Review and update the filename sanitization routine (specifically regarding the centre dot `·`).

## Effort: Medium

### Priority: High
- [ ] **(Bug)** 1C1: Implement validation for file/folder path permissions, length, and special characters.
- [ ] **(Bug)** 1C3: Add a warning or option to prevent silent overwriting of existing files.
- [ ] **(Bug)** 2C2: Add clear, user-friendly error messages if `ffmpeg` or `ffprobe` are missing or fail.
- [ ] **(Bug)** 3C1: Ensure no synchronous I/O or long operations are blocking the UI thread.
- [ ] **(Bug)** 5C1: Audit all async operations to ensure they honor the CancellationToken promptly.
- [ ] **(Bug)** 5C2: Ensure all disposable resources (like file streams) are properly cleaned up, even if exceptions occur.
- [ ] **(Feature)** 9C: Implement a dynamic panel whose visibility and content are controlled by the primary "Mono/Stereo" output setting.							
                        In place for the current "Don't convert mono to stereo." checkbox, in Mono mode, show a checkbox for "Advanced stereo file handling" and when checked show a row of radio buttons (Keep as stereo, Convert to mono, Advanced...) with Convert... selected by default.  
                        In the same place, in Stereo mode, show a checkbox for "Advanced mono file handling" and when checked show a row of radio buttons (Keep as mono, Re-encode to mono, Advanced...) with Keep... selected by default.  
                        Advanced unlocks an otherwise greyed out additional row of standard main compression settings (Bitrate, Sample rate, Channels, etc.) that are only enabled when the advanced options are selected.)
                        Obviously, all should be wired to the corresponding processing logic.

### Priority: Medium
- [ ] **(Bug)** 4C1: Add error handling and user notifications for failures in saving/loading settings.
- [ ] **(Feature)** 1C5: Refine all user-facing status and log messages for better clarity and detail.
- [ ] **(Feature)** 8.3: Offer more granular progress reporting (e.g., per-file progress).
- [ ] **(Feature)** 8.7: Consider packaging the application as a single-file executable for easier distribution.

### Priority: Low
- [ ] **(Refactor)** 3C2: Review `Dispatcher.Invoke` usage and switch to `BeginInvoke` where appropriate to improve performance.
- [ ] **(Bug)** 3C3: Investigate and fix window resizing and layout issues.
- [ ] **(Feature)** 6C1: Test and improve keyboard navigation and tab order.

## Effort: Hard

### Priority: High
- [ ] **(Bug)** 2C1: Refactor all `try/catch` blocks to log exceptions and surface meaningful errors to the user.

### Priority: Medium
- [ ] **(Bug)** 2C3: Add handling for file system edge cases like locked files, symbolic links, or network paths.
- [ ] **(Feature)** 8.1: Add unit and integration tests for core logic in the `AudioProcessor`.
- [ ] **(Feature)** 8.8: Add VBR and multi-pass ABR options to the UI and processing logic.
- [ ] **(Feature)** 1C8: Implement VBR and Multi-pass for ABR and VBR encoding options.
- [ ] **(Bug)** 1C7: Implement a new setting, "Use 2-pass encoding," and relevant processing logic.

### Priority: Low
- [ ] **(Feature)** 6C2: Add explicit support for screen readers and accessibility tools.
- [ ] **(Feature)** 8.4: Allow the user to select the audio codec and other advanced `ffmpeg` options.
- [ ] **(Feature)** 8.5: Add localization support for non-English users.

## Effort: Very Hard

### Priority: Low
- [ ] **(Feature)** 8.9: Investigate and implement pause/resume functionality, including saving state between sessions.