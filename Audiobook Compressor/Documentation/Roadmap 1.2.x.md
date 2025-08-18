Filename: Roadmap 1.2.x.md  
Last Updated: 2025-08-17 04:43 AM CEST  
Version: 1.2.J  
State: Experimental  
Signed: Telos  
Synopsis:  
A strategic roadmap for the development cycles leading to the 1.3.0 release, including a complexity analysis for each cycle.

### **Complexity Analysis of the 1.2.x Roadmap**

This document provides a strategic assessment of the likely complexity for the tasks outlined in our roadmap to version 1.3.0.

### **Current Cycle: 1.2.K \- Final Modularization**

* **Overall Complexity:** **Medium-High**. The main refactoring is straightforward, but the "gremlin hunt" introduces significant uncertainty.  
* **Tasks:**  
  * **(Refactor) \#30: Post-MVVM Modularization (Phases 3 & 4\)**  
    * **Complexity:** **Medium**  
    * *Rationale:* The remaining services (RadioButtonStateService, PathManagementService) are less complex and more isolated than the SettingsBindingService. The patterns for this work are now well-established.  
  * **(Bug) \#33: Advanced panel visibility gremlin**  
    * **Complexity:** **High**  
    * *Rationale:* This bug has proven to be elusive. Its persistence suggests a deep, non-obvious root cause, likely related to the intricacies of the WPF UI update and data-binding lifecycle. The investigation is more complex than the fix itself.  
  * **(Process) \#34: Rename physical test project folders**  
    * **Complexity:** **Low**  
    * *Rationale:* While not technically difficult, this task requires careful, manual changes to the file system and solution files, which can be tedious and error-prone if rushed.  
  * **(Bug) \#35: Stereo mode default radio button**  
    * **Complexity:** **Trivial**  
    * *Rationale:* This is likely a single-line change in the settings model's default value initialization.

### **Upcoming Cycle: 1.2.L \- Input & Settings Robustness**

* **Overall Complexity:** **Medium**. This cycle involves a series of related, moderately complex validation tasks.  
* **Tasks:**  
  * **(Bug) \#23: ComboBox focus loss bug**  
    * **Complexity:** **Medium**  
    * *Rationale:* Requires a solid understanding of WPF focus events and how they interact with the data-binding system to ensure values are committed correctly.  
  * **(Bug) \#24 & \#25: Bitrate validation logic**  
    * **Complexity:** **Medium**  
    * *Rationale:* These tasks are intertwined. Ripping out the old, unreliable logic and implementing new, robust sanitization in the ValidationService will require careful design and thorough testing.  
  * **(Bug) \#14 & \#11: Settings validation on load**  
    * **Complexity:** **Medium**  
    * *Rationale:* This involves making the SettingsService more defensive, adding logic to gracefully handle corrupted or unexpected values in the user-settings.xml file without crashing.

### **Upcoming Cycle: 1.2.M \- File System & External Tool Safety**

* **Overall Complexity:** **High**. This cycle is focused on external interactions (file system, FFmpeg), which are inherently less predictable than internal application logic.  
* **Tasks:**  
  * **(Bug) \#1C1 & \#2C3: File system edge cases**  
    * **Complexity:** **High**  
    * *Rationale:* Properly handling all possible file system issues (permissions, network paths, locked files, symbolic links) is a complex and demanding task that requires extensive, defensive coding.  
  * **(Bug) \#1C3: Overwrite warning**  
    * **Complexity:** **Medium**  
    * *Rationale:* Involves UI work with the DialogService and logic to manage the "do not ask again" state, which will need to be persisted in the settings.  
  * **(Bug) \#2C2: FFmpeg/FFprobe validation**  
    * **Complexity:** **Medium**  
    * *Rationale:* Requires shelling out to an external process, parsing its output, and gracefully handling the case where the tools are not found or are the wrong version.  
  * **(Bug) \#18: JSON parsing robustness**  
    * **Complexity:** **Medium**  
    * *Rationale:* This involves making the AudioProcessor's FFprobe parsing logic more resilient to unexpected or malformed JSON output, which requires robust error handling.

### **Upcoming Cycle: 1.2.N \- Final Polishing Pass**

* **Overall Complexity:** **Low-Medium**. This cycle bundles several smaller, well-defined tasks.  
* **Tasks:**  
  * **(Bug) \#2C1: Refactor all try/catch blocks**  
    * **Complexity:** **Medium**  
    * *Rationale:* While not technically difficult, this is a broad task that requires touching many parts of the codebase to ensure a consistent error handling and logging strategy is applied everywhere.  
  * **(Bug) \#4C1: Settings save/load error handling**  
    * **Complexity:** **Low**  
    * *Rationale:* A smaller, more focused version of the task above, limited to the SettingsService.  
  * **(Feature) \#1C4: "Are you sure?" confirmation**  
    * **Complexity:** **Low**  
    * *Rationale:* A straightforward implementation using the existing DialogService.  
  * **(UI) \#28: Render default radio button text in bold**  
    * **Complexity:** **Trivial**  
    * *Rationale:* A simple change to a XAML style or template.

### **Final Cycle: 1.2.O \- Strategic Code Review**

* **Overall Complexity:** **Process-Oriented**. The complexity of this cycle lies in the analysis and synthesis performed by the consultant, not in a traditional implementation task.