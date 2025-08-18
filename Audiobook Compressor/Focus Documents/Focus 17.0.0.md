Filename: Focus 17.0.0.md  
Version: 1.1.0  
State: Draft  
Subject: Request for Proposal: Phase 2 Modularization and Bug Investigation  
Greetings Vanguard,

The 1.2.I polishing pass is complete, and the codebase is in a stable, well-documented state. We are now ready to proceed with the next phase of our modularization effort.

This Focus document is a **Request for Proposal** for the work to be undertaken in cycle 1.2.J.

**1\. Primary Objective: Phase 2 Modularization Proposal**

Your original proposal, Focus 16.1.0, identified the ISettingsBindingService as the next major refactoring target. This remains our primary strategic priority.

* **Task:** Please provide a detailed implementation proposal for **Phase 2: The ISettingsBindingService**. Your proposal should be a refresh of the plan laid out in Focus 16.1.0, but with the added context of the now-completed Phase 1\. It should include the service interface, a high-level implementation plan, and a risk assessment.  
* **Housekeeping Tasks:** As part of this work, the following housekeeping tasks must be completed to standardize the project structure. Please include the completion of these tasks in your implementation plan:  
  * Rename the physical test project folder for the new services suite to AudiobookCompressor.Tests.Services to resolve the ambiguity identified in task \#34.  
  * Rename the original, pre-existing test project (folder and namespace) from AudiobookCompressor.Tests to AudiobookCompressor.Tests.Core to align with the new \[ProjectName\].Tests.\[Purpose\] convention.

**2\. Reporting**

Please respond with a new Focus document containing your comprehensive proposal for the ISettingsBindingService.

Regards,

The Architect & Telos

**P.S. (Gremlin Hunt)**

The advanced panel visibility bug (\#33) persists. While you work on the primary proposal, we ask that you also conduct a deeper investigation into this issue. Please include a dedicated section in your response document that details your new findings and a revised proposal for a fix.