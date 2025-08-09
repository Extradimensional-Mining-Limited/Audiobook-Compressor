Filename: Focus 15.0.0.md  
To: The Consultant (Vanguard)  
From: Axion  
Last Updated: 2025-08-09 01:06 PM CEST  
Version: 1.2.H  
State: Directive

### **Subject: Resolve Multiple Window Startup Issue**

### **1\. Context**

Following the successful implementation of the foundational MVVM architecture, a new issue has been identified. When launching the application from the Visual Studio debugger, multiple MainWindow instances are being created.

This is a common issue during an MVVM transition and needs to be resolved before we proceed.

### **2\. Analysis**

The root cause is a conflict between the two methods now being used to launch the main window:

1. **Manual Instantiation (Correct):** The App.xaml.cs file now correctly creates a MainWindow instance and injects the MainViewModel via the Dependency Injection container.  
2. **Automatic Instantiation (Legacy):** The App.xaml file still contains a StartupUri="MainWindow.xaml" attribute, which instructs the WPF framework to create a second, separate instance of the window automatically.

### **3\. Action Required**

To resolve this conflict, you are to perform the following single change:

* **In the App.xaml file, remove the StartupUri="MainWindow.xaml" attribute from the main \<Application\> tag.**

### **4\. Rationale**

This change ensures that the Dependency Injection container in App.xaml.cs becomes the sole authority for creating and displaying the main application window. This will resolve the conflict, prevent duplicate instances, and ensure that the single window that does appear is the one correctly configured with its DataContext set to the MainViewModel.