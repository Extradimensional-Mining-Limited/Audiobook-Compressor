/*
    Filename: RadioButtonStateService.cs
    Last Updated: 2025-08-09 19:35 CEST
    Version: 1.2.K
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Phase 3 Modularization per Focus 18.2.0: Concrete radio button state management service.
    Extracted complex radio button coordination logic from MainViewModel (~150 lines).
    Handles atomic state updates, interdependency management, and event-driven coordination.
*/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Concrete implementation of radio button state management service
    /// Centralizes complex radio button coordination logic extracted from MainViewModel
    /// </summary>
    public class RadioButtonStateService : IRadioButtonStateService
    {
        #region Private Fields

        private ApplicationSettings? _settings;

        #endregion

        #region State Properties

        /// <summary>
        /// Whether Mono Copy radio button is selected
        /// </summary>
        public bool IsMonoCopySelected => 
            _settings?.MonoMode?.SelectedAction == "Copy";

        /// <summary>
        /// Whether Mono Convert radio button is selected
        /// </summary>
        public bool IsMonoConvertSelected => 
            _settings?.MonoMode?.SelectedAction == "Convert";

        /// <summary>
        /// Whether Mono Advanced radio button is selected
        /// </summary>
        public bool IsMonoAdvancedSelected => 
            _settings?.MonoMode?.SelectedAction == "Advanced";

        /// <summary>
        /// Whether Stereo Copy radio button is selected
        /// </summary>
        public bool IsStereoCopySelected => 
            _settings?.StereoMode?.SelectedAction == "Copy";

        /// <summary>
        /// Whether Stereo Convert radio button is selected
        /// </summary>
        public bool IsStereoConvertSelected => 
            _settings?.StereoMode?.SelectedAction == "Convert";

        /// <summary>
        /// Whether Stereo Advanced radio button is selected
        /// </summary>
        public bool IsStereoAdvancedSelected => 
            _settings?.StereoMode?.SelectedAction == "Advanced";

        #endregion

        #region State Management Methods

        /// <summary>
        /// Initializes the radio button state service with current settings
        /// </summary>
        /// <param name="settings">Current application settings</param>
        public void Initialize(ApplicationSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            // Refresh all properties after initialization
            RefreshAllStates();
            
            System.Diagnostics.Debug.WriteLine("RadioButtonStateService: Initialized with settings");
        }

        /// <summary>
        /// Sets the selected action for Mono mode with full coordination
        /// </summary>
        /// <param name="action">Selected action: Copy, Convert, or Advanced</param>
        public void SetMonoSelectedAction(string action)
        {
            if (_settings?.MonoMode == null)
            {
                System.Diagnostics.Debug.WriteLine("RadioButtonStateService: Cannot set Mono action - settings not initialized");
                return;
            }

            if (_settings.MonoMode.SelectedAction == action)
                return; // No change needed

            // Atomic state update
            var previousAction = _settings.MonoMode.SelectedAction;
            _settings.MonoMode.SelectedAction = action;
            
            // Update advanced mode flag
            _settings.IsAdvancedMode = action == "Advanced";

            // Raise state change event for external coordination
            var eventArgs = new RadioButtonStateChangedEventArgs
            {
                Mode = "Mono",
                Action = action,
                IsAdvancedMode = _settings.IsAdvancedMode,
                RequiresPanelVisibilityUpdate = true,
                RequiresSettingsSummaryUpdate = true
            };

            StateChanged?.Invoke(this, eventArgs);

            // Refresh all radio button properties
            RefreshMonoStates();

            System.Diagnostics.Debug.WriteLine(
                $"RadioButtonStateService: Mono action changed from '{previousAction}' to '{action}', " +
                $"IsAdvancedMode={_settings.IsAdvancedMode}");
        }

        /// <summary>
        /// Sets the selected action for Stereo mode with full coordination
        /// </summary>
        /// <param name="action">Selected action: Copy, Convert, or Advanced</param>
        public void SetStereoSelectedAction(string action)
        {
            if (_settings?.StereoMode == null)
            {
                System.Diagnostics.Debug.WriteLine("RadioButtonStateService: Cannot set Stereo action - settings not initialized");
                return;
            }

            if (_settings.StereoMode.SelectedAction == action)
                return; // No change needed

            // Atomic state update
            var previousAction = _settings.StereoMode.SelectedAction;
            _settings.StereoMode.SelectedAction = action;
            
            // Update advanced mode flag
            _settings.IsAdvancedMode = action == "Advanced";

            // Raise state change event for external coordination
            var eventArgs = new RadioButtonStateChangedEventArgs
            {
                Mode = "Stereo",
                Action = action,
                IsAdvancedMode = _settings.IsAdvancedMode,
                RequiresPanelVisibilityUpdate = true,
                RequiresSettingsSummaryUpdate = true
            };

            StateChanged?.Invoke(this, eventArgs);

            // Refresh all radio button properties
            RefreshStereoStates();

            System.Diagnostics.Debug.WriteLine(
                $"RadioButtonStateService: Stereo action changed from '{previousAction}' to '{action}', " +
                $"IsAdvancedMode={_settings.IsAdvancedMode}");
        }

        /// <summary>
        /// Updates radio button states for mode change
        /// </summary>
        /// <param name="newMode">New current mode</param>
        public void UpdateForModeChange(string newMode)
        {
            if (_settings == null)
            {
                System.Diagnostics.Debug.WriteLine("RadioButtonStateService: Cannot update for mode change - settings not initialized");
                return;
            }

            // Update advanced mode flag based on new mode's selected action
            var newModeSettings = newMode == "Mono" ? _settings.MonoMode : _settings.StereoMode;
            _settings.IsAdvancedMode = newModeSettings?.SelectedAction == "Advanced";

            // Refresh all properties for the new mode
            RefreshAllStates();

            System.Diagnostics.Debug.WriteLine(
                $"RadioButtonStateService: Updated for mode change to '{newMode}', " +
                $"IsAdvancedMode={_settings.IsAdvancedMode}");
        }

        /// <summary>
        /// Refreshes all radio button state properties
        /// </summary>
        public void RefreshAllStates()
        {
            RefreshMonoStates();
            RefreshStereoStates();
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when radio button state changes require external coordination
        /// </summary>
        public event EventHandler<RadioButtonStateChangedEventArgs>? StateChanged;

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes Mono radio button state properties
        /// </summary>
        private void RefreshMonoStates()
        {
            OnPropertyChanged(nameof(IsMonoCopySelected));
            OnPropertyChanged(nameof(IsMonoConvertSelected));
            OnPropertyChanged(nameof(IsMonoAdvancedSelected));
        }

        /// <summary>
        /// Refreshes Stereo radio button state properties
        /// </summary>
        private void RefreshStereoStates()
        {
            OnPropertyChanged(nameof(IsStereoCopySelected));
            OnPropertyChanged(nameof(IsStereoConvertSelected));
            OnPropertyChanged(nameof(IsStereoAdvancedSelected));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}