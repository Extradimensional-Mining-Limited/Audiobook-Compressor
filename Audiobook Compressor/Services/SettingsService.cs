/*
    Filename: SettingsService.cs
    Last Updated: 2025-08-09 10:10 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Concrete implementation of ISettingsService, migrating settings persistence logic from MainWindow.xaml.cs per Focus 13.1.0 Phase 2.
*/

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing application settings persistence and validation
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private const string SettingsFile = "user-settings.xml";

        /// <summary>
        /// Loads settings from persistent storage
        /// </summary>
        public ApplicationSettings LoadSettings()
        {
            var settings = new ApplicationSettings();

            try
            {
                if (File.Exists(SettingsFile))
                {
                    var doc = XDocument.Load(SettingsFile);
                    var root = doc.Element("UserSettings");
                    if (root != null)
                    {
                        LoadApplicationSettings(root, settings);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue with default settings
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                // In a real application, you might want to show a user notification here
            }

            return settings;
        }

        /// <summary>
        /// Saves settings to persistent storage
        /// </summary>
        public void SaveSettings(ApplicationSettings settings)
        {
            try
            {
                var doc = new XDocument(
                    new XElement("UserSettings",
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
        /// Validates settings and returns any issues found
        /// </summary>
        public bool ValidateSettings(ApplicationSettings settings)
        {
            if (settings == null)
                return false;

            // Basic validation - can be expanded as needed
            if (settings.MonoMode == null || settings.StereoMode == null)
                return false;

            // Validate that selected actions are valid
            var validActions = new[] { "Copy", "Convert", "Advanced" };
            if (!validActions.Contains(settings.MonoMode.SelectedAction) ||
                !validActions.Contains(settings.StereoMode.SelectedAction))
                return false;

            // Validate bitrate values
            if (!Settings.TryParseBitrate(settings.MonoMode.Main.TargetBitrate, out _) ||
                !Settings.TryParseBitrate(settings.StereoMode.Main.TargetBitrate, out _))
                return false;

            return true;
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

        private void LoadApplicationSettings(XElement root, ApplicationSettings settings)
        {
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
}