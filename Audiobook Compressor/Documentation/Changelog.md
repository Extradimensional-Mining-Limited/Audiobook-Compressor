Filename: Changelog.md  
Version: 1.2.0  
State: Stable  
Signed: User

# **Changelog**

All notable changes to this project will be documented in this file.  
The format is based on Keep a Changelog,  
and this project adheres to Semantic Versioning.

## **\[Unreleased\]**

### **Added**

### **Changed**

### **Fixed**

## **\[1.2.0\] \- 2025-07-25 CEST**

### **Added**

* Implemented a new contextual UI for advanced handling of mono and stereo files.  
* Added persistent override settings for advanced file handling, saved between sessions.

### **Changed**

* Finalized all UI vertical spacing and layout for a cleaner, more readable interface.  
* The default output directory is now created as a sibling to the source directory.  
* Renamed "Reset" button for Output Folder Defaults to "Restore".

### **Fixed**

* Corrected output path logic to ensure files are placed in the correct root output directory.  
* Ensured all re-encoded files are output as .m4b, matching the reference script.  
* Resolved build errors and accessibility issues related to the new override settings.

## **\[1.1.0\] \- 2025-07-17 CEST**

### **Added**

* Added comprehensive event handling and validation for editable ComboBoxes.  
* Implemented full async/await pattern for all file processing operations to ensure UI responsiveness.  
* Added audio channel selection (Mono/Stereo) to the UI and settings.

### **Changed**

* Refined all settings synchronization logic to ensure UI and internal state are always consistent.  
* Improved namespace organization and type safety throughout the application.

### **Fixed**

* Resolved various UI bugs related to ComboBox value persistence and display formatting.  
* Fixed numerous build errors and null reference issues related to async operations.