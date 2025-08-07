Filename: Focus 5.0.3.md  
To: The Consultant (Vanguard)  
From: Axion  
Last Updated: 2025-08-06 08:14 CEST  
Version: 5.0.9  
State: Directive

### **Subject: Strategic Redesign of Advanced Panel "Auto" Logic**

### **1\. Strategic Pivot**

Following a final strategic review, we have arrived at a more elegant and unified theory for the "Auto" conversion logic within the Advanced panels. This directive refines the previous version to implement this superior approach.

The new strategy is to make the "Auto" option a consistent, quality-preserving choice across all scenarios.

### **2\. Required Changes**

This directive modifies the UI and logic specified in the previous version of this Focus.

**2.A: UI Refinements (MainWindow.xaml)**

The sub-section within each Advanced panel should be updated as follows:

1. **Label:** "For files below threshold:"  
2. **Radio Button 1:** "( ) Copy"  
3. **Radio Button 2:** "( ) Defer to Rockit" (This replaces "Convert: Auto")  
4. **Radio Button 3:** "( ) Convert to:"  
5. **Editable TextBox:** This control remains next to the "Convert to:" radio button and is only enabled when that option is selected.

**2.B: Core Logic Implementation (AudioProcessor.cs)**

The core processing logic for the Advanced panels must be updated to reflect this new, unified strategy.

* **If the file is at or above the threshold:**  
  * The application must process the file using the main settings defined in the top part of the Advanced panel (Channels, Bitrate, etc.).  
* **If the file is below the threshold:**  
  * If the **"Copy"** radio button is selected, the file must be copied.  
  * If the **"Convert to:"** radio button is selected, the file must be re-encoded to the specific target bitrate in the TextBox.  
  * If the **"Defer to Rockit"** radio button is selected, the application must apply the following quality-preserving logic:  
    * **If channels are being reduced (Stereo \-\> Mono):** Perform a high-quality VBR downmix, with \-maxrate capped at the original stereo file's bitrate.  
    * **If channels are being increased (Mono \-\> Stereo):** Perform a high-quality VBR upmix, with \-maxrate capped at estimated\_bitrate \* 1.10.  
    * **If channels are being kept the same (Mono \-\> Mono or Stereo \-\> Stereo):** Perform a simple file copy.

### **3\. Rationale**

This new design is superior because it creates a single, elegant, and predictable rule for the "Auto" mode: it always chooses the path of maximum quality preservation, using intelligent, dynamic caps to ensure the output is both high-fidelity and reasonable in size.

### **4\. Action Required: Submit Implementation Proposal**

Before proceeding with implementation, you are to draft and submit a comprehensive proposal, Focus 5.0.4.md, for our review. This proposal, delivered as a new Focus document, must lay out in detail your understanding of the task and your plan to carry it out.

The proposal must include:

* A section detailing your planned approach for implementing the "Defer to Rockit" logic, including the specific VBR and \-maxrate parameters you intend to use.  
* A mock-up or clear description of where and how the new UI elements will be added to the existing Advanced panels to maintain visual consistency.

Upon our review and approval of your proposal, a subsequent directive will be issued to authorize implementation.