Filename: ChangelogExperimental.md  
Last Updated: 2025-08-07 12:43 CEST
Version: 1.2.E  
State: Experimental  
Signed: Vanguard

# **Experimental Changelog**

This file contains a detailed, granular log of all changes made on the experimental branch.

## **[Unreleased]**

### **Added**
- 1.2.B: feat: Created ProcessingContext class to bridge UI hierarchical settings with AudioProcessor logic per Focus 5.0.0 directive.
- 1.2.B: feat: Implemented complete Focus 5.0.0 contextual file handling decision tree in AudioProcessor with support for all 6 user scenarios.
- 1.2.B: feat: Added ProcessAsException method for handling mismatched file types (stereo files in mono mode, mono files in stereo mode).
- 1.2.B: feat: Implemented HandleUpmixLogic method for mono-to-stereo conversion with threshold-based quality decisions.
- 1.2.B: feat: Added contextual settings integration where radio button states and advanced overrides directly influence processing behavior.
- 1.2.D: feat: Advanced Panel Sub-threshold Behavior - Added Copy/Defer to Rockit/Convert to radio button controls in both Mono and Stereo advanced panels per Focus 5.0.4.
- 1.2.D: feat: "Defer to Rockit" Quality Logic - Implemented intelligent quality-preserving VBR processing with dynamic maxrate caps for channel reduction/expansion scenarios.
- 1.2.D: feat: Custom Bitrate Conversion - Added support for "Convert to:" option with user-specified target bitrate for sub-threshold files.
- 1.2.D: feat: ProcessWithAdvancedLogic method in AudioProcessor for complete advanced panel decision tree including sub-threshold behavior routing.
- 1.2.D: feat: VBR Command Generation - Added BuildFFmpegVBRCommand method with sophisticated maxrate and buffer size calculation for quality preservation.
- 1.2.F: test: Created AudiobookCompressor.Tests project with xUnit and Moq targeting .NET 8.0-windows per Focus 11.1.0/11.2.0.
- 1.2.F: test: Introduced IProcessRunner and IFileSystem abstraction interfaces enabling dependency injection and comprehensive mocking for AudioProcessor testing.
- 1.2.F: test: Implemented AudioProcessingDecider static class for pure logic testing of all processing decision paths (copy/convert/advanced/sub-threshold/Defer to Rockit) without file/process side effects.
- 1.2.F: test: Created comprehensive xUnit test suite with AudioProcessingDeciderTests covering all Focus 11.1.0 scenarios, including Defer to Rockit, dynamic maxrate, and advanced panel logic.
- 1.2.F: feat: Refactored AudioProcessor constructor to support dependency injection of IProcessRunner and IFileSystem with fallback to default implementations for backward compatibility.
- 1.2.F: feat: Exposed DetailedFileInfo class as public to enable unit testing accessibility while maintaining internal logic separation.
- 1.2.F: docs: Created comprehensive Testing-Architecture.md documentation per Focus 11.4.0, providing architectural guide, usage instructions, and maintenance procedures for the automated test suite.

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
- 1.2.E: refactor: Modified AudioProcessor to use injected IProcessRunner and IFileSystem abstractions in all process execution and file I/O operations, replacing direct calls to Process.Start, File.Copy, Directory.CreateDirectory, and File.Exists.

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
- 1.2.D: fix: Fixed syntax error in CreateCompressionSettingsXml method where duplicate PassMode element was causing compilation issues.
- 1.2.D: docs: UI alignment and spacing polish for advanced panels, ComboBox fine-tuning, and documentation compliance per SOP section 5.0. Signed: Orion
- 1.2.E: fix: Resolved test project compatibility issues by targeting net8.0-windows to match main project framework requirements.
- 1.2.E: fix: Fixed CS0051 accessibility error by changing DetailedFileInfo from internal to public for test accessibility.
- 1.2.E: fix: Corrected AudioProcessingDecider logic to properly handle Advanced/DeferToRockit scenarios, ensuring channel comparison always determines the correct action type for sub-threshold behaviors.

**Note**: GetTime.exe failure reported - timestamp shows as TIMESTAMP_ERROR for version 1.2.B changes.