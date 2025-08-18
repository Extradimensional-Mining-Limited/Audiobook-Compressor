Filename: ToDo.md  
Last Updated: 2025-08-18 04:50 AM CEST  
Version: 1.2.K  
State: Experimental  
Signed: Telos  
Synopsis:  
An actionable checklist for the development cycles leading up to the 1.3.0 release, organized by cycle.

### **Project Roadmap & To-Do List \- Version 1.2.K**

This document outlines the strategic roadmap for the Audiobook Compressor project, organized as an actionable checklist for the development cycles leading to the 1.3.0 release.

### **Task Legend**

* (Refactor): Improving the internal structure of existing code.  
* (Bug): Fixing an error or incorrect behavior.  
* (Feature): Implementing new functionality.  
* (UI): A change focused on the user interface.  
* (Process): A task related to improving our development workflow.  
* (Testing): A task related to creating or improving tests.

### **Current Cycle: 1.2.L \- Input & Settings Robustness**

* **Objective:** Secure the application's "front door" by addressing all bugs related to user input and settings file integrity.  
* **"Gremlin Hunt":** Continue the investigation and attempted fix for persistent UI bugs.  
* **Primary Tasks:**  
  * \[ \] **(Bug)** \#24: Re-implement validation and sanitization for user-entered bitrate strings.  
  * \[ \] **(Bug)** \#25: Remove the faulty and unreliable bitrate/threshold collision detection logic.  
  * \[ \] **(Bug)** \#14: Add settings validation on load to gracefully handle corrupted or malformed user-settings.xml files.  
  * \[ \] **(Bug)** \#11: Add validation for SelectedAction property values ("Copy", "Convert", "Advanced") loaded from settings.  
* **Gremlin Hunt Tasks:**  
  * \[ \] **(Bug)** \#33: Advanced panel visibility fails to update correctly on primary mode switch.  
  * \[ \] **(Bug)** \#23: Refine ComboBox focus loss behavior to commit value when clicking on empty window space.

### **Upcoming Cycle: 1.2.M \- File System & External Tool Safety**

* **Objective:** Make the application's interaction with the file system and external tools (FFmpeg) safe and reliable.  
* \[ \] **(Bug)** \#1C1: Implement validation for file/folder path permissions, length, and special characters.  
* \[ \] **(Bug)** \#1C3: Add a warning with a per-job "do not ask again" option to prevent silent overwriting of existing files.  
* \[ \] **(Bug)** \#2C2: Integrate FFmpeg/FFprobe tool validation at startup.  
* \[ \] **(Bug)** \#18: Improve robustness of JSON parsing from FFprobe.  
* \[ \] **(Bug)** \#2C3: Add handling for file system edge cases (locked files, etc.).

### **Upcoming Cycle: 1.2.N \- Final Polishing Pass**

* **Objective:** Address any remaining medium/low priority bugs.  
* \[ \] **(Bug)** \#2C1: Refactor all try/catch blocks to log exceptions and surface meaningful errors.  
* \[ \] **(Bug)** \#4C1: Add error handling for settings save/load failures.  
* \[ \] **(Feature)** \#1C4: Implement an "Are you sure?" confirmation for the cancel action.  
* \[ \] **(UI)** \#28: Render default radio button text in bold for clarity.

### **Final Cycle: 1.2.O \- Strategic Code Review**

* **Objective:** Conduct a deep code review to identify remaining technical debt and inform the roadmap for 1.3.x.  
* \[ \] **(Process)** Vanguard will perform a comprehensive review of the entire codebase.  
* \[ \] **(Process)** The findings will be documented in a new Review.md file.  
* \[ \] **(Process)** The ToDo.md will be updated with any new tasks identified during the review.

### **Future Cycles (Post-1.3.0 Backlog)**

*Tasks that are valuable but are not on the immediate roadmap for the 1.2.x series.*

* **Theme: Core Features & Enhancements**  
  * \[ \] **(Feature)** \#13: Implement settings schema versioning and migration logic.  
  * \[ \] **(Feature)** \#27: Implement a full settings snapshot feature with a Set | Restore | Default UI.  
  * \[ \] **(Feature)** \#17: Implement complete FFmpeg command generation for 2-pass and VBR.  
  * \[ \] **(Feature)** \#1C8: Implement UI controls and logic for VBR and multi-pass encoding.  
  * \[ \] **(Feature)** \#8.3: Offer more granular, per-file progress reporting in the UI.  
  * \[ \] **(Feature)** \#8.4: Allow user to select audio codec and other advanced FFmpeg options.  
  * \[ \] **(Feature)** \#8.9: Investigate and implement pause/resume functionality.  
* **Theme: Logging & Diagnostics**  
  * \[ \] **(Feature)** \#26: Implement the Serilog framework for structured logging.  
  * \[ \] **(Feature)** \#8.10: Add a UI setting to enable verbose, diagnostic-level logging.  
  * \[ \] **(Feature)** \#26.1: Implement PII anonymization for log files.  
* **Theme: UI/UX Bug Fixes & Polish**  
  * \[ \] **(UI)** \#29: Implement non-modal error indicators for validation issues.  
  * \[ \] **(Feature)** \#32: Retool the info/log panel with a verbal summary section.  
  * \[ \] **(Feature)** \#1C5: Refine all user-facing status and log messages for clarity.  
  * \[ \] **(Bug)** \#3C3: Investigate and fix window resizing and layout issues.  
  * \[ \] **(Feature)** \#6C1: Test and improve keyboard navigation and tab order.  
* **Theme: Backlog & Future Considerations**  
  * \[ \] **(Feature)** \#8.7: Consider packaging as a single-file executable.  
  * \[ \] **(Feature)** \#6C2: Add explicit support for screen readers.  
  * \[ \] **(Feature)** \#8.5: Add localization support.  
  * \[ \] **(Process)** \#22: Establish formal UI/Settings interaction testing procedures.

### **Completed**

* \[X\] **(Refactor)** \#30: Post-MVVM: Progressively refactor code clusters.  
* \[X\] **(Process)** \#34: **(Manual Task)** Rename physical test project folders.  
* \[X\] **(Bug)** \#37: Review and correct default radio button logic.  
* \[X\] **(Bug)** \#36: Refactor settings summary to consistently display main settings followed by the full label of the selected action.  
* \[X\] **(Bug)** \#35: Stereo mode default radio button is incorrect; should be "Copy mono files".  
* \[X\] **(Refactor)** \#31: Refactor MainWindow from Code-Behind to MVVM pattern.  
* \[X\] **(Feature)** 5.0.0: Implement core processing logic for contextual file handling.  
* \[X\] **(Bug)** 15: Bridge disconnect between hierarchical UI settings and AudioProcessor.  
* \[X\] **(Bug)** 16: Replace hardcoded static Settings usage with hierarchical model.  
* \[X\] **(Bug)** 19: Integrate hierarchical settings model with AudioProcessor decision-making.  
* \[X\] **(Bug)** \#1C6: Implement contextual mono-to-stereo conversion logic.  
* \[X\] **(Refactor)** 8.0.1: Implement terminology and naming convention refactor.  
* \[X\] **(Feature)** 8.1: Add unit and integration tests for core logic.  
* \[X\] **(Testing)** 21: Implement automated integration testing for UI-to-processing workflow.