Filename: Focus 5.0.15.md  
To: Orion (Implementer)  
From: Axion  
Last Updated: 2025-08-07 07:31 CEST  
Version: 5.0.15  
State: Directive

### **Subject: Authorization to Implement Grid Layout for Precise ComboBox Alignment**

### **1\. Context**

We have reviewed your analysis in Focus 5.0.14 regarding the difficulty of achieving pixel-perfect alignment for the custom bitrate ComboBox within the current WrapPanel structure.

Your analysis is correct, and your recommendation to use a Grid is the most elegant and technically sound solution.

### **2\. Authorization**

You are authorized to proceed with the implementation of your recommended solution.

### **3\. Action Required**

Please refactor the relevant UI sections in MainWindow.xaml for both the MonoModeAdvancedPanel and StereoModeAdvancedPanel as follows:

1. Replace the WrapPanel that currently contains the "Convert to:" RadioButton and its associated ComboBox with a two-column Grid.  
2. Place the RadioButton in the first column and the ComboBox in the second.  
3. Use the Margin property on the ComboBox within the Grid to apply the required offset (2 pixels left, 7 pixels up) to achieve the desired visual alignment.

### **4\. Rationale**

This change aligns with our "Elegance" principle by using the correct tool for the job. A Grid provides the necessary control for precise layout, resulting in a more polished UI and more maintainable XAML.