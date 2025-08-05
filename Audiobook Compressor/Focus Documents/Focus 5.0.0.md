Filename: Focus [5.0.0.md](http://5.0.0.md)  
To: The Implementer  
From: Praxis  
Last Updated: 2025-08-01 08:37 CEST  
Version: 5.0.0  
State: Directive

### **Subject: Implement Core Processing Logic for Contextual File Handling**

The UI for contextual file handling is now in place. This directive outlines the required changes to the core processing logic in AudioProcessor.cs to make the new UI functional. You are to implement the decision-making process that interprets the user's settings and builds the correct ffmpeg command for each file.

### **1\. Objective**

To refactor the ProcessFileAsync method (or its equivalent) in AudioProcessor.cs to correctly handle all scenarios presented by the new contextual UI, including default behaviors, explicit user choices, and advanced overrides.

### **2\. High-Level Logic Flow**

The core of the ProcessFileAsync method should follow this decision tree for each file:

function ProcessFileAsync(audioFile):  
    // 1\. Get main settings and probe the file  
    mainSettings \= GetMainSettingsFromUI()  
    fileInfo \= ProbeFile(audioFile)

    // 2\. Check if file type matches the main output mode  
    if fileInfo.ChannelCount matches mainSettings.ChannelMode:  
        // This is the simple path: a mono file in Mono mode, or a stereo file in Stereo mode.  
        ProcessAsNormal(audioFile, fileInfo, mainSettings)  
    else:  
        // This is the complex path: a stereo file in Mono mode, or a mono file in Stereo mode.  
        ProcessAsException(audioFile, fileInfo, mainSettings)

### **3\. Detailed Implementation Logic**

**A. ProcessAsException Method:**

This new method will contain the logic for handling mismatched files.

private void ProcessAsException(AudioFileInfo audioFile, Settings mainSettings)  
{  
    if (mainSettings.ChannelMode \== "Mono") // We are in Mono mode, handling a stereo file  
    {  
        // Check the state of the MonoModeOptionsPanel radio buttons  
        if (UserSelected\_CopyStereoFiles\_RadioButton)  
        {  
            CopyFile(audioFile);  
            return;  
        }  
        else if (UserSelected\_Advanced\_RadioButton)  
        {  
            // Use the settings from the AdvancedStereoOverrideSettings object  
            var overrideSettings \= GetAdvancedStereoOverrideSettings();  
            BuildAndRunFFmpeg(audioFile, overrideSettings);  
            return;  
        }  
        else // Default case: "Convert stereo to mono" is selected  
        {  
            // Use the main settings, which will correctly convert to mono  
            BuildAndRunFFmpeg(audioFile, mainSettings);  
            return;  
        }  
    }  
    else // We are in Stereo mode, handling a mono file  
    {  
        // Check the state of the StereoModeOptionsPanel radio buttons  
        if (UserSelected\_CopyMonoFiles\_RadioButton) // This is the default  
        {  
            CopyFile(audioFile);  
            return;  
        }  
        else if (UserSelected\_Advanced\_RadioButton)  
        {  
            // Use the settings from the AdvancedMonoOverrideSettings object  
            var overrideSettings \= GetAdvancedMonoOverrideSettings();  
            BuildAndRunFFmpeg(audioFile, overrideSettings);  
            return;  
        }  
        else if (UserSelected\_ConvertToStereo\_RadioButton)  
        {  
            // This requires the special upmix logic  
            HandleUpmixLogic(audioFile, mainSettings);  
            return;  
        }  
    }  
}

**B. HandleUpmixLogic Method (Special Case):**

This new method implements the nuanced logic for the "Convert mono to stereo" option.

private void HandleUpmixLogic(AudioFileInfo audioFile, Settings mainSettings)  
{  
    // 1\. Estimate the upmixed bitrate  
    long estimatedStereoBitrate \= audioFile.Bitrate \* 2;

    // 2\. Get the user's stereo conversion threshold from the main settings  
    long stereoConversionThreshold \= mainSettings.StereoConversionThreshold;

    // 3\. Compare and decide  
    if (estimatedStereoBitrate \>= stereoConversionThreshold)  
    {  
        // The file is above the threshold, so it needs full compression.  
        // Build an ffmpeg command that upmixes to stereo AND applies the main target bitrate.  
        // Example ffmpeg flag: \-af "pan=stereo|c0=c0|c1=c0"  
        BuildAndRunFFmpeg(audioFile, mainSettings);  
    }  
    else  
    {  
        // The file is below the threshold; it's already "good enough."  
        // Build an ffmpeg command that upmixes to stereo but does NOT apply the target bitrate.  
        // Encode at a high quality to preserve the signal (e.g., using a high VBR setting like \-q:a 2).  
        // The main target bitrate setting is IGNORED in this case.  
        var highQualitySettings \= mainSettings.Clone(); // Create a copy to avoid modifying the original  
        highQualitySettings.TargetBitrate \= "high\_quality\_placeholder"; // Signal to the command builder to use a quality flag instead of a bitrate flag.  
        BuildAndRunFFmpeg(audioFile, highQualitySettings);  
    }  
}

### **4\. Documentation Mandate**

As per the AI-Collaboration-SOP.md, you are required to perform the following as part of the same commit:

1. Update the file header of AudioProcessor.cs with a new version, timestamp, and a synopsis describing these changes.  
2. Add a corresponding entry to ChangelogExperimental.md detailing the implementation of this processing logic.