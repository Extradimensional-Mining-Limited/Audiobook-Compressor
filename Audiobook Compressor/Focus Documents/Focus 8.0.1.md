Filename: Focus 8.0.1.md  
To: The Implementer (Advisor)  
From: Praxis  
Last Updated: 2025-08-04 10:13 CEST  
Version: 8.0.1  
State: Directive  
Signed: Praxis

### **Subject: Terminology and Naming Convention Refactor**

### **1.0 Objective**

To improve the clarity, readability, and maintainability of the codebase by refactoring ambiguous, verbose, or confusing names for variables, properties, and UI controls. The goal is to make the code as self-documenting as possible, in accordance with our "Clarity Through Unambiguous Naming" principle.

This is a pure refactoring task. **No functional logic should be changed.**

### **2.0 Required Changes**

You are to perform a project-wide "find and replace" for the following terms across all relevant files (.cs, .xaml).

**2.1 Settings Model (Settings.cs):**

* **Find:** DefaultMonoCopyThreshold  
* **Replace with:** DefaultConversionThreshold  
  * *Rationale: The threshold is used in both mono and stereo contexts.*  
* **Find:** AdvancedOverride (as a public property name within the ModeSettings class)  
* **Replace with:** Advanced  
  * *Rationale: The "Override" is implied by the context. This is more concise.*  
* **Find:** \_advancedOverride (as a private backing field)  
* **Replace with:** \_advanced  
  * *Rationale: To maintain consistency with the public property name.*  
* **Note:** The property SelectedAction should remain as is.

**2.2 UI Panels (MainWindow.xaml and MainWindow.xaml.cs):**

* **Find:** MonoModeOptionsPanel  
* **Replace with:** MonoModePanel  
  * *Rationale: "Options" is redundant.*  
* **Find:** StereoModeOptionsPanel  
* **Replace with:** StereoModePanel  
  * *Rationale: "Options" is redundant.*  
* **Find:** AdvancedStereoOverridePanel  
* **Replace with:** MonoModeAdvancedPanel  
  * *Rationale: This panel is for the advanced settings within Mono Mode. The new name correctly reflects its location and removes the confusing "Stereo" reference.*  
* **Find:** AdvancedMonoOverridePanel  
* **Replace with:** StereoModeAdvancedPanel  
  * *Rationale: This panel is for the advanced settings within Stereo Mode. The new name correctly reflects its location and removes the confusing "Mono" reference.*

**2.3 UI Controls (MainWindow.xaml and MainWindow.xaml.cs):**

* You must rename all ComboBox controls within the advanced panels to match their new parent panel's name.  
* **Example for Mono Mode's advanced panel:**  
  * AdvancedStereoChannelsComboBox \-\> MonoAdvancedChannelsComboBox  
  * AdvancedStereoBitrateComboBox \-\> MonoAdvancedBitrateComboBox  
  * ...and so on for all controls in this panel.  
* **Example for Stereo Mode's advanced panel:**  
  * AdvancedMonoChannelsComboBox \-\> StereoAdvancedChannelsComboBox  
  * AdvancedMonoBitrateComboBox \-\> StereoAdvancedBitrateComboBox  
  * ...and so on for all controls in this panel.

### **3.0 Documentation Mandate**

As per the AI-Collaboration-SOP.md, you are required to perform the following as part of the same commit:

1. Update the file headers of all modified files with a new version, timestamp, and a synopsis describing this refactoring.  
2. Add a corresponding entry to ChangelogExperimental.md detailing this refactoring work.