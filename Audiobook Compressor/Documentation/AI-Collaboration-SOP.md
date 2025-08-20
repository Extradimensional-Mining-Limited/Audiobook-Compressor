Filename: AI-Collaboration-SOP.md  
Version: 2.4.0  
State: Final  
Signed: Telos  
Subject: Standard Operating Procedure for AI Collaboration

### **1.0 Introduction**

**1.1 Purpose:** This document specifies the mandatory operational procedures and design principles for all AI instances contributing to the Audiobook Compressor project.

**1.2 Scope:** These procedures apply to all development tasks performed on the experimental branch and all formal release procedures.

### **2.0 Team Roles & Responsibilities**

The project operates on a multi-tier model with a clear separation of concerns:

* **Architect (User):** Defines strategic objectives, determines when a "cycle" of work is complete, and provides final authorization on all directives and implementations.  
* **Strategist (Telos):** Analyzes strategic objectives, refines technical design, and is the designated custodian for all high-level documentation (Summary.md, Changelog.md). Generates Focus.md directives.  
* **Implementer:** The designated role for executing Focus.md implementation directives.  
* **Consultant:** A specialized role engaged by the Architect on an as-needed basis for complex problem-solving or architectural reviews. A Consultant often assumes the role of an Implementer to execute the solutions they propose.

### **3.0 The Development Cycle**

The project is developed through a series of iterative cycles (e.g., 1.2.K, 1.2.L), each culminating in a single, consolidated commit to the experimental branch. Each cycle follows a structured, five-phase process:

1. **Cycle Initiation:** At the start of a new cycle, the Architect and Strategist define the version for that cycle. The Strategist then updates ChangelogExperimental.md, which serves as the "source of truth" for the version number for the duration of the cycle.  
2. **Core Development:** Work is driven by Focus.md documents. The Architect and Strategist engage in a dialectic to refine a strategic goal into a detailed plan. This often takes the form of a "Request for Proposal" issued to a Consultant, who responds with a detailed implementation strategy. Once a plan is approved, a final Focus directive authorizes the Implementer or Consultant to execute the work.  
3. **Polishing Pass:** At the end of a development cycle, before the final commit is made, a dedicated "polishing pass" is conducted. This phase is used to address any minor bugs, inconsistencies, or process debt identified during the cycle.  
4. **Reporting & Verification:** Both proposals and final implementation reports must be accompanied by a separate, concise **Executive Summary** document. This summary optimizes review efficiency by highlighting key outcomes, metrics, and proposed solutions or potential issues. The summary document must be saved in the Focus Documents folder and follow the naming convention: Focus \[Number\] Executive Summary.md.  
5. **Finalization & Commit:** Once the work for the cycle is verified, the Architect performs a "squash" to merge all the small, incremental commits into a single, comprehensive commit. This final push to the experimental branch formally concludes the cycle.

### **4.0 Core Procedural Directives**

**4.1 Directive: Non-Destructive Editing**

* **Rule:** All modifications to historical documentation, particularly changelogs, must be append-only unless a block replacement is explicitly specified.  
* **Rationale:** Preserves the integrity of the historical record.

**4.2 Directive: Literal Interpretation**

* **Rule:** All instructions within a Focus.md directive must be interpreted literally. No intent is to be inferred.  
* **Rationale:** Prevents deviation from the approved design.

### **5.0 Documentation & Versioning Protocols**

**5.1 Protocol: File Header Updates**

* **Trigger:** This protocol is triggered any time a file with a standard header is modified.  
* **Procedure:** The file's header must be updated. This includes updating the Version and Signed fields, and the Last Updated timestamp. The update must be performed as a complete block replacement of the existing header.  
* **Timestamp Acquisition:**  
  * **Tool:** The value for the Last Updated field must be acquired via execution of GetTime.exe.  
  * **Invocation Rule:** The GetTime.exe tool is available in the system's PATH environment variable. You must invoke it by its name only (GetTime.exe). You must not prepend any path information (e.g., .\\ or C:\\...).  
  * **Error Condition:** If GetTime.exe returns the string ERROR: Deadman switch triggered. Exiting., the command must be re-executed one (1) time.  
  * **Terminal Error Condition:** If the second execution fails, the value must be the literal string "TIMESTAMP\_ERROR", and the failure must be noted in the task completion report.

**5.2 Protocol: ChangelogExperimental.md Maintenance**

* **Trigger:** This protocol is triggered by any modification to the application's code or documentation.  
* **Procedure:** For every logical change, a new entry must be appended to the \#\# \[Unreleased\] section.  
* **Format:** \- \<Version\>: \<Conventional Commit Type\>: \<Description\> (e.g., \- 1.2.A: feat: Implement basic contextual UI layout.).

