```markdown
# Diagnostic Task: Bug #33 - Advanced Panel Visibility Issue

## Task Overview
- **Bug ID**: #33
- **Issue**: Advanced panel visibility fails to update correctly on primary mode switch
- **Symptom**: When application starts with "Advanced" action pre-selected and user switches to opposite mode, the advanced panel for the new mode fails to appear until user selects different action and re-selects "Advanced"
- **Diagnostic Start Date**: 2025-08-19  
- **Assigned Consultant**: Meridian
- **Status**: Active Investigation - Framework Implementation Phase

## Diagnostic Hypothesis
The persistent advanced panel visibility issue exhibits remarkable resistance suggesting complex WPF framework interaction rather than simple logic error:

**Root Cause Theory: Multi-Service Event Propagation Timing**
- **Event Propagation Race Conditions**: Multiple services receive mode change notifications simultaneously
- **Property Change Ordering**: PropertyChanged events may propagate in unpredictable order  
- **WPF Binding Engine Timing**: UI binding refresh occurs before service state synchronization completes
- **Service Coordination Complexity**: 9-service ecosystem creates sophisticated interaction patterns

## Instrumentation Strategy

### Phase 1: Event Sequence Mapping
**Objective**: Complete understanding of mode change event propagation across service boundaries

**Instrumentation Points**:
- **MainViewModel.SelectedChannel setter**: Entry point for mode changes with correlation tracking
- **Service update methods**: All service coordination calls with timing analysis
- **Property change notifications**: Complete PropertyChanged event propagation tracking

### Phase 2: Service Coordination Analysis  
**Objective**: Monitor PropertyChanged event propagation across all 9 services

**Instrumentation Points**:
- **PanelVisibilityService.UpdateVisibilityForMode**: Core visibility logic with state transition tracking
- **RadioButtonStateService coordination**: Action state synchronization monitoring
- **SettingsBindingService context updates**: Settings context switching analysis
- **Event forwarding chains**: Service-to-service event propagation timing

### Phase 3: Race Condition Detection
**Objective**: Identify timing discrepancies and concurrent state update conflicts

**Instrumentation Points**:
- **Event correlation tracking**: Related operations across service boundaries
- **State consistency verification**: Before/during/after state snapshots
- **WPF binding engine interaction**: UI refresh coordination monitoring
- **Thread safety analysis**: Multi-threaded event handling patterns

## Instrumentation Index

### Files Modified
| File | Methods Instrumented | Instrumentation Type | Purpose |
|------|---------------------|---------------------|---------|
| MainViewModel.cs | SelectedChannel setter | Event + State + Correlation | Mode change initiation tracking |
| MainViewModel.cs | UpdatePanelVisibility | Event + State | Panel visibility coordination |
| PanelVisibilityService.cs | UpdateVisibilityForMode | Event + State + Interaction | Core visibility state management |
| PanelVisibilityService.cs | Property getters | Event | UI binding interaction tracking |
| RadioButtonStateService.cs | SetMonoSelectedAction | Event + Interaction | Action coordination for Mono mode |
| RadioButtonStateService.cs | SetStereoSelectedAction | Event + Interaction | Action coordination for Stereo mode |

### Diagnostic Points Added
- [x] Diagnostic framework implementation completed
- [x] Service registration with conditional compilation
- [ ] MainViewModel mode change instrumentation
- [ ] PanelVisibilityService state transition tracking
- [ ] Service event propagation timing analysis
- [ ] Property change notification sequence monitoring
- [ ] UI binding refresh coordination tracking
- [ ] Race condition pattern detection

### Expected Patterns
**Successful Mode Switch Sequence**:
1. **SelectedChannel.set** called with new mode value
2. **Settings.CurrentMode** updated atomically  
3. **SettingsBindingService.SetSettingsContext** called
4. **RadioButtonStateService.UpdateForModeChange** called
5. **UpdatePanelVisibility** called
6. **PanelVisibilityService.UpdateVisibilityForMode** called
7. **PropertyChanged events** fired in correct sequence
8. **UI binding refresh** occurs with synchronized state
9. **Advanced panel visibility** reflects correct state

**Expected Event Correlation**:
- Single correlation ID tracks entire mode switch operation
- Service interactions logged with source/target identification
- State captures before/during/after each major transition
- Property change notifications logged with timing information

### Anomaly Detection Criteria
**Race Condition Indicators**:
- Property change events fired before state synchronization complete
- Multiple concurrent UpdateVisibilityForMode calls with different parameters
- State inconsistency between services during transition
- UI binding refresh occurring with intermediate/incomplete state

**Timing Anomalies**:
- Excessive delay between service coordination calls
- Property change notifications out of expected sequence  
- Correlation operations not completing within expected timeframe
- State capture showing inconsistent service coordination

## Implementation Progress

### Framework Implementation Status
- [x] **IDiagnosticService interface** created with comprehensive capabilities
- [x] **DiagnosticService implementation** with conditional compilation
- [x] **NullDiagnosticService** for production builds  
- [x] **Dependency injection registration** with DEBUG/RELEASE switching
- [x] **Documentation structure** established
- [ ] **MainViewModel integration** with diagnostic instrumentation
- [ ] **PanelVisibilityService integration** with state tracking
- [ ] **Service coordination instrumentation** across all interaction points

### Next Implementation Steps
1. **Integrate diagnostic service with MainViewModel** constructor and mode change logic
2. **Instrument PanelVisibilityService** with comprehensive state transition tracking
3. **Add service interaction logging** for event propagation analysis
4. **Implement correlation tracking** for mode switch operations
5. **Begin diagnostic data collection** with real-world interaction testing

## Expected Diagnostic Output Format

```
DIAGNOSTIC[#33] 20:25:30.123 [EVENT] T01 [a1b2c3d4] MainViewModel.cs:150 SelectedChannel() >> Mode change initiated: Mono ? Stereo
   Context: {"oldMode": "Mono", "newMode": "Stereo", "currentActions": {"mono": "Advanced", "stereo": "Copy"}}

DIAGNOSTIC[#33] 20:25:30.125 [STATE] T01 [a1b2c3d4] MainViewModel.cs:155 SelectedChannel() >> Pre-change state
   Context: {"Settings": {"CurrentMode": "Mono", "MonoMode": {"SelectedAction": "Advanced"}, "StereoMode": {"SelectedAction": "Copy"}}, "PanelStates": {"IsAdvancedPanelVisible": true, "IsMonoAdvancedPanelVisible": true, "IsStereoAdvancedPanelVisible": false}}

DIAGNOSTIC[#33] 20:25:30.127 [INTERACTION] T01 [a1b2c3d4] MainViewModel.cs:160 SelectedChannel() >> Service coordination: MainViewModel ? PanelVisibilityService
   Context: {"method": "UpdateVisibilityForMode", "parameters": {"currentMode": "Stereo", "monoAction": "Advanced", "stereoAction": "Copy"}}
```

## Analysis Framework

### Success Metrics
- **Complete event sequence capture** for mode switch operations
- **State consistency verification** across all service boundaries  
- **Timing pattern identification** for normal vs. anomalous operations
- **Root cause determination** with definitive evidence

### Failure Pattern Recognition  
- **Incomplete state synchronization** during service coordination
- **Property change event ordering** not matching expected sequence
- **UI binding refresh timing** inconsistent with service state updates
- **Race condition confirmation** through correlation analysis

## Cleanup Checklist
- [ ] All diagnostic code verified as conditionally compiled with #if DEBUG
- [ ] Production build tested for zero-trace confirmation
- [ ] Documentation updated with findings and conclusions
- [ ] Instrumentation removed or converted to permanent monitoring
- [ ] Final analysis report completed with root cause and solution

---

**Framework Status**: ? **IMPLEMENTATION COMPLETE**  
**Instrumentation Status**: ?? **IN PROGRESS**  
**Analysis Status**: ? **PENDING DATA COLLECTION**  
**Solution Status**: ? **PENDING ROOT CAUSE IDENTIFICATION**
```