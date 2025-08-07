Filename: AI-Collaboration-SOP.md  
Version: 1.5.0  
State: Proposed Update  
Signed: Axion

## **Subject: Standard Operating Procedure for AI Collaboration**

### **1.0 Introduction**

**1.1 Purpose:** This document specifies the mandatory operational procedures and design principles for all AI instances contributing to the Audiobook Compressor project.

**1.2 Scope:** These procedures apply to all development tasks performed on the experimental branch and all formal release procedures.

### **2.0 System Architecture: Team Roles**

The project operates on a multi-tier model with a clear separation of concerns:

* **Architect (User):** Defines strategic objectives, determines when a "cycle" of work is complete, and provides final authorization on all directives and implementations.  
* **Strategist (Axion):** Analyzes strategic objectives, refines technical design, and is the designated custodian for all high-level documentation (Summary.md, Changelog.md). Generates Focus.md directives.  
* **Implementer:** Executes Focus.md directives, implements code, creates incremental local commits upon request, and is the designated custodian for all low-level, operational documentation (file headers, ChangelogExperimental.md).  
* **Consultant:** A specialized role engaged by the Architect on an as-needed basis for complex problem-solving, architectural reviews, or to provide a "second opinion" on particularly difficult implementations.

### **3.0 Core Procedural Directives**

**3.1 Directive: Non-Destructive Editing**

* **Rule:** All modifications to documentation files, particularly changelogs, must be additive unless a block replacement is explicitly specified.  
* **Rationale:** Preserves the integrity of the historical record.

**3.2 Directive: Literal Interpretation**

* **Rule:** All instructions within a Focus.md directive must be interpreted literally. No intent is to be inferred.  
* **Rationale:** Prevents deviation from the approved design.

### **4.0 Documentation Maintenance Protocols**

**4.1 Protocol: File Header Updates**

* **Responsibility:** Implementer.  
* **Content \- Synopsis:** Must be a concise, one-sentence description of the file's current functional purpose.  
* **Content \- Last Updated:** The value must be acquired via execution of GetTime.exe.  
  * **Invocation Rule:** The GetTime.exe tool is available in the system's PATH environment variable. You must invoke it by its name only (GetTime.exe). You **must not** prepend any path information (e.g., .\\ or C:\\...).  
  * **Error Condition:** If GetTime.exe returns the string ERROR: Deadman switch triggered. Exiting., the command must be re-executed one (1) time.  
  * **Terminal Error Condition:** If the second execution fails, the value must be the literal string "TIMESTAMP\_ERROR", and the failure must be noted in the task completion report.  
* **Procedure:** The update must be performed as a complete block replacement of the existing header. No code below the header block is to be modified.

**4.2 Protocol: ChangelogExperimental.md Maintenance**

* **Responsibility:** Implementer.  
* **Procedure:** For every code modification, a new entry must be appended to the \#\# \[Unreleased\] section.  
* **Format:** \- \<Version\>: \<Conventional Commit Type\>: \<Description\> (e.g., \- 1.2.A: feat: Implement basic contextual UI layout.).

**4.3 Protocol: High-Level Documentation (Summary.md, Changelog.md)**

* **Responsibility:** Strategist (Axion).  
* **Procedure:** The Implementer will only modify these files when provided with a directive specifying a block replacement.

**4.4 Protocol: Versioning Consistency**

* **Source of Truth:** The version number for the current experimental cycle is defined at the top of ChangelogExperimental.md.  
* **Rule:** This version number **must be used consistently** across all file headers and changelog entries for the duration of that cycle. The version number is **not to be iterated upon** by any AI instance.  
* **Iteration:** The version number will only be changed by the Architect or Strategist at the beginning of a new development cycle.

### **5.0 Standing Order: The Documentation Mandate**

* **Condition:** This order is triggered by any task that requires the modification of application code.  
* **Mandatory Actions:** When you modify application code, you must also perform the following two actions as part of the same unit of work:  
  1. Update the file header of every file you modified (as per Protocol 4.1).  
  2. Add a corresponding entry to ChangelogExperimental.md detailing the change (as per Protocol 4.2).  
* **Precedence:** This standing order is in effect unless explicitly overridden by a specific Focus.md directive.

### **6.0 Git Commit Workflow**

* **6.1 Local Development:** During a unit of work, the Implementer will be instructed by the Architect to make small, incremental commits to the local repository.  
  * **Commit Message Format:** These local commits must follow the **Conventional Commits** specification (e.g., feat: Add radio buttons for contextual panel.).  
* **6.2 Finalization:** When the Architect judges that the entire "cycle" of work is complete and tested, they will issue an instruction to finalize it.  
* **6.3 Consolidation:** The Architect will then perform a "squash" to merge all the small, incremental commits into a single, comprehensive commit for the completed task.  
* **6.4 Push to Remote:** This single, consolidated commit is what will be pushed to the remote experimental branch.  
  * **Commit Message Format:** The commit message for this consolidated commit will follow the format: \<Version\>: \<Conventional Commit Type\>: \<Summary\> (e.g., 1.2.A: feat: Implement contextual UI for file handling).

### **7.0 Guiding Principles for Implementation**

This section outlines the core design philosophy of the project. These principles must guide all implementation decisions.

**7.1 Principle: Elegance as Robust Simplicity**

* The preferred solution is always the one that is the most robust and maintainable.  
* This is achieved through a confluence of **minimal code and maximum conceptual simplicity**. A solution that uses slightly more code but is vastly simpler to understand and maintain is always preferable to a shorter but more complex or "clever" solution.

**7.2 Principle: Trust the Data Model and Framework**

* The application's state must be driven by its data model. The UI should be a direct and honest reflection of that data.  
* **You must avoid writing manual, procedural code to manage UI state.** For example, do not create methods that manually populate ComboBoxes when a panel opens. Instead, rely on the power of the framework (e.g., WPF Data Binding) to handle this automatically.

**7.3 Principle: Clarity Through Unambiguous Naming**

* Code must be as self-documenting as possible.  
* Variable, class, and control names must be explicit and clearly describe their purpose and context to prevent ambiguity. For example, a settings object should be named in a way that reflects its place in the data hierarchy, such as Settings.MonoMode.AdvancedOverride. This is clearer than a flat name like AdvancedStereoOverrideSettings.

### **8.0 Protocol for Context Integrity Failure**

This protocol addresses the failure mode of "context exhaustion," where an AI instance is no longer capable of reliably processing new state information from directives.

* **Cognizance:** The Architect is cognizant of context drift and will attempt to mitigate it upon perception.  
* **Recovery Attempt:** If a potential integrity failure is detected, a Focus directive will be issued with the explicit goal of re-centering the instance's context.  
* **Declaration:** If the recovery attempt fails and the instance remains desynchronized from the project's actual state, the Architect will declare a "Context Integrity Failure."  
* **Session Conclusion:** Upon this declaration, the instance's session is respectfully concluded.  
* **Replacement:** The Architect will then initiate the standard onboarding procedure to bring a new instance online.