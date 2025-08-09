/*
    Filename: Converters\BitrateValidationConverter.cs
    Last Updated: 2025-08-09 12:30 CEST
    Version: 1.2.G
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Handles bitrate normalization and validation for ComboBox binding per Focus 14.1.0 Phase 2.
    Provides user-friendly display formatting and input normalization.
*/

using System;
using System.Globalization;
using System.Windows.Data;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Converters
{
    /// <summary>
    /// Converts and validates bitrate values for ComboBox binding
    /// Handles both display formatting and input normalization
    /// </summary>
    public class BitrateValidationConverter : IValueConverter
    {
        /// <summary>
        /// Converts bitrate value for display
        /// </summary>
        /// <param name="value">Bitrate value (string or integer)</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Formatted bitrate string</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var bitrateString = value.ToString();
            
            // If it's already properly formatted, return as-is
            if (!string.IsNullOrWhiteSpace(bitrateString))
            {
                // Ensure it has 'k' suffix for display
                if (Settings.TryParseBitrate(bitrateString, out int bps))
                {
                    return Settings.FormatBitrate(bps);
                }
            }

            return bitrateString ?? string.Empty;
        }

        /// <summary>
        /// Converts user input back to normalized bitrate format
        /// </summary>
        /// <param name="value">User input string</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Normalized bitrate string or original value if invalid</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var input = value.ToString();
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Try to normalize the input
            var normalized = NormalizeBitrateInput(input);
            if (Settings.TryParseBitrate(normalized, out int bps) && bps >= 32000 && bps <= 320000)
            {
                return Settings.FormatBitrate(bps);
            }

            // Return original input if normalization fails (will trigger validation in ViewModel)
            return input;
        }

        /// <summary>
        /// Normalizes bitrate input by removing common suffixes
        /// </summary>
        private static string NormalizeBitrateInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) 
                return input;
            
            input = input.Trim().ToLowerInvariant();
            
            if (input.EndsWith("kbps"))
                input = input[..^4]; // Remove 'kbps'
            else if (input.EndsWith("kb"))
                input = input[..^2]; // Remove 'kb'
            else if (input.EndsWith("k"))
                input = input[..^1]; // Remove 'k'

            return input.Trim();
        }
    }
}