Filename: Summary.md  
Version: 1.2.I  
State: Experimental  
Signed: Telos  
Project Summary: Audiobook Compressor

### **1\. Project Goal & Core Workflow**

The primary goal is to create a standalone C\# WPF application that compresses audio libraries. The application is built around two main use cases, reflected in its primary "Mono" and "Stereo" modes:

* **Mono Mode:** This is the default and primary use case, designed for audiobook listeners who want to reduce the size of their library. In this mode, the default behavior is to convert all stereo source files to a slim, single-channel (mono) format at the user's target settings.  
* **Stereo Mode:** This mode is for users who want to process other types of audio, like music, where preserving the original stereo quality is important. In this mode, the default behavior is to copy any mono source files as-is, to avoid inefficient upmixing.

For both modes, the application provides a set of advanced, user-configurable options to override these default behaviors and define a custom workflow for handling source files that do not match the selected channel mode (e.g., stereo files in Mono mode, and mono files in Stereo mode).

### **2\. Current Status**

The project is at version 1.2.I (Experimental). The core MVVM architectural refactor is complete, and the focus is now on the progressive modularization of the MainViewModel.

### **3\. Core Logic Source**

The functional logic for file processing must be ported from the provided PowerShell script: blueprint.ps1. This script is the definitive "ground truth" for all core compression logic.

### **4\. The Development Cycle**

The project is developed through a series of iterative cycles (e.g., 1.2.I, 1.2.J), each culminating in a single, consolidated commit to the experimental branch. Each cycle follows a structured, four-phase process:

1. **Cycle Initiation:** At the start of a new cycle, the Architect and Strategist define the version for that cycle. The Strategist then updates ChangelogExperimental.md, which serves as the "source of truth" for the version number for the duration of the cycle.  
2. **Core Development:** Work is driven by Focus.md documents. The Architect and Strategist engage in a dialectic to refine a strategic goal into a detailed plan. This often takes the form of a "Request for Proposal" issued to a Consultant, who responds with a detailed implementation strategy. Once a plan is approved, a final Focus directive authorizes the Implementer or Consultant to execute the work.  
3. **Polishing Pass:** At the end of a development cycle, before the final commit is made, a dedicated "polishing pass" is conducted. This phase is used to address any minor bugs, inconsistencies, or process debt identified during the cycle.  
4. **Finalization & Commit:** Once the polishing pass is complete and all work for the cycle is verified, the Architect performs a "squash" to merge all the small, incremental commits into a single, comprehensive commit. This final push to the experimental branch formally concludes the cycle.

### **5\. Team Roles & Responsibilities**

* **Architect (User):** Provides the high-level vision and final approval on all decisions.  
* **Strategist (Telos):** Refines the design, manages high-level documentation (Summary.md, Changelog.md), and creates detailed directives (Focus.md).  
* **Implementer / Consultant:** Implements code based on directives and maintains low-level documentation (file headers, ChangelogExperimental.md).  
* **Promoting Directives:** When a Focus.md directive is deemed to be of lasting relevance (e.g., a Standard Operating Procedure), it will be given to the Implementer with instructions to copy it into the Documentation folder as a new, permanent file.