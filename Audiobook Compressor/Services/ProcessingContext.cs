/*
    Filename: ProcessingContext.cs
    Last Updated: 2025-08-05 04:48
    Version: 1.2.B
    State: Experimental
    Signed: Advisor

    Synopsis:
    Bridge class between UI hierarchical settings model and AudioProcessor logic per Focus 5.0.0 directive.
*/

using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Bridges the UI settings model with the processing logic by providing contextual settings access
    /// </summary>
    public class ProcessingContext
    {
        /// <summary>
        /// Current mode: "Mono" or "Stereo"
        /// </summary>
        public string CurrentMode { get; init; } = string.Empty;

        /// <summary>
        /// Selected action for the current mode: "Copy", "Convert", or "Advanced"
        /// </summary>
        public string SelectedAction { get; init; } = string.Empty;

        /// <summary>
        /// Main compression settings for the current mode
        /// </summary>
        public CompressionSettings MainSettings { get; init; } = new();

        /// <summary>
        /// Advanced override settings for the current mode
        /// </summary>
        public CompressionSettings AdvancedSettings { get; init; } = new();

        /// <summary>
        /// Source path for files to process
        /// </summary>
        public string SourcePath { get; init; } = string.Empty;

        /// <summary>
        /// Output path for processed files
        /// </summary>
        public string OutputPath { get; init; } = string.Empty;

        /// <summary>
        /// Creates a ProcessingContext from the current UI settings
        /// </summary>
        public static ProcessingContext FromCurrentSettings()
        {
            var currentMode = Settings.Current.CurrentMode;
            var modeSettings = currentMode == "Mono" ? Settings.Current.MonoMode : Settings.Current.StereoMode;

            return new ProcessingContext
            {
                CurrentMode = currentMode,
                SelectedAction = modeSettings.SelectedAction,
                MainSettings = modeSettings.Main,
                AdvancedSettings = modeSettings.AdvancedOverride,
                SourcePath = Settings.Current.SourcePath,
                OutputPath = Settings.Current.OutputPath
            };
        }

        /// <summary>
        /// Gets the appropriate settings based on the selected action
        /// </summary>
        public CompressionSettings GetActiveSettings()
        {
            return SelectedAction == "Advanced" ? AdvancedSettings : MainSettings;
        }

        /// <summary>
        /// Determines if a file's channel count matches the current mode
        /// </summary>
        public bool IsChannelModeMatch(int fileChannels)
        {
            return (CurrentMode == "Mono" && fileChannels == 1) ||
                   (CurrentMode == "Stereo" && fileChannels == 2);
        }
    }
}