/*
    Filename: MainWindow.xaml.cs
    Last Updated: 2025-07-22 03:56 CEST
    Version: 1.1.12
    State: Experimental
    Signed: GitHub Copilot

    Synopsis:
    - Added user notification if settings file is missing or corrupted and defaults are used (4C2).
    - Refined ComboBox input handling for bitrate and threshold: accepts and normalizes 'kb' (e.g., '67kb' -> '67k') and updates the field and summary accordingly (1C2).
    - Updated StartButton logic: removed XAML event handler, now compression only starts after user closes collision dialog and chooses OK (blocking MessageBox). (2024-XX-XX)
    - Restored blocking collision warning at Start Compression: StartButton.Click now checks for collision and only calls StartButton_Click if user chooses OK. (2024-XX-XX)
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
            BitrateComboBox.SelectionChanged += (s, e) => 
            {
                if (s is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string bitrate)
                {
                    var normalized = NormalizeBitrateInput(bitrate);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        Settings.TargetBitrate = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
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
                    var normalized = NormalizeBitrateInput(comboBox.Text);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        Settings.TargetBitrate = bps;
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
                    var normalized = NormalizeBitrateInput(comboBox.Text);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        Settings.TargetBitrate = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a bitrate between 32k and 192k.", "Invalid Bitrate", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatBitrate(Settings.DefaultBitrate);
                    }
                }
            };

            // Setup sample rate options (display with Hz, store as int)
            SampleRateComboBox.ItemsSource = Settings.SampleRateOptions;
            SampleRateComboBox.SelectedItem = $"{Settings.DefaultSampleRate} Hz";
            SampleRateComboBox.SelectionChanged += SampleRate_SelectionChanged;
            SampleRateComboBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (Settings.TryParseSampleRate(comboBox.Text, out int sr) && (sr == 22050 || sr == 44100 || sr == 48000))
                    {
                        Settings.TargetSampleRate = sr;
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
                    if (Settings.TryParseSampleRate(comboBox.Text, out int sr) && (sr == 22050 || sr == 44100 || sr == 48000))
                    {
                        Settings.TargetSampleRate = sr;
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

            // Setup threshold options
            ThresholdComboBox.ItemsSource = Settings.BitrateOptions;
            ThresholdComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
            ThresholdComboBox.SelectionChanged += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string threshold)
                {
                    var normalized = NormalizeBitrateInput(threshold);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        Settings.MonoCopyThreshold = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a threshold between 32k and 192k.", "Invalid Threshold", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
                    }
                }
            };
            ThresholdComboBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && s is System.Windows.Controls.ComboBox comboBox)
                {
                    var normalized = NormalizeBitrateInput(comboBox.Text);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        Settings.MonoCopyThreshold = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a threshold between 32k and 192k.", "Invalid Threshold", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
                    }
                    e.Handled = true;
                }
            };
            ThresholdComboBox.LostFocus += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox)
                {
                    var normalized = NormalizeBitrateInput(comboBox.Text);
                    if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 192000)
                    {
                        Settings.MonoCopyThreshold = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter a threshold between 32k and 192k.", "Invalid Threshold", MessageBoxButton.OK, MessageBoxImage.Warning);
                        comboBox.Text = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
                    }
                }
            };

            // Setup bitrate control options
            BitrateControlComboBox.ItemsSource = new[] { "ABR", "CBR" };
            BitrateControlComboBox.SelectedItem = Settings.DefaultBitrateControl;
            BitrateControlComboBox.SelectionChanged += BitrateControl_SelectionChanged;

            // Setup passes options
            PassesComboBox.SelectionChanged += (s, e) => UpdateSettingsSummary();
        }

        private void Channels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string channel)
            {
                Settings.CurrentChannel = channel;
                UpdateSettingsSummary();
            }
        }

        private void SampleRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string rate)
            {
                if (Settings.TryParseSampleRate(rate, out int sampleRate))
                {
                    Settings.TargetSampleRate = sampleRate;
                    UpdateSettingsSummary();
                }
            }
        }

        private void BitrateControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string mode)
            {
                Settings.BitrateControl = mode;
                Settings.UseConstantBitrate = mode == "CBR";

                if (Settings.UseConstantBitrate)
                {
                    PassesComboBox.SelectedIndex = 0;
                    PassesComboBox.IsEnabled = false;
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
                        var src = root.Element("SourcePath")?.Value;
                        var outp = root.Element("OutputPath")?.Value;
                        var defOutp = root.Element("DefaultOutputPath")?.Value;
                        if (!string.IsNullOrWhiteSpace(src))
                            SourcePathTextBox.Text = src;
                        if (!string.IsNullOrWhiteSpace(outp))
                            OutputPathTextBox.Text = outp;
                        if (!string.IsNullOrWhiteSpace(defOutp))
                            _defaultOutputPath = defOutp;
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
                var doc = new XDocument(
                    new XElement("UserSettings",
                        new XElement("SourcePath", SourcePathTextBox.Text),
                        new XElement("OutputPath", OutputPathTextBox.Text),
                        new XElement("DefaultOutputPath", _defaultOutputPath)
                    )
                );
                doc.Save(SettingsFile);
            }
            catch { /* Ignore errors */ }
        }

        private void SaveDefaultOutputPath()
        {
            _defaultOutputPath = OutputPathTextBox.Text;
            SaveUserSettings();
        }

        private void LoadDefaultOutputPath()
        {
            if (!string.IsNullOrWhiteSpace(_defaultOutputPath))
                OutputPathTextBox.Text = _defaultOutputPath;
        }

        private void CheckSourceOutputCollision()
        {
            if (IsSourceOutputCollision())
            {
                System.Windows.MessageBox.Show(
                    "Warning: Source and Output folders are the same. This may overwrite your source files.",
                    "Folder Collision Detected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
    }
}