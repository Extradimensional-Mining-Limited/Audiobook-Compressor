/*
    Last Updated: 2025-07-17 07:15 CEST
    Version: 1.1.0
    State: Stable
    Signed: User
    
    Synopsis:
    Complete UI functionality with comprehensive ComboBox handling.
    All settings update properly and immediately reflect in summary.
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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
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

                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    SourcePathTextBox.Text = dialog.SelectedPath;
                    // Set default output path if empty
                    if (string.IsNullOrWhiteSpace(OutputPathTextBox.Text))
                    {
                        OutputPathTextBox.Text = Path.Combine(dialog.SelectedPath, "Compressed_Audiobooks");
                    }
                }
            };

            OutputBrowseButton.Click += (s, e) =>
            {
                using var dialog = new WinForms.FolderBrowserDialog
                {
                    Description = "Select Output Folder",
                    UseDescriptionForTitle = true
                };

                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    OutputPathTextBox.Text = dialog.SelectedPath;
                }
            };

            StartButton.Click += StartButton_Click;
            CancelButton.Click += CancelButton_Click;
            
            UpdateSettingsSummary();
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
                    if (Settings.TryParseBitrate(bitrate, out int bps))
                    {
                        Settings.TargetBitrate = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                }
            };
            BitrateComboBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (Settings.TryParseBitrate(comboBox.Text, out int bps))
                    {
                        Settings.TargetBitrate = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                    e.Handled = true;
                }
            };
            BitrateComboBox.LostFocus += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (Settings.TryParseBitrate(comboBox.Text, out int bps))
                    {
                        Settings.TargetBitrate = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                }
            };

            // Setup sample rate options
            SampleRateComboBox.ItemsSource = Settings.SampleRateOptions;
            SampleRateComboBox.SelectedItem = Settings.DefaultSampleRate.ToString();
            SampleRateComboBox.SelectionChanged += SampleRate_SelectionChanged;

            // Setup threshold options
            ThresholdComboBox.ItemsSource = Settings.BitrateOptions;
            ThresholdComboBox.SelectedItem = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
            ThresholdComboBox.SelectionChanged += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox && comboBox.SelectedItem is string threshold)
                {
                    if (Settings.TryParseBitrate(threshold, out int bps))
                    {
                        Settings.MonoCopyThreshold = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                }
            };
            ThresholdComboBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (Settings.TryParseBitrate(comboBox.Text, out int bps))
                    {
                        Settings.MonoCopyThreshold = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
                    }
                    e.Handled = true;
                }
            };
            ThresholdComboBox.LostFocus += (s, e) =>
            {
                if (s is System.Windows.Controls.ComboBox comboBox)
                {
                    if (Settings.TryParseBitrate(comboBox.Text, out int bps))
                    {
                        Settings.MonoCopyThreshold = bps;
                        comboBox.Text = Settings.FormatBitrate(bps);
                        UpdateSettingsSummary();
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
                if (int.TryParse(rate.Replace("Hz", ""), out int sampleRate))
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
            var sampleRate = SampleRateComboBox.SelectedItem?.ToString() ?? $"{Settings.DefaultSampleRate}";
            var threshold = ThresholdComboBox.Text;
            var mode = BitrateControlComboBox.SelectedItem?.ToString() ?? Settings.DefaultBitrateControl;
            var passes = (PassesComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "1-Pass";

            // Ensure consistent formatting
            if (!bitrate.EndsWith("k", StringComparison.OrdinalIgnoreCase))
                bitrate += "k";
            if (!threshold.EndsWith("k", StringComparison.OrdinalIgnoreCase))
                threshold += "k";
            if (!sampleRate.EndsWith("Hz", StringComparison.OrdinalIgnoreCase))
                sampleRate += "Hz";

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
                    if (_cancellationSource.Token.IsCancellationRequested)
                        break;

                    var outputPath = file.GetOutputPath(OutputPathTextBox.Text);
                    UpdateStatus($"Processing {Path.GetFileName(file.SourcePath)}...", processed / (double)totalFiles);

                    await _audioProcessor.ProcessAudioFileAsync(file, outputPath);
                    processed++;
                }

                if (_cancellationSource.Token.IsCancellationRequested)
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
    }
}