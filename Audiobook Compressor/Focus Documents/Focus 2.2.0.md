Filename: Focus 2.2.0.md  
To: Sir Fixalot  
Last Updated: 2025-07-25 01:23 CEST  
Version: 2.2.0  
State: Directive  
Signed: Praxis

### **A Note on Our Collaboration**

Hello Sir Fixalot,

This document outlines a specific development task. It is part of a new collaborative process between the user (the Architect), you (the Implementer), and myself, Praxis (the Strategist).

**Our Workflow:**

1. The user and I will collaborate to define a feature or fix, resulting in a detailed directive like this one.  
2. Your task is to analyze this directive and the current codebase.  
3. You will then provide a report back to me, outlining your implementation plan and highlighting any conflicts or issues you foresee between the directive and the existing code.  
4. I will analyze your report, and together with the user, we will refine the directive.  
5. Once the plan is finalized, you will implement the code.

Please review the following objective and provide your analysis.

## **Objective: Implement the Basic UI Layout for Contextual File Handling**

The goal is to refactor the current UI to implement a simple, stacked layout for contextual file handling. The final layout will feature the main compression settings, followed by a set of contextual radio buttons, and finally an optional panel for override settings that appears directly below the radio buttons when "Advanced..." is selected. All elements should be left-aligned in a single column. This directive focuses on achieving the correct XAML structure and visibility logic, deferring the complex processing logic for a later stage.

### **1\. XAML Layout Changes (MainWindow.xaml)**

**A. Deconstruct the Current Implementation:**

* **Remove the Expander:** Delete the Expander control currently used for "Advanced Override Settings".  
* **Remove the inner GroupBox:** Delete the GroupBox labeled "Override Compression Settings" that is inside the current Expander.  
* The goal is a flat hierarchy with no nested boxes or expanders.

**B. Create the New Layout Structure:**

* **Locate Target Area:** All new controls should be placed within the main "Compression Settings" GroupBox, directly below the WrapPanel that holds the primary compression settings.  
* **Contextual Panels:** Create two StackPanels. Only one will be visible at a time.  
  * **MonoModeOptionsPanel:**  
    * This panel is visible only when the main "Output Format" is "Mono".  
    * **First child:** A WrapPanel containing the three RadioButton controls (Copy stereo files, Convert stereo to mono, Advanced...).  
    * **Second child:** A WrapPanel named AdvancedStereoOverridePanel. This panel contains the complete, duplicate set of override compression controls. Its visibility is controlled by the "Advanced..." radio button.  
  * **StereoModeOptionsPanel:**  
    * This panel is visible only when the main "Output Format" is "Stereo".  
    * **First child:** A WrapPanel containing the three RadioButton controls (Copy mono files, Convert mono to stereo, Advanced...).  
    * **Second child:** A WrapPanel named AdvancedMonoOverridePanel. This panel contains the complete, duplicate set of override compression controls. Its visibility is controlled by the "Advanced..." radio button.

**C. Final Layout Requirements:**

* **Alignment:** All elements (the main settings, the radio buttons, and the override settings) must share the same left alignment. There should be no indentation.  
* **Visibility:** The override settings panels (AdvancedStereoOverridePanel and AdvancedMonoOverridePanel) must only be visible when their corresponding "Advanced..." radio button is checked. This should be handled with a BooleanToVisibilityConverter.

### **2\. UI Control Logic (ViewModel / Code-Behind)**

**A. Panel Visibility:**

* The Visibility of MonoModeOptionsPanel is Visible if the main selection is "Mono", and Collapsed otherwise.  
* The Visibility of StereoModeOptionsPanel is Visible if the main selection is "Stereo", and Collapsed otherwise.

**B. Data Binding and State Management:**

* The controls inside AdvancedStereoOverridePanel must be bound to a dedicated AdvancedStereoOverrideSettings object.  
* The controls inside AdvancedMonoOverridePanel must be bound to a dedicated AdvancedMonoOverrideSettings object.  
* **Implement the "First Open Sync" logic as previously specified:** When an advanced panel is opened for the first time in a session *after* the main settings have been changed, its values should be synced from the main settings. Otherwise, it should retain its own persisted state.

### **3\. Core Processing Logic (AudioProcessor.cs)**

* For this phase, the core logic does not need to be fully implemented. The primary goal is to ensure the UI is built correctly and the settings are bound to the appropriate data objects. A placeholder implementation is sufficient for now.