/*
    Filename: MainViewModel.cs
    Last Updated: 2025-08-09 10:05 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Main ViewModel implementing MVVM pattern for MainWindow, centralizing UI logic and state management per Focus 13.1.0 Phase 1.
    This class replaces the monolithic MainWindow.xaml.cs code-behind with a testable, service-oriented architecture.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Audiobook_Compressor.Models;
using Audiobook_Compressor.Services;

namespace Audiobook_Compressor.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application, implementing MVVM pattern
    /// Replaces monolithic MainWindow code-behind with service-oriented, testable architecture
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private readonly ISettingsService _settingsService;
        private readonly IAudioService _audioService;
        private readonly IDialogService _dialogService;
        private readonly IValidationService _validationService;

        private ApplicationSettings _settings;
        private CancellationTokenSource? _cancellationTokenSource;
        private List<AudioFileInfo> _pendingFiles = new();

        // UI State Properties
        private double _statusProgress;
        private bool _isProgressVisible;
        private string _statusText = "Ready";
        private bool _isProcessing;

        #endregion

        #region Constructor

        public MainViewModel(
            ISettingsService settingsService,
            IAudioService audioService,
            IDialogService dialogService,
            IValidationService validationService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));

            // Load settings
            _settings = _settingsService.LoadSettings();

            // Initialize commands
            InitializeCommands();

            // Subscribe to audio service events
            _audioService.ProgressChanged += OnAudioProgressChanged;
            _audioService.FileProcessed += OnAudioFileProcessed;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Application settings
        /// </summary>
        public ApplicationSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartProcessing));
                OnPropertyChanged(nameof(SettingsSummary));
                UpdateModeVisibility();
            }
        }

        /// <summary>
        /// Current processing progress (0.0 to 1.0)
        /// </summary>
        public double StatusProgress
        {
            get => _statusProgress;
            set
            {
                _statusProgress = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether progress bar should be visible
        /// </summary>
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set
            {
                _isProgressVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Current status text
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether audio processing is currently running
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartProcessing));
                OnPropertyChanged(nameof(CanCancelProcessing));
            }
        }

        /// <summary>
        /// Whether the Start command can be executed
        /// </summary>
        public bool CanStartProcessing =>
            !IsProcessing &&
            !string.IsNullOrWhiteSpace(Settings.SourcePath) &&
            !string.IsNullOrWhiteSpace(Settings.OutputPath);

        /// <summary>
        /// Whether the Cancel command can be executed
        /// </summary>
        public bool CanCancelProcessing => IsProcessing;

        /// <summary>
        /// Whether Mono mode panel should be visible
        /// </summary>
        public bool IsMonoModeVisible => Settings.CurrentMode == "Mono";

        /// <summary>
        /// Whether Stereo mode panel should be visible
        /// </summary>
        public bool IsStereoModeVisible => Settings.CurrentMode == "Stereo";

        /// <summary>
        /// Whether Advanced panel should be visible for current mode
        /// </summary>
        public bool IsAdvancedPanelVisible
        {
            get
            {
                var currentModeSettings = Settings.CurrentMode == "Mono" ? Settings.MonoMode : Settings.StereoMode;
                return currentModeSettings.SelectedAction == "Advanced";
            }
        }

        /// <summary>
        /// Settings summary text for display
        /// </summary>
        public string SettingsSummary
        {
            get
            {
                var activeSettings = Settings.GetActiveSettings();
                var mode = Settings.CurrentMode;
                var bitrate = activeSettings.TargetBitrate;
                var sampleRate = activeSettings.SampleRate;
                var threshold = activeSettings.ConversionThreshold;
                var encodingType = activeSettings.EncodingType;
                var passMode = activeSettings.PassMode;

                return $"{mode} | {bitrate} | {sampleRate} | {threshold} Threshold | {encodingType} | {passMode}";
            }
        }

        /// <summary>
        /// Available channel options
        /// </summary>
        public ReadOnlyCollection<string> ChannelOptions => Models.Settings.ChannelOptions;

        /// <summary>
        /// Available bitrate options
        /// </summary>
        public ReadOnlyCollection<string> BitrateOptions => Models.Settings.BitrateOptions;

        /// <summary>
        /// Available sample rate options
        /// </summary>
        public ReadOnlyCollection<string> SampleRateOptions => Models.Settings.SampleRateOptions;

        #endregion

        #region Commands

        public ICommand StartProcessingCommand { get; private set; } = null!;
        public ICommand CancelProcessingCommand { get; private set; } = null!;
        public ICommand BrowseSourceCommand { get; private set; } = null!;
        public ICommand BrowseOutputCommand { get; private set; } = null!;
        public ICommand SaveDefaultCommand { get; private set; } = null!;
        public ICommand RestoreDefaultCommand { get; private set; } = null!;
        public ICommand SaveSettingsCommand { get; private set; } = null!;

        #endregion

        #region Command Implementations

        private void InitializeCommands()
        {
            StartProcessingCommand = new RelayCommand(
                execute: async () => await ExecuteStartProcessingAsync(),
                canExecute: () => CanStartProcessing);

            CancelProcessingCommand = new RelayCommand(
                execute: () => ExecuteCancelProcessing(),
                canExecute: () => CanCancelProcessing);

            BrowseSourceCommand = new RelayCommand(
                execute: () => ExecuteBrowseSource());

            BrowseOutputCommand = new RelayCommand(
                execute: () => ExecuteBrowseOutput());

            SaveDefaultCommand = new RelayCommand(
                execute: () => ExecuteSaveDefault());

            RestoreDefaultCommand = new RelayCommand(
                execute: () => ExecuteRestoreDefault());

            SaveSettingsCommand = new RelayCommand(
                execute: () => ExecuteSaveSettings());
        }

        private async Task ExecuteStartProcessingAsync()
        {
            try
            {
                // Validate paths and settings
                var pathValidation = _validationService.ValidatePaths(Settings.SourcePath, Settings.OutputPath);
                if (!pathValidation.IsValid)
                {
                    var errorMessage = string.Join("\n", pathValidation.Errors);
                    _dialogService.ShowErrorDialog(errorMessage, "Invalid Paths");
                    return;
                }

                // Check for potential overwrites
                var overwriteValidation = _validationService.CheckForPotentialOverwrites(Settings.OutputPath);
                if (overwriteValidation.HasWarnings)
                {
                    var warningMessage = string.Join("\n", overwriteValidation.Warnings);
                    if (!_dialogService.ShowConfirmationDialog(
                        $"{warningMessage}\n\nDo you want to continue?",
                        "Potential File Overwrites"))
                    {
                        return;
                    }
                }

                // Start processing
                IsProcessing = true;
                _pendingFiles.Clear();
                UpdateStatus("Scanning files...", 0);

                _cancellationTokenSource = new CancellationTokenSource();

                // Scan for files
                var files = new List<AudioFileInfo>();
                await foreach (var file in _audioService.ScanDirectoryAsync(Settings.SourcePath))
                {
                    files.Add(file);
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        return;
                }

                if (files.Count == 0)
                {
                    UpdateStatus("No supported audio files found.", null);
                    IsProcessing = false;
                    return;
                }

                _pendingFiles.AddRange(files);
                UpdateStatus($"Found {files.Count} files to process.", null);

                // Process files
                await _audioService.ProcessFilesAsync(files, Settings.OutputPath, _cancellationTokenSource.Token);

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    UpdateStatus("All files processed successfully.", 1.0);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorDialog($"An error occurred: {ex.Message}", "Processing Error");
                UpdateStatus("Error occurred", null);
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void ExecuteCancelProcessing()
        {
            _cancellationTokenSource?.Cancel();
            _audioService.CancelProcessing();
            UpdateStatus("Cancelling...", null);
        }

        private void ExecuteBrowseSource()
        {
            var selectedPath = _dialogService.ShowFolderDialog(
                "Select Source Library Folder",
                Settings.SourcePath);

            if (!string.IsNullOrEmpty(selectedPath))
            {
                Settings.SourcePath = selectedPath;
                OnPropertyChanged(nameof(Settings));
                CheckForPathCollisions("Source");
            }
        }

        private void ExecuteBrowseOutput()
        {
            var selectedPath = _dialogService.ShowFolderDialog(
                "Select Output Folder",
                Settings.OutputPath);

            if (!string.IsNullOrEmpty(selectedPath))
            {
                Settings.OutputPath = selectedPath;
                OnPropertyChanged(nameof(Settings));
                CheckForPathCollisions("Output");
            }
        }

        private void ExecuteSaveDefault()
        {
            _settingsService.SetDefaultOutputPath(Settings.OutputPath);
            _dialogService.ShowInformationDialog(
                "Default output path saved successfully.",
                "Settings Saved");
        }

        private void ExecuteRestoreDefault()
        {
            var defaultPath = _settingsService.GetDefaultOutputPath();
            if (!string.IsNullOrWhiteSpace(defaultPath))
            {
                Settings.OutputPath = defaultPath;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void ExecuteSaveSettings()
        {
            _settingsService.SaveSettings(Settings);
        }

        #endregion

        #region Event Handlers

        private void OnAudioProgressChanged(object? sender, AudioProcessingProgressEventArgs e)
        {
            var fileIndex = _pendingFiles.IndexOf(e.File);
            var overallProgress = (fileIndex + e.Progress) / _pendingFiles.Count;
            UpdateStatus($"Processing {System.IO.Path.GetFileName(e.File.SourcePath)}...", overallProgress);
        }

        private void OnAudioFileProcessed(object? sender, AudioFileProcessedEventArgs e)
        {
            var status = e.Success ? "Success" : "Failed";
            var fileName = System.IO.Path.GetFileName(e.File.SourcePath);
            var bitrateInfo = e.File.Bitrate.HasValue ? $" ({Models.Settings.FormatBitrate(e.File.Bitrate.Value)})" : "";

            // Note: In full implementation, this would be logged to a proper log display
            System.Diagnostics.Debug.WriteLine($"Processed {fileName}: {status}{bitrateInfo}");
        }

        #endregion

        #region Private Methods

        private void UpdateStatus(string message, double? progress = null)
        {
            StatusText = message;
            if (progress.HasValue)
            {
                StatusProgress = progress.Value;
                IsProgressVisible = true;
            }
            else
            {
                IsProgressVisible = false;
            }
        }

        private void CheckForPathCollisions(string context)
        {
            if (string.Equals(Settings.SourcePath, Settings.OutputPath, StringComparison.OrdinalIgnoreCase))
            {
                var message = "Source and Output folders are the same. This may overwrite your source files.\n\n" +
                             "Do you want to continue with these paths?";

                if (!_dialogService.ShowConfirmationDialog(message, "Folder Collision Detected"))
                {
                    // User chose to change paths - could implement path clearing logic here
                    if (context == "Source")
                        Settings.SourcePath = "";
                    else
                        Settings.OutputPath = "";
                    
                    OnPropertyChanged(nameof(Settings));
                }
            }
        }

        private void UpdateModeVisibility()
        {
            OnPropertyChanged(nameof(IsMonoModeVisible));
            OnPropertyChanged(nameof(IsStereoModeVisible));
            OnPropertyChanged(nameof(IsAdvancedPanelVisible));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _audioService.ProgressChanged -= OnAudioProgressChanged;
            _audioService.FileProcessed -= OnAudioFileProcessed;
            _cancellationTokenSource?.Dispose();
        }

        #endregion
    }
}