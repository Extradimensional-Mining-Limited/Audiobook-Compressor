/*
    Filename: MainViewModel.cs
    Last Updated: 2025-08-09 15:05 CEST
    Version: 1.2.I
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Main ViewModel implementing MVVM pattern for MainWindow, centralizing UI logic and state management per Focus 13.1.0 Phase 1.
    Enhanced with comprehensive data binding properties per Focus 14.1.0 Phase 1 implementation.
    Added OnApplicationExit() method for graceful shutdown and settings persistence per Focus 15.4.0 implementation.
    Phase 1 modularization per Focus 16.2.0: UI state and panel visibility delegated to specialized services.
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
        private readonly IUIStateService _uiStateService;
        private readonly IPanelVisibilityService _panelVisibilityService;

        private ApplicationSettings _settings;
        private CancellationTokenSource? _cancellationTokenSource;
        private List<AudioFileInfo> _pendingFiles = new();

        // UI State Properties
        private double _statusProgress;
        private bool _isProgressVisible;
        private string _statusText = "Ready";
        private bool _isProcessing;
        private string _logContent = string.Empty;

        #endregion

        #region Constructor

        public MainViewModel(
            ISettingsService settingsService,
            IAudioService audioService,
            IDialogService dialogService,
            IValidationService validationService,
            IUIStateService uiStateService,
            IPanelVisibilityService panelVisibilityService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _uiStateService = uiStateService ?? throw new ArgumentNullException(nameof(uiStateService));
            _panelVisibilityService = panelVisibilityService ?? throw new ArgumentNullException(nameof(panelVisibilityService));

            // Load settings first
            _settings = _settingsService.LoadSettings();

            // Initialize panel visibility with loaded settings to fix advanced panel bug
            _panelVisibilityService.Initialize(
                Settings.CurrentMode,
                Settings.MonoMode.SelectedAction,
                Settings.StereoMode.SelectedAction);

            // Initialize commands
            InitializeCommands();

            // Subscribe to audio service events
            _audioService.ProgressChanged += OnAudioProgressChanged;
            _audioService.FileProcessed += OnAudioFileProcessed;

            // Subscribe to UI state service changes for property forwarding
            _uiStateService.PropertyChanged += OnUIStateServicePropertyChanged;

            // Subscribe to panel visibility service changes for property forwarding
            _panelVisibilityService.PropertyChanged += OnPanelVisibilityServicePropertyChanged;
        }

        #endregion

        #region Core UI Properties

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
                UpdatePanelVisibility();
            }
        }

        /// <summary>
        /// Current processing progress (0.0 to 1.0) - Delegated to UIStateService
        /// </summary>
        public double StatusProgress => _uiStateService.StatusProgress;

        /// <summary>
        /// Whether progress bar should be visible - Delegated to UIStateService
        /// </summary>
        public bool IsProgressVisible => _uiStateService.IsProgressVisible;

        /// <summary>
        /// Current status text - Delegated to UIStateService
        /// </summary>
        public string StatusText => _uiStateService.StatusText;

        /// <summary>
        /// Whether audio processing is currently running - Delegated to UIStateService
        /// </summary>
        public bool IsProcessing => _uiStateService.IsProcessing;

        /// <summary>
        /// Log content for display in log expander - Delegated to UIStateService
        /// </summary>
        public string LogContent => _uiStateService.LogContent;

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

        #endregion

        #region Panel Visibility Properties

        /// <summary>
        /// Whether Mono mode panel should be visible - Delegated to PanelVisibilityService
        /// </summary>
        public bool IsMonoModeVisible => _panelVisibilityService.IsMonoModeVisible;

        /// <summary>
        /// Whether Stereo mode panel should be visible - Delegated to PanelVisibilityService
        /// /// </summary>
        public bool IsStereoModeVisible => _panelVisibilityService.IsStereoModeVisible;

        /// <summary>
        /// Whether Advanced panel should be visible for current mode - Delegated to PanelVisibilityService
        /// </summary>
        public bool IsAdvancedPanelVisible => _panelVisibilityService.IsAdvancedPanelVisible;

        /// <summary>
        /// Whether Mono Advanced panel should be visible - Delegated to PanelVisibilityService
        /// </summary>
        public bool IsMonoAdvancedPanelVisible => _panelVisibilityService.IsMonoAdvancedPanelVisible;

        /// <summary>
        /// Whether Stereo Advanced panel should be visible - Delegated to PanelVisibilityService
        /// </summary>
        public bool IsStereoAdvancedPanelVisible => _panelVisibilityService.IsStereoAdvancedPanelVisible;

        #endregion

        #region ComboBox Options

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

        /// <summary>
        /// Available encoding type options
        /// </summary>
        public ReadOnlyCollection<string> EncodingTypeOptions { get; } = 
            new ReadOnlyCollection<string>(new[] { "ABR", "CBR" });

        /// <summary>
        /// Available pass mode options
        /// </summary>
        public ReadOnlyCollection<string> PassModeOptions { get; } =
            new ReadOnlyCollection<string>(new[] { "1-Pass", "2-Pass" });

        /// <summary>
        /// Available sub-threshold action options
        /// </summary>
        public ReadOnlyCollection<string> SubThresholdOptions { get; } =
            new ReadOnlyCollection<string>(new[] { "Copy", "Defer to Rockit", "Convert to:" });

        #endregion

        #region Channel Mode Binding

        /// <summary>
        /// Selected channel mode for binding
        /// </summary>
        public string SelectedChannel
        {
            get => Settings.CurrentMode;
            set
            {
                if (Settings.CurrentMode != value)
                {
                    Settings.CurrentMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SettingsSummary));
                    UpdatePanelVisibility();
                    RebindMainSettings();
                }
            }
        }

        #endregion

        #region Main Settings Binding

        /// <summary>
        /// Selected bitrate with validation for binding
        /// </summary>
        public string SelectedBitrate
        {
            get => GetCurrentBitrate();
            set
            {
                if (ValidateAndSetBitrate(value))
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Selected sample rate for binding
        /// </summary>
        public string SelectedSampleRate
        {
            get => GetCurrentSampleRate();
            set
            {
                if (ValidateAndSetSampleRate(value))
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Selected conversion threshold with validation for binding
        /// </summary>
        public string SelectedThreshold
        {
            get => GetCurrentThreshold();
            set
            {
                if (ValidateAndSetThreshold(value))
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Selected encoding type for binding
        /// </summary>
        public string SelectedEncodingType
        {
            get => GetCurrentEncodingType();
            set
            {
                if (GetCurrentEncodingType() != value)
                {
                    SetCurrentEncodingType(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SettingsSummary));
                    OnPropertyChanged(nameof(IsPassModeEnabled));
                    
                    // Handle CBR/ABR logic
                    if (value == "CBR")
                    {
                        SelectedPassMode = "1-Pass";
                    }
                }
            }
        }

        /// <summary>
        /// Selected pass mode for binding
        /// </summary>
        public string SelectedPassMode
        {
            get => GetCurrentPassMode();
            set
            {
                if (GetCurrentPassMode() != value)
                {
                    SetCurrentPassMode(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Whether pass mode ComboBox should be enabled
        /// </summary>
        public bool IsPassModeEnabled => GetCurrentEncodingType() != "CBR";

        #endregion

        #region Radio Button State Properties

        /// <summary>
        /// Whether Mono Copy radio button is selected
        /// </summary>
        public bool IsMonoCopySelected
        {
            get => Settings.MonoMode.SelectedAction == "Copy";
            set
            {
                if (value && Settings.MonoMode.SelectedAction != "Copy")
                {
                    Settings.MonoMode.SelectedAction = "Copy";
                    Settings.IsAdvancedMode = false;
                    UpdatePanelVisibility();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsMonoConvertSelected));
                    OnPropertyChanged(nameof(IsMonoAdvancedSelected));
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Whether Mono Convert radio button is selected
        /// </summary>
        public bool IsMonoConvertSelected
        {
            get => Settings.MonoMode.SelectedAction == "Convert";
            set
            {
                if (value && Settings.MonoMode.SelectedAction != "Convert")
                {
                    Settings.MonoMode.SelectedAction = "Convert";
                    Settings.IsAdvancedMode = false;
                    UpdatePanelVisibility();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsMonoCopySelected));
                    OnPropertyChanged(nameof(IsMonoAdvancedSelected));
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Whether Mono Advanced radio button is selected
        /// </summary>
        public bool IsMonoAdvancedSelected
        {
            get => Settings.MonoMode.SelectedAction == "Advanced";
            set
            {
                if (value && Settings.MonoMode.SelectedAction != "Advanced")
                {
                    Settings.MonoMode.SelectedAction = "Advanced";
                    Settings.IsAdvancedMode = true;
                    UpdatePanelVisibility();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsMonoCopySelected));
                    OnPropertyChanged(nameof(IsMonoConvertSelected));
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Whether Stereo Copy radio button is selected
        /// /// </summary>
        public bool IsStereoCopySelected
        {
            get => Settings.StereoMode.SelectedAction == "Copy";
            set
            {
                if (value && Settings.StereoMode.SelectedAction != "Copy")
                {
                    Settings.StereoMode.SelectedAction = "Copy";
                    Settings.IsAdvancedMode = false;
                    UpdatePanelVisibility();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsStereoConvertSelected));
                    OnPropertyChanged(nameof(IsStereoAdvancedSelected));
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Whether Stereo Convert radio button is selected
        /// /// </summary>
        public bool IsStereoConvertSelected
        {
            get => Settings.StereoMode.SelectedAction == "Convert";
            set
            {
                if (value && Settings.StereoMode.SelectedAction != "Convert")
                {
                    Settings.StereoMode.SelectedAction = "Convert";
                    Settings.IsAdvancedMode = false;
                    UpdatePanelVisibility();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsStereoCopySelected));
                    OnPropertyChanged(nameof(IsStereoAdvancedSelected));
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        /// <summary>
        /// Whether Stereo Advanced radio button is selected
        /// /// </summary>
        public bool IsStereoAdvancedSelected
        {
            get => Settings.StereoMode.SelectedAction == "Advanced";
            set
            {
                if (value && Settings.StereoMode.SelectedAction != "Advanced")
                {
                    Settings.StereoMode.SelectedAction = "Advanced";
                    Settings.IsAdvancedMode = true;
                    UpdatePanelVisibility();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsStereoCopySelected));
                    OnPropertyChanged(nameof(IsStereoConvertSelected));
                    OnPropertyChanged(nameof(SettingsSummary));
                }
            }
        }

        #endregion

        #region Advanced Settings Binding

        /// <summary>
        /// Mono Advanced settings wrapper for binding
        /// </summary>
        public CompressionSettings MonoAdvancedSettings => Settings.MonoMode.AdvancedOverride;

        /// <summary>
        /// Stereo Advanced settings wrapper for binding
        /// </summary>
        public CompressionSettings StereoAdvancedSettings => Settings.StereoMode.AdvancedOverride;

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

                // Start processing using UIStateService
                _uiStateService.SetProcessingState(true);
                _pendingFiles.Clear();
                _uiStateService.UpdateStatus("Scanning files...", 0);

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
                    _uiStateService.UpdateStatus("No supported audio files found.", null);
                    _uiStateService.SetProcessingState(false);
                    return;
                }

                _pendingFiles.AddRange(files);
                _uiStateService.UpdateStatus($"Found {files.Count} files to process.", null);

                // Process files
                await _audioService.ProcessFilesAsync(files, Settings.OutputPath, _cancellationTokenSource.Token);

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _uiStateService.UpdateStatus("All files processed successfully.", 1.0);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorDialog($"An error occurred: {ex.Message}", "Processing Error");
                _uiStateService.UpdateStatus("Error occurred", null);
            }
            finally
            {
                _uiStateService.SetProcessingState(false);
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void ExecuteCancelProcessing()
        {
            _cancellationTokenSource?.Cancel();
            _audioService.CancelProcessing();
            _uiStateService.UpdateStatus("Cancelling...", null);
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

        /// <summary>
        /// Called when application is exiting to perform cleanup and save settings
        /// </summary>
        public void OnApplicationExit()
        {
            try
            {
                // Save current settings state
                _settingsService.SaveSettings(Settings);
                
                // Cancel any ongoing operations
                if (IsProcessing)
                {
                    _cancellationTokenSource?.Cancel();
                }
                
                // Cleanup resources
                Dispose();
            }
            catch (Exception ex)
            {
                // Log error but don't prevent application exit
                System.Diagnostics.Debug.WriteLine($"Error during application exit: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        private void OnAudioProgressChanged(object? sender, AudioProcessingProgressEventArgs e)
        {
            var fileIndex = _pendingFiles.IndexOf(e.File);
            var overallProgress = (fileIndex + e.Progress) / _pendingFiles.Count;
            _uiStateService.UpdateStatus($"Processing {System.IO.Path.GetFileName(e.File.SourcePath)}...", overallProgress);
        }

        private void OnAudioFileProcessed(object? sender, AudioFileProcessedEventArgs e)
        {
            var status = e.Success ? "Success" : "Failed";
            var fileName = System.IO.Path.GetFileName(e.File.SourcePath);
            var bitrateInfo = e.File.Bitrate.HasValue ? $" ({Models.Settings.FormatBitrate(e.File.Bitrate.Value)})" : "";

            var logMessage = $"Processed {fileName}: {status}{bitrateInfo}";
            
            // Update log content using UIStateService
            _uiStateService.AppendLog(logMessage);
            
            System.Diagnostics.Debug.WriteLine(logMessage);
        }

        private void OnUIStateServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Forward property changes from UIStateService to MainViewModel
            switch (e.PropertyName)
            {
                case nameof(IUIStateService.StatusProgress):
                    OnPropertyChanged(nameof(StatusProgress));
                    break;
                case nameof(IUIStateService.IsProgressVisible):
                    OnPropertyChanged(nameof(IsProgressVisible));
                    break;
                case nameof(IUIStateService.StatusText):
                    OnPropertyChanged(nameof(StatusText));
                    break;
                case nameof(IUIStateService.LogContent):
                    OnPropertyChanged(nameof(LogContent));
                    break;
                case nameof(IUIStateService.IsProcessing):
                    OnPropertyChanged(nameof(IsProcessing));
                    OnPropertyChanged(nameof(CanStartProcessing));
                    OnPropertyChanged(nameof(CanCancelProcessing));
                    break;
            }
        }

        private void OnPanelVisibilityServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Forward property changes from PanelVisibilityService to MainViewModel
            switch (e.PropertyName)
            {
                case nameof(IPanelVisibilityService.IsMonoModeVisible):
                    OnPropertyChanged(nameof(IsMonoModeVisible));
                    break;
                case nameof(IPanelVisibilityService.IsStereoModeVisible):
                    OnPropertyChanged(nameof(IsStereoModeVisible));
                    break;
                case nameof(IPanelVisibilityService.IsAdvancedPanelVisible):
                    OnPropertyChanged(nameof(IsAdvancedPanelVisible));
                    break;
                case nameof(IPanelVisibilityService.IsMonoAdvancedPanelVisible):
                    OnPropertyChanged(nameof(IsMonoAdvancedPanelVisible));
                    break;
                case nameof(IPanelVisibilityService.IsStereoAdvancedPanelVisible):
                    OnPropertyChanged(nameof(IsStereoAdvancedPanelVisible));
                    break;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates panel visibility service with current settings
        /// </summary>
        private void UpdatePanelVisibility()
        {
            _panelVisibilityService.UpdateVisibilityForMode(
                Settings.CurrentMode,
                Settings.MonoMode.SelectedAction,
                Settings.StereoMode.SelectedAction);
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
            OnPropertyChanged(nameof(IsMonoAdvancedPanelVisible));
            OnPropertyChanged(nameof(IsStereoAdvancedPanelVisible));
        }

        #region Settings Helper Methods

        private string GetCurrentBitrate()
        {
            var mainSettings = Settings.CurrentMode == "Mono" ?
                Settings.MonoMode.Main : Settings.StereoMode.Main;
            return mainSettings.TargetBitrate;
        }

        private string GetCurrentSampleRate()
        {
            var mainSettings = Settings.CurrentMode == "Mono" ?
                Settings.MonoMode.Main : Settings.StereoMode.Main;
            return mainSettings.SampleRate;
        }

        private string GetCurrentThreshold()
        {
            var mainSettings = Settings.CurrentMode == "Mono" ?
                Settings.MonoMode.Main : Settings.StereoMode.Main;
            return mainSettings.ConversionThreshold;
        }

        private string GetCurrentEncodingType()
        {
            var mainSettings = Settings.CurrentMode == "Mono" ?
                Settings.MonoMode.Main : Settings.StereoMode.Main;
            return mainSettings.EncodingType;
        }

        private string GetCurrentPassMode()
        {
            var mainSettings = Settings.CurrentMode == "Mono" ?
                Settings.MonoMode.Main : Settings.StereoMode.Main;
            return mainSettings.PassMode;
        }

        private void SetCurrentEncodingType(string value)
        {
            var mainSettings = Settings.CurrentMode == "Mono" ?
                Settings.MonoMode.Main : Settings.StereoMode.Main;
            mainSettings.EncodingType = value;
        }

        private void SetCurrentPassMode(string value)
        {
            var mainSettings = Settings.CurrentMode == "Mono" ?
                Settings.MonoMode.Main : Settings.StereoMode.Main;
            mainSettings.PassMode = value;
        }

        private bool ValidateAndSetBitrate(string bitrateString)
        {
            var validation = _validationService.ValidateBitrate(bitrateString, out string normalized);
            if (validation.IsValid)
            {
                var mainSettings = Settings.CurrentMode == "Mono" ?
                    Settings.MonoMode.Main : Settings.StereoMode.Main;
                mainSettings.TargetBitrate = normalized;

                // Show warnings if any
                if (validation.HasWarnings)
                {
                    _dialogService.ShowWarningDialog(
                        string.Join("\n", validation.Warnings),
                        "Bitrate Warning");
                }

                // Check threshold logic
                CheckBitrateThresholdLogic(mainSettings);
                return true;
            }
            else
            {
                _dialogService.ShowWarningDialog(
                    string.Join("\n", validation.Errors),
                    "Invalid Bitrate");
                return false;
            }
        }

        private bool ValidateAndSetSampleRate(string sampleRateString)
        {
            var validation = _validationService.ValidateSampleRate(sampleRateString);
            if (validation.IsValid)
            {
                var mainSettings = Settings.CurrentMode == "Mono" ?
                    Settings.MonoMode.Main : Settings.StereoMode.Main;
                mainSettings.SampleRate = sampleRateString;
                return true;
            }
            else
            {
                _dialogService.ShowWarningDialog(
                    string.Join("\n", validation.Errors),
                    "Invalid Sample Rate");
                return false;
            }
        }

        private bool ValidateAndSetThreshold(string thresholdString)
        {
            var validation = _validationService.ValidateBitrate(thresholdString, out string normalized);
            if (validation.IsValid)
            {
                var mainSettings = Settings.CurrentMode == "Mono" ?
                    Settings.MonoMode.Main : Settings.StereoMode.Main;
                mainSettings.ConversionThreshold = normalized;

                // Check threshold logic
                CheckBitrateThresholdLogic(mainSettings);
                return true;
            }
            else
            {
                _dialogService.ShowWarningDialog(
                    string.Join("\n", validation.Errors),
                    "Invalid Threshold");
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
                    _dialogService.ShowWarningDialog(
                        "Warning: Target bitrate is higher than conversion threshold. " +
                        "Files below the threshold will be copied instead of re-encoded.",
                        "Bitrate/Threshold Warning");
                }
            }
        }

        private void RebindMainSettings()
        {
            // Notify all main setting properties to refresh their values
            OnPropertyChanged(nameof(SelectedBitrate));
            OnPropertyChanged(nameof(SelectedSampleRate));
            OnPropertyChanged(nameof(SelectedThreshold));
            OnPropertyChanged(nameof(SelectedEncodingType));
            OnPropertyChanged(nameof(SelectedPassMode));
            OnPropertyChanged(nameof(IsPassModeEnabled));

            // Update radio button states for new mode
            OnPropertyChanged(nameof(IsMonoCopySelected));
            OnPropertyChanged(nameof(IsMonoConvertSelected));
            OnPropertyChanged(nameof(IsMonoAdvancedSelected));
            OnPropertyChanged(nameof(IsStereoCopySelected));
            OnPropertyChanged(nameof(IsStereoConvertSelected));
            OnPropertyChanged(nameof(IsStereoAdvancedSelected));

            // Update panel visibility for new mode
            UpdatePanelVisibility();
        }

        #endregion

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
            _uiStateService.PropertyChanged -= OnUIStateServicePropertyChanged;
            _panelVisibilityService.PropertyChanged -= OnPanelVisibilityServicePropertyChanged;
            _cancellationTokenSource?.Dispose();
        }

        #endregion
    }
}