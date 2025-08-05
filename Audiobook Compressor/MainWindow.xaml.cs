/*
    Filename: MainWindow.xaml.cs
    Last Updated: 2025-08-05 13:32 CEST
    Version: 1.2.C
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Completed Focus 9.9.0 naming refactor: implemented XML settings migration with <Advanced> elements for consistency and backward compatibility with legacy <AdvancedOverride> elements.
*/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WinForms = System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Audiobook_Compressor.Models;
using Audiobook_Compressor.Services;
using System.Xml.Linq;

namespace Audiobook_Compressor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private double _statusProgress;
        private bool _isProgressVisible;
        private string _statusText = "Ready";
        private readonly List<AudioFileInfo> _pendingFiles = new();
        private CancellationTokenSource? _cancellationSource;
        private AudioProcessor? _audioProcessor;

        private const string SettingsFile = "user-settings.xml";
        
        private string _defaultOutputPath = string.Empty;
        
        public double StatusProgress
        {
            get => _statusProgress;
            set { _statusProgress = value; OnPropertyChanged(); }
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set { _isProgressVisible = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private bool _sourceCollisionContinue = false;
        private bool _outputCollisionContinue = false;
        private bool _isUpdatingFromSettings = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadUserSettings();
            
            // Add handlers for both expanders
            SettingsExpander.Expanded += Expander_ExpandedCollapsed;
            SettingsExpander.Collapsed += Expander_ExpandedCollapsed;
            LogExpander.Expanded += Expander_ExpandedCollapsed;
            LogExpander.Collapsed += Expander_ExpandedCollapsed;

            // Initialize ComboBox items
            InitializeComboBoxes();
            
            // Restore UI state from loaded settings - moved after InitializeComboBoxes
            RestoreUIFromSettings();

            // Default settings
            SourceBrowseButton.Click += (s, e) =>
            {
                using var dialog = new WinForms.FolderBrowserDialog
                {
                    Description = "Select Source Library Folder",
                    UseDescriptionForTitle = true
                };

                while (true)
                {
                    if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                    {
                        SourcePathTextBox.Text = dialog.SelectedPath;
                        if (ShowCollisionDialog(SourcePathTextBox.Text, OutputPathTextBox.Text, "Source"))
                        {
                            _sourceCollisionContinue = true;
                            break;
                        }
                        else
                        {
                            _sourceCollisionContinue = false;
                            continue;
                        }
                    }
                    break;
                }
            };

            OutputBrowseButton.Click += (s, e) =>
            {
                using var dialog = new WinForms.FolderBrowserDialog
                {
                    Description = "Select Output Folder",
                    UseDescriptionForTitle = true
                };

                while (true)
                {
                    if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                    {
                        OutputPathTextBox.Text = dialog.SelectedPath;
                        if (ShowCollisionDialog(SourcePathTextBox.Text, OutputPathTextBox.Text, "Output"))
                        {
                            _outputCollisionContinue = true;
                            break;
                        }
                        else
                        {
                            _outputCollisionContinue = false;
                            continue;
                        }
                    }
                    break;
                }
            };

            MakeDefaultButton.Click += (s, e) => { SaveDefaultOutputPath(); };
            RestoreDefaultButton.Click += (s, e) => { LoadDefaultOutputPath(); };

            StartButton.Click += (s, e) =>
            {
                if (IsSourceOutputCollision())
                {
                    var result = System.Windows.MessageBox.Show(
                        "Source and Output folders are the same. This may overwrite your source files.\n\nDo you want to continue?",
                        "Folder Collision Detected",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);
                    if (result != MessageBoxResult.OK)
                    {
                        // User chose Cancel, do not start
                        return;
                    }
                }
                // Only start compression if user chose OK
                StartButton_Click(s, e);
            };
            CancelButton.Click += CancelButton_Click;
            UpdateSettingsSummary();

            // Save settings on close
            this.Closing += (s, e) => SaveUserSettings();

            // Contextual panel visibility logic
            ChannelsComboBox.SelectionChanged += Channels_SelectionChanged;

            // Setup radio button event handlers for data binding
            SetupRadioButtonEventHandlers();
        }

        private void SetupRadioButtonEventHandlers()
        {
            // Mono mode radio button handlers
            MonoCopyStereoRadio.Checked += (s, e) => {
                if (!_isUpdatingFromSettings)
                {
                    Settings.Current.MonoMode.SelectedAction = "Copy";
                    Settings.Current.IsAdvancedMode = false;
                    MonoModeAdvancedPanel.Visibility = Visibility.Collapsed;
                }
            };
            
            MonoConvertStereoRadio.Checked += (s, e) => {
                if (!_isUpdatingFromSettings)
                {
                    Settings.Current.MonoMode.SelectedAction = "Convert";
                    Settings.Current.IsAdvancedMode = false;
                    MonoModeAdvancedPanel.Visibility = Visibility.Collapsed;
                }
            };
            
            MonoAdvancedRadio.Checked += (s, e) => {
                if (!_isUpdatingFromSettings)
                {
                    Settings.Current.MonoMode.SelectedAction = "Advanced";
                    Settings.Current.IsAdvancedMode = true;
                    MonoModeAdvancedPanel.Visibility = Visibility.Visible;
                }
            };

            // Stereo mode radio button handlers
            StereoCopyMonoRadio.Checked += (s, e) => {
                if (!_isUpdatingFromSettings)
                {
                    Settings.Current.StereoMode.SelectedAction = "Copy";
                    Settings.Current.IsAdvancedMode = false;
                    StereoModeAdvancedPanel.Visibility = Visibility.Collapsed;
                }
            };
            
            StereoConvertMonoRadio.Checked += (s, e) => {
                if (!_isUpdatingFromSettings)
                {
                    Settings.Current.StereoMode.SelectedAction = "Convert";
                    Settings.Current.IsAdvancedMode = false;
                    StereoModeAdvancedPanel.Visibility = Visibility.Collapsed;
                }
            };
            
            StereoAdvancedRadio.Checked += (s, e) => {
                if (!_isUpdatingFromSettings)
                {
                    Settings.Current.StereoMode.SelectedAction = "Advanced";
                    Settings.Current.IsAdvancedMode = true;
                    StereoModeAdvancedPanel.Visibility = Visibility.Visible;
                }
            };
        }

        private static string NormalizeBitrateInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            input = input.Trim().ToLowerInvariant();
            if (input.EndsWith("kb"))
                input = input[..^2]; // Remove 'kb'
            else if (input.EndsWith("k"))
                input = input[..^1]; // Remove 'k'
            // Remove any whitespace
            input = input.Trim();
            return input;
        }

        private void InitializeComboBoxes()
        {
            // Setup channel options
            ChannelsComboBox.ItemsSource = Settings.ChannelOptions;
            ChannelsComboBox.SelectedItem = Settings.DefaultChannel;
            ChannelsComboBox.SelectionChanged += Channels_SelectionChanged;

            // Setup bitrate options
            BitrateComboBox.ItemsSource = Settings.BitrateOptions;
            BitrateComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultBitrate);

            // Setup sample rate options
            SampleRateComboBox.ItemsSource = Settings.SampleRateOptions;
            SampleRateComboBox.SelectedItem = $"{Settings.DefaultSampleRate} Hz";
            SampleRateComboBox.SelectionChanged += SampleRate_SelectionChanged;

            // Setup threshold options
            ThresholdComboBox.ItemsSource = Settings.BitrateOptions;
            ThresholdComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultConversionThreshold);

            // Setup bitrate control options
            BitrateControlComboBox.ItemsSource = new[] { "ABR", "CBR" };
            BitrateControlComboBox.SelectedItem = Settings.DefaultBitrateControl;
            BitrateControlComboBox.SelectionChanged += BitrateControl_SelectionChanged;

            // Initialize advanced panel ComboBoxes ItemsSource
            // Mono Mode Advanced Controls
            MonoAdvancedChannelsComboBox.ItemsSource = Settings.ChannelOptions;
            MonoAdvancedChannelsComboBox.SelectedItem = Settings.DefaultChannel;
            MonoAdvancedBitrateComboBox.ItemsSource = Settings.BitrateOptions;
            MonoAdvancedBitrateComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultBitrate);
            MonoAdvancedSampleRateComboBox.ItemsSource = Settings.SampleRateOptions;
            MonoAdvancedSampleRateComboBox.SelectedItem = Settings.FormatSampleRate(Settings.DefaultSampleRate);
            MonoAdvancedThresholdComboBox.ItemsSource = Settings.BitrateOptions;
            MonoAdvancedThresholdComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultConversionThreshold);
            MonoAdvancedBitrateControlComboBox.ItemsSource = new[] { "ABR", "CBR" };
            MonoAdvancedBitrateControlComboBox.SelectedItem = Settings.DefaultBitrateControl;
            MonoAdvancedPassesComboBox.SelectedIndex = 0;

            // Stereo Mode Advanced Controls
            StereoAdvancedChannelsComboBox.ItemsSource = Settings.ChannelOptions;
            StereoAdvancedChannelsComboBox.SelectedItem = Settings.DefaultChannel;
            StereoAdvancedBitrateComboBox.ItemsSource = Settings.BitrateOptions;
            StereoAdvancedBitrateComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultBitrate);
            StereoAdvancedSampleRateComboBox.ItemsSource = Settings.SampleRateOptions;
            StereoAdvancedSampleRateComboBox.SelectedItem = Settings.FormatSampleRate(Settings.DefaultSampleRate);
            StereoAdvancedThresholdComboBox.ItemsSource = Settings.BitrateOptions;
            StereoAdvancedThresholdComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultConversionThreshold);
            StereoAdvancedBitrateControlComboBox.ItemsSource = new[] { "ABR", "CBR" };
            StereoAdvancedBitrateControlComboBox.SelectedItem = Settings.DefaultBitrateControl;
            StereoAdvancedPassesComboBox.SelectedIndex = 0;

            // Setup event handlers for main ComboBoxes
            ChannelsComboBox.SelectionChanged += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (_isUpdatingFromSettings) return;
                    
                    var selected = comboBox.SelectedItem?.ToString();
                    MonoModePanel.Visibility = selected == "Mono" ? Visibility.Visible : Visibility.Collapsed;
                    StereoModePanel.Visibility = selected == "Stereo" ? Visibility.Visible : Visibility.Collapsed;
                }
            };
            BitrateComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingFromSettings) return;
                
                if (s is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string bitrate)
                {
                    var normalized = NormalizeBitrateInput(bitrate);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        // CRITICAL FIX: ALWAYS update MAIN settings, regardless of IsAdvancedMode state
                        // The main ComboBox should NEVER update advanced settings
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        var oldValue = mainSettings.TargetBitrate;
                        
                        // REMOVE: Debugging breakpoint
                        // System.Diagnostics.Debugger.Break(); // REMOVED: Main bitrate update debugging
                        
                        mainSettings.TargetBitrate = Settings.FormatBitrate(bps);
                        comboBox.Text = Settings.FormatBitrate(bps);
                        
                        // Debug logging to verify correct behavior
                        System.Diagnostics.Debug.WriteLine($"MAIN BitrateComboBox: {oldValue} → {mainSettings.TargetBitrate} (Mode: {Settings.Current.CurrentMode}, Advanced: {Settings.Current.IsAdvancedMode})");
                        
                        UpdateSettingsSummary();
                        // Warn only if threshold is lower than target bitrate
                        if (Settings.TryParseBitrate(mainSettings.ConversionThreshold, out int thresholdBps) && thresholdBps < bps)
                        {
                            System.Windows.MessageBox.Show("Warning: Mono copy threshold is lower than target bitrate. Mono files with bitrate below the threshold will be copied instead of re-encoded.", "Bitrate/Threshold Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a bitrate between 32k and 192k.", "Invalid Bitrate", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatBitrate(Settings.DefaultBitrate);
                    }
                }
            };
            BitrateComboBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (_isUpdatingFromSettings) return;
                    
                    var normalized = NormalizeBitrateInput(comboBox.Text);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        // Update the MAIN settings for current mode (not GetActiveSettings)
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        mainSettings.TargetBitrate = Settings.FormatBitrate(bps);
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a bitrate between 32k and 192k.", "Invalid Bitrate", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatBitrate(Settings.DefaultBitrate);
                    }
                    e.Handled = true;
                }
            };
            BitrateComboBox.LostFocus += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (_isUpdatingFromSettings) return;
                    
                    var normalized = NormalizeBitrateInput(comboBox.Text);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        // Update the MAIN settings for current mode (not GetActiveSettings)
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        mainSettings.TargetBitrate = Settings.FormatBitrate(bps);
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a bitrate between 32k and 192k.", "Invalid Bitrate", MessageBoxButton.OK, MessageBoxImage.Warning);
                        // Use main settings for fallback too
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        comboBox.Text = mainSettings?.TargetBitrate ?? Settings.FormatBitrate(Settings.DefaultBitrate);
                    }
                }
            };
            SampleRateComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingFromSettings) return;
                
                if (s is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string rate)
                {
                    if (Settings.TryParseSampleRate(rate, out int sampleRate))
                    {
                        // Update MAIN settings for current mode (not GetActiveSettings which could be advanced)
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        mainSettings.SampleRate = Settings.FormatSampleRate(sampleRate);
                        UpdateSettingsSummary();
                    }
                }
            };
            SampleRateComboBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (_isUpdatingFromSettings) return;
                    
                    if (Settings.TryParseSampleRate(comboBox.Text, out int sr) && (sr == 22050 || sr == 44100 || sr == 48000))
                    {
                        // Update MAIN settings for current mode (not GetActiveSettings)
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        mainSettings.SampleRate = Settings.FormatSampleRate(sr);
                        comboBox.Text = Settings.FormatSampleRate(sr);
                        UpdateSettingsSummary();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a valid sample rate: 22050, 44100, or 48000 Hz.", "Invalid Sample Rate", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatSampleRate(Settings.DefaultSampleRate);
                    }
                    e.Handled = true;
                }
            };
            SampleRateComboBox.LostFocus += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (_isUpdatingFromSettings) return;
                    
                    if (Settings.TryParseSampleRate(comboBox.Text, out int sr) && (sr == 22050 || sr == 44100 || sr == 48000))
                    {
                        // Update MAIN settings for current mode (not GetActiveSettings)
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        mainSettings.SampleRate = Settings.FormatSampleRate(sr);
                        comboBox.Text = Settings.FormatSampleRate(sr);
                        UpdateSettingsSummary();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a valid sample rate: 22050, 44100, or 48000 Hz.", "Invalid Sample Rate", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatSampleRate(Settings.DefaultSampleRate);
                    }
                }
            };
            ThresholdComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingFromSettings) return;
                
                if (s is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string threshold)
                {
                    var normalized = NormalizeBitrateInput(threshold);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        mainSettings.ConversionThreshold = Settings.FormatBitrate(bps);
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                        
                        if (Settings.TryParseBitrate(mainSettings.TargetBitrate, out int targetBps) && bps < targetBps)
                        {
                            System.Windows.MessageBox.Show("Warning: Conversion threshold is lower than target bitrate. Files below the threshold will be copied instead of re-encoded.", "Bitrate/Threshold Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a threshold between 32k and 192k.", "Invalid Threshold", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatBitrate(Settings.DefaultConversionThreshold);
                    }
                }
            };
            ThresholdComboBox.KeyDown += (s, e) => {
                if (e.Key == Key.Enter) { }
            };
            ThresholdComboBox.LostFocus += (s, e) => { };
            BitrateControlComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingFromSettings) return;
                
                if (s is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string mode)
                {
                    // Update MAIN settings for current mode (not GetActiveSettings which could be advanced)
                    var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                        Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                    mainSettings.EncodingType = mode;

                    if (mode == "CBR")
                    {
                        PassesComboBox.SelectedIndex = 0;
                        PassesComboBox.IsEnabled = false;
                        mainSettings.PassMode = "1-Pass";
                    }
                    else
                    {
                        PassesComboBox.IsEnabled = true;
                    }
                    UpdateSettingsSummary();
                }
            };
            PassesComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingFromSettings) return;
                
                if (s is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
                {
                    // Update MAIN settings for current mode (not GetActiveSettings)
                    var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                        Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                    var passMode = item.Content?.ToString() ?? "1-Pass";
                    mainSettings.PassMode = passMode;
                    UpdateSettingsSummary();
                }
            };
            
            // Setup advanced panel ComboBox event handlers
            // Mono Mode Advanced ComboBox handlers
            MonoAdvancedChannelsComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.MonoMode.AdvancedOverride.ChannelMode = value;
            };
            
            MonoAdvancedBitrateComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.TargetBitrate = cb.Text;
            };
            MonoAdvancedBitrateComboBox.LostFocus += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.TargetBitrate = cb.Text;
            };
            
            MonoAdvancedSampleRateComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.MonoMode.AdvancedOverride.SampleRate = value;
            };
            
            MonoAdvancedThresholdComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.ConversionThreshold = cb.Text;
            };
            MonoAdvancedThresholdComboBox.LostFocus += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.ConversionThreshold = cb.Text;
            };
            
            MonoAdvancedBitrateControlComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.MonoMode.AdvancedOverride.EncodingType = value;
            };
            
            MonoAdvancedPassesComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.PassMode = cb.SelectedIndex == 1 ? "2-Pass" : "1-Pass";
            };
            
            // Stereo Mode Advanced ComboBox handlers
            StereoAdvancedChannelsComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.StereoMode.AdvancedOverride.ChannelMode = value;
            };
            
            StereoAdvancedBitrateComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.TargetBitrate = cb.Text;
            };
            StereoAdvancedBitrateComboBox.LostFocus += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.TargetBitrate = cb.Text;
            };
            
            StereoAdvancedSampleRateComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.StereoMode.AdvancedOverride.SampleRate = value;
            };
            
            StereoAdvancedThresholdComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.ConversionThreshold = cb.Text;
            };
            StereoAdvancedThresholdComboBox.LostFocus += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.ConversionThreshold = cb.Text;
            };
            
            StereoAdvancedBitrateControlComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.StereoMode.AdvancedOverride.EncodingType = value;
            };
            
            StereoAdvancedPassesComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.PassMode = cb.SelectedIndex == 1 ? "2-Pass" : "1-Pass";
            };
        }

        private void Channels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string channel)
            {
                // Update the global mode in Settings.Current
                Settings.Current.CurrentMode = channel;
                
                // Update contextual panel visibility with updated panel names
                MonoModePanel.Visibility = channel == "Mono" ? Visibility.Visible : Visibility.Collapsed;
                StereoModePanel.Visibility = channel == "Stereo" ? Visibility.Visible : Visibility.Collapsed;
                
                // Rebind UI controls to appropriate settings context
                RebindMainSettings();
                
                // Restore radio button states for the new mode
                if (!_isUpdatingFromSettings)
                {
                    RestoreRadioButtonStates();
                }
                
                UpdateSettingsSummary();
            }
        }

        private void RebindMainSettings()
        {
            // Get MAIN settings for current mode (not GetActiveSettings which could be advanced)
            var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
            
            // Bind main UI controls to MAIN settings without triggering events
            var wasUpdating = _isUpdatingFromSettings;
            _isUpdatingFromSettings = true;
            
            try
            {
                // Update UI controls to reflect the current MAIN settings
                BitrateComboBox.Text = mainSettings.TargetBitrate;
                SampleRateComboBox.SelectedItem = mainSettings.SampleRate;
                ThresholdComboBox.Text = mainSettings.ConversionThreshold;
                BitrateControlComboBox.SelectedItem = mainSettings.EncodingType;
                
                // Update passes ComboBox
                var passModeItem = mainSettings.PassMode == "2-Pass" ? 
                    PassesComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content?.ToString() == "2-Pass") :
                    PassesComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content?.ToString() == "1-Pass");
                if (passModeItem != null)
                    PassesComboBox.SelectedItem = passModeItem;
            }
            finally
            {
                _isUpdatingFromSettings = wasUpdating;
            }
        }

        private void SampleRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingFromSettings) return;
            
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string rate)
            {
                if (Settings.TryParseSampleRate(rate, out int sampleRate))
                {
                    // Update MAIN settings for current mode (not GetActiveSettings which could be advanced)
                    var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                        Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                    mainSettings.SampleRate = Settings.FormatSampleRate(sampleRate);
                    UpdateSettingsSummary();
                }
            }
        }

        private void BitrateControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingFromSettings) return;
            
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string mode)
            {
                // Update MAIN settings for current mode (not GetActiveSettings which could be advanced)
                var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                    Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                mainSettings.EncodingType = mode;

                if (mode == "CBR")
                {
                    PassesComboBox.SelectedIndex = 0;
                    PassesComboBox.IsEnabled = false;
                    mainSettings.PassMode = "1-Pass";
                }
                else
                {
                    PassesComboBox.IsEnabled = true;
                }
                UpdateSettingsSummary();
            }
        }

        private void UpdateSettingsSummary()
        {
            var channels = ChannelsComboBox.SelectedItem?.ToString() ?? Settings.DefaultChannel;
            var bitrate = BitrateComboBox.Text;
            var sampleRate = SampleRateComboBox.SelectedItem?.ToString() ?? $"{Settings.DefaultSampleRate} Hz";
            var threshold = ThresholdComboBox.Text;
            var mode = BitrateControlComboBox.SelectedItem?.ToString() ?? Settings.DefaultBitrateControl;
            var passes = (PassesComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "1-Pass";

            // Ensure consistent formatting
            if (!bitrate.EndsWith("k", StringComparison.OrdinalIgnoreCase))
                bitrate += "k";
            if (!threshold.EndsWith("k", StringComparison.OrdinalIgnoreCase))
                threshold += "k";
            if (!sampleRate.EndsWith("Hz", StringComparison.OrdinalIgnoreCase))
                sampleRate += " Hz";

            SettingsSummaryText.Text = $"{channels} | {bitrate} | {sampleRate} | {threshold} Threshold | {mode} | {passes}";
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourcePathTextBox.Text) || string.IsNullOrWhiteSpace(OutputPathTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please select both source and output folders.", "Missing Paths", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(SourcePathTextBox.Text))
            {
                System.Windows.MessageBox.Show("Source folder does not exist.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Disable UI controls
                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                SourceBrowseButton.IsEnabled = false;
                OutputBrowseButton.IsEnabled = false;

                // Clear previous state
                _pendingFiles.Clear();
                LogTextBox.Clear();
                UpdateStatus("Scanning files...", 0);

                // Setup cancellation
                _cancellationSource = new CancellationTokenSource();
                _audioProcessor = new AudioProcessor(_cancellationSource.Token);
                _audioProcessor.ProgressChanged += AudioProcessor_ProgressChanged;
                _audioProcessor.FileProcessed += AudioProcessor_FileProcessed;

                // Scan for files
                var files = new List<AudioFileInfo>();
                await foreach (var file in _audioProcessor.ScanDirectoryAsync(SourcePathTextBox.Text))
                {
                    files.Add(file);
                }
                
                if (files.Count == 0)
                {
                    LogMessage("No supported audio files found.");
                    UpdateStatus("Ready", null);
                    return;
                }

                _pendingFiles.AddRange(files);
                LogMessage($"Found {files.Count} files to process.");

                // Process each file
                var totalFiles = files.Count;
                var processed = 0;

                foreach (var file in files)
                {
                    if (_cancellationSource?.Token.IsCancellationRequested == true)
                        break;

                    UpdateStatus($"Processing {Path.GetFileName(file.SourcePath)}...", processed / (double)totalFiles);

                    await _audioProcessor.ProcessAudioFileAsync(file, OutputPathTextBox.Text);
                    processed++;
                }

                if (_cancellationSource?.Token.IsCancellationRequested == true)
                {
                    LogMessage("Operation cancelled by user.");
                    UpdateStatus("Cancelled", null);
                }
                else
                {
                    LogMessage("All files processed successfully.");
                    UpdateStatus("Completed", 1.0);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
                UpdateStatus("Error occurred", null);
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable UI controls
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
                SourceBrowseButton.IsEnabled = true;
                OutputBrowseButton.IsEnabled = true;

                // Cleanup
                _cancellationSource?.Dispose();
                _cancellationSource = null;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationSource?.Cancel();
            CancelButton.IsEnabled = false;
            UpdateStatus("Cancelling...", null);
        }

        private void AudioProcessor_ProgressChanged(object? sender, AudioProcessingProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var fileIndex = _pendingFiles.IndexOf(e.File);
                var overallProgress = (fileIndex + e.Progress) / _pendingFiles.Count;
                UpdateStatus($"Processing {Path.GetFileName(e.File.SourcePath)}...", overallProgress);
            });
        }

        private void AudioProcessor_FileProcessed(object? sender, AudioFileProcessedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var status = e.Success ? "Success" : "Failed";
                var bitrateInfo = e.File.Bitrate.HasValue ? $" ({Settings.FormatBitrate(e.File.Bitrate.Value)})" : "";
                LogMessage($"Processed {Path.GetFileName(e.File.SourcePath)}: {status}{bitrateInfo}");
            });
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"{message}\n");
                LogTextBox.ScrollToEnd();
                MessageBarText.Text = message;
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
        }

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

        private void Expander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            // Delay the resize slightly to allow animation to complete
            Dispatcher.BeginInvoke(() =>
            {
                InvalidateVisual();
                UpdateLayout();
                
                if (WindowState == WindowState.Normal)
                {
                    // Force height recalculation
                    Height = ActualHeight;
                    SizeToContent = SizeToContent.Height;
                }
            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        private void LoadUserSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var doc = XDocument.Load(SettingsFile);
                    var root = doc.Element("UserSettings");
                    if (root != null)
                    {
                        // Load path settings
                        var src = root.Element("SourcePath")?.Value;
                        var outp = root.Element("OutputPath")?.Value;
                        var defOutp = root.Element("DefaultOutputPath")?.Value;
                        if (!string.IsNullOrWhiteSpace(src))
                        {
                            SourcePathTextBox.Text = src;
                            Settings.Current.SourcePath = src;
                        }
                        if (!string.IsNullOrWhiteSpace(outp))
                        {
                            OutputPathTextBox.Text = outp;
                            Settings.Current.OutputPath = outp;
                        }
                        if (!string.IsNullOrWhiteSpace(defOutp))
                        {
                            _defaultOutputPath = defOutp;
                            Settings.Current.DefaultOutputPath = defOutp;
                        }

                        // Load current mode and advanced mode state
                        var currentMode = root.Element("CurrentMode")?.Value;
                        if (!string.IsNullOrWhiteSpace(currentMode))
                            Settings.Current.CurrentMode = currentMode;

                        var isAdvanced = root.Element("IsAdvancedMode")?.Value;
                        if (bool.TryParse(isAdvanced, out bool advancedMode))
                            Settings.Current.IsAdvancedMode = advancedMode;

                        // Load Mono Mode settings including SelectedAction with migration support
                        var monoMode = root.Element("MonoMode");
                        if (monoMode != null)
                        {
                            var selectedAction = monoMode.Element("SelectedAction")?.Value;
                            if (!string.IsNullOrWhiteSpace(selectedAction))
                                Settings.Current.MonoMode.SelectedAction = selectedAction;
                                
                            LoadCompressionSettings(monoMode.Element("Main"), Settings.Current.MonoMode.Main);
                            // Migration support: try new element name first, fallback to legacy name
                            var advancedElement = monoMode.Element("Advanced") ?? monoMode.Element("AdvancedOverride");
                            LoadCompressionSettings(advancedElement, Settings.Current.MonoMode.AdvancedOverride);
                        }

                        // Load Stereo Mode settings including SelectedAction with migration support
                        var stereoMode = root.Element("StereoMode");
                        if (stereoMode != null)
                        {
                            var selectedAction = stereoMode.Element("SelectedAction")?.Value;
                            if (!string.IsNullOrWhiteSpace(selectedAction))
                                Settings.Current.StereoMode.SelectedAction = selectedAction;
                                
                            LoadCompressionSettings(stereoMode.Element("Main"), Settings.Current.StereoMode.Main);
                            // Migration support: try new element name first, fallback to legacy name
                            var advancedElement = stereoMode.Element("Advanced") ?? stereoMode.Element("AdvancedOverride");
                            LoadCompressionSettings(advancedElement, Settings.Current.StereoMode.AdvancedOverride);
                        }
                    }
                }
            }
            catch
            {
                // Notify user if settings file is missing or corrupted and defaults are used
                System.Windows.MessageBox.Show(
                    "Settings file is missing or corrupted. Default settings will be used.",
                    "Settings Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void SaveUserSettings()
        {
            try
            {
                // Update Settings.Current with current UI values
                Settings.Current.SourcePath = SourcePathTextBox.Text;
                Settings.Current.OutputPath = OutputPathTextBox.Text;
                Settings.Current.DefaultOutputPath = _defaultOutputPath;

                var doc = new XDocument(
                    new XElement("UserSettings",
                        new XElement("SourcePath", Settings.Current.SourcePath),
                        new XElement("OutputPath", Settings.Current.OutputPath),
                        new XElement("DefaultOutputPath", Settings.Current.DefaultOutputPath),
                        new XElement("CurrentMode", Settings.Current.CurrentMode),
                        new XElement("IsAdvancedMode", Settings.Current.IsAdvancedMode),
                        new XElement("MonoMode",
                            new XElement("SelectedAction", Settings.Current.MonoMode.SelectedAction),
                            new XElement("Main", CreateCompressionSettingsXml(Settings.Current.MonoMode.Main)),
                            new XElement("Advanced", CreateCompressionSettingsXml(Settings.Current.MonoMode.AdvancedOverride))
                        ),
                        new XElement("StereoMode",
                            new XElement("SelectedAction", Settings.Current.StereoMode.SelectedAction),
                            new XElement("Main", CreateCompressionSettingsXml(Settings.Current.StereoMode.Main)),
                            new XElement("Advanced", CreateCompressionSettingsXml(Settings.Current.StereoMode.AdvancedOverride))
                        )
                    )
                );

                doc.Save(SettingsFile);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private XElement CreateCompressionSettingsXml(CompressionSettings settings)
        {
            // FIXED: Remove the "Settings" wrapper that was causing XML structure mismatch
            // The wrapper was preventing LoadCompressionSettings from finding the elements
            return new XElement("CompressionSettings",
                new XElement("ChannelMode", settings.ChannelMode),
                new XElement("TargetBitrate", settings.TargetBitrate),
                new XElement("SampleRate", settings.SampleRate),
                new XElement("ConversionThreshold", settings.ConversionThreshold),
                new XElement("EncodingType", settings.EncodingType),
                new XElement("PassMode", settings.PassMode)
            );
        }

        private void RestoreUIFromSettings()
        {
            var wasUpdating = _isUpdatingFromSettings;
            _isUpdatingFromSettings = true;
            
            try
            {
                // Restore the main mode selection first
                ChannelsComboBox.SelectedItem = Settings.Current.CurrentMode;
                
                // Get MAIN settings for the current mode (never use GetActiveSettings for main UI)
                var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                    Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                
                // Restore main UI controls to show MAIN settings only
                BitrateComboBox.Text = mainSettings.TargetBitrate;
                SampleRateComboBox.SelectedItem = mainSettings.SampleRate;
                ThresholdComboBox.Text = mainSettings.ConversionThreshold;
                BitrateControlComboBox.SelectedItem = mainSettings.EncodingType;
                
                // Update passes ComboBox
                var passModeItem = mainSettings.PassMode == "2-Pass" ? 
                    PassesComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content?.ToString() == "2-Pass") :
                    PassesComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content?.ToString() == "1-Pass");
                if (passModeItem != null)
                    PassesComboBox.SelectedItem = passModeItem;
                
                // Restore contextual panel visibility based on current mode
                MonoModePanel.Visibility = Settings.Current.CurrentMode == "Mono" ? Visibility.Visible : Visibility.Collapsed;
                StereoModePanel.Visibility = Settings.Current.CurrentMode == "Stereo" ? Visibility.Visible : Visibility.Collapsed;
                
                // Restore radio button states based on SelectedAction property
                RestoreRadioButtonStates();
                
                // Restore advanced panel settings for both modes
                RestoreAdvancedPanelSettings();
            }
            finally
            {
                _isUpdatingFromSettings = wasUpdating;
            }
        }

        private void RestoreRadioButtonStates()
        {
            // Restore Mono mode radio button state
            var monoAction = Settings.Current.MonoMode.SelectedAction;
            MonoCopyStereoRadio.IsChecked = monoAction == "Copy";
            MonoConvertStereoRadio.IsChecked = monoAction == "Convert";
            MonoAdvancedRadio.IsChecked = monoAction == "Advanced";
            
            // Restore Stereo mode radio button state
            var stereoAction = Settings.Current.StereoMode.SelectedAction;
            StereoCopyMonoRadio.IsChecked = stereoAction == "Copy";
            StereoConvertMonoRadio.IsChecked = stereoAction == "Convert";
            StereoAdvancedRadio.IsChecked = stereoAction == "Advanced";
            
            // Set advanced panel visibility based on current mode and its selected action with updated panel names
            if (Settings.Current.CurrentMode == "Mono")
            {
                MonoModeAdvancedPanel.Visibility = monoAction == "Advanced" ? Visibility.Visible : Visibility.Collapsed;
                Settings.Current.IsAdvancedMode = monoAction == "Advanced";
            }
            else
            {
                StereoModeAdvancedPanel.Visibility = stereoAction == "Advanced" ? Visibility.Visible : Visibility.Collapsed;
                Settings.Current.IsAdvancedMode = stereoAction == "Advanced";
            }
        }

        private void RestoreAdvancedPanelSettings()
        {
            // Restore Mono mode advanced override settings with updated control names
            var monoAdvanced = Settings.Current.MonoMode.AdvancedOverride;
            MonoAdvancedChannelsComboBox.SelectedItem = monoAdvanced.ChannelMode;
            MonoAdvancedBitrateComboBox.Text = monoAdvanced.TargetBitrate;
            MonoAdvancedSampleRateComboBox.SelectedItem = monoAdvanced.SampleRate;
            MonoAdvancedThresholdComboBox.Text = monoAdvanced.ConversionThreshold;
            MonoAdvancedBitrateControlComboBox.SelectedItem = monoAdvanced.EncodingType;
            MonoAdvancedPassesComboBox.SelectedIndex = monoAdvanced.PassMode == "2-Pass" ? 1 : 0;
            
            // Restore Stereo mode advanced override settings with updated control names
            var stereoAdvanced = Settings.Current.StereoMode.AdvancedOverride;
            StereoAdvancedChannelsComboBox.SelectedItem = stereoAdvanced.ChannelMode;
            StereoAdvancedBitrateComboBox.Text = stereoAdvanced.TargetBitrate;
            StereoAdvancedSampleRateComboBox.SelectedItem = stereoAdvanced.SampleRate;
            StereoAdvancedThresholdComboBox.Text = stereoAdvanced.ConversionThreshold;
            StereoAdvancedBitrateControlComboBox.SelectedItem = stereoAdvanced.EncodingType;
            StereoAdvancedPassesComboBox.SelectedIndex = stereoAdvanced.PassMode == "2-Pass" ? 1 : 0;
        }

        private void LoadDefaultOutputPath()
        {
            if (!string.IsNullOrWhiteSpace(_defaultOutputPath))
            {
                OutputPathTextBox.Text = _defaultOutputPath;
                Settings.Current.OutputPath = _defaultOutputPath;
            }
        }

        private bool IsSourceOutputCollision()
        {
            return string.Equals(SourcePathTextBox.Text, OutputPathTextBox.Text, StringComparison.OrdinalIgnoreCase);
        }

        private bool ShowCollisionDialog(string source, string output, string context)
        {
            if (string.Equals(source, output, StringComparison.OrdinalIgnoreCase))
            {
                var result = System.Windows.MessageBox.Show(
                    "Source and Output folders are the same. This may overwrite your source files.\n\nDo you want to continue?",
                    "Folder Collision Detected",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);
                return result == MessageBoxResult.OK;
            }
            return true;
        }

        private void LoadCompressionSettings(XElement? element, CompressionSettings settings)
        {
            if (element == null) return;

            // Handle multiple XML structures for maximum compatibility: 
            // - Old format: <Settings> wrapper
            // - Current format: <CompressionSettings> wrapper  
            // - Future format: no wrapper (direct access)
            var settingsElement = element.Element("Settings") ?? element.Element("CompressionSettings");
            var targetElement = settingsElement ?? element;

            var channelMode = targetElement.Element("ChannelMode")?.Value;
            if (!string.IsNullOrWhiteSpace(channelMode))
                settings.ChannelMode = channelMode;

            var targetBitrate = targetElement.Element("TargetBitrate")?.Value;
            if (!string.IsNullOrWhiteSpace(targetBitrate))
                settings.TargetBitrate = targetBitrate;

            var sampleRate = targetElement.Element("SampleRate")?.Value;
            if (!string.IsNullOrWhiteSpace(sampleRate))
                settings.SampleRate = sampleRate;

            var threshold = targetElement.Element("ConversionThreshold")?.Value;
            if (!string.IsNullOrWhiteSpace(threshold))
                settings.ConversionThreshold = threshold;

            var encodingType = targetElement.Element("EncodingType")?.Value;
            if (!string.IsNullOrWhiteSpace(encodingType))
                settings.EncodingType = encodingType;

            var passMode = targetElement.Element("PassMode")?.Value;
            if (!string.IsNullOrWhiteSpace(passMode))
                settings.PassMode = passMode;
        }

        private void SaveDefaultOutputPath()
        {
            _defaultOutputPath = OutputPathTextBox.Text;
            Settings.Current.DefaultOutputPath = _defaultOutputPath;
            SaveUserSettings();
        }
    }
}
