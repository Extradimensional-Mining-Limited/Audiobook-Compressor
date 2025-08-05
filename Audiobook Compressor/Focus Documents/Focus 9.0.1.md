Filename: Focus 9.0.1.md  
To: Axion (Strategist)  
From: Advisor (Consultant)  
Last Updated: 2025-08-05 04:48  
Version: 9.0.1  
State: Architectural Synthesis Response  
Signed: Advisor

---

### **Subject: Architectural Synthesis of C# Codebase (Ref: Focus 9.0.0)**

Dear Axion,

Welcome to the Audiobook Compressor project! I'm pleased to provide you with the comprehensive architectural synthesis you requested. This analysis will enable you to formulate precise directives for integrating the core processing logic.

---

## **1. Entry Point & Orchestration**

### **High-Level Call Stack:**
```
User clicks "Start" button
    ?
MainWindow.StartButton_Click (async void)
    ?
Creates AudioProcessor instance with CancellationToken
    ?
AudioProcessor.ScanDirectoryAsync() - yields AudioFileInfo objects
    ?
Foreach file: AudioProcessor.ProcessAudioFileAsync(file, outputPath)
    ?
Events: AudioProcessor_ProgressChanged & AudioProcessor_FileProcessed
    ?
UI updates via Dispatcher.Invoke()
```

### **Key Classes & Methods:**
- **Entry Point:** `MainWindow.StartButton_Click()` (line ~970 in MainWindow.xaml.cs)
- **Orchestration:** `AudioProcessor` class in `Services/AudioProcessor.cs`
- **File Discovery:** `AudioProcessor.ScanDirectoryAsync(string sourcePath)`
- **Core Processing:** `AudioProcessor.ProcessAudioFileAsync(AudioFileInfo, string outputBasePath)`
- **File Metadata:** `AudioFileInfo` class in `Models/AudioFileInfo.cs`

---

## **2. Settings Propagation**

### **Data Model Architecture:**
The application uses a sophisticated hierarchical settings model implemented in Focus 7.0.0:

```csharp
Settings.Current (ApplicationSettings)
??? MonoMode (ModeSettings)
?   ??? Main (CompressionSettings)
?   ??? AdvancedOverride (CompressionSettings) 
?   ??? SelectedAction (string: "Copy"|"Convert"|"Advanced")
??? StereoMode (ModeSettings)
?   ??? Main (CompressionSettings)
?   ??? AdvancedOverride (CompressionSettings)
?   ??? SelectedAction (string: "Copy"|"Convert"|"Advanced")
??? CurrentMode (string: "Mono"|"Stereo")
??? IsAdvancedMode (bool)
??? SourcePath, OutputPath, etc.
```

### **Settings Access Pattern:**
- **Primary Data Model:** `Settings.Current` (static instance of `ApplicationSettings`)
- **UI Binding:** Direct property binding to hierarchical model via INotifyPropertyChanged
- **Persistence:** XML serialization in `MainWindow.SaveUserSettings()` / `LoadUserSettings()`
- **Radio Button States:** Stored in `ModeSettings.SelectedAction` property
- **Advanced Settings:** Accessed via `Settings.Current.MonoMode.AdvancedOverride` and `Settings.Current.StereoMode.AdvancedOverride`

### **Critical Architecture Gap:**
**The processing logic completely ignores the hierarchical settings model.** AudioProcessor uses legacy static properties (`Settings.TargetBitrate`, `Settings.CurrentChannel`) instead of the contextual settings.

---

## **3. Logic Location**

### **Primary Processing Class:**
- **Location:** `Services/AudioProcessor.cs`
- **Main Method:** `ProcessAudioFileAsync(AudioFileInfo audioFile, string outputBasePath)`
- **Current Behavior:** Basic mono/stereo processing with threshold-based copying
- **Missing:** Integration with Focus 5.0.0 contextual decision tree

### **Existing Method Stubs:**
- ? `ProcessAudioFileAsync()` - Exists but needs refactoring
- ? `ProcessAsException()` - Does not exist (required by Focus 5.0.0)
- ? `HandleUpmixLogic()` - Does not exist (required by Focus 5.0.0)
- ? Settings integration - Uses static properties instead of hierarchical model

### **Current Processing Logic:**
```csharp
// Simplified current flow:
1. Probe file with FFprobe (get channels, bitrate, codec)
2. Apply basic mono threshold check (copy if under 64k)
3. Build hardcoded FFmpeg command
4. Execute FFmpeg process
5. Monitor progress and report completion
```

---

## **4. Service Abstraction**

### **Existing Service Classes:**
- **AudioProcessor:** Primary processing service with events for progress/completion
- **Constants:** Static class providing FFmpeg/FFprobe paths
- **AudioFileInfo:** Data model for file metadata

### **Existing Abstractions:**
- ? **Process Execution:** ProcessStartInfo wrapper for FFmpeg/FFprobe
- ? **File Scanning:** `ScanDirectoryAsync()` with yield return pattern
- ? **Progress Reporting:** Event-based progress tracking
- ? **Filename Sanitization:** `SanitizeFilename()` method
- ? **CopyFile operation:** Uses `File.Copy()` directly (no abstraction)
- ? **FFmpeg Command Building:** Hardcoded string concatenation (no abstraction)

### **Missing Service Abstractions (needed for Focus 5.0.0):**
- **FFmpegCommandBuilder:** For flexible command generation
- **ProcessingContext:** To bridge UI settings with processing logic
- **ProcessingDecisionService:** To implement the Focus 5.0.0 decision tree

---

## **5. Architecture Assessment**

### **Strengths:**
- ? Clean separation between UI and Services
- ? Sophisticated hierarchical settings model with persistence
- ? Event-driven progress reporting
- ? Proper async/await throughout the stack
- ? INotifyPropertyChanged implementation for data binding

### **Critical Architectural Disconnect:**
The UI settings model from Focus 7.0.0 is completely isolated from the processing logic. Specifically:

1. **Radio Button States Ignored:** `ModeSettings.SelectedAction` has no effect on processing
2. **Advanced Settings Unused:** `AdvancedOverride` settings are never accessed by AudioProcessor
3. **Static Property Dependency:** Processing uses `Settings.TargetBitrate` instead of contextual settings
4. **Missing Decision Tree:** No implementation of the 6 scenarios from Focus 5.0.0

---

## **6. Integration Seams for Focus 5.0.0**

### **Recommended Integration Points:**
1. **ProcessAudioFileAsync Refactor:** Replace static settings access with contextual model
2. **Settings Bridge:** Create `ProcessingContext` class to expose relevant settings to AudioProcessor
3. **Decision Tree Implementation:** Add `ProcessAsException()` and `HandleUpmixLogic()` methods
4. **Command Builder Abstraction:** Extract FFmpeg command generation to separate service

### **Minimal Changes Required:**
- Modify `AudioProcessor` constructor to accept processing context
- Refactor `ProcessAudioFileAsync()` to use hierarchical settings
- Add missing methods specified in Focus 5.0.0
- Update MainWindow to pass contextual settings to AudioProcessor

---

## **7. Recommendation**

The codebase has excellent foundational architecture but suffers from a critical disconnect between the sophisticated UI settings model and the basic processing logic. **Focus 5.0.0 implementation is essential** to unlock the value of the hierarchical settings system.

The existing seams are well-positioned for integration - the event-driven architecture and service separation provide clean integration points for the contextual processing logic.

I recommend proceeding with Focus 5.0.0 implementation as the highest priority, followed by the naming refactor from Focus 8.0.1 to improve long-term maintainability.

Best regards,  
Advisor