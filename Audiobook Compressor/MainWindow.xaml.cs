/*
    Filename: MainWindow.xaml.cs
    Last Updated: 2025-08-02 07:01
    Version: 1.2.A
    State: Experimental
    Signed: Claude

    Synopsis:
    Complete hierarchical settings system implementation with dynamic context switching, independent persistence for all four settings contexts, and advanced override panel binding per Focus 6.2.0.md directive.
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

            // Advanced override panel visibility logic with settings binding
            MonoAdvancedRadio.Checked += (s, e) => {
                Settings.Current.IsAdvancedMode = true;
                AdvancedStereoOverridePanel.Visibility = Visibility.Visible;
                // Only rebind on first open after load - remove automatic rebinding
            };
            MonoAdvancedRadio.Unchecked += (s, e) => {
                Settings.Current.IsAdvancedMode = false;
                AdvancedStereoOverridePanel.Visibility = Visibility.Collapsed;
            };
            MonoCopyStereoRadio.Checked += (s, e) => {
                Settings.Current.IsAdvancedMode = false;
                AdvancedStereoOverridePanel.Visibility = Visibility.Collapsed;
            };
            MonoConvertStereoRadio.Checked += (s, e) => {
                Settings.Current.IsAdvancedMode = false;
                AdvancedStereoOverridePanel.Visibility = Visibility.Collapsed;
            };

            StereoAdvancedRadio.Checked += (s, e) => {
                Settings.Current.IsAdvancedMode = true;
                AdvancedMonoOverridePanel.Visibility = Visibility.Visible;
                // Only rebind on first open after load - remove automatic rebinding
            };
            StereoAdvancedRadio.Unchecked += (s, e) => {
                Settings.Current.IsAdvancedMode = false;
                AdvancedMonoOverridePanel.Visibility = Visibility.Collapsed;
            };
            StereoCopyMonoRadio.Checked += (s, e) => {
                Settings.Current.IsAdvancedMode = false;
                AdvancedMonoOverridePanel.Visibility = Visibility.Collapsed;
            };
            StereoConvertMonoRadio.Checked += (s, e) => {
                Settings.Current.IsAdvancedMode = false;
                AdvancedMonoOverridePanel.Visibility = Visibility.Collapsed;
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

            // Setup sample rate options - FIX: Missing ItemsSource
            SampleRateComboBox.ItemsSource = Settings.SampleRateOptions;
            SampleRateComboBox.SelectedItem = $"{Settings.DefaultSampleRate} Hz";
            SampleRateComboBox.SelectionChanged += SampleRate_SelectionChanged;

            // Setup threshold options
            ThresholdComboBox.ItemsSource = Settings.BitrateOptions;
            ThresholdComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);

            // Setup bitrate control options
            BitrateControlComboBox.ItemsSource = new[] { "ABR", "CBR" };
            BitrateControlComboBox.SelectedItem = Settings.DefaultBitrateControl;
            BitrateControlComboBox.SelectionChanged += BitrateControl_SelectionChanged;

            // Setup passes options (already defined in XAML)
            // PassesComboBox items are defined in XAML

            // FIX: Initialize advanced panel ComboBoxes ItemsSource
            // Advanced Stereo Override ComboBoxes (for Mono mode advanced settings)
            AdvancedStereoChannelsComboBox.ItemsSource = Settings.ChannelOptions;
            AdvancedStereoChannelsComboBox.SelectedItem = Settings.DefaultChannel;
            AdvancedStereoBitrateComboBox.ItemsSource = Settings.BitrateOptions;
            AdvancedStereoBitrateComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultBitrate);
            AdvancedStereoSampleRateComboBox.ItemsSource = Settings.SampleRateOptions;
            AdvancedStereoSampleRateComboBox.SelectedItem = Settings.FormatSampleRate(Settings.DefaultSampleRate);
            AdvancedStereoThresholdComboBox.ItemsSource = Settings.BitrateOptions;
            AdvancedStereoThresholdComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
            AdvancedStereoBitrateControlComboBox.ItemsSource = new[] { "ABR", "CBR" };
            AdvancedStereoBitrateControlComboBox.SelectedItem = Settings.DefaultBitrateControl;
            AdvancedStereoPassesComboBox.SelectedIndex = 0;

            // Advanced Mono Override ComboBoxes (for Stereo mode advanced settings)
            AdvancedMonoChannelsComboBox.ItemsSource = Settings.ChannelOptions;
            AdvancedMonoChannelsComboBox.SelectedItem = Settings.DefaultChannel;
            AdvancedMonoBitrateComboBox.ItemsSource = Settings.BitrateOptions;
            AdvancedMonoBitrateComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultBitrate);
            AdvancedMonoSampleRateComboBox.ItemsSource = Settings.SampleRateOptions;
            AdvancedMonoSampleRateComboBox.SelectedItem = Settings.FormatSampleRate(Settings.DefaultSampleRate);
            AdvancedMonoThresholdComboBox.ItemsSource = Settings.BitrateOptions;
            AdvancedMonoThresholdComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
            AdvancedMonoBitrateControlComboBox.ItemsSource = new[] { "ABR", "CBR" };
            AdvancedMonoBitrateControlComboBox.SelectedItem = Settings.DefaultBitrateControl;
            AdvancedMonoPassesComboBox.SelectedIndex = 0;

            // Setup event handlers for main ComboBoxes
            ChannelsComboBox.SelectionChanged += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (_isUpdatingFromSettings) return;
                    
                    var selected = comboBox.SelectedItem?.ToString();
                    MonoModeOptionsPanel.Visibility = selected == "Mono" ? Visibility.Visible : Visibility.Collapsed;
                    StereoModeOptionsPanel.Visibility = selected == "Stereo" ? Visibility.Visible : Visibility.Collapsed;
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
                        // Update MAIN settings for current mode (not GetActiveSettings)
                        var mainSettings = Settings.Current.CurrentMode == "Mono" ? 
                            Settings.Current.MonoMode.Main : Settings.Current.StereoMode.Main;
                        mainSettings.ConversionThreshold = Settings.FormatBitrate(bps);
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                        
                        // Warn if threshold is lower than target bitrate
                        if (Settings.TryParseBitrate(mainSettings.TargetBitrate, out int targetBps) && bps < targetBps)
                        {
                            System.Windows.MessageBox.Show("Warning: Conversion threshold is lower than target bitrate. Files below the threshold will be copied instead of re-encoded.", "Bitrate/Threshold Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a threshold between 32k and 192k.", "Invalid Threshold", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
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
            
            // Advanced Stereo Override ComboBox handlers (for Mono mode advanced settings)
            AdvancedStereoChannelsComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.MonoMode.AdvancedOverride.ChannelMode = value;
            };
            
            AdvancedStereoBitrateComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                {
                    // BREAKPOINT 2: Set breakpoint here to trace advanced bitrate changes
                    // System.Diagnostics.Debugger.Break(); // REMOVED: Advanced bitrate update debugging
                    Settings.Current.MonoMode.AdvancedOverride.TargetBitrate = cb.Text;
                }
            };
            AdvancedStereoBitrateComboBox.LostFocus += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.TargetBitrate = cb.Text;
            };
            
            AdvancedStereoSampleRateComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.MonoMode.AdvancedOverride.SampleRate = value;
            };
            
            AdvancedStereoThresholdComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.ConversionThreshold = cb.Text;
            };
            AdvancedStereoThresholdComboBox.LostFocus += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.ConversionThreshold = cb.Text;
            };
            
            AdvancedStereoBitrateControlComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.MonoMode.AdvancedOverride.EncodingType = value;
            };
            
            AdvancedStereoPassesComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.MonoMode.AdvancedOverride.PassMode = cb.SelectedIndex == 1 ? "2-Pass" : "1-Pass";
            };
            
            // Advanced Mono Override ComboBox handlers (for Stereo mode advanced settings)
            AdvancedMonoChannelsComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.StereoMode.AdvancedOverride.ChannelMode = value;
            };
            
            AdvancedMonoBitrateComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.TargetBitrate = cb.Text;
            };
            AdvancedMonoBitrateComboBox.LostFocus += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.TargetBitrate = cb.Text;
            };
            
            AdvancedMonoSampleRateComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.StereoMode.AdvancedOverride.SampleRate = value;
            };
            
            AdvancedMonoThresholdComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.ConversionThreshold = cb.Text;
            };
            AdvancedMonoThresholdComboBox.LostFocus += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb)
                    Settings.Current.StereoMode.AdvancedOverride.ConversionThreshold = cb.Text;
            };
            
            AdvancedMonoBitrateControlComboBox.SelectionChanged += (s, e) => {
                if (!_isUpdatingFromSettings && s is System.Windows.Controls.ComboBox cb && cb.SelectedItem is string value)
                    Settings.Current.StereoMode.AdvancedOverride.EncodingType = value;
            };
            
            AdvancedMonoPassesComboBox.SelectionChanged += (s, e) => {
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
                
                // Update contextual panel visibility
                MonoModeOptionsPanel.Visibility = channel == "Mono" ? Visibility.Visible : Visibility.Collapsed;
                StereoModeOptionsPanel.Visibility = channel == "Stereo" ? Visibility.Visible : Visibility.Collapsed;
                
                // Rebind UI controls to appropriate settings context
                RebindMainSettings();
                
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

                        // Load Mono Mode settings
                        var monoMode = root.Element("MonoMode");
                        if (monoMode != null)
                        {
                            LoadCompressionSettings(monoMode.Element("Main"), Settings.Current.MonoMode.Main);
                            LoadCompressionSettings(monoMode.Element("AdvancedOverride"), Settings.Current.MonoMode.AdvancedOverride);
                        }

                        // Load Stereo Mode settings
                        var stereoMode = root.Element("StereoMode");
                        if (stereoMode != null)
                        {
                            LoadCompressionSettings(stereoMode.Element("Main"), Settings.Current.StereoMode.Main);
                            LoadCompressionSettings(stereoMode.Element("AdvancedOverride"), Settings.Current.StereoMode.AdvancedOverride);
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

        private void SaveUserSettings()
        {
            // BREAKPOINT 3: Set breakpoint here to trace settings save process
            // System.Diagnostics.Debugger.Break(); // REMOVED: Settings save entry point debugging
            
            try
            {
                // Update Settings.Current with current UI values
                Settings.Current.SourcePath = SourcePathTextBox.Text;
                Settings.Current.OutputPath = OutputPathTextBox.Text;
                Settings.Current.DefaultOutputPath = _defaultOutputPath;

                // BREAKPOINT 4: Check settings values before saving
                // var monoMainBitrate = Settings.Current.MonoMode.Main.TargetBitrate;
                // var monoAdvancedBitrate = Settings.Current.MonoMode.AdvancedOverride.TargetBitrate;
                // System.Diagnostics.Debug.WriteLine($"SAVE DEBUG: Mono Main={monoMainBitrate}, Advanced={monoAdvancedBitrate}");
                // System.Diagnostics.Debugger.Break(); // REMOVED: Settings values check debugging

                var doc = new XDocument(
                    new XElement("UserSettings",
                        new XElement("SourcePath", Settings.Current.SourcePath),
                        new XElement("OutputPath", Settings.Current.OutputPath),
                        new XElement("DefaultOutputPath", Settings.Current.DefaultOutputPath),
                        new XElement("CurrentMode", Settings.Current.CurrentMode),
                        new XElement("IsAdvancedMode", Settings.Current.IsAdvancedMode),
                        new XElement("MonoMode",
                            new XElement("Main", CreateCompressionSettingsXml(Settings.Current.MonoMode.Main)),
                            new XElement("AdvancedOverride", CreateCompressionSettingsXml(Settings.Current.MonoMode.AdvancedOverride))
                        ),
                        new XElement("StereoMode",
                            new XElement("Main", CreateCompressionSettingsXml(Settings.Current.StereoMode.Main)),
                            new XElement("AdvancedOverride", CreateCompressionSettingsXml(Settings.Current.StereoMode.AdvancedOverride))
                        )
                    )
                );

                doc.Save(SettingsFile);
                
                // BREAKPOINT 5: Confirm save completed
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
                MonoModeOptionsPanel.Visibility = Settings.Current.CurrentMode == "Mono" ? Visibility.Visible : Visibility.Collapsed;
                StereoModeOptionsPanel.Visibility = Settings.Current.CurrentMode == "Stereo" ? Visibility.Visible : Visibility.Collapsed;
                
                // CRITICAL FIX: Restore BOTH mode's radio button states and advanced settings
                // This ensures that when user switches modes, the inactive mode state is preserved
                RestoreAllModeStates();
            }
            finally
            {
                _isUpdatingFromSettings = wasUpdating;
            }
        }

        private void RestoreAllModeStates()
        {
            // Restore Mono mode radio button states and advanced settings
            RestoreModeState("Mono", Settings.Current.MonoMode);
            
            // Restore Stereo mode radio button states and advanced settings  
            RestoreModeState("Stereo", Settings.Current.StereoMode);
        }

        private void RestoreModeState(string mode, ModeSettings modeSettings)
        {
            if (mode == "Mono")
            {
                // Check if Mono mode was in advanced state when saved
                var isMonoAdvanced = Settings.Current.CurrentMode == "Mono" && Settings.Current.IsAdvancedMode;
                
                if (isMonoAdvanced)
                {
                    MonoAdvancedRadio.IsChecked = true;
                    AdvancedStereoOverridePanel.Visibility = Visibility.Visible;
                    
                    // Restore MonoMode.AdvancedOverride settings to AdvancedStereo panel
                    var advancedSettings = modeSettings.AdvancedOverride;
                    AdvancedStereoChannelsComboBox.SelectedItem = advancedSettings.ChannelMode;
                    AdvancedStereoBitrateComboBox.Text = advancedSettings.TargetBitrate;
                    AdvancedStereoSampleRateComboBox.SelectedItem = advancedSettings.SampleRate;
                    AdvancedStereoThresholdComboBox.Text = advancedSettings.ConversionThreshold;
                    AdvancedStereoBitrateControlComboBox.SelectedItem = advancedSettings.EncodingType;
                    AdvancedStereoPassesComboBox.SelectedIndex = advancedSettings.PassMode == "2-Pass" ? 1 : 0;
                }
                else
                {
                    MonoConvertStereoRadio.IsChecked = true;
                    AdvancedStereoOverridePanel.Visibility = Visibility.Collapsed;
                }
            }
            else // Stereo mode
            {
                // Check if Stereo mode was in advanced state when saved
                var isStereoAdvanced = Settings.Current.CurrentMode == "Stereo" && Settings.Current.IsAdvancedMode;
                
                if (isStereoAdvanced)
                {
                    StereoAdvancedRadio.IsChecked = true;
                    AdvancedMonoOverridePanel.Visibility = Visibility.Visible;
                    
                    // Restore StereoMode.AdvancedOverride settings to AdvancedMono panel
                    var advancedSettings = modeSettings.AdvancedOverride;
                    AdvancedMonoChannelsComboBox.SelectedItem = advancedSettings.ChannelMode;
                    AdvancedMonoBitrateComboBox.Text = advancedSettings.TargetBitrate;
                    AdvancedMonoSampleRateComboBox.SelectedItem = advancedSettings.SampleRate;
                    AdvancedMonoThresholdComboBox.Text = advancedSettings.ConversionThreshold;
                    AdvancedMonoBitrateControlComboBox.SelectedItem = advancedSettings.EncodingType;
                    AdvancedMonoPassesComboBox.SelectedIndex = advancedSettings.PassMode == "2-Pass" ? 1 : 0;
                }
                else
                {
                    StereoCopyMonoRadio.IsChecked = true;
                    AdvancedMonoOverridePanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SaveDefaultOutputPath()
        {
            _defaultOutputPath = OutputPathTextBox.Text;
            Settings.Current.DefaultOutputPath = _defaultOutputPath;
            SaveUserSettings();
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

        private void RebindAdvancedOverrideSettings()
        {
            var wasUpdating = _isUpdatingFromSettings;
            _isUpdatingFromSettings = true;
            
            try
            {
                if (Settings.Current.CurrentMode == "Mono")
                {
                    // Bind to stereo override settings
                    var stereoOverride = Settings.Current.MonoMode.AdvancedOverride;
                    AdvancedStereoChannelsComboBox.SelectedItem = stereoOverride.ChannelMode;
                    AdvancedStereoBitrateComboBox.Text = stereoOverride.TargetBitrate;
                    AdvancedStereoSampleRateComboBox.SelectedItem = stereoOverride.SampleRate;
                    AdvancedStereoThresholdComboBox.Text = stereoOverride.ConversionThreshold;
                    AdvancedStereoBitrateControlComboBox.SelectedItem = stereoOverride.EncodingType;
                    AdvancedStereoPassesComboBox.SelectedIndex = stereoOverride.PassMode == "2-Pass" ? 1 : 0;
                }
                else
                {
                    // Bind to mono override settings
                    var monoOverride = Settings.Current.StereoMode.AdvancedOverride;
                    AdvancedMonoChannelsComboBox.SelectedItem = monoOverride.ChannelMode;
                    AdvancedMonoBitrateComboBox.Text = monoOverride.TargetBitrate;
                    AdvancedMonoSampleRateComboBox.SelectedItem = monoOverride.SampleRate;
                    AdvancedMonoThresholdComboBox.Text = monoOverride.ConversionThreshold;
                    AdvancedMonoBitrateControlComboBox.SelectedItem = monoOverride.EncodingType;
                    AdvancedMonoPassesComboBox.SelectedIndex = monoOverride.PassMode == "2-Pass" ? 1 : 0;
                }
            }
            finally
            {
                _isUpdatingFromSettings = wasUpdating;
            }
        }
    }
}
