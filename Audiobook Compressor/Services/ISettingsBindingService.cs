/*
    Filename: ISettingsBindingService.cs
    Last Updated: 2025-08-09 16:00 CEST
    Version: 1.2.J
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Service interface for complex settings binding and validation logic per Focus 17.2.0 Phase 2 implementation.
    Provides context-aware settings management with ValidationService integration and event-based validation feedback.
*/

using System;
using System.ComponentModel;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Event args for validation warnings
    /// </summary>
    public class ValidationWarningEventArgs : EventArgs
    {
        public string[] Warnings { get; }

        public ValidationWarningEventArgs(string[] warnings)
        {
            Warnings = warnings ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Event args for validation errors
    /// </summary>
    public class ValidationErrorEventArgs : EventArgs
    {
        public string[] Errors { get; }

        public ValidationErrorEventArgs(string[] errors)
        {
            Errors = errors ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Service for managing settings binding with context-aware validation
    /// </summary>
    public interface ISettingsBindingService : INotifyPropertyChanged
    {
        #region Context-Aware Properties

        /// <summary>
        /// Selected bitrate with validation for current mode context
        /// </summary>
        string SelectedBitrate { get; set; }

        /// <summary>
        /// Selected sample rate with validation for current mode context
        /// </summary>
        string SelectedSampleRate { get; set; }

        /// <summary>
        /// Selected conversion threshold with validation for current mode context
        /// </summary>
        string SelectedThreshold { get; set; }

        /// <summary>
        /// Selected encoding type with CBR/ABR logic coordination
        /// </summary>
        string SelectedEncodingType { get; set; }

        /// <summary>
        /// Selected pass mode with conditional availability
        /// </summary>
        string SelectedPassMode { get; set; }

        /// <summary>
        /// Whether pass mode selection is enabled (not CBR)
        /// </summary>
        bool IsPassModeEnabled { get; }

        #endregion

        #region Context Management

        /// <summary>
        /// Sets the settings context for mode-aware property delegation
        /// </summary>
        /// <param name="settings">Application settings instance</param>
        /// <param name="currentMode">Current mode (Mono/Stereo)</param>
        void SetSettingsContext(ApplicationSettings settings, string currentMode);

        /// <summary>
        /// Refreshes all binding properties to reflect current context
        /// </summary>
        void RefreshBindings();

        /// <summary>
        /// Validates current settings with cross-field business rules
        /// </summary>
        void ValidateCurrentSettings();

        #endregion

        #region Events

        /// <summary>
        /// Raised when validation warnings need to be displayed
        /// </summary>
        event EventHandler<ValidationWarningEventArgs> ValidationWarning;

        /// <summary>
        /// Raised when validation errors need to be displayed
        /// </summary>
        event EventHandler<ValidationErrorEventArgs> ValidationError;

        #endregion
    }
}