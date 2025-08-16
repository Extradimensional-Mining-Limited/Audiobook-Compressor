/*
    Filename: PanelVisibilityService.cs
    Last Updated: 2025-08-09 15:05 CEST
    Version: 1.2.I
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Concrete implementation of IPanelVisibilityService, managing panel visibility logic per Focus 16.2.0 Phase 1 implementation.
    Centralizes mode-dependent visibility calculations extracted from MainViewModel.
*/

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing panel visibility based on current mode and settings
    /// </summary>
    public class PanelVisibilityService : IPanelVisibilityService
    {
        #region Private Fields

        private string _currentMode = "Mono";
        private string _monoSelectedAction = "Copy";
        private string _stereoSelectedAction = "Copy";

        #endregion

        #region Properties

        /// <summary>
        /// Whether Mono mode panel should be visible
        /// </summary>
        public bool IsMonoModeVisible => _currentMode == "Mono";

        /// <summary>
        /// Whether Stereo mode panel should be visible
        /// </summary>
        public bool IsStereoModeVisible => _currentMode == "Stereo";

        /// <summary>
        /// Whether Advanced panel should be visible for current mode
        /// </summary>
        public bool IsAdvancedPanelVisible
        {
            get
            {
                var selectedAction = _currentMode == "Mono" ? _monoSelectedAction : _stereoSelectedAction;
                return selectedAction == "Advanced";
            }
        }

        /// <summary>
        /// Whether Mono Advanced panel should be visible
        /// </summary>
        public bool IsMonoAdvancedPanelVisible => 
            _currentMode == "Mono" && _monoSelectedAction == "Advanced";

        /// <summary>
        /// Whether Stereo Advanced panel should be visible
        /// </summary>
        public bool IsStereoAdvancedPanelVisible => 
            _currentMode == "Stereo" && _stereoSelectedAction == "Advanced";

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates visibility for the specified mode and action
        /// </summary>
        /// <param name="currentMode">Current mode (Mono/Stereo)</param>
        /// <param name="monoAction">Selected action for Mono mode</param>
        /// <param name="stereoAction">Selected action for Stereo mode</param>
        public void UpdateVisibilityForMode(string currentMode, string monoAction, string stereoAction)
        {
            var modeChanged = _currentMode != currentMode;
            var monoActionChanged = _monoSelectedAction != monoAction;
            var stereoActionChanged = _stereoSelectedAction != stereoAction;

            _currentMode = currentMode ?? "Mono";
            _monoSelectedAction = monoAction ?? "Copy";
            _stereoSelectedAction = stereoAction ?? "Copy";

            // Notify property changes only if values actually changed
            if (modeChanged)
            {
                OnPropertyChanged(nameof(IsMonoModeVisible));
                OnPropertyChanged(nameof(IsStereoModeVisible));
                OnPropertyChanged(nameof(IsAdvancedPanelVisible));
            }

            if (monoActionChanged)
            {
                OnPropertyChanged(nameof(IsMonoAdvancedPanelVisible));
                if (_currentMode == "Mono")
                {
                    OnPropertyChanged(nameof(IsAdvancedPanelVisible));
                }
            }

            if (stereoActionChanged)
            {
                OnPropertyChanged(nameof(IsStereoAdvancedPanelVisible));
                if (_currentMode == "Stereo")
                {
                    OnPropertyChanged(nameof(IsAdvancedPanelVisible));
                }
            }
        }

        /// <summary>
        /// Initializes the service with actual settings state
        /// </summary>
        /// <param name="currentMode">Current mode from loaded settings</param>
        /// <param name="monoAction">Mono mode action from loaded settings</param>
        /// <param name="stereoAction">Stereo mode action from loaded settings</param>
        public void Initialize(string currentMode, string monoAction, string stereoAction)
        {
            _currentMode = currentMode ?? "Mono";
            _monoSelectedAction = monoAction ?? "Copy";
            _stereoSelectedAction = stereoAction ?? "Copy";
            
            // Refresh all visibility properties to reflect initialized state
            RefreshAllVisibility();
        }

        /// <summary>
        /// Refreshes all visibility properties
        /// </summary>
        public void RefreshAllVisibility()
        {
            OnPropertyChanged(nameof(IsMonoModeVisible));
            OnPropertyChanged(nameof(IsStereoModeVisible));
            OnPropertyChanged(nameof(IsAdvancedPanelVisible));
            OnPropertyChanged(nameof(IsMonoAdvancedPanelVisible));
            OnPropertyChanged(nameof(IsStereoAdvancedPanelVisible));
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}