/*
    Filename: IUIStateService.cs
    Last Updated: 2025-08-09 15:05 CEST
    Version: 1.2.I
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Service interface for UI state management, extracting status, progress, and logging logic from MainViewModel per Focus 16.2.0 Phase 1 implementation.
    Provides centralized UI state coordination with property change notifications for MVVM binding.
*/

using System;
using System.ComponentModel;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing UI state including status, progress, and logging
    /// </summary>
    public interface IUIStateService : INotifyPropertyChanged
    {
        /// <summary>
        /// Current processing progress (0.0 to 1.0)
        /// </summary>
        double StatusProgress { get; }

        /// <summary>
        /// Whether progress bar should be visible
        /// </summary>
        bool IsProgressVisible { get; }

        /// <summary>
        /// Current status text
        /// </summary>
        string StatusText { get; }

        /// <summary>
        /// Log content for display in log expander
        /// </summary>
        string LogContent { get; }

        /// <summary>
        /// Whether audio processing is currently running
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// Updates status with optional progress
        /// </summary>
        /// <param name="message">Status message to display</param>
        /// <param name="progress">Optional progress value (0.0 to 1.0)</param>
        void UpdateStatus(string message, double? progress = null);

        /// <summary>
        /// Updates processing progress
        /// </summary>
        /// <param name="progress">Progress value (0.0 to 1.0)</param>
        void UpdateProgress(double progress);

        /// <summary>
        /// Appends message to log content
        /// </summary>
        /// <param name="message">Message to append to log</param>
        void AppendLog(string message);

        /// <summary>
        /// Clears all log content
        /// </summary>
        void ClearLog();

        /// <summary>
        /// Sets processing state
        /// </summary>
        /// <param name="isProcessing">Whether processing is active</param>
        void SetProcessingState(bool isProcessing);
    }
}