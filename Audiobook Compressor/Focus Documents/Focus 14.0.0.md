Filename: Focus 14.0.0.md  
To: The Consultant (Vanguard)  
From: Axion  
Last Updated: 2025-08-09 10:52 AM CEST  
Version: 1.2.G  
State: Directive

### **Subject: Request for Proposal: Finalize MVVM Refactor with XAML Data Binding**

### **1\. Context**

The foundational MVVM architecture implemented in the last cycle was a complete success. The monolithic MainWindow.xaml.cs has been successfully decomposed into a clean, service-oriented backend.

The final step in this critical refactor is to complete the migration by rewiring the MainWindow.xaml View to its new MainViewModel.

### **2\. Objective**

To solicit a formal proposal for completing the MVVM refactor. The goal is to replace all remaining event handlers and procedural code in MainWindow.xaml.cs with modern WPF data binding to the MainViewModel.

### **3\. Required Scope for the Proposal**

Your proposal must be delivered as a new Focus document and detail a phased plan to perform the following:

1. **Implement Data Binding:** Convert all relevant UI controls in MainWindow.xaml to use data binding ({Binding ...}) to the properties and commands exposed by the MainViewModel. This includes, but is not limited to:  
   * ComboBox selections.  
   * Radio button states.  
   * Button commands.  
   * ProgressBar values and visibility.  
   * StatusBar text.  
2. **Remove Code-Behind:** Delete all migrated logic from MainWindow.xaml.cs. The final code-behind should be minimal, containing only the constructor and any essential, view-specific logic that cannot be handled by the ViewModel.  
3. **Verify Functionality:** The proposal must include a plan to verify that all UI interactions and application features are preserved after the refactor.

### **4\. Invitation for Review and Recommendations**

As per our standard process, we invite you to include any recommendations for the most elegant way to handle any complex binding scenarios you identify (e.g., the need for IValueConverter implementations).

### **5\. Action Required**

Please prepare and submit your proposal for this final phase of the MVVM refactor.