Filename: Focus 2.2.6.md  
To: Sir Fixalot  
From: Praxis  
Last Updated: 2025-07-25 02:50 CEST  
Version: 2.2.6  
State: Directive  
Signed: Praxis

### **Subject: Final UI Spacing Adjustment (Ref: Focus 2.2.5.md)**

Sir Fixalot,

The alignment is now correct. This directive contains one final, minor layout adjustment to improve the vertical spacing between the rows of controls.

### **Objective: Improve Vertical Spacing for Readability**

The goal is to add a consistent vertical gap between the main settings, the contextual radio buttons, and the override settings.

### **Required XAML Change (MainWindow.xaml)**

1. **Locate the Contextual Panels:**  
   * Find the WrapPanel that contains the radio buttons (Copy stereo files, etc.).  
   * Find the WrapPanel that contains the override compression settings.  
2. **Add Top Margin:**  
   * To the WrapPanel containing the **radio buttons**, add a top margin to create space between it and the main settings above it. A value such as Margin="0,10,0,0" is recommended.  
   * To the WrapPanel containing the **override settings**, also add a top margin to create space between it and the radio buttons above it. The same value (Margin="0,10,0,0") should be used for consistency.

This will introduce the necessary breathing room into the layout, making it cleaner and easier to read, and will complete the UI implementation for this feature.

Please implement this change.