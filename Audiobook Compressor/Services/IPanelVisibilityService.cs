/*
    Filename: IPanelVisibilityService.cs
    Last Updated: 2025-08-09 15:05 CEST
    Version: 1.2.I
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Service interface for panel visibility management, extracting visibility logic from MainViewModel per Focus 16.2.0 Phase 1 implementation.
    Provides centralized visibility coordination for mode-dependent UI panels.
*/

using System.ComponentModel;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing panel visibility based on current mode and settings
    /// </summary>
    public interface IPanelVisibilityService : INotifyPropertyChanged
    {
        /// <summary>
        /// Whether Mono mode panel should be visible
        /// </summary>
        bool IsMonoModeVisible { get; }

        /// <summary>
        /// Whether Stereo mode panel should be visible
        /// </summary>
        bool IsStereoModeVisible { get; }

        /// <summary>
        /// Whether Advanced panel should be visible for current mode
        /// </summary>
        bool IsAdvancedPanelVisible { get; }

        /// <summary>
        /// Whether Mono Advanced panel should be visible
        /// </summary>
        bool IsMonoAdvancedPanelVisible { get; }

        /// <summary>
        /// Whether Stereo Advanced panel should be visible
        /// </summary>
        bool IsStereoAdvancedPanelVisible { get; }

        /// <summary>
        /// Updates visibility for the specified mode and action
        /// </summary>
        /// <param name="currentMode">Current mode (Mono/Stereo)</param>
        /// <param name="monoAction">Selected action for Mono mode</param>
        /// <param name="stereoAction">Selected action for Stereo mode</param>
        void UpdateVisibilityForMode(string currentMode, string monoAction, string stereoAction);

        /// <summary>
        /// Initializes the service with actual settings state
        /// </summary>
        /// <param name="currentMode">Current mode from loaded settings</param>
        /// <param name="monoAction">Mono mode action from loaded settings</param>
        /// <param name="stereoAction">Stereo mode action from loaded settings</param>
        void Initialize(string currentMode, string monoAction, string stereoAction);

        /// <summary>
        /// Refreshes all visibility properties
        /// </summary>
        void RefreshAllVisibility();
    }
}