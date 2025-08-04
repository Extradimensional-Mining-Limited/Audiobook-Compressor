/*
    Filename: Settings.cs
    Last Updated: TIMESTAMP_ERROR
    Version: 1.2.A
    State: Experimental
    Signed: Claude

    Synopsis:
    Refactored Settings.cs to implement hierarchical data model with CompressionSettings, ModeSettings, and ApplicationSettings classes as specified in Focus 6.2.0.md.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Audiobook_Compressor.Models
{
    /// <summary>
    /// Represents a set of compression settings for audio processing
    /// </summary>
    public class CompressionSettings : INotifyPropertyChanged
    {
        private string _channelMode = Settings.DefaultChannel;
        private string _targetBitrate = Settings.FormatBitrate(Settings.DefaultBitrate);
        private string _sampleRate = Settings.FormatSampleRate(Settings.DefaultSampleRate);
        private string _conversionThreshold = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
        private string _encodingType = Settings.DefaultBitrateControl;
        private string _passMode = "1-Pass";

        public string ChannelMode
        {
            get => _channelMode;
            set { _channelMode = value; OnPropertyChanged(); }
        }

        public string TargetBitrate
        {
            get => _targetBitrate;
            set { _targetBitrate = value; OnPropertyChanged(); }
        }

        public string SampleRate
        {
            get => _sampleRate;
            set { _sampleRate = value; OnPropertyChanged(); }
        }

        public string ConversionThreshold
        {
            get => _conversionThreshold;
            set { _conversionThreshold = value; OnPropertyChanged(); }
        }

        public string EncodingType
        {
            get => _encodingType;
            set { _encodingType = value; OnPropertyChanged(); }
        }

        public string PassMode
        {
            get => _passMode;
            set { _passMode = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Contains main and advanced override settings for a specific mode (Mono or Stereo)
    /// </summary>
    public class ModeSettings : INotifyPropertyChanged
    {
        private CompressionSettings _main = new();
        private CompressionSettings _advancedOverride = new();

        public CompressionSettings Main
        {
            get => _main;
            set { _main = value; OnPropertyChanged(); }
        }

        public CompressionSettings AdvancedOverride
        {
            get => _advancedOverride;
            set { _advancedOverride = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Top-level application settings containing all mode-specific settings and application state
    /// </summary>
    public class ApplicationSettings : INotifyPropertyChanged
    {
        private ModeSettings _monoMode = new();
        private ModeSettings _stereoMode = new();
        private string _sourcePath = string.Empty;
        private string _outputPath = string.Empty;
        private string _defaultOutputPath = string.Empty;
        private string _currentMode = "Mono";
        private bool _isAdvancedMode = false;

        public ModeSettings MonoMode
        {
            get => _monoMode;
            set { _monoMode = value; OnPropertyChanged(); }
        }

        public ModeSettings StereoMode
        {
            get => _stereoMode;
            set { _stereoMode = value; OnPropertyChanged(); }
        }

        public string SourcePath
        {
            get => _sourcePath;
            set { _sourcePath = value; OnPropertyChanged(); }
        }

        public string OutputPath
        {
            get => _outputPath;
            set { _outputPath = value; OnPropertyChanged(); }
        }

        public string DefaultOutputPath
        {
            get => _defaultOutputPath;
            set { _defaultOutputPath = value; OnPropertyChanged(); }
        }

        public string CurrentMode
        {
            get => _currentMode;
            set { _currentMode = value; OnPropertyChanged(); }
        }

        public bool IsAdvancedMode
        {
            get => _isAdvancedMode;
            set { _isAdvancedMode = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets the currently active compression settings based on current mode and advanced mode state
        /// </summary>
        public CompressionSettings GetActiveSettings()
        {
            var modeSettings = CurrentMode == "Mono" ? MonoMode : StereoMode;
            return IsAdvancedMode ? modeSettings.AdvancedOverride : modeSettings.Main;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Manages settings for audio compression
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Default bitrate for compressed audio (48kbps)
        /// </summary>
        public const int DefaultBitrate = 48000;

        /// <summary>
        /// Default threshold for copying mono files (64kbps)
        /// </summary>
        public const int DefaultMonoCopyThreshold = 64000;

        /// <summary>
        /// Default sample rate for compressed audio (22050Hz)
        /// </summary>
        public const int DefaultSampleRate = 22050;

        /// <summary>
        /// Default channel setting for audio output
        /// </summary>
        public const string DefaultChannel = "Mono";

        /// <summary>
        /// Default bitrate control mode
        /// </summary>
        public const string DefaultBitrateControl = "ABR";

        /// <summary>
        /// Supported audio file extensions
        /// </summary>
        public static readonly ReadOnlyCollection<string> SupportedExtensions = new(new[]
        {
            ".m4b", ".mp3", ".aac", ".m4a", ".flac", 
            ".ogg", ".wma", ".wav", ".webma", ".opus"
        });

        /// <summary>
        /// Common bitrate options in kbps
        /// </summary>
        public static readonly ReadOnlyCollection<string> BitrateOptions = new(new[]
        {
            "32k", "48k", "64k", "96k", "128k", "192k"
        });

        /// <summary>
        /// Sample rate options in Hz (displayed as Hz in UI, but values are numeric)
        /// </summary>
        public static readonly ReadOnlyCollection<string> SampleRateOptions = new(new[]
        {
            "22050 Hz", "44100 Hz", "48000 Hz"
        });

        /// <summary>
        /// Channel options for audio output
        /// </summary>
        public static readonly ReadOnlyCollection<string> ChannelOptions = new(new[]
        {
            "Mono", "Stereo"
        });

        /// <summary>
        /// Current application settings instance
        /// </summary>
        public static ApplicationSettings Current { get; set; } = new();

        // Backward compatibility properties - these delegate to the hierarchical structure
        
        /// <summary>
        /// Current target bitrate in bits per second
        /// </summary>
        public static int TargetBitrate
        {
            get => TryParseBitrate(Current.GetActiveSettings().TargetBitrate, out int value) ? value : DefaultBitrate;
            set => Current.GetActiveSettings().TargetBitrate = FormatBitrate(value);
        }

        /// <summary>
        /// Current mono copy threshold in bits per second
        /// </summary>
        public static int MonoCopyThreshold
        {
            get => TryParseBitrate(Current.GetActiveSettings().ConversionThreshold, out int value) ? value : DefaultMonoCopyThreshold;
            set => Current.GetActiveSettings().ConversionThreshold = FormatBitrate(value);
        }

        /// <summary>
        /// Current target sample rate in Hz
        /// </summary>
        public static int TargetSampleRate
        {
            get => TryParseSampleRate(Current.GetActiveSettings().SampleRate, out int value) ? value : DefaultSampleRate;
            set => Current.GetActiveSettings().SampleRate = FormatSampleRate(value);
        }

        /// <summary>
        /// Whether to use constant bitrate encoding
        /// </summary>
        public static bool UseConstantBitrate
        {
            get => Current.GetActiveSettings().EncodingType == "CBR";
            set => Current.GetActiveSettings().EncodingType = value ? "CBR" : "ABR";
        }

        /// <summary>
        /// Whether to use two-pass encoding (ignored for CBR)
        /// </summary>
        public static bool UseTwoPass
        {
            get => Current.GetActiveSettings().PassMode == "2-Pass";
            set => Current.GetActiveSettings().PassMode = value ? "2-Pass" : "1-Pass";
        }

        /// <summary>
        /// Current channel setting
        /// </summary>
        public static string CurrentChannel
        {
            get => Current.GetActiveSettings().ChannelMode;
            set => Current.GetActiveSettings().ChannelMode = value;
        }

        /// <summary>
        /// Current bitrate control mode (ABR/CBR)
        /// </summary>
        public static string BitrateControl
        {
            get => Current.GetActiveSettings().EncodingType;
            set => Current.GetActiveSettings().EncodingType = value;
        }

        /// <summary>
        /// Whether to skip converting mono files to stereo
        /// </summary>
        public static bool DontConvertMonoToStereo { get; set; } = true;

        /// <summary>
        /// Parses a bitrate string (e.g., "48k") to bits per second
        /// </summary>
        public static bool TryParseBitrate(string input, out int bitsPerSecond)
        {
            bitsPerSecond = DefaultBitrate;
            
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Remove 'k' suffix if present
            input = input.Trim().ToLowerInvariant();
            if (input.EndsWith("k"))
                input = input[..^1];

            // Parse as integer and convert to bits per second
            if (int.TryParse(input, out int kbps))
            {
                bitsPerSecond = kbps * 1000;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Formats a bitrate value as a string (e.g., "48k")
        /// </summary>
        public static string FormatBitrate(int bitsPerSecond)
        {
            return $"{bitsPerSecond / 1000}k";
        }

        /// <summary>
        /// Parses a sample rate string (e.g., "22050 Hz") to integer Hz
        /// </summary>
        public static bool TryParseSampleRate(string input, out int sampleRate)
        {
            sampleRate = DefaultSampleRate;
            if (string.IsNullOrWhiteSpace(input))
                return false;
            input = input.Trim().ToLowerInvariant().Replace("hz", "").Trim();
            return int.TryParse(input, out sampleRate);
        }

        /// <summary>
        /// Formats a sample rate value as a string with "Hz" suffix
        /// </summary>
        public static string FormatSampleRate(int sampleRate)
        {
            return $"{sampleRate} Hz";
        }
    }
}