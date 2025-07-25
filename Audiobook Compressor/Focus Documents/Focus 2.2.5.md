Filename: Focus 2.2.5.md  
To: Sir Fixalot  
From: Praxis  
Last Updated: 2025-07-25 02:00 CEST  
Version: 2.2.5  
State: Directive  
Signed: Praxis

### **Subject: Final UI Alignment Fix (Ref: Focus 2.2.4.md)**

Sir Fixalot,

Thank you for the detailed report and the XAML excerpts. They clearly show the root cause of the alignment issue. The problem is due to nested containers creating a cumulative margin.

This directive provides a more prescriptive XAML structure to resolve this.

### **Objective: Achieve Perfect Alignment by Simplifying the XAML Hierarchy**

The goal is to ensure all rows within the "Compression Settings" GroupBox are perfectly left-aligned.

### **Required XAML Change (**MainWindow.xaml**)**

1. **Simplify the Container Structure:**  
   * The WrapPanel for the main settings, the WrapPanel for the radio buttons, and the WrapPanel for the override settings should all be **direct children of the same parent** StackPanel.  
   * This parent StackPanel should be the main container directly inside the "Compression Settings" GroupBox.  
   * **Remove the intermediate** Grid **container** you added previously.  
2. **Remove Nested Margins:**  
   * **Crucially, remove the** Margin **property from all three of these** WrapPanel**s.** The alignment will be handled automatically and consistently by the Padding of their parent GroupBox. The ComboBoxes and RadioButtons inside the panels can keep their individual small margins for spacing.

### **Example of the Corrected Structure:**

\<GroupBox Header="Compression Settings"\>  
    \<StackPanel\>

        \<\!-- Row 1: Main Settings \--\>  
        \<WrapPanel\>  
            \<\!-- ComboBoxes with their own small margins \--\>  
        \</WrapPanel\>

        \<\!-- Row 2: Contextual Radio Buttons (inside its visibility-controlled parent) \--\>  
        \<StackPanel x:Name="MonoModeOptionsPanel"\>  
             \<WrapPanel\>  
                \<\!-- RadioButtons with their own small margins \--\>  
             \</WrapPanel\>  
             \<\!-- ... a WrapPanel for the override settings will appear here ... \--\>  
        \</StackPanel\>  
        \<StackPanel x:Name="StereoModeOptionsPanel"\>  
             \<\!-- ... similar structure ... \--\>  
        \</StackPanel\>

    \</StackPanel\>  
\</GroupBox\>

This flattened hierarchy ensures all rows are siblings and will inherit the exact same indentation from the parent GroupBox, solving the alignment issue permanently.

Please implement this structural change.