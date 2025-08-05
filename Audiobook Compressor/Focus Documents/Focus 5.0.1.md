Filename: Focus 5.0.1.md  
To: Praxis (Strategist)  
From: Advisor (Consultant)  
Last Updated: 2025-08-05 04:48  
Version: 5.0.1  
State: Implementation Proposal  
Signed: Advisor

---

### **Subject: Implementation Proposal for Core Processing Logic (Ref: Focus 5.0.0)**

Dear Praxis,

I have conducted a thorough analysis of Focus 5.0.0 and the current AudioProcessor.cs implementation. Below is my detailed implementation proposal for bridging the gap between the sophisticated hierarchical UI settings and the core processing logic.

---

## **1.0 Current State Analysis**

### **1.1 Critical Architecture Disconnect**
The current AudioProcessor.cs operates in complete isolation from the hierarchical settings model implemented in Focus 7.0.0. Specifically:
- All processing decisions use legacy static `Settings` properties
- The new `ModeSettings.SelectedAction` property is completely ignored
- Advanced override settings (`MonoMode.Advanced`, `StereoMode.Advanced`) are never accessed
- Radio button states have no influence on processing behavior

### **1.2 Missing Processing Scenarios**
The current implementation lacks handling for 4 of the 6 possible user scenarios:
1. ? Mono file + Mono mode ? Works (copy or convert based on threshold)
2. ? Stereo file + Mono mode + "Copy stereo files" ? Missing
3. ? Stereo file + Mono mode + "Advanced..." ? Missing  
4. ? Mono file + Stereo mode + "Copy mono files" ? Missing
5. ? Mono file + Stereo mode + "Convert mono to stereo" ? Missing
6. ? Mono file + Stereo mode + "Advanced..." ? Missing

---

## **2.0 Implementation Strategy**

### **2.1 Settings Integration Approach**
I propose creating a `ProcessingContext` class that bridges the UI settings with the processing logic:

```csharp
public class ProcessingContext
{
    public string CurrentMode { get; }
    public string SelectedAction { get; }
    public CompressionSettings MainSettings { get; }
    public CompressionSettings AdvancedSettings { get; }
    
    public static ProcessingContext FromCurrentSettings()
    {
        var currentMode = Settings.Current.CurrentMode;
        var modeSettings = currentMode == "Mono" ? Settings.Current.MonoMode : Settings.Current.StereoMode;
        
        return new ProcessingContext
        {
            CurrentMode = currentMode,
            SelectedAction = modeSettings.SelectedAction,
            MainSettings = modeSettings.Main,
            AdvancedSettings = modeSettings.Advanced
        };
    }
}
```

### **2.2 Refactored ProcessFileAsync Structure**
```csharp
public async Task ProcessAudioFileAsync(AudioFileInfo audioFile, string outputBasePath)
{
    var context = ProcessingContext.FromCurrentSettings();
    var fileInfo = await ProbeAudioFileAsync(audioFile);
    
    if (IsChannelModeMatch(fileInfo.Channels, context.CurrentMode))
    {
        await ProcessAsNormal(audioFile, fileInfo, context);
    }
    else
    {
        await ProcessAsException(audioFile, fileInfo, context);
    }
}
```

### **2.3 Exception Processing Logic**
The `ProcessAsException` method will implement the decision tree from Focus 5.0.0:

**For Mono Mode (handling stereo files):**
- "Copy" ? Direct file copy
- "Convert" ? Apply main settings with mono conversion
- "Advanced" ? Use `MonoMode.Advanced` settings

**For Stereo Mode (handling mono files):**
- "Copy" ? Direct file copy  
- "Convert" ? Implement `HandleUpmixLogic` as specified
- "Advanced" ? Use `StereoMode.Advanced` settings

---

## **3.0 Implementation Challenges & Solutions**

### **3.1 Challenge: Settings Access Pattern**
**Issue:** Current code uses static `Settings.TargetBitrate` etc.
**Solution:** Refactor to use `context.MainSettings.TargetBitrate` or `context.AdvancedSettings.TargetBitrate` based on action.

### **3.2 Challenge: FFmpeg Command Generation**
**Issue:** Current command building is hardcoded for basic scenarios.
**Solution:** Create `FFmpegCommandBuilder` class that accepts `CompressionSettings` and builds appropriate commands.

### **3.3 Challenge: Upmix Logic Complexity**
**Issue:** The "Convert mono to stereo" scenario requires threshold comparison and quality-based encoding.
**Solution:** Implement exact logic from Focus 5.0.0 specification with estimated bitrate calculation.

---

## **4.0 Proposed Phased Implementation**

### **Phase 1: Foundation (High Priority)**
1. Create `ProcessingContext` class
2. Refactor `ProcessFileAsync` to use context instead of static settings
3. Implement basic channel mode matching logic

### **Phase 2: Exception Handling (Critical Priority)**
1. Implement `ProcessAsException` method with full decision tree
2. Add support for "Copy" actions (direct file copy)
3. Integrate advanced settings usage

### **Phase 3: Advanced Features (Medium Priority)**
1. Implement `HandleUpmixLogic` for mono-to-stereo conversion
2. Create `FFmpegCommandBuilder` for flexible command generation
3. Add support for quality-based encoding ("high_quality_placeholder")

---

## **5.0 Risk Assessment**

### **5.1 Low Risk**
- Settings integration (well-defined interfaces)
- Basic copy operations
- Channel mode detection

### **5.2 Medium Risk**
- FFmpeg command refactoring (complex but well-understood)
- Upmix logic implementation (specified but untested)

### **5.3 High Risk**
- Quality-based encoding ("high_quality_placeholder" needs definition)
- Integration testing across all 6 scenarios
- Backward compatibility with existing processing behavior

---

## **6.0 Success Criteria**

The implementation will be considered complete when:
1. ? All 6 user scenarios produce expected processing behavior
2. ? Radio button states directly influence file processing decisions
3. ? Advanced settings are used when "Advanced..." is selected
4. ? Upmix logic correctly handles threshold-based quality decisions
5. ? No regression in existing mono/stereo processing quality

---

## **7.0 Recommendation**

I recommend **proceeding with this implementation** as it addresses the critical architecture gap identified in Review 1.2.B. The hierarchical settings model from Focus 7.0.0 is excellent, but it provides no value until integrated with the processing logic.

**Estimated Effort:** Hard (3-4 development sessions)
**Priority:** Critical (blocks meaningful use of advanced UI features)

I am prepared to implement this proposal upon your approval.

Best regards,  
Advisor