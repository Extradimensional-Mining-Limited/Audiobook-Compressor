Filename: Summary.md  
Version: 1.2.B  
State: Experimental  
Signed: Praxis, User

## **Project Summary: Audiobook Compressor**

### **1\. Project Goal & Core Workflow**

The primary goal is to create a standalone C\# WPF application that compresses audio libraries. The application is built around two main use cases, reflected in its primary "Mono" and "Stereo" modes:

* **Mono Mode:** This is the default and primary use case, designed for audiobook listeners who want to reduce the size of their library. In this mode, the default behavior is to convert all stereo source files to a slim, single-channel (mono) format at the user's target settings.  
* **Stereo Mode:** This mode is for users who want to process other types of audio, like music, where preserving the original stereo quality is important. In this mode, the default behavior is to copy any mono source files as-is, to avoid inefficient upmixing.

For both modes, the application provides a set of advanced, user-configurable options to override these default behaviors and define a custom workflow for handling source files that do not match the selected channel mode (e.g., stereo files in Mono mode, and mono files in Stereo mode).

### **2\. Current Status**

The project is at version 1.2.B (Experimental). The core UI is complete, user settings persists across sessions. The immediate focus is on implementing the core processing logic and addressing the high-priority items in ToDo.md.

### **3\. Core Logic Source**

The functional logic for file processing must be ported from the provided PowerShell script: blueprint.ps1. This script is the definitive "ground truth" for all core compression logic.

### **4\. Team Workflow**

* **Architect (User):** Provides the high-level vision and final approval on all decisions.  
* **Axion (Strategist):** Refines the design, manages high-level documentation (Summary.md, Changelog.md), and creates detailed directives (Focus.md).  
* **Implementer:** Implements code based on directives and maintains low-level documentation (file headers, ChangelogExperimental.md).  
* **Consultant:** A specialized role engaged by the Architect on an as-needed basis for complex problem-solving or architectural reviews.  
* **Promoting Directives:** When a Focus.md directive is deemed to be of lasting relevance (e.g., a Standard Operating Procedure), it will be given to the Implementer with instructions to copy it into the Documentation folder as a new, permanent file.