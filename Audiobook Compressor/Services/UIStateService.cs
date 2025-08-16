/*
    Filename: UIStateService.cs
    Last Updated: 2025-08-09 15:05 CEST
    Version: 1.2.I
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Concrete implementation of IUIStateService, managing UI state for status, progress, and logging per Focus 16.2.0 Phase 1 implementation.
    Centralizes UI state logic extracted from MainViewModel with comprehensive property change notifications.
*/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing UI state including status, progress, and logging
    /// </summary>
    public class UIStateService : IUIStateService
    {
        #region Private Fields

        private double _statusProgress;
        private bool _isProgressVisible;
        private string _statusText = "Ready";
        private string _logContent = string.Empty;
        private bool _isProcessing;

        #endregion

        #region Properties

        /// <summary>
        /// Current processing progress (0.0 to 1.0)
        /// </summary>
        public double StatusProgress
        {
            get => _statusProgress;
            private set
            {
                if (_statusProgress != value)
                {
                    _statusProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether progress bar should be visible
        /// </summary>
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            private set
            {
                if (_isProgressVisible != value)
                {
                    _isProgressVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Current status text
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Log content for display in log expander
        /// </summary>
        public string LogContent
        {
            get => _logContent;
            private set
            {
                if (_logContent != value)
                {
                    _logContent = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether audio processing is currently running
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            private set
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates status with optional progress
        /// </summary>
        /// <param name="message">Status message to display</param>
        /// <param name="progress">Optional progress value (0.0 to 1.0)</param>
        public void UpdateStatus(string message, double? progress = null)
        {
            StatusText = message ?? string.Empty;
            
            if (progress.HasValue)
            {
                StatusProgress = Math.Clamp(progress.Value, 0.0, 1.0);
                IsProgressVisible = true;
            }
            else
            {
                IsProgressVisible = false;
            }
        }

        /// <summary>
        /// Updates processing progress
        /// </summary>
        /// <param name="progress">Progress value (0.0 to 1.0)</param>
        public void UpdateProgress(double progress)
        {
            StatusProgress = Math.Clamp(progress, 0.0, 1.0);
            IsProgressVisible = true;
        }

        /// <summary>
        /// Appends message to log content
        /// </summary>
        /// <param name="message">Message to append to log</param>
        public void AppendLog(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                LogContent += message + Environment.NewLine;
            }
        }

        /// <summary>
        /// Clears all log content
        /// </summary>
        public void ClearLog()
        {
            LogContent = string.Empty;
        }

        /// <summary>
        /// Sets processing state
        /// </summary>
        /// <param name="isProcessing">Whether processing is active</param>
        public void SetProcessingState(bool isProcessing)
        {
            IsProcessing = isProcessing;
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