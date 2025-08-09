Filename: Versioning-Strategy.md  
Version: 1.0.0  
State: Stable  
Signed: Axion

## **Subject: Project Versioning and Release Strategy**

### **1.0 Overview**

This document defines the official versioning and branching strategy for the Audiobook Compressor project. Its purpose is to ensure a consistent, predictable, and traceable development workflow.

### **2.0 Branching Model**

The project utilizes a simple two-branch model:

* **main Branch:** This branch is the repository for **stable development releases**. It is considered production-ready at any given point. Code is never developed directly on this branch; it only receives code when a new stable version is released.  
* **experimental Branch:** All development, including new features, bug fixes, and refactoring, occurs on this branch. It is considered a work-in-progress and may be in an unstable state between development cycles.

### **3.0 Version Numbering Scheme**

We use a dual system to differentiate between stable releases and the work done between them.

**3.1 Stable Releases**

Stable releases follow a standard **Semantic Versioning** format: MAJOR.MINOR.PATCH (e.g., 1.2.0).

* **MAJOR:** Incremented for incompatible API changes.  
* **MINOR:** Incremented for new, backward-compatible functionality.  
* **PATCH:** Incremented for backward-compatible bug fixes.

**3.2 Experimental Cycles**

Work on the experimental branch is organized into **development cycles**. Each cycle is identified by a version that links it to the last stable release, followed by an iterating letter: MAJOR.MINOR.LETTER (e.g., 1.2.A, 1.2.B, 1.2.F).

* **MAJOR.MINOR:** Corresponds to the last stable release the work is based on.  
* **LETTER:** Denotes a specific, sequential development cycle.

### **4.0 Workflow**

1. **Start of Cycle:** At the beginning of a new development cycle, the Architect or Strategist will define the version for that cycle (e.g., 1.2.G) and update the ChangelogExperimental.md accordingly.  
2. **During Cycle:** For the entire duration of a single development cycle, the designated version number is **fixed**.  
   * All file headers updated during the cycle **must** use this version.  
   * All entries added to ChangelogExperimental.md **must** use this version.  
   * The version letter is **never iterated** by an Implementer or Consultant during a cycle.  
3. **End of Cycle:** When the work for the cycle is complete, tested, and approved, the Architect will perform the final push to the experimental branch and create the corresponding tag. The conclusion of this process marks the end of the cycle, and the letter will be iterated for the start of the next one.