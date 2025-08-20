Filename: Summary.md  
Version: 1.2.J  
State: Experimental  
Signed: Telos  
Project Summary: Audiobook Compressor

### **1\. Project Goal & Core Workflow**

The primary goal is to create a standalone C\# WPF application that compresses audio libraries. The application is built around two main use cases, reflected in its primary "Mono" and "Stereo" modes:

* **Mono Mode:** This is the default and primary use case, designed for audiobook listeners who want to reduce the size of their library. In this mode, the default behavior is to convert all stereo source files to a slim, single-channel (mono) format at the user's target settings.  
* **Stereo Mode:** This mode is for users who want to process other types of audio, like music, where preserving the original stereo quality is important. In this mode, the default behavior is to copy any mono source files as-is, to avoid inefficient upmixing.

For both modes, the application provides a set of advanced, user-configurable options to override these default behaviors and define a custom workflow for handling source files that do not match the selected channel mode (e.g., stereo files in Mono mode, and mono files in Stereo mode).

### **2\. Current Status**

The project is at version 1.2.J (Experimental). The core MVVM architectural refactor and the first phase of modularization are complete. The immediate focus is on continuing the progressive modularization of the MainViewModel.

### **3\. Core Logic Source**

The functional logic for file processing must be ported from the provided PowerShell script: blueprint.ps1. This script is the definitive "ground truth" for all core compression logic.