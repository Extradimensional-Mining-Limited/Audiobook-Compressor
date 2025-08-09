/*
    Filename: ValidationService.cs
    Last Updated: 2025-08-09 10:10 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Concrete implementation of IValidationService, centralizing validation logic from MainWindow per Focus 13.1.0 Phase 2.
*/

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for validating application data and user inputs
    /// </summary>
    public class ValidationService : IValidationService
    {
        /// <summary>
        /// Validates a bitrate string and normalizes it
        /// </summary>
        public ServiceValidationResult ValidateBitrate(string bitrateString, out string normalizedValue)
        {
            var result = new ServiceValidationResult { IsValid = true };
            normalizedValue = string.Empty;

            if (string.IsNullOrWhiteSpace(bitrateString))
            {
                result.IsValid = false;
                result.Errors.Add("Bitrate cannot be empty");
                return result;
            }

            try
            {
                // Normalize the input (similar to MainWindow.NormalizeBitrateInput)
                var normalized = bitrateString.Trim().ToLowerInvariant();
                if (normalized.EndsWith("kbps"))
                    normalized = normalized[..^4]; // Remove 'kbps'
                else if (normalized.EndsWith("kb"))
                    normalized = normalized[..^2]; // Remove 'kb'
                else if (normalized.EndsWith("k"))
                    normalized = normalized[..^1]; // Remove 'k'

                normalized = normalized.Trim();

                if (!int.TryParse(normalized, out int bitrateKbps))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Invalid bitrate format: '{bitrateString}'. Expected format like '64k', '128kb', or '96kbps'");
                    return result;
                }

                // Validate range
                if (bitrateKbps < 32 || bitrateKbps > 320)
                {
                    if (bitrateKbps < 32)
                    {
                        result.Warnings.Add($"Bitrate {bitrateKbps}k is very low. Consider using at least 32k for acceptable quality.");
                    }
                    else
                    {
                        result.Warnings.Add($"Bitrate {bitrateKbps}k is very high. Most audiobooks don't need more than 192k.");
                    }
                }

                // Return normalized format
                normalizedValue = $"{bitrateKbps}k";
                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating bitrate: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Validates a sample rate string
        /// </summary>
        public ServiceValidationResult ValidateSampleRate(string sampleRateString)
        {
            var result = new ServiceValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(sampleRateString))
            {
                result.IsValid = false;
                result.Errors.Add("Sample rate cannot be empty");
                return result;
            }

            try
            {
                var normalized = sampleRateString.Trim().ToLowerInvariant().Replace("hz", "").Trim();
                
                if (!int.TryParse(normalized, out int sampleRate))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Invalid sample rate format: '{sampleRateString}'. Expected format like '22050', '44100 Hz', or '48000Hz'");
                    return result;
                }

                var validSampleRates = new[] { 22050, 44100, 48000 };
                if (!validSampleRates.Contains(sampleRate))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Unsupported sample rate: {sampleRate}Hz. Supported rates are: {string.Join(", ", validSampleRates.Select(sr => sr + "Hz"))}");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating sample rate: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Validates source and output paths
        /// </summary>
        public ServiceValidationResult ValidatePaths(string sourcePath, string outputPath)
        {
            var result = new ServiceValidationResult { IsValid = true };

            try
            {
                // Check if paths are provided
                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    result.IsValid = false;
                    result.Errors.Add("Source path is required");
                }

                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    result.IsValid = false;
                    result.Errors.Add("Output path is required");
                }

                if (!result.IsValid)
                    return result;

                // Check if source path exists
                if (!Directory.Exists(sourcePath))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Source directory does not exist: {sourcePath}");
                }

                // Check if output path is valid (create if it doesn't exist)
                try
                {
                    if (!Directory.Exists(outputPath))
                    {
                        result.Warnings.Add($"Output directory will be created: {outputPath}");
                    }

                    // Test write access
                    var testFile = Path.Combine(outputPath, $"test_write_{Guid.NewGuid()}.tmp");
                    Directory.CreateDirectory(outputPath);
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Cannot write to output directory '{outputPath}': {ex.Message}");
                }

                // Check for path collision
                if (string.Equals(sourcePath, outputPath, StringComparison.OrdinalIgnoreCase))
                {
                    result.Warnings.Add("Source and output paths are the same. This may overwrite source files.");
                }

                // Check if output is a subdirectory of source
                if (IsSubdirectory(sourcePath, outputPath))
                {
                    result.Warnings.Add("Output directory is inside the source directory. This may cause processing issues.");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating paths: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Validates that required tools (FFmpeg, FFprobe) are available
        /// </summary>
        public ServiceValidationResult ValidateRequiredTools()
        {
            var result = new ServiceValidationResult { IsValid = true };

            try
            {
                // Check FFmpeg
                if (!File.Exists(Constants.FFmpegPath))
                {
                    result.IsValid = false;
                    result.Errors.Add($"FFmpeg not found at: {Constants.FFmpegPath}");
                }

                // Check FFprobe
                if (!File.Exists(Constants.FFprobePath))
                {
                    result.IsValid = false;
                    result.Errors.Add($"FFprobe not found at: {Constants.FFprobePath}");
                }

                if (!result.IsValid)
                {
                    result.Errors.Add("Please ensure FFmpeg and FFprobe are installed and accessible.");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating required tools: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Validates settings configuration for logical consistency
        /// </summary>
        public ServiceValidationResult ValidateSettings(ApplicationSettings settings)
        {
            var result = new ServiceValidationResult { IsValid = true };

            if (settings == null)
            {
                result.IsValid = false;
                result.Errors.Add("Settings cannot be null");
                return result;
            }

            try
            {
                // Validate mode settings
                if (settings.MonoMode == null || settings.StereoMode == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("Mode settings cannot be null");
                    return result;
                }

                // Validate selected actions
                var validActions = new[] { "Copy", "Convert", "Advanced" };
                if (!validActions.Contains(settings.MonoMode.SelectedAction))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Invalid mono mode selected action: {settings.MonoMode.SelectedAction}");
                }

                if (!validActions.Contains(settings.StereoMode.SelectedAction))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Invalid stereo mode selected action: {settings.StereoMode.SelectedAction}");
                }

                // Validate bitrate settings
                ValidateCompressionSettings(settings.MonoMode.Main, "Mono Main", result);
                ValidateCompressionSettings(settings.MonoMode.AdvancedOverride, "Mono Advanced", result);
                ValidateCompressionSettings(settings.StereoMode.Main, "Stereo Main", result);
                ValidateCompressionSettings(settings.StereoMode.AdvancedOverride, "Stereo Advanced", result);

                // Check for logical inconsistencies
                CheckBitrateThresholdLogic(settings, result);

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating settings: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Checks for potential file overwrites in output directory
        /// </summary>
        public ServiceValidationResult CheckForPotentialOverwrites(string outputPath)
        {
            var result = new ServiceValidationResult { IsValid = true };

            try
            {
                if (string.IsNullOrWhiteSpace(outputPath) || !Directory.Exists(outputPath))
                {
                    return result; // No directory to check
                }

                var existingFiles = Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => Settings.SupportedExtensions.Any(ext => 
                        file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (existingFiles.Count > 0)
                {
                    result.Warnings.Add($"Found {existingFiles.Count} existing audio files in output directory that may be overwritten.");
                    
                    if (existingFiles.Count <= 5)
                    {
                        foreach (var file in existingFiles)
                        {
                            var relativePath = Path.GetRelativePath(outputPath, file);
                            result.Warnings.Add($"  - {relativePath}");
                        }
                    }
                    else
                    {
                        result.Warnings.Add($"  - {Path.GetFileName(existingFiles[0])} and {existingFiles.Count - 1} more files");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Could not check for existing files: {ex.Message}");
                return result;
            }
        }

        #region Private Methods

        private void ValidateCompressionSettings(CompressionSettings settings, string context, ServiceValidationResult result)
        {
            if (settings == null) return;

            // Validate bitrate
            var bitrateValidation = ValidateBitrate(settings.TargetBitrate, out _);
            if (!bitrateValidation.IsValid)
            {
                result.IsValid = false;
                foreach (var error in bitrateValidation.Errors)
                {
                    result.Errors.Add($"{context} - Target Bitrate: {error}");
                }
            }

            // Validate threshold
            var thresholdValidation = ValidateBitrate(settings.ConversionThreshold, out _);
            if (!thresholdValidation.IsValid)
            {
                result.IsValid = false;
                foreach (var error in thresholdValidation.Errors)
                {
                    result.Errors.Add($"{context} - Conversion Threshold: {error}");
                }
            }

            // Validate sample rate
            var sampleRateValidation = ValidateSampleRate(settings.SampleRate);
            if (!sampleRateValidation.IsValid)
            {
                result.IsValid = false;
                foreach (var error in sampleRateValidation.Errors)
                {
                    result.Errors.Add($"{context} - Sample Rate: {error}");
                }
            }
        }

        private void CheckBitrateThresholdLogic(ApplicationSettings settings, ServiceValidationResult result)
        {
            CheckBitrateThresholdForMode(settings.MonoMode.Main, "Mono Main", result);
            CheckBitrateThresholdForMode(settings.MonoMode.AdvancedOverride, "Mono Advanced", result);
            CheckBitrateThresholdForMode(settings.StereoMode.Main, "Stereo Main", result);
            CheckBitrateThresholdForMode(settings.StereoMode.AdvancedOverride, "Stereo Advanced", result);
        }

        private void CheckBitrateThresholdForMode(CompressionSettings settings, string context, ServiceValidationResult result)
        {
            if (Settings.TryParseBitrate(settings.TargetBitrate, out int targetBps) &&
                Settings.TryParseBitrate(settings.ConversionThreshold, out int thresholdBps))
            {
                if (targetBps > thresholdBps)
                {
                    result.Warnings.Add($"{context}: Target bitrate ({Settings.FormatBitrate(targetBps)}) is higher than conversion threshold ({Settings.FormatBitrate(thresholdBps)}). This may increase file sizes.");
                }
            }
        }

        private static bool IsSubdirectory(string parentPath, string childPath)
        {
            try
            {
                var parentUri = new Uri(parentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
                var childUri = new Uri(childPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
                
                return parentUri.IsBaseOf(childUri);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}