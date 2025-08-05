Filename: ChangelogExperimental.md  
Last Updated: 2025-08-05 13:32 CEST
Version: 1.2.C  
State: Experimental  
Signed: Vanguard

# **Experimental Changelog**

This file contains a detailed, granular log of all changes made on the experimental branch.

## **\[Unreleased\]**

### **Added**
- 1.2.B: feat: Created ProcessingContext class to bridge UI hierarchical settings with AudioProcessor logic per Focus 5.0.0 directive.
- 1.2.B: feat: Implemented complete Focus 5.0.0 contextual file handling decision tree in AudioProcessor with support for all 6 user scenarios.
- 1.2.B: feat: Added ProcessAsException method for handling mismatched file types (stereo files in mono mode, mono files in stereo mode).
- 1.2.B: feat: Implemented HandleUpmixLogic method for mono-to-stereo conversion with threshold-based quality decisions.
- 1.2.B: feat: Added contextual settings integration where radio button states and advanced overrides directly influence processing behavior.

### **Changed**
- 1.2.A: refactor: Remove all conditional sync logic and session flags for main/advanced/mono/stereo settings. All ComboBox and related values are now simply persistent and user-driven. No automatic copying or inheritance between settings objects; values only change via user action.
- 1.2.A: removed: All persistence and code references for AdvancedStereoOverrideSettings and AdvancedMonoOverrideSettings. Only settings-specific logic for these advanced overrides was affected; all other app logic is untouched.
- 1.2.A: refactor: Implement hierarchical data model in Settings.cs with CompressionSettings, ModeSettings, and ApplicationSettings classes per Focus 6.2.0.md directive.
- 1.2.A: feat: Enhanced XML persistence in MainWindow.xaml.cs to support complete hierarchical settings structure with save/load for all four contexts.
- 1.2.A: feat: Complete Phase 3 UI binding refactor with dynamic context switching, radio button state management, and advanced override panel binding.
- 1.2.A: cleanup: Fixed ComboBox ambiguity issues and completed Phase 4 code cleanup for hierarchical settings system implementation.
- 1.2.A: fix: Added missing utility methods and RestoreUIFromSettings functionality to complete Focus 6.2.0 implementation.
- 1.2.A: feat: Added SelectedAction property to ModeSettings class to track radio button state for persistence per Focus 7.0.0 directive.
- 1.2.A: refactor: Replaced procedural radio button logic with direct data binding to SelectedAction property in Settings model.
- 1.2.A: feat: Enhanced XML persistence to save and load SelectedAction property for both MonoMode and StereoMode.
- 1.2.A: refactor: Simplified UI restoration logic using property-based approach instead of complex procedural methods.
- 1.2.B: refactor: Completely restructured AudioProcessor.ProcessAudioFileAsync to use ProcessingContext instead of static Settings properties.
- 1.2.B: refactor: Replaced hardcoded FFmpeg command generation with flexible BuildFFmpegCommand method supporting contextual settings.

### **Fixed**
- 1.2.A: fix: Corrected variable name casing issues in InitializeComboBoxes method that caused build errors with MonoModeOptionsPanel and StereoModeOptionsPanel references.
- 1.2.A: fix: Added missing ItemsSource assignments for SampleRateComboBox, BitrateControlComboBox, and all 12 advanced override ComboBoxes to resolve empty dropdown issues.
- 1.2.A: fix: Corrected method call order in constructor to ensure ComboBoxes have ItemsSource before RestoreUIFromSettings attempts to set values.
- 1.2.A: fix: Enhanced RestoreUIFromSettings logic to properly restore UI state from hierarchical settings including mode selection, radio button states, and panel visibility.
- 1.2.A: fix: Updated legacy static property usage in SampleRate_SelectionChanged and BitrateControl_SelectionChanged to use hierarchical settings structure.
- 1.2.A: fix: Removed duplicate event handlers and consolidated ComboBox event handling to prevent conflicts and ensure proper hierarchical settings management.
- 1.2.A: fix: Resolved radio button state persistence issues where selected action would not persist correctly when switching between modes.
- 1.2.B: fix: Resolved critical architecture disconnect where UI settings had no effect on processing behavior.
- 1.2.B: fix: Implemented proper JSON parsing in ProbeAudioFileAsync to populate AudioFileInfo.Bitrate property.
- 1.2.C: fix: Applied naming convention refactor per Focus 8.0.2 and 9.9.0 - renamed DefaultMonoCopyThreshold to DefaultConversionThreshold, updated all UI panels and ComboBox controls to contextually appropriate names, and implemented seamless XML settings migration.

**Note**: GetTime.exe failure reported - timestamp shows as TIMESTAMP_ERROR for version 1.2.B changes.