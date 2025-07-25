Filename: Focus.md  
Last Updated: 2025-07-23 23:25 CEST  
Version: 1.3.0  
State: Directive  
From: Gemini

To: Sir Fixalot



## **Objective: Implement Contextual File Handling UI**

The goal is to refactor the "Compression Settings" section of MainWindow.xaml to provide a clear, powerful, and intuitive way for users to handle files that do not match their primary output selection. This directive includes a detailed specification for state management of the advanced settings panels.

### **1. XAML Layout Changes (MainWindow.xaml)**

**A. Locate Target Area:**

* The new UI elements must be inserted directly **after** the WrapPanel containing the main compression ComboBoxes and **before** the WrapPanel for "File Types".
* **Do not remove or replace the existing "File Types" or "Output Folder Defaults" sections.**

**B. Create the Contextual Panels and Their Expanders:**

* **Mono Mode Panel:**

  * Create a StackPanel named MonoModeOptionsPanel (visible only when main format is "Mono").
  * Inside, place a StackPanel with Orientation="Horizontal" containing three RadioButton controls:

    1. Content="Copy stereo files"
    2. Content="Convert stereo to mono" (This one must have IsChecked="True").
    3. Content="Advanced..."

  * **Directly below these radio buttons, inside this same panel**, place the first Expander:

    * Name: AdvancedStereoHandlingExpander.
    * Header: "Advanced Override Settings for Stereo Files".
    * Inside this expander, place a complete, duplicate set of the main compression controls, **including its own "Channels" ComboBox**.

* **Stereo Mode Panel:**

  * Create a second StackPanel named StereoModeOptionsPanel (visible only when main format is "Stereo").
  * Inside, place a StackPanel with Orientation="Horizontal" containing three RadioButton controls:

    1. Content="Copy mono files" (This one must have IsChecked="True").
    2. Content="Convert mono to stereo"
    3. Content="Advanced..."

  * **Directly below these radio buttons, inside this same panel**, place the second Expander:

    * Name: AdvancedMonoHandlingExpander.
    * Header: "Advanced Override Settings for Mono Files".
    * Inside this expander, place another complete, duplicate set of the main compression controls, **including its own "Channels" ComboBox**.

### **2. UI Control Logic (ViewModel / Code-Behind)**

**A. Panel Visibility:**

* The Visibility of MonoModeOptionsPanel is Visible if the main selection is "Mono", and Collapsed otherwise.
* The Visibility of StereoModeOptionsPanel is Visible if the main selection is "Stereo", and Collapsed otherwise.

**B. Expander State and Behavior:**

* Each Expander's IsExpanded property is exclusively controlled by the "Advanced..." radio button *within its own parent panel*.
* **The user must not be able to manually toggle the Expanders.** The header should not function as a button.

**C. Data Binding:**

* The controls inside AdvancedStereoHandlingExpander must be bound to a dedicated AdvancedStereoOverrideSettings object.
* The controls inside AdvancedMonoHandlingExpander must be bound to a dedicated AdvancedMonoOverrideSettings object.
* These settings objects must be persisted between application sessions.

**D. Advanced Settings State Management (Crucial Logic):**

* **State Tracking:**

  * At application startup, create a boolean flag, mainSettingsHaveChangedThisSession, and initialize it to false.
  * Any time the user changes a value in any of the *main* compression settings ComboBoxes, set this flag to true.

* **"First Open" Logic:**

  * When the user selects an "Advanced..." radio button, the application must check if this is the *first time* that specific advanced panel is being opened *during the current session*.
  * **If it is the first time AND mainSettingsHaveChangedThisSession is true:**

    * The application should perform a **one-time sync**. Copy the current values from the main settings to the corresponding advanced override settings object. This provides the user with a convenient starting point.

  * **Otherwise (if it's not the first open, OR if main settings haven't been changed):**

    * Do nothing. The advanced panel will show its own persisted values from the last session, respecting the user's saved configuration.

* **Persistence:**

  * The settings within both advanced panels are independent. Changes made in one do not affect the other.
  * All changes made in the advanced panels are persisted when the application closes.

### **3. Core Processing Logic (AudioProcessor.cs)**

**A. High-Level Logic:**

* If a file's channel count does not match the main output mode, check the radio buttons in the relevant contextual panel to decide the action.

**B. Specific Action Logic:**

* **If "Copy" is selected:** Copy the original file.
* **If "Advanced..." is selected:** Use the settings from the appropriate override object (AdvancedStereoOverrideSettings or AdvancedMonoOverrideSettings).
* **If "Convert" is selected:** Apply the special logic for the "Convert mono to stereo" case as previously defined.
