Filename: Focus 15.2.0.md  
To: The Consultant (Vanguard)  
From: Axion  
Last Updated: 2025-08-09 01:29 PM CEST  
Version: 1.2.H  
State: Directive

### **Subject: Request for Proposal: Resolve Settings Persistence Failure**

### **1\. Context**

Following the successful MVVM refactor and the resolution of the startup issues, a critical bug has been identified: user settings are no longer persisting between application sessions.

The original "save on exit" logic, which was part of the old MainWindow.xaml.cs, was removed during the refactor and not replaced with an equivalent mechanism in the new architecture.

### **2\. Objective**

To solicit a formal proposal for re-implementing the settings persistence logic in a way that is robust, elegant, and fully compatible with our new MVVM and service-oriented architecture.

### **3\. Problem Description**

* **Symptom:** Changes made to any settings in the UI are not saved when the application is closed and reopened. The application always reverts to the previously saved settings.  
* **Root Cause:** There is no longer a mechanism that calls the SettingsService.SaveSettings() method when the application shuts down.

### **4\. Required Scope for the Proposal**

Your proposal must be delivered as a new Focus document and detail a plan to resolve this issue. The recommended approach is to:

1. **Create a Shutdown Method:** Implement a new public method in the MainViewModel (e.g., OnApplicationExit()) that is responsible for saving the settings.  
2. **Hook into Application Exit:** Modify App.xaml.cs to hook into the application's Exit event and call this new shutdown method on the MainViewModel.

Your proposal should confirm this approach or recommend a more elegant alternative, and outline the specific implementation steps.

### **5\. Invitation for Review and Recommendations**

As per our standard process, we invite you to provide any critiques or recommendations you may have for the most robust way to handle application lifecycle events within our new architecture.

### **6\. Action Required**

Please prepare and submit your proposal for fixing this critical persistence bug.