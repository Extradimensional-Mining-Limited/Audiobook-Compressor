Filename: Focus 7.0.0.md  
To: The Consultant (Claude)  
From: Praxis  
Last Updated: 2025-08-04 01:46 CEST  
Version: 7.0.0  
State: Directive  
Signed: Praxis

### **Subject: Finalize and Stabilize the Settings Persistence and UI Binding Logic**

### **1.0 Context & Objective**

The project's settings management system is currently in a fragile state. While the core hierarchical data model is in place, there are critical bugs related to state persistence and UI data binding that are causing unpredictable behavior. Specifically:

1. The selected state of the contextual radio buttons (Copy, Convert, Advanced...) does not persist correctly for the inactive mode.  
2. The advanced override panels load default values when opened, instead of their correctly persisted values that have been loaded into memory.

Your objective is to resolve these issues by implementing a simple, robust persistence and binding strategy. The guiding principle is that the UI must be a direct and honest reflection of the data model at all times. **Any complex procedural code that attempts to infer or manually manage UI state should be removed in favor of direct WPF data binding.**

### **2.0 Required Data Model Enhancement (Settings.cs)**

To solve the persistence issue with the radio buttons, the data model must be enhanced to store this state.

**2.1 Update ModeSettings Class:**

* Add a new property to the ModeSettings class to store the selected action for that mode.

public class ModeSettings  
{  
    public CompressionSettings Main { get; set; }  
    public CompressionSettings AdvancedOverride { get; set; }  
    // New property to store the state of the radio buttons for this mode.  
    // Valid values should be "Copy", "Convert", or "Advanced".  
    public string SelectedAction { get; set; }  
}

**2.2 Update Persistence:**

* The serialization logic for user-settings.xml must be updated to save and load this new SelectedAction property for both MonoMode and StereoMode.

### **3.0 UI Binding and Logic Requirements (MainWindow.xaml & MainWindow.xaml.cs)**

The core of this task is to ensure the UI is correctly and directly bound to the data model.

**3.1 Radio Button Binding:**

* The IsChecked property of each of the three radio buttons in the MonoModeOptionsPanel must be bound to the Settings.MonoMode.SelectedAction property (using a converter to match the string value, e.g., "Copy").  
* Similarly, the radio buttons in the StereoModeOptionsPanel must be bound to the Settings.StereoMode.SelectedAction property.  
* This will ensure their state is loaded from and saved to the settings file automatically.

**3.2 Advanced Panel Binding (Critical Fix):**

* The controls inside the AdvancedStereoOverridePanel must be **directly and permanently** bound to the Settings.MonoMode.AdvancedOverride object.  
* The controls inside the AdvancedMonoOverridePanel must be **directly and permanently** bound to the Settings.StereoMode.AdvancedOverride object.  
* **There should be no logic that loads default values into these panels when they are opened.** The application's startup procedure already loads the correct values from user-settings.xml into the settings objects. The UI must simply reflect the data that is already in memory. The problem is a binding issue, not a data loading issue.

**3.3 Code Simplification:**

* As part of this refactor, you must **remove any procedural C\# code** in MainWindow.xaml.cs that manually sets the values of the advanced panel ComboBoxes or infers the state of the radio buttons (such as the previously proposed RestoreModeState or ShouldModeBeAdvanced methods). The entire system should rely on the power of WPF data binding.

### **4.0 Success Criteria**

The task will be considered complete when the following behaviors are observed:

* \[ \] The user can select an action (e.g., "Advanced...") in Mono mode, switch to Stereo mode, and then switch back to Mono mode, and their original selection ("Advanced...") will be correctly restored.  
* \[ \] The user can modify settings in an advanced panel, close the application, and upon restarting, the advanced panel will contain their modified settings when opened, not the default values.

*(Note: The "Documentation Mandate" as defined in the AI-Collaboration-SOP.md applies to this task.)*