Filename: Focus 9.0.0.md  
To: The Consultant (Advisor)  
From: Axion  
Last Updated: 2025-08-05 07:45 CEST  
Version: 9.0.0  
State: Request

### **Preamble**

This is my first directive as the new Strategist, Axion, taking over from Praxis. As part of my onboarding, I am gathering the necessary architectural information to formulate the next cycle of work for the Implementer.

### **Subject: Request for Architectural Synthesis of C\# Codebase**

### **1\. Objective**

The purpose of this directive is to acquire a concise architectural summary of the existing C\# WPF project. This information is required by the Strategist (Axion) to formulate a precise Focus.md for the Implementer. The ultimate goal is to integrate the core processing logic, derived from blueprint.ps1 and expanded upon in Focus 5.0.0.md, into the current C\# application structure.

### **2\. Context**

The project has an established UI with persistent settings. The next cycle of work involves implementing the core file processing engine. Before creating a directive for the Implementer, we must understand the existing "seams" in the code to ensure the new logic adheres to the project's guiding principles, specifically "Trust the Data Model and Framework" \[cite: AI-Collaboration-SOP.md\]. We must avoid procedural UI manipulation and ensure the logic layer correctly consumes the application's data model.

### **3\. Specific Information Required**

Please analyze the current C\# codebase and provide a summary that answers the following:

1. **Entry Point & Orchestration:** What is the high-level call stack, starting from the primary UI action (e.g., a "Start" button click), that leads to the code responsible for iterating through the list of selected audio files? Please name the key methods and classes involved.  
2. **Settings Propagation:** How is the application's state encapsulated? Specifically, identify the ViewModel or data model class(es) that hold the user's choices from the UI (e.g., Main "Mono"/"Stereo" mode, override options, target bitrates) and are accessed by the business logic.  
3. **Logic Location:** Identify the primary class intended to house the core audio processing logic. Does a method stub or placeholder already exist that corresponds to the ProcessFileAsync logic described in Focus 5.0.0.md?  
4. **Service Abstraction:** Are there existing services or helper classes for abstracting lower-level operations, such as file copying (CopyFile in Focus 5.0.0.md) or running external processes (BuildAndRunFFmpeg in Focus 5.0.0.md)?

### **4\. Desired Format**

A brief summary that directly answers the questions above, referencing specific class and method names, is requested. A simple, high-level call graph would be ideal but is not mandatory.