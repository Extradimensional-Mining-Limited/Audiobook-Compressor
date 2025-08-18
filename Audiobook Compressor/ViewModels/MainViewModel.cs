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
    /// Streamlined orchestration layer with service-oriented architecture
    /// Final modularization: 9 services handling specialized concerns (~48-52% size reduction)
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
        private readonly IRadioButtonStateService _radioButtonStateService;
        private readonly IPathManagementService _pathManagementService;

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
            ISettingsBindingService settingsBindingService,
            IRadioButtonStateService radioButtonStateService,
            IPathManagementService pathManagementService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _uiStateService = uiStateService ?? throw new ArgumentNullException(nameof(uiStateService));
            _panelVisibilityService = panelVisibilityService ?? throw new ArgumentNullException(nameof(panelVisibilityService));
            _settingsBindingService = settingsBindingService ?? throw new ArgumentNullException(nameof(settingsBindingService));
            _radioButtonStateService = radioButtonStateService ?? throw new ArgumentNullException(nameof(radioButtonStateService));
            _pathManagementService = pathManagementService ?? throw new ArgumentNullException(nameof(pathManagementService));

            // Load settings first
            _settings = _settingsService.LoadSettings();

            // Initialize all services with loaded settings
            InitializeServices();

            // Subscribe to advanced settings property changes for Gremlin #36 fix
            Settings.MonoMode.AdvancedOverride.PropertyChanged += OnAdvancedSettingsPropertyChanged;
            Settings.StereoMode.AdvancedOverride.PropertyChanged += OnAdvancedSettingsPropertyChanged;

            // Initialize commands
            InitializeCommands();

            // Subscribe to all service events
            SubscribeToServiceEvents();
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
                
                // Update all services with new settings
                InitializeServices();
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

        #region PanelVisibility Properties - Delegated to PanelVisibilityService

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
                    
                    // Update radio button service for mode change
                    _radioButtonStateService.UpdateForModeChange(value);
                    
                    // Update panel visibility AFTER all context is set to prevent race conditions
                    UpdatePanelVisibility();
                    
                    // Then update UI bindings
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SettingsSummary));
                    
                    // Refresh settings binding service for new mode
                    _settingsBindingService.RefreshBindings();
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

        #region Radio Button State Properties - Delegated to RadioButtonStateService

        /// <summary>
        /// Whether Mono Copy radio button is selected - Delegated to RadioButtonStateService
        /// </summary>
        public bool IsMonoCopySelected
        {
            get => _radioButtonStateService.IsMonoCopySelected;
            set
            {
                if (value && !_radioButtonStateService.IsMonoCopySelected)
                {
                    _radioButtonStateService.SetMonoSelectedAction("Copy");
                }
            }
        }

        /// <summary>
        /// Whether Mono Convert radio button is selected - Delegated to RadioButtonStateService
        /// </summary>
        public bool IsMonoConvertSelected
        {
            get => _radioButtonStateService.IsMonoConvertSelected;
            set
            {
                if (value && !_radioButtonStateService.IsMonoConvertSelected)
                {
                    _radioButtonStateService.SetMonoSelectedAction("Convert");
                }
            }
        }

        /// <summary>
        /// Whether Mono Advanced radio button is selected - Delegated to RadioButtonStateService
        /// </summary>
        public bool IsMonoAdvancedSelected
        {
            get => _radioButtonStateService.IsMonoAdvancedSelected;
            set
            {
                if (value && !_radioButtonStateService.IsMonoAdvancedSelected)
                {
                    _radioButtonStateService.SetMonoSelectedAction("Advanced");
                }
            }
        }

        /// <summary>
        /// Whether Stereo Copy radio button is selected - Delegated to RadioButtonStateService
        /// /// </summary>
        public bool IsStereoCopySelected
        {
            get => _radioButtonStateService.IsStereoCopySelected;
            set
            {
                if (value && !_radioButtonStateService.IsStereoCopySelected)
                {
                    _radioButtonStateService.SetStereoSelectedAction("Copy");
                }
            }
        }

        /// <summary>
        /// Whether Stereo Convert radio button is selected - Delegated to RadioButtonStateService
        /// /// </summary>
        public bool IsStereoConvertSelected
        {
            get => _radioButtonStateService.IsStereoConvertSelected;
            set
            {
                if (value && !_radioButtonStateService.IsStereoConvertSelected)
                {
                    _radioButtonStateService.SetStereoSelectedAction("Convert");
                }
            }
        }

        /// <summary>
        /// Whether Stereo Advanced radio button is selected - Delegated to RadioButtonStateService
        /// /// </summary>
        public bool IsStereoAdvancedSelected
        {
            get => _radioButtonStateService.IsStereoAdvancedSelected;
            set
            {
                if (value && !_radioButtonStateService.IsStereoAdvancedSelected)
                {
                    _radioButtonStateService.SetStereoSelectedAction("Advanced");
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
                execute: async () => await ExecuteBrowseSourceAsync());

            BrowseOutputCommand = new RelayCommand(
                execute: async () => await ExecuteBrowseOutputAsync());

            SaveDefaultCommand = new RelayCommand(
                execute: async () => await ExecuteSaveDefaultAsync());

            RestoreDefaultCommand = new RelayCommand(
                execute: async () => await ExecuteRestoreDefaultAsync());

            SaveSettingsCommand = new RelayCommand(
                execute: () => ExecuteSaveSettings());
        }

        private async Task ExecuteStartProcessingAsync()
        {
            try
            {
                // Validate paths using PathManagementService
                var pathValidation = _pathManagementService.ValidatePathCombination(Settings.SourcePath, Settings.OutputPath);
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

        // Path management commands now delegated to PathManagementService
        private async Task ExecuteBrowseSourceAsync()
        {
            var result = await _pathManagementService.BrowseSourcePathAsync(Settings.SourcePath);
            if (result.Success && result.NewPath != null)
            {
                Settings.SourcePath = result.NewPath;
                OnPropertyChanged(nameof(Settings));
                OnPropertyChanged(nameof(CanStartProcessing));
            }
            else if (!result.Success && result.Errors.Any())
            {
                var errorMessage = string.Join("\n", result.Errors);
                _dialogService.ShowErrorDialog(errorMessage, "Browse Source Error");
            }
        }

        private async Task ExecuteBrowseOutputAsync()
        {
            var result = await _pathManagementService.BrowseOutputPathAsync(Settings.OutputPath);
            if (result.Success && result.NewPath != null)
            {
                Settings.OutputPath = result.NewPath;
                OnPropertyChanged(nameof(Settings));
                OnPropertyChanged(nameof(CanStartProcessing));
            }
            else if (!result.Success && result.Errors.Any())
            {
                var errorMessage = string.Join("\n", result.Errors);
                _dialogService.ShowErrorDialog(errorMessage, "Browse Output Error");
            }
        }

        private async Task ExecuteSaveDefaultAsync()
        {
            var result = await _pathManagementService.SaveDefaultOutputPathAsync(Settings.OutputPath);
            if (result.Success)
            {
                _dialogService.ShowInformationDialog(
                    result.Message ?? "Default output path saved successfully.",
                    "Settings Saved");
            }
            else if (result.Errors.Any())
            {
                var errorMessage = string.Join("\n", result.Errors);
                _dialogService.ShowErrorDialog(errorMessage, "Save Default Error");
            }
        }

        private async Task ExecuteRestoreDefaultAsync()
        {
            var result = await _pathManagementService.RestoreDefaultOutputPathAsync();
            if (result.Success && result.NewPath != null)
            {
                Settings.OutputPath = result.NewPath;
                OnPropertyChanged(nameof(Settings));
                OnPropertyChanged(nameof(CanStartProcessing));
            }
            else if (!result.Success && result.Errors.Any())
            {
                var errorMessage = string.Join("\n", result.Errors);
                _dialogService.ShowErrorDialog(errorMessage, "Restore Default Error");
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

        #region Service Initialization and Event Management

        /// <summary>
        /// Initializes all services with current settings
        /// </summary>
        private void InitializeServices()
        {
            // Initialize panel visibility with loaded settings
            _panelVisibilityService.Initialize(
                Settings.CurrentMode,
                Settings.MonoMode.SelectedAction,
                Settings.StereoMode.SelectedAction);

            // Initialize settings binding service with current context
            _settingsBindingService.SetSettingsContext(Settings, Settings.CurrentMode);

            // Initialize radio button state service
            _radioButtonStateService.Initialize(Settings);
        }

        /// <summary>
        /// Subscribes to all service events for property forwarding and coordination
        /// </summary>
        private void SubscribeToServiceEvents()
        {
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

            // Subscribe to radio button state service events
            _radioButtonStateService.PropertyChanged += OnRadioButtonStateServicePropertyChanged;
            _radioButtonStateService.StateChanged += OnRadioButtonStateChanged;

            // Subscribe to path management service events
            _pathManagementService.PathChanged += OnPathManagementPathChanged;
            _pathManagementService.ConfirmationRequired += OnPathManagementConfirmationRequired;
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
        /// Handles property changes from RadioButtonStateService
        /// </summary>
        private void OnRadioButtonStateServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Forward property changes from RadioButtonStateService to MainViewModel
            switch (e.PropertyName)
            {
                case nameof(IRadioButtonStateService.IsMonoCopySelected):
                    OnPropertyChanged(nameof(IsMonoCopySelected));
                    break;
                case nameof(IRadioButtonStateService.IsMonoConvertSelected):
                    OnPropertyChanged(nameof(IsMonoConvertSelected));
                    break;
                case nameof(IRadioButtonStateService.IsMonoAdvancedSelected):
                    OnPropertyChanged(nameof(IsMonoAdvancedSelected));
                    break;
                case nameof(IRadioButtonStateService.IsStereoCopySelected):
                    OnPropertyChanged(nameof(IsStereoCopySelected));
                    break;
                case nameof(IRadioButtonStateService.IsStereoConvertSelected):
                    OnPropertyChanged(nameof(IsStereoConvertSelected));
                    break;
                case nameof(IRadioButtonStateService.IsStereoAdvancedSelected):
                    OnPropertyChanged(nameof(IsStereoAdvancedSelected));
                    break;
            }
        }

        /// <summary>
        /// Handles state change events from RadioButtonStateService
        /// </summary>
        private void OnRadioButtonStateChanged(object? sender, RadioButtonStateChangedEventArgs e)
        {
            if (e.RequiresPanelVisibilityUpdate)
            {
                UpdatePanelVisibility();
            }

            if (e.RequiresSettingsSummaryUpdate)
            {
                OnPropertyChanged(nameof(SettingsSummary));
            }
        }

        /// <summary>
        /// Handles path change events from PathManagementService
        /// </summary>
        private void OnPathManagementPathChanged(object? sender, PathChangedEventArgs e)
        {
            if (e.RequiresCollisionCheck)
            {
                var collisionResult = _pathManagementService.CheckPathCollisions(Settings.SourcePath, Settings.OutputPath);
                if (collisionResult.HasCollisions && collisionResult.RequiresUserConfirmation)
                {
                    var confirmed = _dialogService.ShowConfirmationDialog(
                        collisionResult.ConfirmationMessage ?? "Path collision detected",
                        "Folder Collision Detected");

                    if (!confirmed)
                    {
                        // User chose to change paths - clear the conflicting path
                        if (e.PathType == "Source")
                            Settings.SourcePath = "";
                        else
                            Settings.OutputPath = "";
                        
                        OnPropertyChanged(nameof(Settings));
                        OnPropertyChanged(nameof(CanStartProcessing));
                    }
                }
            }
        }

        /// <summary>
        /// Handles confirmation required events from PathManagementService
        /// </summary>
        private void OnPathManagementConfirmationRequired(object? sender, PathConfirmationRequiredEventArgs e)
        {
            e.UserConfirmed = _dialogService.ShowConfirmationDialog(e.Message, e.Title);
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
            // Unsubscribe from audio service events
            _audioService.ProgressChanged -= OnAudioProgressChanged;
            _audioService.FileProcessed -= OnAudioFileProcessed;
            
            // Unsubscribe from UI state service events
            _uiStateService.PropertyChanged -= OnUIStateServicePropertyChanged;
            
            // Unsubscribe from panel visibility service events
            _panelVisibilityService.PropertyChanged -= OnPanelVisibilityServicePropertyChanged;
            
            // Unsubscribe from settings binding service events
            _settingsBindingService.PropertyChanged -= OnSettingsBindingServicePropertyChanged;
            _settingsBindingService.ValidationWarning -= OnSettingsBindingValidationWarning;
            _settingsBindingService.ValidationError -= OnSettingsBindingValidationError;
            
            // Unsubscribe from radio button state service events
            _radioButtonStateService.PropertyChanged -= OnRadioButtonStateServicePropertyChanged;
            _radioButtonStateService.StateChanged -= OnRadioButtonStateChanged;
            
            // Unsubscribe from path management service events
            _pathManagementService.PathChanged -= OnPathManagementPathChanged;
            _pathManagementService.ConfirmationRequired -= OnPathManagementConfirmationRequired;
            
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