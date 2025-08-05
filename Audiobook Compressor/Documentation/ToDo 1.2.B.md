Filename: ToDo 1.2.B.md  
Last Updated: 2025-08-05 04:48  
Version: 1.2.B  
State: Experimental  
Signed: Advisor

Synopsis:
Updated project to-do list reflecting current development priorities following Focus 7.0.0 implementation and comprehensive code review.

---

# **Project To-Do List - Version 1.2.B**

This list reflects the current development priorities following the successful implementation of Focus 7.0.0 and comprehensive code review.

## **Process Implementation**

### **Priority: Critical**

* [X] **(Process)** 9.1: Create the AI-Collaboration-SOP.md document.
* [X] **(Process)** 9.2: Create the ChangelogExperimental.md document.
* [X] **(Process)** 9.3: Update Summary.md to reflect the new workflow and onboarding checklist.
* [X] **(Process)** 9.4: Refactor Changelog.md into a high-level release notes format.
* [X] **(Process)** 9.5: Rename AudiobookCompressor-v6.2.ps1 to blueprint.ps1.

## **Critical Architecture Issues**

### **Priority: Critical**

* [ ] **(Feature)** 5.0.0: Implement core processing logic for contextual file handling in AudioProcessor
* [ ] **(Bug)** 15: Bridge disconnect between hierarchical UI settings and AudioProcessor implementation
* [ ] **(Bug)** 16: Replace hardcoded static Settings usage with hierarchical model in AudioProcessor
* [ ] **(Feature)** 13: Implement settings schema versioning and migration logic

## **Effort: Easy**

### **Priority: High**

* [ ] **(Feature)** 8.6: Implement logging to a file for troubleshooting
* [ ] **(Feature)** 8.10: Add a setting to enable verbose, diagnostic logging
* [ ] **(Bug)** 1C6: Implement contextual mono-to-stereo conversion logic in AudioProcessor *(UI implemented, processing logic missing)*
* [ ] **(Bug)** 22: Establish manual testing procedures for settings persistence verification

### **Priority: Medium**

* [ ] **(Feature)** 1C4: Implement "Are you sure?" confirmation dialog for the cancel action
* [ ] **(Feature)** 8.2: Provide a settings reset option in the UI
* [ ] **(Bug)** 11: Add validation for SelectedAction property values ("Copy", "Convert", "Advanced")
* [ ] **(Refactor)** 8.0.1: Implement terminology and naming convention refactor

### **Priority: Low**

* [X] **(Bug)** 2C4: Review and update the filename sanitization routine
* [ ] **(Naming)** 12: Address naming inconsistency in advanced panel bindings
* [ ] **(Naming)** 20: Improve naming conventions per SOP 7.3 principle

## **Effort: Medium**

### **Priority: High**

* [ ] **(Bug)** 1C1: Implement validation for file/folder path permissions, length, and special characters
* [ ] **(Bug)** 1C3: Add warning or option to prevent silent overwriting of existing files
* [ ] **(Bug)** 2C2: Integrate FFmpeg/FFprobe tool validation into UI startup process
* [ ] **(Bug)** 3C1: Ensure no synchronous I/O or long operations are blocking the UI thread
* [ ] **(Bug)** 5C1: Audit all async operations to ensure they honor CancellationToken promptly
* [ ] **(Bug)** 5C2: Ensure all disposable resources properly cleaned up with using statements
* [ ] **(Bug)** 14: Add settings validation on load with proper error handling
* [ ] **(Bug)** 18: Improve JSON parsing robustness in AudioProcessor

### **Priority: Medium**

* [ ] **(Bug)** 4C1: Add error handling and user notifications for settings save/load failures
* [ ] **(Feature)** 1C5: Refine all user-facing status and log messages for better clarity
* [ ] **(Feature)** 8.3: Offer more granular progress reporting (per-file progress)
* [ ] **(Feature)** 8.7: Consider packaging as single-file executable for distribution

### **Priority: Low**

* [ ] **(Refactor)** 3C2: Review Dispatcher.Invoke usage and switch to BeginInvoke where appropriate
* [ ] **(Bug)** 3C3: Investigate and fix window resizing and layout issues
* [ ] **(Feature)** 6C1: Test and improve keyboard navigation and tab order

## **Effort: Hard**

### **Priority: Critical**

* [ ] **(Bug)** 19: Integrate hierarchical settings model with AudioProcessor decision-making logic

### **Priority: High**

* [ ] **(Bug)** 2C1: Refactor all try/catch blocks to log exceptions and surface meaningful errors
* [ ] **(Feature)** 17: Implement complete FFmpeg command generation (2-pass, VBR, advanced options)
* [ ] **(Bug)** 1C7: Implement proper 2-pass encoding with dual FFmpeg execution

### **Priority: Medium**

* [ ] **(Bug)** 2C3: Add handling for file system edge cases (locked files, symbolic links, network paths)
* [ ] **(Feature)** 8.1: Add unit and integration tests for core logic in AudioProcessor
* [ ] **(Feature)** 8.8: Add VBR and multi-pass ABR options to UI and processing logic
* [ ] **(Feature)** 1C8: Implement VBR and multi-pass encoding options
* [ ] **(Testing)** 21: Implement automated integration testing for UI-to-processing workflow

### **Priority: Low**

* [ ] **(Feature)** 6C2: Add explicit support for screen readers and accessibility tools
* [ ] **(Feature)** 8.4: Allow user to select audio codec and other advanced FFmpeg options
* [ ] **(Feature)** 8.5: Add localization support for non-English users

## **Effort: Very Hard**

### **Priority: Low**

* [ ] **(Feature)** 8.9: Investigate and implement pause/resume functionality with state persistence

## **Completed in 1.2.B**

* [X] **(Feature)** 9C: Implement dynamic contextual UI panels with proper visibility control
* [X] **(Bug)** 1C2: Validate and handle manual entry in editable ComboBoxes
* [X] **(Bug)** 4C2: Notify user when corrupted settings file forces default values
* [X] **(Architecture)** 7.0.0: Implement hierarchical settings persistence and direct UI binding
* [X] **(Process)** 7C1: File header consistency *(Superseded by SOP)*
* [X] **(Process)** 7C2: Changelog discipline *(Superseded by SOP)*

## **Notes**

- **Focus 5.0.0** is the highest priority as it bridges the critical gap between the sophisticated UI and the processing logic
- **Focus 8.0.1** should be implemented after 5.0.0 to improve code maintainability
- Settings schema versioning is critical for production readiness
- Manual testing procedures should be established before next major feature development

This list should be updated after implementing Focus 5.0.0 and conducting the next development cycle.