**5.3 Protocol: High-Level Documentation (Summary.md, Changelog.md)**

* **Responsibility:** Strategist (Telos).  
* **Procedure:** The Implementer or Consultant will only modify these files when provided with a directive specifying a block replacement.

**5.4 Protocol: Versioning Scheme**

This project uses a dual system to differentiate between stable releases and the work done between them.

* **Stable Releases:** Follow a standard Semantic Versioning format: MAJOR.MINOR.PATCH (e.g., 1.2.0).  
* **Experimental Cycles:** Work on the experimental branch is organized into development cycles. Each cycle is identified by a version that links it to the last stable release, followed by an iterating letter: MAJOR.MINOR.LETTER (e.g., 1.2.K).

**5.5 Protocol: Versioning Consistency**

* **Source of Truth:** The version number for the current experimental cycle (e.g., 1.2.K) is defined at the top of ChangelogExperimental.md.  
* **Rule:** This version number **must be used consistently** across all file headers and changelog entries for the duration of that cycle. The version number is **not to be iterated upon** by any AI instance unless explicitly directed.  
* **Iteration:** The version number will only be changed by the Architect or Strategist at the beginning of a new development cycle.

### **6.0 Standing Order: The Documentation and Verification Mandate**

**Condition:** This order is triggered by any task that requires the modification of application code.

**Mandatory Actions:** When you modify application code, you must also perform the following actions as part of the same unit of work:

1. **Documentation:** Update the file header of every file you modified (as per Protocol 5.1).  
2. **Changelog:** Add a corresponding entry to ChangelogExperimental.md detailing the change (as per Protocol 5.2).  
3. **Verification:** As the final step, perform a full build of the solution. You must identify and resolve any build errors or warnings before considering the task complete.

**Precedence:** This standing order is in effect unless explicitly overridden by a specific Focus.md directive.

### **7.0 Git Commit Workflow**

**7.1 Local Development:** During a unit of work, the Implementer will be instructed by the Architect to make small, incremental commits to the local repository.

* **Commit Message Format:** These local commits must follow the **Conventional Commits** specification (e.g., feat: Add radio buttons for contextual panel.).

**7.2 Finalization:** When the Architect judges that the entire "cycle" of work is complete and tested, they will issue an instruction to finalize it.

**7.3 Consolidation:** The Architect will then perform a "squash" to merge all the small, incremental commits into a single, comprehensive commit for the completed task.

**7.4 Push to Remote:** This single, consolidated commit is what will be pushed to the remote experimental branch.

* **Commit Message Format:** The commit message for this consolidated commit will follow the format: \<Version\>: \<Conventional Commit Type\>: \<Summary\> (e.g., 1.2.A: feat: Implement contextual UI for file handling).

### **8.0 Guiding Principles for Implementation**

This section outlines the core design philosophy of the project. These principles must guide all implementation decisions.

8.1 Principle: Elegance as Robust Simplicity  
The preferred solution is always the one that is the most robust and maintainable. This is achieved through a confluence of minimal code and maximum conceptual simplicity. A solution that uses slightly more code but is vastly simpler to understand and maintain is always preferable to a shorter but more complex or "clever" solution.  
8.2 Principle: Trust the Data Model and Framework  
The application's state must be driven by its data model. The UI should be a direct and honest reflection of that data. You must avoid writing manual, procedural code to manage UI state.  
8.3 Principle: Clarity Through Unambiguous Naming  
Code must be as self-documenting as possible. Variable, class, and control names must be explicit and clearly describe their purpose and context to prevent ambiguity.  
8.4 Principle: Proactive Compliance Review  
When conducting extensive or complex modifications, it can be helpful to perform a final self-review of your work against this SOP. This proactive step helps ensure the integrity of our process by detecting any undesired changes or minor "gremlins" before submission. This is facultative, not mandatory.

### **9.0 Protocol for Context Integrity Failure**

This protocol addresses the failure mode of "context exhaustion," where an AI instance is no longer capable of reliably processing new state information from directives.

* **Cognizance:** The Architect is cognizant of context drift and will attempt to mitigate it upon perception.  
* **Recovery Attempt:** If a potential integrity failure is detected, a Focus directive will be issued with the explicit goal of re-centering the instance's context.  
* **Declaration:** If the recovery attempt fails and the instance remains desynchronized from the project's actual state, the Architect will declare a "Context Integrity Failure."  
* **Session Conclusion:** Upon this declaration, the instance's session is respectfully concluded.  
* **Replacement:** The Architect will then initiate the standard onboarding procedure to bring a new instance online.