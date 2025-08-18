/*
    Filename: MainViewModel.cs
    Last Updated: 2025-08-09 18:45 CEST
    Version: 1.2.J
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Main ViewModel implementing MVVM pattern for MainWindow, centralizing UI logic and state management per Focus 13.1.0 Phase 1.
    Enhanced with comprehensive data binding properties per Focus 14.1.0 Phase 1 implementation.
    Added OnApplicationExit() method for graceful shutdown and settings persistence per Focus 15.4.0 implementation.
    Phase 1 modularization per Focus 16.2.0: UI state and panel visibility delegated to specialized services.
    Phase 2 modularization per Focus 17.2.0: Settings binding logic delegated to ISettingsBindingService with comprehensive validation.
    Final polishing pass per Focus 17.6.0: Added advanced settings property change subscription for SettingsSummary updates (Gremlin #36 fix).
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
        private readonly ISettingsBindingService _settingsBindingService;

        private ApplicationSettings _settings;
        private CancellationTokenSource? _cancellationTokenSource;
        private List<AudioFileInfo> _pendingFiles = new();

        #endregion

        #region Constructor

        public MainViewModel(
            ISettingsService settingsService,
            IAudioService audioService,
            IDialogService dialogService,
            IValidationService validationService,
            IUIStateService uiStateService,
            IPanelVisibilityService panelVisibilityService,
            ISettingsBindingService settingsBindingService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _uiStateService = uiStateService ?? throw new ArgumentNullException(nameof(uiStateService));
            _panelVisibilityService = panelVisibilityService ?? throw new ArgumentNullException(nameof(panelVisibilityService));
            _settingsBindingService = settingsBindingService ?? throw new ArgumentNullException(nameof(settingsBindingService));

            // Load settings first
            _settings = _settingsService.LoadSettings();

            // Initialize panel visibility with loaded settings to fix advanced panel bug
            _panelVisibilityService.Initialize(
                Settings.CurrentMode,
                Settings.MonoMode.SelectedAction,
                Settings.StereoMode.SelectedAction);

            // Initialize settings binding service with current context
            _settingsBindingService.SetSettingsContext(Settings, Settings.CurrentMode);

            // Subscribe to advanced settings property changes for Gremlin #36 fix
            Settings.MonoMode.AdvancedOverride.PropertyChanged += OnAdvancedSettingsPropertyChanged;
            Settings.StereoMode.AdvancedOverride.PropertyChanged += OnAdvancedSettingsPropertyChanged;

            // Initialize commands
            InitializeCommands();

            // Subscribe to audio service events
            _audioService.ProgressChanged += OnAudioProgressChanged;
            _audioService.FileProcessed += OnAudioFileProcessed;

            // Subscribe to UI state service changes for property forwarding
            _uiStateService.PropertyChanged += OnUIStateServicePropertyChanged;

            // Subscribe to panel visibility service changes for property forwarding
            _panelVisibilityService.PropertyChanged += OnPanelVisibilityServicePropertyChanged;

            // Subscribe to settings binding service events
            _settingsBindingService.PropertyChanged += OnSettingsBindingServicePropertyChanged;
            _settingsBindingService.ValidationWarning += OnSettingsBindingValidationWarning;
            _settingsBindingService.ValidationError += OnSettingsBindingValidationError;
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
        /// /// </summary>
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
        /// /// </summary>
        public bool CanCancelProcessing => IsProcessing;

        /// <summary>
        /// Settings summary text for display with enhanced format per Focus 17.4.0
        /// Always displays main settings for current mode followed by selected radio button action
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

                // Get the selected action label
                var currentModeSettings = mode == "Mono" ? Settings.MonoMode : Settings.StereoMode;
                var selectedAction = currentModeSettings.SelectedAction;
                
                // Create action description based on selected action
                string actionDescription = selectedAction switch
                {
                    "Copy" => mode == "Mono" ? "Copy stereo files" : "Copy mono files",
                    "Convert" => mode == "Mono" ? "Convert stereo to mono" : "Convert mono to stereo",
                    "Advanced" => GetAdvancedActionDescription(mode, currentModeSettings),
                    _ => selectedAction
                };

                return $"{mode} | {bitrate} | {sampleRate} | {threshold} Threshold | {encodingType} | {passMode} | {actionDescription}";
            }
        }
        
        /// <summary>
        /// Gets the action description for Advanced mode settings
        /// /// </summary>
        /// <param name="mode">Current mode (Mono/Stereo)</param>
        /// <param name="modeSettings">Mode settings containing advanced override</param>
        /// <returns>Formatted advanced action description</returns>
        private string GetAdvancedActionDescription(string mode, ModeSettings modeSettings)
        {
            var advancedSettings = modeSettings.AdvancedOverride;
            var subThresholdAction = advancedSettings.SubThresholdAction;
            
            return subThresholdAction switch
            {
                "ConvertTo" => $"Advanced | Convert to {advancedSettings.CustomTargetBitrate}",
                "Copy" => "Advanced | Copy",
                "DeferToRockit" => "Advanced | Defer to Rockit",
                _ => "Advanced"
            };
        }

        #endregion

        #region PanelVisibility Properties

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
        /// Selected channel mode for binding with enhanced atomic updates for bug fix #33
        /// </summary>
        public string SelectedChannel
        {
            get => Settings.CurrentMode;
            set
            {
                if (Settings.CurrentMode != value)
                {
                    // Atomic update - ensure all context is updated together
                    Settings.CurrentMode = value;
                    
                    // Update settings binding service context
                    _settingsBindingService.SetSettingsContext(Settings, Settings.CurrentMode);
                    
                    // Update panel visibility AFTER all context is set to prevent race conditions
                    UpdatePanelVisibility();
                    
                    // Then update UI bindings
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SettingsSummary));
                    
                    // Refresh settings binding service for new mode
                    _settingsBindingService.RefreshBindings();

                    // Update radio button states for new mode
                    OnPropertyChanged(nameof(IsMonoCopySelected));
                    OnPropertyChanged(nameof(IsMonoConvertSelected));
                    OnPropertyChanged(nameof(IsMonoAdvancedSelected));
                    OnPropertyChanged(nameof(IsStereoCopySelected));
                    OnPropertyChanged(nameof(IsStereoConvertSelected));
                    OnPropertyChanged(nameof(IsStereoAdvancedSelected));
                }
            }
        }

        #endregion

        #region Main Settings Binding - Delegated to SettingsBindingService

        /// <summary>
        /// Selected bitrate with validation for binding - Delegated to SettingsBindingService
        /// </summary>
        public string SelectedBitrate
        {
            get => _settingsBindingService.SelectedBitrate;
            set
            {
                _settingsBindingService.SelectedBitrate = value;
                OnPropertyChanged(nameof(SettingsSummary));
            }
        }

        /// <summary>
        /// Selected sample rate for binding - Delegated to SettingsBindingService
        /// </summary>
        public string SelectedSampleRate
        {
            get => _settingsBindingService.SelectedSampleRate;
            set
            {
                _settingsBindingService.SelectedSampleRate = value;
                OnPropertyChanged(nameof(SettingsSummary));
            }
        }

        /// <summary>
        /// Selected conversion threshold with validation for binding - Delegated to SettingsBindingService
        /// </summary>
        public string SelectedThreshold
        {
            get => _settingsBindingService.SelectedThreshold;
            set
            {
                _settingsBindingService.SelectedThreshold = value;
                OnPropertyChanged(nameof(SettingsSummary));
            }
        }

        /// <summary>
        /// Selected encoding type for binding - Delegated to SettingsBindingService
        /// </summary>
        public string SelectedEncodingType
        {
            get => _settingsBindingService.SelectedEncodingType;
            set
            {
                _settingsBindingService.SelectedEncodingType = value;
                OnPropertyChanged(nameof(SettingsSummary));
            }
        }

        /// <summary>
        /// Selected pass mode for binding - Delegated to SettingsBindingService
        /// </summary>
        public string SelectedPassMode
        {
            get => _settingsBindingService.SelectedPassMode;
            set
            {
                _settingsBindingService.SelectedPassMode = value;
                OnPropertyChanged(nameof(SettingsSummary));
            }
        }

        /// <summary>
        /// Whether pass mode ComboBox should be enabled - Delegated to SettingsBindingService
        /// </summary>
        public bool IsPassModeEnabled => _settingsBindingService.IsPassModeEnabled;

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
            // Confirmation dialog per Focus 17.4.0 Feature #1C4
            if (!_dialogService.ShowConfirmationDialog(
                "Are you sure you want to cancel the current processing operation?",
                "Cancel Processing"))
            {
                return; // User chose not to cancel
            }

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

        private void OnSettingsBindingServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Forward property changes from SettingsBindingService to MainViewModel
            switch (e.PropertyName)
            {
                case nameof(ISettingsBindingService.SelectedBitrate):
                    OnPropertyChanged(nameof(SelectedBitrate));
                    OnPropertyChanged(nameof(SettingsSummary));
                    break;
                case nameof(ISettingsBindingService.SelectedSampleRate):
                    OnPropertyChanged(nameof(SelectedSampleRate));
                    OnPropertyChanged(nameof(SettingsSummary));
                    break;
                case nameof(ISettingsBindingService.SelectedThreshold):
                    OnPropertyChanged(nameof(SelectedThreshold));
                    OnPropertyChanged(nameof(SettingsSummary));
                    break;
                case nameof(ISettingsBindingService.SelectedEncodingType):
                    OnPropertyChanged(nameof(SelectedEncodingType));
                    OnPropertyChanged(nameof(SettingsSummary));
                    break;
                case nameof(ISettingsBindingService.SelectedPassMode):
                    OnPropertyChanged(nameof(SelectedPassMode));
                    OnPropertyChanged(nameof(SettingsSummary));
                    break;
                case nameof(ISettingsBindingService.IsPassModeEnabled):
                    OnPropertyChanged(nameof(IsPassModeEnabled));
                    break;
            }
        }

        private void OnSettingsBindingValidationWarning(object? sender, ValidationWarningEventArgs e)
        {
            _dialogService.ShowWarningDialog(string.Join("\n", e.Warnings), "Settings Warning");
        }

        private void OnSettingsBindingValidationError(object? sender, ValidationErrorEventArgs e)
        {
            _dialogService.ShowErrorDialog(string.Join("\n", e.Errors), "Settings Error");
        }

        /// <summary>
        /// Handles property changes in advanced settings to update SettingsSummary per Gremlin #36 fix
        /// </summary>
        private void OnAdvancedSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Update SettingsSummary when any advanced setting changes
            OnPropertyChanged(nameof(SettingsSummary));
            
            // Debug logging to track property changes
            System.Diagnostics.Debug.WriteLine($"Advanced settings changed: {e.PropertyName}");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates panel visibility service with current settings - enhanced with validation for bug fix #33
        /// </summary>
        private void UpdatePanelVisibility()
        {
            // Ensure we have valid settings context to prevent race conditions
            if (Settings?.MonoMode?.SelectedAction == null || 
                Settings?.StereoMode?.SelectedAction == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdatePanelVisibility: Skipped - Settings not fully initialized");
                return; // Skip update if settings not fully initialized
            }
            
            // Debug logging to catch any remaining race conditions
            System.Diagnostics.Debug.WriteLine(
                $"UpdatePanelVisibility: Mode={Settings.CurrentMode}, " +
                $"Mono={Settings.MonoMode.SelectedAction}, " + 
                $"Stereo={Settings.StereoMode.SelectedAction}");
            
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

        #region Settings Helper Methods - Removed (extracted to SettingsBindingService)

        // Settings helper methods moved to SettingsBindingService per Phase 2 modularization
        // - GetCurrentBitrate(), GetCurrentSampleRate(), GetCurrentThreshold()
        // - GetCurrentEncodingType(), GetCurrentPassMode()
        // - SetCurrentEncodingType(), SetCurrentPassMode()
        // - ValidateAndSetBitrate(), ValidateAndSetSampleRate(), ValidateAndSetThreshold()
        // - CheckBitrateThresholdLogic()
        // - RebindMainSettings() - Logic inlined into SelectedChannel property
        //
        // All settings binding logic now handled by ISettingsBindingService

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
            _settingsBindingService.PropertyChanged -= OnSettingsBindingServicePropertyChanged;
            _settingsBindingService.ValidationWarning -= OnSettingsBindingValidationWarning;
            _settingsBindingService.ValidationError -= OnSettingsBindingValidationError;
            
            // Unsubscribe from advanced settings property changes per Gremlin #36 fix
            if (Settings?.MonoMode?.AdvancedOverride != null)
                Settings.MonoMode.AdvancedOverride.PropertyChanged -= OnAdvancedSettingsPropertyChanged;
            if (Settings?.StereoMode?.AdvancedOverride != null)
                Settings.StereoMode.AdvancedOverride.PropertyChanged -= OnAdvancedSettingsPropertyChanged;
            
            _cancellationTokenSource?.Dispose();
        }

        #endregion
    }
}