Filename: Focus 6.2.0.md  
To: The Consultant / Implementer  
From: Praxis  
Last Updated: 2025-08-02 06:33 CEST  
Version: 6.2.0  
State: Directive  
Signed: Praxis

### **Subject: Final Approved Strategy for Settings System Refactor (Ref: Focus 6.1.3.md)**

### **1.0 Approval and Refinement**

The analysis and implementation strategy presented in Focus 6.1.3.md has been reviewed and is **approved**. The proposed hierarchical data model and phased implementation approach are robust and elegant.

One strategic refinement has been made by the Architect:

* **No Backward Compatibility Required:** As the application has no existing user base, the requirement for migrating old user-settings.xml files is removed. The implementation can be simplified by assuming a clean slate. If an old settings file is found, it can be ignored or overwritten.

### **2.0 Final Implementation Plan**

This is the definitive plan for the settings system refactor.

**Phase 1: Implement the Hierarchical Data Model**

* Refactor Settings.cs to implement the new object model as specified in Focus 6.1.3.md, section 2.1.  
* The model will consist of a top-level ApplicationSettings class containing two ModeSettings objects (MonoMode, StereoMode), each of which contains two CompressionSettings objects (Main, AdvancedOverride).  
* The old static properties in Settings.cs should be removed.

**Phase 2: Implement Persistence**

* Update the XML serialization logic to save and load the new, complete ApplicationSettings object.  
* The resulting user-settings.xml file should match the hierarchical structure outlined in Focus 6.1.3.md, section 2.2.  
* No logic for migrating old settings files is necessary.

**Phase 3: Refactor UI Data Binding**

* Re-wire the UI controls in MainWindow.xaml to bind to the new hierarchical data model.  
* Implement dynamic context switching as proposed, ensuring the correct UI panels are bound to the correct settings objects (MonoMode.Main, StereoMode.AdvancedOverride, etc.) based on the user's selections.  
* The use of INotifyPropertyChanged is recommended for robust data binding.

**Phase 4: Code Cleanup**

* Once the new system is fully functional, remove all legacy code, including the old static settings properties and any commented-out code related to the previous implementation.

### **3.0 Success Criteria**

The refactor will be considered complete when the following criteria are met:

* \[ \] The four settings contexts (Mono.Main, Mono.AdvancedOverride, Stereo.Main, Stereo.AdvancedOverride) are persisted independently and correctly between sessions.  
* \[ \] There is no cross-contamination or unintended synchronization of settings between the four contexts.  
* \[ \] The state of the contextual radio buttons persists correctly for each mode.  
* \[ \] All existing ComboBox validation logic continues to function correctly with the new data bindings.

*(Note: The "Documentation Mandate" as defined in the AI-Collaboration-SOP.md applies to this task.)*