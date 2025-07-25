/*
    Filename: Settings.cs
    Last Updated: 2025-07-25 03:32
    Version: 1.2.0
    State: Stable
    Signed: User

    Synopsis:
    - All file headers updated to v1.2.0, state Stable, signed User, with unified timestamp.
    - Documentation and changelog discipline enforced for release.
    - No code changes since last version except header and documentation updates.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Audiobook_Compressor.Models
{
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
        /// Current target bitrate in bits per second
        /// </summary>
        public static int TargetBitrate { get; set; } = DefaultBitrate;

        /// <summary>
        /// Current mono copy threshold in bits per second
        /// </summary>
        public static int MonoCopyThreshold { get; set; } = DefaultMonoCopyThreshold;

        /// <summary>
        /// Current target sample rate in Hz
        /// </summary>
        public static int TargetSampleRate { get; set; } = DefaultSampleRate;

        /// <summary>
        /// Whether to use constant bitrate encoding
        /// </summary>
        public static bool UseConstantBitrate { get; set; } = false;

        /// <summary>
        /// Whether to use two-pass encoding (ignored for CBR)
        /// </summary>
        public static bool UseTwoPass { get; set; } = false;

        /// <summary>
        /// Current channel setting
        /// </summary>
        public static string CurrentChannel { get; set; } = DefaultChannel;

        /// <summary>
        /// Current bitrate control mode (ABR/CBR)
        /// </summary>
        public static string BitrateControl { get; set; } = DefaultBitrateControl;

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

        public class AdvancedOverrideSettings
        {
            public string Channel { get; set; }
            public string Bitrate { get; set; }
            public string SampleRate { get; set; }
            public string Threshold { get; set; }
            public string BitrateControl { get; set; }
            public int PassesIndex { get; set; }

            public AdvancedOverrideSettings()
            {
                Channel = Settings.DefaultChannel;
                Bitrate = Settings.FormatBitrate(Settings.DefaultBitrate);
                SampleRate = Settings.FormatSampleRate(Settings.DefaultSampleRate);
                Threshold = Settings.FormatBitrate(Settings.DefaultMonoCopyThreshold);
                BitrateControl = Settings.DefaultBitrateControl;
                PassesIndex = 0;
            }
        }

        public static AdvancedOverrideSettings AdvancedStereoOverrideSettings { get; set; } = new AdvancedOverrideSettings();
        public static AdvancedOverrideSettings AdvancedMonoOverrideSettings { get; set; } = new AdvancedOverrideSettings();
    }
}