Filename: Focus 6.1.2.md  
To: The Consultant (Claude)  
From: Praxis  
Last Updated: 2025-08-02 04:55 CEST  
Version: 6.1.2  
State: Directive  
Signed: Praxis

### **Subject: Refactor and Stabilize the Application's Settings Management System**

### **1.0 Context & Objective**

The application's current settings management system has become unstable and difficult to debug due to an ambiguous data model. Settings are being cross-wired, leading to unpredictable behavior and persistence errors.

Your objective is to refactor the entire settings system based on a new, clear, and unambiguous data model. The goal is to create a robust foundation where all user settings are managed and persisted predictably and independently. This task is focused exclusively on the data model (Settings.cs), persistence (user-settings.xml), and UI data binding.

### **2.0 Conceptual Overview**

The application has two primary modes, each with a main set of settings and an optional, subordinate set of advanced override settings. This results in a hierarchical data structure.

* **Mono Mode:**  
  * Main: The primary settings for mono conversion.  
  * AdvancedOverride: The optional override settings for handling stereo files encountered in this mode.  
* **Stereo Mode:**  
  * Main: The primary settings for stereo conversion.  
  * AdvancedOverride: The optional override settings for handling mono files encountered in this mode.

### **3.0 Required Implementation: The New Data Model**

You are to refactor Settings.cs to implement a clean, hierarchical data model that reflects the UI structure.

3.1 Define a Reusable Settings Class:  
First, ensure there is a single, reusable class that defines the structure for a set of compression settings.  
public class CompressionSettings  
{  
    public string ChannelMode { get; set; }  
    public string TargetBitrate { get; set; }  
    public string SampleRate { get; set; }  
    public string ConversionThreshold { get; set; }  
    public string EncodingType { get; set; }  
    public string PassMode { get; set; }  
    // Add any other relevant properties...  
}

3.2 Define a Container for Mode-Specific Settings:  
Next, create a class that groups a "Main" and "AdvancedOverride" setting together.  
public class ModeSettings  
{  
    public CompressionSettings Main { get; set; }  
    public CompressionSettings AdvancedOverride { get; set; }  
}

3.3 Implement the Top-Level Settings Class:  
Finally, the main Settings class will contain two instances of the ModeSettings class.  
public class Settings  
{  
    public ModeSettings MonoMode { get; set; }  
    public ModeSettings StereoMode { get; set; }

    // Other application settings (e.g., FileTypes, OutputFolderDefaults) can remain here.  
}

### **4.0 Persistence and UI Binding**

**4.1 Persistence (user-settings.xml):**

* The new hierarchical structure (Settings.MonoMode.Main, Settings.MonoMode.AdvancedOverride, etc.) must be correctly serialized to and deserialized from the user-settings.xml file.

**4.2 UI Data Binding (MainWindow.xaml.cs / ViewModel):**

* The UI controls must be correctly bound to this new hierarchical data model.  
  * When the main output mode is "Mono", the primary settings panel must be bound to Settings.MonoMode.Main.  
  * The advanced override panel in Mono mode must be bound to Settings.MonoMode.AdvancedOverride.  
  * When the main output mode is "Stereo", the primary settings panel must be bound to Settings.StereoMode.Main.  
  * The advanced override panel in Stereo mode must be bound to Settings.StereoMode.AdvancedOverride.

**4.3 Deprecation of "First Open Sync" Logic:**

* The new rule is simple persistence. All four settings groups are completely independent. They load their saved values at startup and persist their values on close. There is no automatic copying of values between them.

*(Note: The "Documentation Mandate" is defined in the AI-Collaboration-SOP.md and applies to this task.)*