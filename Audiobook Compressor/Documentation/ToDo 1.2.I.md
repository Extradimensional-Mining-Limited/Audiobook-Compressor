Filename: ToDo.md  
Last Updated: 2025-08-16 01:48 AM CEST  
Version: 1.2.I  
State: Experimental  
Signed: Telos  
Synopsis:  
A strategically re-architected project roadmap, organizing tasks into development themes to provide a clearer view of priorities and goals.

### **Project Roadmap & To-Do List \- Version 1.2.I**

This document outlines the strategic roadmap for the Audiobook Compressor project. Tasks are grouped into **Development Themes** to align our work with major project goals.

### **Theme 1: Architectural Refactoring**

Tasks focused on improving the core structure, maintainability, and stability of the codebase.  
Priority: Critical

* \[X\] **(Refactor)** \#31: Refactor MainWindow from Code-Behind to MVVM pattern to improve maintainability.  
* \[ \] **(Refactor)** \#30: Post-MVVM: Progressively refactor code clusters (e.g., validation, file helpers) into separate, single-purpose modules.

### **Theme 2: Robustness & User Safety**

Tasks focused on input validation, error handling, and pre-processing checks to make the application trustworthy and safe to use.  
Priority: High

* \[ \] **(Bug)** \#1C1: Implement validation for file/folder path permissions, length, and special characters.  
* \[ \] **(Bug)** \#1C3: Add a warning with a per-job "do not ask again" option to prevent silent overwriting of existing files.  
* \[ \] **(Bug)** \#2C2: Integrate FFmpeg/FFprobe tool validation into the application startup process.  
* \[ \] **(Bug)** \#14: Add settings validation on load to gracefully handle corrupted or malformed user-settings.xml files.  
* \[ \] **(Bug)** \#18: Improve JSON parsing robustness in AudioProcessor to handle potential malformed FFprobe output.  
* \[ \] **(Bug)** \#2C1: Refactor all try/catch blocks to log exceptions and surface meaningful errors to the user (dependency for Theme 4).

**Priority: Medium**

* \[ \] **(Bug)** \#11: Add validation for SelectedAction property values ("Copy", "Convert", "Advanced") loaded from settings.  
* \[ \] **(Bug)** \#4C1: Add error handling and user notifications for settings save/load failures.  
* \[ \] **(Bug)** \#2C3: Add handling for file system edge cases (locked files, symbolic links, network paths).

### **Theme 3: Core Features & Enhancements**

Tasks focused on implementing new functionality or significantly improving existing features.  
Priority: High

* \[ \] **(Feature)** \#13: Implement settings schema versioning and migration logic to ensure long-term compatibility.  
* \[ \] **(Feature)** \#27: Implement a full settings snapshot feature with a Set | Restore | Default UI.  
* \[ \] **(Feature)** \#17: Implement complete FFmpeg command generation to support 2-pass and VBR encoding options.

**Priority: Medium**

* \[ \] **(Feature)** \#1C8: Implement UI controls and logic for VBR and multi-pass encoding options.  
* \[ \] **(Feature)** \#8.3: Offer more granular, per-file progress reporting in the UI.

**Priority: Low**

* \[ \] **(Feature)** \#8.4: Allow user to select audio codec and other advanced FFmpeg options.  
* \[ \] **(Feature)** \#8.9: Investigate and implement pause/resume functionality with state persistence.

### **Theme 4: Logging & Diagnostics**

A dedicated theme for implementing a professional, structured logging framework.  
Priority: High

* \[ \] **(Feature)** \#26: Implement the Serilog framework for structured logging.  
* \[ \] **(Feature)** \#8.10: Add a UI setting to enable verbose, diagnostic-level logging.  
* \[ \] **(Feature)** \#26.1: Implement PII (Personally Identifiable Information) anonymization for log files to protect user privacy.

### **Theme 5: UI/UX Bug Fixes & Polish**

Tasks focused on improving the user experience by fixing UI bugs and refining the interface.  
Priority: High

* \[ \] **(Bug)** \#33: Advanced panel visibility fails to update correctly on primary mode switch.  
* \[ \] **(Bug)** \#23: Ensure editable ComboBoxes commit their value to the settings model on focus loss.  
* \[ \] **(Bug)** \#24: Re-implement validation and sanitization for user-entered bitrate strings.  
* \[ \] **(Bug)** \#25: Remove the faulty and unreliable bitrate/threshold collision detection logic.

**Priority: Medium**

* \[ \] **(UI)** \#29: Implement non-modal error indicators (e.g., warning icons) for validation issues.  
* \[ \] **(Feature)** \#32: Retool the info/log panel to contain two distinct sections: a persistent, live verbal summary of settings, and the standard log window below it.  
* \[ \] **(Feature)** \#1C5: Refine all user-facing status and log messages for better clarity.  
* \[ \] **(Feature)** \#1C4: Implement an "Are you sure?" confirmation dialog for the cancel action.

**Priority: Low**

* \[ \] **(UI)** \#28: Render default radio button text in bold for clarity.  
* \[ \] **(Bug)** \#3C3: Investigate and fix window resizing and layout issues.  
* \[ \] **(Feature)** \#6C1: Test and improve keyboard navigation and tab order.

### **Theme 6: Backlog & Future Considerations**

*Tasks that are valuable but not on the immediate roadmap.*

* \[ \] **(Feature)** \#8.7: Consider packaging as a single-file executable for distribution.  
* \[ \] **(Feature)** \#6C2: Add explicit support for screen readers and accessibility tools.  
* \[ \] **(Feature)** \#8.5: Add localization support for non-English users.  
* \[ \] **(Process)** \#22: Establish formal UI/Settings interaction testing procedures for release milestones.  
* \[ \] **(Process)** \#34: Rename physical test project folders to align with standardized naming convention (\[ProjectName\].Tests.\[Purpose\]).

### **Completed in 1.2.E**

* \[X\] **(Feature)** 5.0.0: Implement core processing logic for contextual file handling in AudioProcessor  
* \[X\] **(Bug)** 15: Bridge disconnect between hierarchical UI settings and AudioProcessor implementation  
* \[X\] **(Bug)** 16: Replace hardcoded static Settings usage with hierarchical model in AudioProcessor  
* \[X\] **(Bug)** 19: Integrate hierarchical settings model with AudioProcessor decision-making logic  
* \[X\] **(Bug)** \#1C6: Implement contextual mono-to-stereo conversion logic in AudioProcessor  
* \[X\] **(Refactor)** 8.0.1: Implement terminology and naming convention refactor  
* \[X\] **(Feature)** 8.1: Add unit and integration tests for core logic in AudioProcessor  
* \[X\] **(Testing)** 21: Implement automated integration testing for UI-to-processing workflow