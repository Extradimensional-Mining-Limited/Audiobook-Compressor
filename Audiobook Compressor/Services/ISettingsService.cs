/*
    Filename: ISettingsService.cs
    Last Updated: 2025-08-09 10:05 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Service interface for settings management, abstracting XML persistence logic from MainWindow for MVVM architecture per Focus 13.1.0 Phase 1.
*/

using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing application settings persistence and validation
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Loads settings from persistent storage
        /// </summary>
        /// <returns>Loaded settings or default settings if load fails</returns>
        ApplicationSettings LoadSettings();

        /// <summary>
        /// Saves settings to persistent storage
        /// </summary>
        /// <param name="settings">Settings to save</param>
        void SaveSettings(ApplicationSettings settings);

        /// <summary>
        /// Validates settings and returns any issues found
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        /// <returns>True if settings are valid</returns>
        bool ValidateSettings(ApplicationSettings settings);

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