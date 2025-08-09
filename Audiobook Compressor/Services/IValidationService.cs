/*
    Filename: IValidationService.cs
    Last Updated: 2025-08-09 10:05 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Service interface for validation operations, centralizing all validation logic for MVVM architecture per Focus 13.1.0 Phase 1.
*/

using System.Collections.Generic;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Validation result with details about any issues found
    /// </summary>
    public class ServiceValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public bool HasErrors => Errors.Count > 0;
        public bool HasWarnings => Warnings.Count > 0;
    }

    /// <summary>
    /// Service for validating application data and user inputs
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates a bitrate string and normalizes it
        /// </summary>
        /// <param name="bitrateString">Bitrate string to validate (e.g., "64k", "128kb")</param>
        /// <returns>Validation result with normalized value if valid</returns>
        ServiceValidationResult ValidateBitrate(string bitrateString, out string normalizedValue);

        /// <summary>
        /// Validates a sample rate string
        /// </summary>
        /// <param name="sampleRateString">Sample rate string to validate</param>
        /// <returns>Validation result</returns>
        ServiceValidationResult ValidateSampleRate(string sampleRateString);

        /// <summary>
        /// Validates source and output paths
        /// </summary>
        /// <param name="sourcePath">Source directory path</param>
        /// <param name="outputPath">Output directory path</param>
        /// <returns>Validation result</returns>
        ServiceValidationResult ValidatePaths(string sourcePath, string outputPath);

        /// <summary>
        /// Validates that required tools (FFmpeg, FFprobe) are available
        /// </summary>
        /// <returns>Validation result</returns>
        ServiceValidationResult ValidateRequiredTools();

        /// <summary>
        /// Validates settings configuration for logical consistency
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        /// <returns>Validation result</returns>
        ServiceValidationResult ValidateSettings(ApplicationSettings settings);

        /// <summary>
        /// Checks for potential file overwrites in output directory
        /// </summary>
        /// <param name="outputPath">Output directory to check</param>
        /// <returns>Validation result with warnings about potential overwrites</returns>
        ServiceValidationResult CheckForPotentialOverwrites(string outputPath);
    }
}