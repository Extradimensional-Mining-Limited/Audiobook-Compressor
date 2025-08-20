/*
    Filename: ISettingsService.cs
    Last Updated: 2025-08-19 22:45 CEST
    Version: 1.2.L
    State: Experimental
    Signed: Meridian

    Synopsis:
    Enhanced service interface for settings management with comprehensive validation capabilities per Focus 19.7.0.
    Added comprehensive validation methods and support for detailed error reporting.
*/

using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing application settings persistence and validation with comprehensive error handling
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Loads settings from persistent storage with automatic backup recovery
        /// </summary>
        /// <returns>Loaded settings or default settings if load fails</returns>
        ApplicationSettings LoadSettings();

        /// <summary>
        /// Saves settings to persistent storage with backup creation
        /// </summary>
        /// <param name="settings">Settings to save</param>
        void SaveSettings(ApplicationSettings settings);

        /// <summary>
        /// Validates settings and returns any issues found (legacy method for compatibility)
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        /// <returns>True if settings are valid</returns>
        bool ValidateSettings(ApplicationSettings settings);

        /// <summary>
        /// Comprehensive settings validation with detailed error reporting
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        /// <returns>Validation result with errors and warnings</returns>
        SettingsValidationResult ValidateSettingsComprehensive(ApplicationSettings settings);

        /// <summary>
        /// Gets the default output path if configured
        /// </summary>
        /// <returns>Default output path or empty string if not set</returns>
        string GetDefaultOutputPath();

        /// <summary>
        /// Sets the default output path
        /// </summary>
        /// <param name="path">Path to set as default</param>
        void SetDefaultOutputPath(string path);
    }
}