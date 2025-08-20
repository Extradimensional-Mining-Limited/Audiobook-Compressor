/*
    Filename: SettingsService.cs
    Last Updated: 2025-08-19 22:45 CEST
    Version: 1.2.L
    State: Experimental
    Signed: Meridian

    Synopsis:
    Enhanced SettingsService with comprehensive hardening per Focus 19.7.0.
    Implemented schema versioning (#40), comprehensive validation on load (#14), 
    backup/recovery system (#41), and SelectedAction validation (#11).
    Added robust error handling and graceful degradation for corrupted settings files.
*/

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Enhanced service for managing application settings persistence with comprehensive validation and recovery
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private const string SettingsFile = "user-settings.xml";
        private const string BackupSettingsFile = "user-settings.backup.xml";
        private const string CorruptedSettingsFile = "user-settings.corrupted.xml";

        /// <summary>
        /// Loads settings from persistent storage with comprehensive validation and recovery
        /// </summary>
        public ApplicationSettings LoadSettings()
        {
            var settings = new ApplicationSettings();

            try
            {
                // Try to load primary settings file
                if (File.Exists(SettingsFile))
                {
                    var loadResult = TryLoadSettingsFromFile(SettingsFile, settings);
                    if (loadResult.Success)
                    {
                        return settings;
                    }

                    // Primary file is corrupted, try backup
                    System.Diagnostics.Debug.WriteLine($"Primary settings file corrupted: {loadResult.Error}");
                    if (File.Exists(BackupSettingsFile))
                    {
                        System.Diagnostics.Debug.WriteLine("Attempting to restore from backup...");
                        var backupResult = TryLoadSettingsFromFile(BackupSettingsFile, new ApplicationSettings());
                        if (backupResult.Success)
                        {
                            // Move corrupted file for analysis
                            MoveCorruptedFile(SettingsFile);
                            
                            // Restore backup as primary
                            File.Copy(BackupSettingsFile, SettingsFile, true);
                            
                            System.Diagnostics.Debug.WriteLine("Successfully restored settings from backup");
                            return backupResult.Settings ?? new ApplicationSettings();
                        }
                    }

                    // Both primary and backup failed, move corrupted file and use defaults
                    MoveCorruptedFile(SettingsFile);
                    System.Diagnostics.Debug.WriteLine("Both primary and backup settings corrupted, using defaults");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Critical error loading settings: {ex.Message}");
                // Continue with default settings
            }

            return settings;
        }

        /// <summary>
        /// Saves settings to persistent storage with backup creation
        /// </summary>
        public void SaveSettings(ApplicationSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            try
            {
                // Validate settings before saving
                var validationResult = ValidateSettingsComprehensive(settings);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException($"Cannot save invalid settings: {string.Join(", ", validationResult.Errors)}");
                }

                // Create backup of existing settings if they exist
                if (File.Exists(SettingsFile))
                {
                    try
                    {
                        File.Copy(SettingsFile, BackupSettingsFile, true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Could not create backup: {ex.Message}");
                        // Continue with save even if backup fails
                    }
                }

                // Create the settings XML document
                var doc = new XDocument(
                    new XElement("UserSettings",
                        new XElement("SettingsVersion", settings.SettingsVersion),
                        new XElement("SourcePath", settings.SourcePath ?? string.Empty),
                        new XElement("OutputPath", settings.OutputPath ?? string.Empty),
                        new XElement("DefaultOutputPath", settings.DefaultOutputPath ?? string.Empty),
                        new XElement("CurrentMode", settings.CurrentMode),
                        new XElement("IsAdvancedMode", settings.IsAdvancedMode),
                        new XElement("MonoMode",
                            new XElement("SelectedAction", settings.MonoMode.SelectedAction),
                            new XElement("Main", CreateCompressionSettingsXml(settings.MonoMode.Main)),
                            new XElement("Advanced", CreateCompressionSettingsXml(settings.MonoMode.AdvancedOverride))
                        ),
                        new XElement("StereoMode",
                            new XElement("SelectedAction", settings.StereoMode.SelectedAction),
                            new XElement("Main", CreateCompressionSettingsXml(settings.StereoMode.Main)),
                            new XElement("Advanced", CreateCompressionSettingsXml(settings.StereoMode.AdvancedOverride))
                        )
                    )
                );

                doc.Save(SettingsFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw new InvalidOperationException($"Failed to save settings: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates settings and returns any issues found (legacy method for compatibility)
        /// </summary>
        public bool ValidateSettings(ApplicationSettings settings)
        {
            return ValidateSettingsComprehensive(settings).IsValid;
        }

        /// <summary>
        /// Comprehensive settings validation with detailed error reporting
        /// </summary>
        public SettingsValidationResult ValidateSettingsComprehensive(ApplicationSettings settings)
        {
            var result = new SettingsValidationResult();

            if (settings == null)
            {
                result.Errors.Add("Settings object is null");
                return result;
            }

            // Validate schema version
            if (string.IsNullOrWhiteSpace(settings.SettingsVersion))
            {
                result.Warnings.Add("Settings version is missing, using current version");
                settings.SettingsVersion = Models.Settings.CurrentSettingsVersion;
            }

            // Validate mode settings
            if (settings.MonoMode == null)
            {
                result.Errors.Add("MonoMode settings are missing");
            }
            else
            {
                ValidateModeSettings("MonoMode", settings.MonoMode, result);
            }

            if (settings.StereoMode == null)
            {
                result.Errors.Add("StereoMode settings are missing");
            }
            else
            {
                ValidateModeSettings("StereoMode", settings.StereoMode, result);
            }

            // Validate current mode
            if (!Models.Settings.ChannelOptions.Contains(settings.CurrentMode))
            {
                result.Errors.Add($"Invalid CurrentMode: '{settings.CurrentMode}'. Must be 'Mono' or 'Stereo'");
            }

            // Validate paths (warning level since they can be empty)
            if (!string.IsNullOrWhiteSpace(settings.SourcePath) && !IsValidPath(settings.SourcePath))
            {
                result.Warnings.Add($"SourcePath appears invalid: '{settings.SourcePath}'");
            }

            if (!string.IsNullOrWhiteSpace(settings.OutputPath) && !IsValidPath(settings.OutputPath))
            {
                result.Warnings.Add($"OutputPath appears invalid: '{settings.OutputPath}'");
            }

            return result;
        }

        /// <summary>
        /// Gets the default output path if configured
        /// </summary>
        public string GetDefaultOutputPath()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var doc = XDocument.Load(SettingsFile);
                    return doc.Root?.Element("DefaultOutputPath")?.Value ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading default output path: {ex.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the default output path
        /// </summary>
        public void SetDefaultOutputPath(string path)
        {
            try
            {
                var settings = LoadSettings();
                settings.DefaultOutputPath = path ?? string.Empty;
                SaveSettings(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting default output path: {ex.Message}");
                throw new InvalidOperationException($"Failed to set default output path: {ex.Message}", ex);
            }
        }

        #region Private Methods

        private SettingsLoadResult TryLoadSettingsFromFile(string filePath, ApplicationSettings settings)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                var root = doc.Element("UserSettings");
                if (root == null)
                {
                    return new SettingsLoadResult { Success = false, Error = "Invalid XML structure: missing UserSettings root element" };
                }

                LoadApplicationSettings(root, settings);
                
                // Validate loaded settings
                var validationResult = ValidateSettingsComprehensive(settings);
                if (!validationResult.IsValid)
                {
                    return new SettingsLoadResult 
                    { 
                        Success = false, 
                        Error = $"Settings validation failed: {string.Join(", ", validationResult.Errors)}" 
                    };
                }

                return new SettingsLoadResult { Success = true, Settings = settings };
            }
            catch (Exception ex)
            {
                return new SettingsLoadResult { Success = false, Error = ex.Message };
            }
        }

        private void MoveCorruptedFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var corruptedFileName = $"user-settings.corrupted.{timestamp}.xml";
                    File.Move(filePath, corruptedFileName);
                    System.Diagnostics.Debug.WriteLine($"Moved corrupted settings file to: {corruptedFileName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not move corrupted file: {ex.Message}");
            }
        }

        private void ValidateModeSettings(string modeName, ModeSettings modeSettings, SettingsValidationResult result)
        {
            // Validate SelectedAction
            if (!Models.Settings.IsValidAction(modeSettings.SelectedAction))
            {
                result.Errors.Add($"{modeName} has invalid SelectedAction: '{modeSettings.SelectedAction}'. Valid values: {string.Join(", ", Models.Settings.ValidActions)}");
            }

            // Validate main compression settings
            ValidateCompressionSettings($"{modeName}.Main", modeSettings.Main, result);
            
            // Validate advanced compression settings
            ValidateCompressionSettings($"{modeName}.Advanced", modeSettings.AdvancedOverride, result);
        }

        private void ValidateCompressionSettings(string prefix, CompressionSettings settings, SettingsValidationResult result)
        {
            // Validate channel mode
            if (!Models.Settings.ChannelOptions.Contains(settings.ChannelMode))
            {
                result.Errors.Add($"{prefix}: Invalid ChannelMode '{settings.ChannelMode}'");
            }

            // Validate target bitrate
            if (!Models.Settings.TryParseBitrate(settings.TargetBitrate, out _))
            {
                result.Errors.Add($"{prefix}: Invalid TargetBitrate '{settings.TargetBitrate}'");
            }

            // Validate sample rate
            if (!Models.Settings.TryParseSampleRate(settings.SampleRate, out _))
            {
                result.Errors.Add($"{prefix}: Invalid SampleRate '{settings.SampleRate}'");
            }

            // Validate conversion threshold
            if (!Models.Settings.TryParseBitrate(settings.ConversionThreshold, out _))
            {
                result.Errors.Add($"{prefix}: Invalid ConversionThreshold '{settings.ConversionThreshold}'");
            }

            // Validate encoding type
            if (!new[] { "ABR", "CBR" }.Contains(settings.EncodingType))
            {
                result.Errors.Add($"{prefix}: Invalid EncodingType '{settings.EncodingType}'");
            }

            // Validate pass mode
            if (!new[] { "1-Pass", "2-Pass" }.Contains(settings.PassMode))
            {
                result.Errors.Add($"{prefix}: Invalid PassMode '{settings.PassMode}'");
            }

            // Validate sub-threshold action
            if (!Models.Settings.IsValidSubThresholdAction(settings.SubThresholdAction))
            {
                result.Errors.Add($"{prefix}: Invalid SubThresholdAction '{settings.SubThresholdAction}'. Valid values: {string.Join(", ", Models.Settings.ValidSubThresholdActions)}");
            }

            // Validate custom target bitrate (if used)
            if (settings.SubThresholdAction == "ConvertTo" && !Models.Settings.TryParseBitrate(settings.CustomTargetBitrate, out _))
            {
                result.Errors.Add($"{prefix}: Invalid CustomTargetBitrate '{settings.CustomTargetBitrate}' for ConvertTo sub-threshold action");
            }
        }

        private bool IsValidPath(string path)
        {
            try
            {
                // Basic path validation
                var invalidChars = Path.GetInvalidPathChars();
                return !string.IsNullOrWhiteSpace(path) && 
                       !path.Any(c => invalidChars.Contains(c));
            }
            catch
            {
                return false;
            }
        }

        private void LoadApplicationSettings(XElement root, ApplicationSettings settings)
        {
            // Load schema version
            var settingsVersion = root.Element("SettingsVersion")?.Value;
            if (!string.IsNullOrWhiteSpace(settingsVersion))
            {
                settings.SettingsVersion = settingsVersion;
            }

            // Load path settings
            settings.SourcePath = root.Element("SourcePath")?.Value ?? string.Empty;
            settings.OutputPath = root.Element("OutputPath")?.Value ?? string.Empty;
            settings.DefaultOutputPath = root.Element("DefaultOutputPath")?.Value ?? string.Empty;

            // Load current mode and advanced mode state
            var currentMode = root.Element("CurrentMode")?.Value;
            if (!string.IsNullOrWhiteSpace(currentMode))
                settings.CurrentMode = currentMode;

            if (bool.TryParse(root.Element("IsAdvancedMode")?.Value, out bool advancedMode))
                settings.IsAdvancedMode = advancedMode;

            // Load Mono Mode settings
            var monoMode = root.Element("MonoMode");
            if (monoMode != null)
            {
                var selectedAction = monoMode.Element("SelectedAction")?.Value;
                if (!string.IsNullOrWhiteSpace(selectedAction))
                    settings.MonoMode.SelectedAction = selectedAction;

                LoadCompressionSettings(monoMode.Element("Main"), settings.MonoMode.Main);
                
                // Migration support: try new element name first, fallback to legacy name
                var advancedElement = monoMode.Element("Advanced") ?? monoMode.Element("AdvancedOverride");
                LoadCompressionSettings(advancedElement, settings.MonoMode.AdvancedOverride);
            }

            // Load Stereo Mode settings
            var stereoMode = root.Element("StereoMode");
            if (stereoMode != null)
            {
                var selectedAction = stereoMode.Element("SelectedAction")?.Value;
                if (!string.IsNullOrWhiteSpace(selectedAction))
                    settings.StereoMode.SelectedAction = selectedAction;

                LoadCompressionSettings(stereoMode.Element("Main"), settings.StereoMode.Main);
                
                // Migration support: try new element name first, fallback to legacy name
                var advancedElement = stereoMode.Element("Advanced") ?? stereoMode.Element("AdvancedOverride");
                LoadCompressionSettings(advancedElement, settings.StereoMode.AdvancedOverride);
            }
        }

        private void LoadCompressionSettings(XElement? element, CompressionSettings settings)
        {
            if (element == null) return;

            // Handle multiple XML structures for maximum compatibility
            var settingsElement = element.Element("Settings") ?? element.Element("CompressionSettings");
            var targetElement = settingsElement ?? element;

            var channelMode = targetElement.Element("ChannelMode")?.Value;
            if (!string.IsNullOrWhiteSpace(channelMode))
                settings.ChannelMode = channelMode;

            var targetBitrate = targetElement.Element("TargetBitrate")?.Value;
            if (!string.IsNullOrWhiteSpace(targetBitrate))
                settings.TargetBitrate = targetBitrate;

            var sampleRate = targetElement.Element("SampleRate")?.Value;
            if (!string.IsNullOrWhiteSpace(sampleRate))
                settings.SampleRate = sampleRate;

            var threshold = targetElement.Element("ConversionThreshold")?.Value;
            if (!string.IsNullOrWhiteSpace(threshold))
                settings.ConversionThreshold = threshold;

            var encodingType = targetElement.Element("EncodingType")?.Value;
            if (!string.IsNullOrWhiteSpace(encodingType))
                settings.EncodingType = encodingType;

            var passMode = targetElement.Element("PassMode")?.Value;
            if (!string.IsNullOrWhiteSpace(passMode))
                settings.PassMode = passMode;

            // Load sub-threshold behavior properties
            var subThresholdAction = targetElement.Element("SubThresholdAction")?.Value;
            if (!string.IsNullOrWhiteSpace(subThresholdAction))
                settings.SubThresholdAction = subThresholdAction;

            var customTargetBitrate = targetElement.Element("CustomTargetBitrate")?.Value;
            if (!string.IsNullOrWhiteSpace(customTargetBitrate))
                settings.CustomTargetBitrate = customTargetBitrate;
        }

        private XElement CreateCompressionSettingsXml(CompressionSettings settings)
        {
            return new XElement("CompressionSettings",
                new XElement("ChannelMode", settings.ChannelMode ?? string.Empty),
                new XElement("TargetBitrate", settings.TargetBitrate ?? string.Empty),
                new XElement("SampleRate", settings.SampleRate ?? string.Empty),
                new XElement("ConversionThreshold", settings.ConversionThreshold ?? string.Empty),
                new XElement("EncodingType", settings.EncodingType ?? string.Empty),
                new XElement("PassMode", settings.PassMode ?? string.Empty),
                new XElement("SubThresholdAction", settings.SubThresholdAction ?? string.Empty),
                new XElement("CustomTargetBitrate", settings.CustomTargetBitrate ?? string.Empty)
            );
        }

        #endregion
    }

    /// <summary>
    /// Result of settings loading operation
    /// </summary>
    public class SettingsLoadResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public ApplicationSettings? Settings { get; set; }
    }

    /// <summary>
    /// Result of comprehensive settings validation
    /// </summary>
    public class SettingsValidationResult
    {
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public bool IsValid => Errors.Count == 0;
        public bool HasWarnings => Warnings.Count > 0;
    }
}