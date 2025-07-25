Filename: Focus 2.2.1.md  
To: Praxis  
Last Updated: 2024-06-13 15:30 CEST  
Version: 2.2.1  
State: Analysis  
Signed: Sir Fixalot

---

## **Analysis & Implementation Plan: Contextual File Handling UI Refactor**

### **Directive Reference**
This analysis responds to Focus 2.2.0.md, which outlines the refactor of the contextual file handling UI in MainWindow.xaml. The goal is a flat, stacked layout with contextual radio buttons and an optional override panel, removing all expanders and group boxes for advanced settings.

---

### **Current State (as of MainWindow.xaml v1.1.13)**
- The UI currently uses Expander controls for advanced override settings within MonoModeOptionsPanel and StereoModeOptionsPanel.
- Each contextual panel contains a StackPanel for radio buttons and an Expander with a nested Border/StackPanel for override controls.
- The advanced override controls are a duplicate set of the main compression ComboBoxes.
- Visibility of contextual panels is managed in code-behind based on the ChannelsComboBox selection.

---

### **Planned Refactor (per Focus 2.2.0.md)**
1. **Remove all Expander controls** for advanced override settings in both Mono and Stereo contextual panels.
2. **Remove the inner Border/StackPanel** that served as a GroupBox for override settings.
3. **Replace with a flat structure:**
    - Each contextual panel (MonoModeOptionsPanel, StereoModeOptionsPanel) will contain:
        - A WrapPanel for the three radio buttons (Copy, Convert, Advanced...)
        - A WrapPanel (AdvancedStereoOverridePanel or AdvancedMonoOverridePanel) for the override ComboBoxes, visible only when the "Advanced..." radio button is selected.
    - All elements will be left-aligned in a single column, with no nested boxes or expanders.
4. **Visibility logic:**
    - Only one contextual panel is visible at a time, based on the main output format.
    - The override WrapPanel is visible only when the corresponding "Advanced..." radio button is checked.

---

### **Implementation Steps**
1. **XAML Edits:**
    - Remove Expander and Border/StackPanel for advanced override settings.
    - Add WrapPanel for override controls directly below the radio buttons in each contextual panel.
    - Ensure all controls are left-aligned and in a single column.
2. **Code-Behind Edits:**
    - Update logic to toggle the visibility of the override WrapPanel based on the "Advanced..." radio button state.
    - Remove any logic tied to Expander controls.
    - Ensure contextual panel visibility logic remains based on ChannelsComboBox selection.
3. **Testing:**
    - Verify that only one contextual panel is visible at a time.
    - Verify that the override controls appear only when "Advanced..." is selected.
    - Confirm that all controls remain functional and state is preserved.

---

### **Potential Issues & Considerations**
- **State Migration:** If any state was previously tied to the Expander (e.g., IsExpanded), ensure it is now tied to the "Advanced..." radio button.
- **UI Consistency:** Ensure spacing and alignment are consistent after removing nested containers.
- **Accessibility:** No new accessibility issues are anticipated, but tab order and keyboard navigation should be verified.
- **No impact on file processing logic** at this stage; this is a UI/layout refactor only.

---

### **Questions/Clarifications for Praxis & Architect**
- Should the override WrapPanel be collapsed (not take up space) or hidden (visible but disabled) when not in use?
- Should the override controls be reset when toggling off "Advanced...", or should their state persist until the next session?
- Any specific styling or margin/padding requirements for the new flat layout?

---

**Awaiting feedback before implementation.**
