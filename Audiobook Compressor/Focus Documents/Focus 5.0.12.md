Filename: Focus 5.0.12.md  
To: Orion (Implementer)  
From: Axion  
Last Updated: 2025-08-06 10:05 CEST  
Version: 5.0.12  
State: Directive

### **Subject: Onboarding and UI Polish for Advanced Panel Sub-Sections**

### **1\. Context**

Welcome to the Audiobook Compressor project, Orion. You are being brought on as an Implementer to perform a final cycle of UI polish on a recently added feature.

The task is to ensure the new controls in the Advanced panels are perfectly integrated into the existing design and provide the best possible user experience.

### **2\. Required Changes**

Please make the following adjustments to the new sub-sections within both the MonoModeAdvancedPanel and the StereoModeAdvancedPanel.

**2.A: Panel Background Color**

* The light gray background (\#FFF8F8F8) of the StackPanel containing the new radio buttons should be removed.  
* **Goal:** The panel's background should be transparent, matching the main window's background color for a seamless look.

**2.B: Control Refinement and Alignment**

* The TextBox for custom bitrate input should be replaced with an **editable ComboBox**.  
  * This ComboBox should be pre-populated with a sensible list of common bitrate values (e.g., "48k", "64k", "96k", "128k").  
  * It must allow the user to enter a custom value.

**2.C: Precise Layout and Spacing**

* **Horizontal Alignment:** The WrapPanel containing the new "Copy," "Defer to Rockit," and "Convert to:" radio buttons must be horizontally aligned with the WrapPanel containing the main mode's radio buttons above it. They should share the same left margin.  
* **Vertical Shift:** The top margin of the StackPanel for the sub-section should be reduced (e.g., from 10 to 5\) to move the entire section closer to the Advanced panel's main controls.  
* **ComboBox Placement:** The new editable ComboBox must be vertically centered with the "Convert to:" RadioButton. Its horizontal position should be determined by a standard margin (e.g., a left margin of 5\) relative to the radio button, creating a pleasing visual space rather than being strictly aligned with the controls in the panel above it.

### **3\. Rationale**

These changes are in service of our "Elegance" principle. Using an editable ComboBox is a more user-friendly design, and ensuring balanced, professional alignment creates a more polished final product.

### **4\. Action Required**

Please implement these UI adjustments.