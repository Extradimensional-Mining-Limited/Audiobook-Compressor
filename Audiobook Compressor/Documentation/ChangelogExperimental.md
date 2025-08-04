Filename: ChangelogExperimental.md  
Version: 1.2.A  
State: Experimental  
Signed: Praxis

# **Experimental Changelog**

This file contains a detailed, granular log of all changes made on the experimental branch.

## **\[Unreleased\]**

### **Added**

### **Changed**
- 1.2.A: refactor: Remove all conditional sync logic and session flags for main/advanced/mono/stereo settings. All ComboBox and related values are now simply persistent and user-driven. No automatic copying or inheritance between settings objects; values only change via user action.
- 1.2.A: removed: All persistence and code references for AdvancedStereoOverrideSettings and AdvancedMonoOverrideSettings. Only settings-specific logic for these advanced overrides was affected; all other app logic is untouched.
- 1.2.B: refactor: Implement hierarchical data model in Settings.cs with CompressionSettings, ModeSettings, and ApplicationSettings classes per Focus 6.2.0.md directive.
- 1.2.B: feat: Enhanced XML persistence in MainWindow.xaml.cs to support complete hierarchical settings structure with save/load for all four contexts.
- 1.2.B: feat: Complete Phase 3 UI binding refactor with dynamic context switching, radio button state management, and advanced override panel binding.
- 1.2.B: cleanup: Fixed ComboBox ambiguity issues and completed Phase 4 code cleanup for hierarchical settings system implementation.
- 1.2.B: fix: Added missing utility methods and RestoreUIFromSettings functionality to complete Focus 6.2.0 implementation.

### **Fixed**
- 1.2.B: fix: Corrected variable name casing issues in InitializeComboBoxes method that caused build errors with MonoModeOptionsPanel and StereoModeOptionsPanel references.
- 1.2.B: fix: Added missing ItemsSource assignments for SampleRateComboBox, BitrateControlComboBox, and all 12 advanced override ComboBoxes to resolve empty dropdown issues.
- 1.2.B: fix: Corrected method call order in constructor to ensure ComboBoxes have ItemsSource before RestoreUIFromSettings attempts to set values.
- 1.2.B: fix: Enhanced RestoreUIFromSettings logic to properly restore UI state from hierarchical settings including mode selection, radio button states, and panel visibility.
- 1.2.B: fix: Updated legacy static property usage in SampleRate_SelectionChanged and BitrateControl_SelectionChanged to use hierarchical settings structure.
- 1.2.B: fix: Removed duplicate event handlers and consolidated ComboBox event handling to prevent conflicts and ensure proper hierarchical settings management.

**Note**: GetTime.exe failure reported - timestamp shows as TIMESTAMP_ERROR for version 1.2.B changes.