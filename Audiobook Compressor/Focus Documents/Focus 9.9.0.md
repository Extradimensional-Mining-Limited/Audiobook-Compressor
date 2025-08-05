Filename: Focus 9.9.0.md  
To: The Consultant (Vanguard)  
From: Axion  
Last Updated: 2025-08-05 13:01 CEST  
Version: 9.9.0  
State: Directive

### **Subject: Finalize Naming Refactor with Settings Migration**

### **1\. Context**

Your execution of the naming refactor was successful and has significantly improved the clarity of the codebase.

During our review, we identified one remaining instance of the old naming convention in the XML serialization logic. This directive is to correct that final item and to implement a seamless migration path for existing user settings.

### **2\. Required Changes**

This task now involves a two-part change to Settings.cs to ensure both forward-writing consistency and backward-reading compatibility.

**2.A: XML Writing Logic (Consistency)**

* In the method responsible for saving settings (SaveUserSettings or similar), modify the XElement creation to use the new, correct name.  
* **Find:** new XElement("AdvancedOverride", ...)  
* **Replace with:** new XElement("Advanced", ...)

**2.B: XML Reading Logic (Migration)**

* In the method responsible for loading settings (LoadUserSettings or similar), you must add logic to handle legacy settings files.  
* The logic must first attempt to find and parse the \<Advanced\> element.  
* **If the \<Advanced\> element is not found,** it must then attempt to find and parse the legacy \<AdvancedOverride\> element.  
* This ensures that settings from older files are correctly loaded. The new writing logic from part 2.A will ensure that on the next save, the file is updated to the new format.

### **3\. Rationale**

This two-part approach provides a seamless, one-way migration path for existing user-settings.xml files. It ensures that users do not lose their settings after the update, while also guaranteeing that all settings files are progressively migrated to the new, more consistent format.

### **4\. Action Required**

Please implement both the writing and reading logic changes as specified to complete the naming refactor.