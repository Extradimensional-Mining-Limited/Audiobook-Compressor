/*
    Filename: SettingsBindingService.cs
    Last Updated: 2025-08-09 16:00 CEST
    Version: 1.2.J
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Concrete implementation of ISettingsBindingService per Focus 17.2.0 Phase 2 implementation.
    Manages complex settings binding with context-aware validation and ValidationService integration.
    Extracts ~120 lines of settings logic from MainViewModel into focused, testable service.
*/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing settings binding with context-aware validation
    /// </summary>
    public class SettingsBindingService : ISettingsBindingService
    {
        #region Dependencies

        private readonly IValidationService _validationService;

        #endregion

        #region Context State

        private ApplicationSettings? _settings;
        private string _currentMode = "Mono";
        private CompressionSettings? _currentModeSettings;

        #endregion

        #region Constructor

        public SettingsBindingService(IValidationService validationService)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        #endregion

        #region Context-Aware Properties

        /// <summary>
        /// Selected bitrate with validation for current mode context
        /// </summary>
        public string SelectedBitrate
        {
            get => _currentModeSettings?.TargetBitrate ?? "";
            set
            {
                if (_currentModeSettings != null && ValidateAndSetBitrate(value))
                {
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Selected sample rate with validation for current mode context
        /// </summary>
        public string SelectedSampleRate
        {
            get => _currentModeSettings?.SampleRate ?? "";
            set
            {
                if (_currentModeSettings != null && ValidateAndSetSampleRate(value))
                {
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Selected conversion threshold with validation for current mode context
        /// </summary>
        public string SelectedThreshold
        {
            get => _currentModeSettings?.ConversionThreshold ?? "";
            set
            {
                if (_currentModeSettings != null && ValidateAndSetThreshold(value))
                {
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Selected encoding type with CBR/ABR logic coordination
        /// </summary>
        public string SelectedEncodingType
        {
            get => _currentModeSettings?.EncodingType ?? "ABR";
            set
            {
                if (_currentModeSettings != null && _currentModeSettings.EncodingType != value)
                {
                    _currentModeSettings.EncodingType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPassModeEnabled));
                    
                    // Handle CBR/ABR logic - CBR forces 1-Pass
                    if (value == "CBR")
                    {
                        SelectedPassMode = "1-Pass";
                    }
                }
            }
        }

        /// <summary>
        /// Selected pass mode with conditional availability
        /// </summary>
        public string SelectedPassMode
        {
            get => _currentModeSettings?.PassMode ?? "1-Pass";
            set
            {
                if (_currentModeSettings != null && _currentModeSettings.PassMode != value)
                {
                    _currentModeSettings.PassMode = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether pass mode selection is enabled (not CBR)
        /// </summary>
        public bool IsPassModeEnabled => _currentModeSettings?.EncodingType != "CBR";

        #endregion

        #region Context Management

        /// <summary>
        /// Sets the settings context for mode-aware property delegation
        /// </summary>
        /// <param name="settings">Application settings instance</param>
        /// <param name="currentMode">Current mode (Mono/Stereo)</param>
        public void SetSettingsContext(ApplicationSettings settings, string currentMode)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _currentMode = currentMode ?? "Mono";
            
            // Update current mode settings reference
            _currentModeSettings = _currentMode == "Mono" 
                ? settings.MonoMode.Main 
                : settings.StereoMode.Main;
            
            RefreshBindings();
        }

        /// <summary>
        /// Refreshes all binding properties to reflect current context
        /// </summary>
        public void RefreshBindings()
        {
            OnPropertyChanged(nameof(SelectedBitrate));
            OnPropertyChanged(nameof(SelectedSampleRate));
            OnPropertyChanged(nameof(SelectedThreshold));
            OnPropertyChanged(nameof(SelectedEncodingType));
            OnPropertyChanged(nameof(SelectedPassMode));
            OnPropertyChanged(nameof(IsPassModeEnabled));
        }

        /// <summary>
        /// Validates current settings with cross-field business rules
        /// </summary>
        public void ValidateCurrentSettings()
        {
            if (_currentModeSettings == null) return;

            // Check bitrate vs threshold logic
            CheckBitrateThresholdLogic(_currentModeSettings);
        }

        #endregion

        #region Validation Methods

        private bool ValidateAndSetBitrate(string bitrateString)
        {
            if (_currentModeSettings == null) return false;

            var validation = _validationService.ValidateBitrate(bitrateString, out string normalized);
            if (validation.IsValid)
            {
                _currentModeSettings.TargetBitrate = normalized;

                // Show warnings if any
                if (validation.HasWarnings)
                {
                    OnValidationWarning(new ValidationWarningEventArgs(validation.Warnings.ToArray()));
                }

                // Check cross-field business rules
                CheckBitrateThresholdLogic(_currentModeSettings);
                return true;
            }
            else
            {
                OnValidationError(new ValidationErrorEventArgs(validation.Errors.ToArray()));
                return false;
            }
        }

        private bool ValidateAndSetSampleRate(string sampleRateString)
        {
            if (_currentModeSettings == null) return false;

            var validation = _validationService.ValidateSampleRate(sampleRateString);
            if (validation.IsValid)
            {
                _currentModeSettings.SampleRate = sampleRateString;
                return true;
            }
            else
            {
                OnValidationError(new ValidationErrorEventArgs(validation.Errors.ToArray()));
                return false;
            }
        }

        private bool ValidateAndSetThreshold(string thresholdString)
        {
            if (_currentModeSettings == null) return false;

            var validation = _validationService.ValidateBitrate(thresholdString, out string normalized);
            if (validation.IsValid)
            {
                _currentModeSettings.ConversionThreshold = normalized;

                // Check cross-field business rules
                CheckBitrateThresholdLogic(_currentModeSettings);
                return true;
            }
            else
            {
                OnValidationError(new ValidationErrorEventArgs(validation.Errors.ToArray()));
                return false;
            }
        }

        private void CheckBitrateThresholdLogic(CompressionSettings settings)
        {
            if (Models.Settings.TryParseBitrate(settings.TargetBitrate, out int targetBps) &&
                Models.Settings.TryParseBitrate(settings.ConversionThreshold, out int thresholdBps))
            {
                if (targetBps > thresholdBps)
                {
                    var warning = new[]
                    {
                        "Warning: Target bitrate is higher than conversion threshold.",
                        "Files below the threshold will be copied instead of re-encoded."
                    };
                    OnValidationWarning(new ValidationWarningEventArgs(warning));
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when validation warnings need to be displayed
        /// </summary>
        public event EventHandler<ValidationWarningEventArgs>? ValidationWarning;

        /// <summary>
        /// Raised when validation errors need to be displayed
        /// </summary>
        public event EventHandler<ValidationErrorEventArgs>? ValidationError;

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Event Raising Methods

        protected virtual void OnValidationWarning(ValidationWarningEventArgs e)
        {
            ValidationWarning?.Invoke(this, e);
        }

        protected virtual void OnValidationError(ValidationErrorEventArgs e)
        {
            ValidationError?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}