Filename: Focus 5.0.13.md  
To: Orion (Implementer)  
From: Axion  
Last Updated: 2025-08-06 23:00 CEST  
Version: 5.0.13  
State: Directive

### **Subject: Corrective UI Polish for Advanced Panels**

### **1\. Context**

The previous implementation in Focus 5.0.12 correctly replaced the TextBox with a ComboBox. However, the final layout did not match the required spacing and alignment from the design mock-up.

This directive provides precise, technical instructions to achieve the final, polished design, using the existing XAML as a direct template.

### **2\. Reference Implementation**

The XAML for the existing, correctly-styled controls provides the definitive template for this task. The Margin and Style properties of the new controls must be set to match their corresponding elements in the main and advanced panels.

### **3\. Required Changes**

You are to make the following specific changes to the XAML of the sub-sections within both the MonoModeAdvancedPanel and the StereoModeAdvancedPanel.

**3.A: Correct Vertical Spacing**

* **Target:** The StackPanel that contains the "For files below threshold:" TextBlock and the new radio buttons.  
* **Action:** Modify its Margin property to reduce the space above it. A value of Margin="0,5,0,0" should be appropriate to match the visual mock-up.

**3.B: Correct Horizontal Alignment**

* **Target 1 (Label):** The TextBlock with the content "For files below threshold:".  
  * **Action:** Its Margin must be set to Margin="0,0,5,5" to match the "Compression Settings" label above it.  
* **Target 2 (Radio Buttons):** The WrapPanel that contains the new radio buttons.  
  * **Action:** Its Margin must be set to Margin="0,10,0,0" to match the WrapPanel containing the main mode's radio buttons above it.

**3.C: Control Refinement and Styling**

* **Target:** The new editable ComboBox for custom bitrate input.  
* **Action 1 (Styling):** The ComboBox must use the style Style="{StaticResource SettingsComboBoxStyle}".  
* **Action 2 (Sizing):** The ComboBox must have its MinWidth property set to 80 to match the Threshold ComboBox.  
* **Action 3 (Placement):** The ComboBox must remain vertically centered with the "Convert to:" RadioButton next to it.

### **4\. Rationale**

These precise adjustments, using the existing XAML as a template, will ensure the final UI is visually balanced, consistent, and professional, in accordance with our "Elegance" principle.

### **5\. Action Required**

Please implement these specific XAML property changes.