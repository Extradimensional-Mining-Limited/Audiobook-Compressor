/*
    Filename: IRadioButtonStateService.cs
    Last Updated: 2025-08-09 19:30 CEST
    Version: 1.2.K
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Phase 3 Modularization per Focus 18.2.0: Radio button state management service interface.
    Centralizes complex radio button coordination logic extracted from MainViewModel.
    Handles interdependencies, mode management, and event-driven external coordination.
*/

using System;
using System.ComponentModel;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service interface for managing radio button state coordination and mode synchronization
    /// Centralizes complex radio button logic extracted from MainViewModel
    /// </summary>
    public interface IRadioButtonStateService : INotifyPropertyChanged
    {
        #region State Properties
        
        /// <summary>
        /// Whether Mono Copy radio button is selected
        /// </summary>
        bool IsMonoCopySelected { get; }
        
        /// <summary>
        /// Whether Mono Convert radio button is selected
        /// </summary>
        bool IsMonoConvertSelected { get; }
        
        /// <summary>
        /// Whether Mono Advanced radio button is selected
        /// </summary>
        bool IsMonoAdvancedSelected { get; }
        
        /// <summary>
        /// Whether Stereo Copy radio button is selected
        /// </summary>
        bool IsStereoCopySelected { get; }
        
        /// <summary>
        /// Whether Stereo Convert radio button is selected
        /// </summary>
        bool IsStereoConvertSelected { get; }
        
        /// <summary>
        /// Whether Stereo Advanced radio button is selected
        /// </summary>
        bool IsStereoAdvancedSelected { get; }
        
        #endregion
        
        #region State Management Methods
        
        /// <summary>
        /// Initializes the radio button state service with current settings
        /// </summary>
        /// <param name="settings">Current application settings</param>
        void Initialize(ApplicationSettings settings);
        
        /// <summary>
        /// Sets the selected action for Mono mode with full coordination
        /// </summary>
        /// <param name="action">Selected action: Copy, Convert, or Advanced</param>
        void SetMonoSelectedAction(string action);
        
        /// <summary>
        /// Sets the selected action for Stereo mode with full coordination
        /// </summary>
        /// <param name="action">Selected action: Copy, Convert, or Advanced</param>
        void SetStereoSelectedAction(string action);
        
        /// <summary>
        /// Updates radio button states for mode change
        /// </summary>
        /// <param name="newMode">New current mode</param>
        void UpdateForModeChange(string newMode);
        
        /// <summary>
        /// Refreshes all radio button state properties
        /// </summary>
        void RefreshAllStates();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Raised when radio button state changes require external coordination
        /// </summary>
        event EventHandler<RadioButtonStateChangedEventArgs>? StateChanged;
        
        #endregion
    }
    
    /// <summary>
    /// Event arguments for radio button state change notifications
    /// </summary>
    public class RadioButtonStateChangedEventArgs : EventArgs
    {
        public string Mode { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsAdvancedMode { get; set; }
        public bool RequiresPanelVisibilityUpdate { get; set; }
        public bool RequiresSettingsSummaryUpdate { get; set; }
    }
}