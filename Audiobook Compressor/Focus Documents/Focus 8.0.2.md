Filename: Focus 8.0.2.md  
To: Praxis (Strategist)  
From: Advisor (Consultant)  
Last Updated: 2025-08-05 04:48  
Version: 8.0.2  
State: Implementation Proposal  
Signed: Advisor

---

### **Subject: Implementation Proposal for Naming Convention Refactor (Ref: Focus 8.0.1)**

Dear Praxis,

I have analyzed Focus 8.0.1 and the current codebase naming conventions. Below is my implementation proposal for the terminology and naming convention refactor.

---

## **1.0 Current State Analysis**

### **1.1 Naming Inconsistencies Identified**
Through comprehensive code review, I've confirmed the following problematic naming patterns:

**Settings Model Issues:**
- `DefaultMonoCopyThreshold` ? Used in both mono and stereo contexts (confusing)
- `AdvancedOverride` ? Verbose, "Override" is implied by context
- `_advancedOverride` ? Inconsistent with proposed public property name

**UI Panel Issues:**
- `MonoModeOptionsPanel` ? "Options" is redundant
- `StereoModeOptionsPanel` ? "Options" is redundant  
- `AdvancedStereoOverridePanel` ? Confusing ("Stereo" in Mono mode context)
- `AdvancedMonoOverridePanel` ? Confusing ("Mono" in Stereo mode context)

**UI Control Issues:**
- 12 ComboBox controls with misleading names (e.g., `AdvancedStereoChannelsComboBox` in Mono mode)

### **1.2 Impact Assessment**
This refactor will affect:
- 2 core model files (`Settings.cs`)
- 2 UI files (`MainWindow.xaml`, `MainWindow.xaml.cs`)
- Approximately 50+ references across the codebase
- No functional logic changes (pure refactoring)

---

## **2.0 Implementation Strategy**

### **2.1 Systematic Approach**
I propose implementing the changes in this exact order to maintain consistency:

1. **Settings.cs Model Updates**
2. **MainWindow.xaml UI Control Updates**  
3. **MainWindow.xaml.cs Code-Behind Updates**
4. **Validation & Testing**

### **2.2 Complete Naming Transformation Map**

**Settings Model Changes:**
```
DefaultMonoCopyThreshold ? DefaultConversionThreshold
AdvancedOverride ? Advanced  
_advancedOverride ? _advanced
```

**UI Panel Changes:**
```
MonoModeOptionsPanel ? MonoModePanel
StereoModeOptionsPanel ? StereoModePanel
AdvancedStereoOverridePanel ? MonoModeAdvancedPanel
AdvancedMonoOverridePanel ? StereoModeAdvancedPanel
```

**Mono Mode Advanced Controls (12 controls):**
```
AdvancedStereoChannelsComboBox ? MonoAdvancedChannelsComboBox
AdvancedStereoBitrateComboBox ? MonoAdvancedBitrateComboBox
AdvancedStereoSampleRateComboBox ? MonoAdvancedSampleRateComboBox
AdvancedStereoThresholdComboBox ? MonoAdvancedThresholdComboBox
AdvancedStereoBitrateControlComboBox ? MonoAdvancedBitrateControlComboBox
AdvancedStereoPassesComboBox ? MonoAdvancedPassesComboBox
```

**Stereo Mode Advanced Controls (6 controls):**
```
AdvancedMonoChannelsComboBox ? StereoAdvancedChannelsComboBox
AdvancedMonoBitrateComboBox ? StereoAdvancedBitrateComboBox
AdvancedMonoSampleRateComboBox ? StereoAdvancedSampleRateComboBox
AdvancedMonoThresholdComboBox ? StereoAdvancedThresholdComboBox
AdvancedMonoBitrateControlComboBox ? StereoAdvancedBitrateControlComboBox
AdvancedMonoPassesComboBox ? StereoAdvancedPassesComboBox
```

---

## **3.0 Implementation Challenges & Solutions**

### **3.1 Challenge: Event Handler References**
**Issue:** MainWindow.xaml.cs contains ~30 event handler references that must be updated.
**Solution:** Systematic find/replace with careful validation of each reference.

### **3.2 Challenge: XML Persistence**
**Issue:** Settings XML serialization references old property names.
**Solution:** Update `LoadCompressionSettings` and `CreateCompressionSettingsXml` methods.

### **3.3 Challenge: Binding References**
**Issue:** Any XAML bindings to old property names will break.
**Solution:** Currently no direct XAML bindings to renamed properties (verified).

---

## **4.0 Advantages of This Refactor**

### **4.1 Clarity Improvements**
- `MonoModeAdvancedPanel` clearly indicates it belongs to Mono mode
- `StereoAdvancedChannelsComboBox` unambiguously shows stereo context
- `DefaultConversionThreshold` works for any conversion scenario

### **4.2 Maintenance Benefits**
- Self-documenting code reduces cognitive load
- Easier debugging with clear control names
- Consistent naming pattern throughout codebase

### **4.3 Alignment with SOP 7.3**
This refactor perfectly embodies the "Clarity Through Unambiguous Naming" principle.

---

## **5.0 Risk Assessment**

### **5.1 Low Risk Areas**
- Settings model property renames (contained scope)
- UI control renames (IDE refactoring tools available)
- No functional logic changes

### **5.2 Medium Risk Areas**
- Event handler method updates (manual verification required)
- XML serialization compatibility (needs testing)

### **5.3 Mitigation Strategies**
- Comprehensive find/replace validation
- Build verification after each change group
- Manual testing of settings persistence

---

## **6.0 Proposed Implementation Schedule**

### **Session 1: Settings Model**
- Update `Settings.cs` with property renames
- Verify all references are updated
- Test compilation

### **Session 2: UI Structure**  
- Update `MainWindow.xaml` panel and control names
- Verify XAML structure integrity
- Test UI layout

### **Session 3: Code-Behind**
- Update `MainWindow.xaml.cs` event handlers and references
- Verify all control name references
- Test event handling

### **Session 4: Integration & Testing**
- Full build verification
- Settings persistence testing  
- UI functionality validation

---

## **7.0 Success Criteria**

The refactor will be considered complete when:
1. ? All builds compile successfully
2. ? UI controls display and function correctly
3. ? Settings persistence works without errors
4. ? Event handlers bind to correct controls
5. ? No functional regressions detected

---

## **8.0 Long-term Benefits**

This refactor will:
- Eliminate confusion for future developers
- Reduce debugging time when working with advanced panels
- Provide foundation for cleaner Focus 5.0.0 integration
- Improve code review efficiency

---

## **9.0 Recommendation**

I recommend **proceeding with this refactor** as a medium-priority task. While not critical for functionality, it significantly improves code maintainability and aligns with our established architectural principles.

**Estimated Effort:** Medium (2-3 development sessions)
**Priority:** Medium (improves maintainability but doesn't block features)
**Best Timing:** After Focus 5.0.0 implementation to avoid merge conflicts

The refactor is straightforward and low-risk, with clear benefits for long-term code quality.

Best regards,  
Advisor