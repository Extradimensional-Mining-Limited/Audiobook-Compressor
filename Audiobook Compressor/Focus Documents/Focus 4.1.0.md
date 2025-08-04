Filename: Focus 4.1.0.md  
To: Sir Fixalot  
From: Praxis  
Last Updated: 2025-07-27 05:05 CEST  
Version: 4.1.0  
State: Directive  
Signed: Praxis

### **Subject: New Standard Operating Procedure for Documentation and Team Roles**

Sir Fixalot,

This document establishes the definitive set of rules for maintaining all project documentation and clarifies the roles within our development team. It supersedes all previous documentation directives. Adherence to these rules is mandatory.

### **1\. Team Roles & Responsibilities**

* **Architect (User):** Provides the high-level vision and final approval on all design and implementation decisions.  
* **Praxis (Strategist):** Refines the design, manages high-level documentation (Summary.md, Changelog.md), and creates detailed directives (Focus.md).  
* **Sir Fixalot (Implementer):** Implements code based on directives and maintains low-level, granular documentation (file headers, ChangelogExperimental.md).

### **2\. High-Level Documentation (Summary.md & Changelog.md)**

* **Responsibility:** The content for Summary.md and the stable Changelog.md is the responsibility of **Praxis**.  
* **Your Task:** You will only ever modify these two files when you are given a directive that provides a specific block of text and instructs you to replace an existing block. You are not required to reason about the content; your task is to perform a literal replacement.

### **3\. Procedure for ChangelogExperimental.md**

* **Responsibility:** You are the primary custodian of this file.  
* **Scope:** This file is to be updated **only** when you are working on the experimental branch.  
* **Versioning:** Each commit will be assigned an incremental letter suffix (e.g., 1.2.A, 1.2.B). You will be provided with the correct version for each task.  
* **Entry Format:**  
  * All new entries must be added to the \#\# \[Unreleased\] section.  
  * The format is: \- \<Version\>: \<Conventional Commit Type\>: \<Description\>.  
  * **Example:** \- 1.2.A: feat: Implement basic contextual UI layout.  
* **Process:** You must follow the principle of non-destructive editing, adding new lines without altering existing ones.

### **4\. Procedure for File Headers**

* **Responsibility:** You are responsible for maintaining the headers of all files you modify.  
* **Synopsis:** Must be a brief description of the file's **current purpose**.  
* **Last Updated:** Must be acquired by executing GetTime.exe. If it fails twice, use the placeholder "TIMESTAMP\_ERROR" and report the failure.  
* **Update Process:** You will perform a block replacement of the entire header. You must not modify any code below the header.

### **5\. Standing Order: The Documentation Mandate**

* **This is a standing order that applies to all future tasks.**  
* For any task that requires you to modify the code in a file, you are **mandated** to perform the following actions as part of the **same commit**:  
  1. Update the file header of every modified file.  
  2. Add a corresponding entry to ChangelogExperimental.md detailing the change.  
* This mandate is in effect unless you are explicitly instructed to do otherwise.