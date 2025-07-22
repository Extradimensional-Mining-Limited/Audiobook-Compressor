/*
    Filename: ProgressWidthConverter.cs
    Last Updated: 2025-07-17 03:10 CEST
    Version: 1.1.5
    State: Experimental
    Signed: Claude
    
    Synopsis:
    Enhanced progress bar width converter with configurable debug logging and improved type handling.
    Added XML documentation and better progress validation with detailed error reporting.
*/
using System;
using System.Globalization;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows;

namespace Audiobook_Compressor
{
    /// <summary>
    /// Converts a total width and progress value (0-1) into an actual pixel width for progress bars.
    /// </summary>
    public class ProgressWidthConverter : IMultiValueConverter
    {
        // Enable detailed debug logging in development environments
#if DEBUG
        private static readonly bool IsDebugLoggingEnabled = true;
#else
        private static readonly bool IsDebugLoggingEnabled = false;
#endif

        private void LogDebug(string message)
        {
            if (IsDebugLoggingEnabled)
            {
                Debug.WriteLine($"ProgressWidthConverter: {message}");
            }
        }

        /// <summary>
        /// Converts a total width and progress value into an actual pixel width.
        /// </summary>
        /// <param name="values">Array containing [totalWidth (double), progress (double)]</param>
        /// <param name="targetType">Expected return type (should be double)</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>Calculated width as a double, or 0 if conversion fails</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Validate input array
                if (values == null || values.Length != 2)
                {
                    LogDebug($"Invalid number of values. Expected 2, got {values?.Length ?? 0}");
                    return 0.0;
                }

                // Validate target type
                if (targetType != typeof(double))
                {
                    LogDebug($"Unexpected target type: {targetType.Name}. Expected: Double");
                }

                // Validate and convert totalWidth
                if (!(values[0] is double totalWidth))
                {
                    LogDebug($"Invalid totalWidth type. Expected double, got {values[0]?.GetType().Name ?? "null"}");
                    return 0.0;
                }

                // Validate and convert progress
                if (!(values[1] is double progress))
                {
                    LogDebug($"Invalid progress type. Expected double, got {values[1]?.GetType().Name ?? "null"}");
                    return 0.0;
                }

                // Handle negative width
                if (totalWidth < 0)
                {
                    LogDebug($"Negative totalWidth ({totalWidth}) detected, using 0");
                    totalWidth = 0;
                }

                // Log warning if progress is out of range
                if (progress < 0 || progress > 1)
                {
                    LogDebug($"Progress value ({progress}) out of range [0,1], will be normalized");
                }

                // Ensure progress is between 0 and 1
                var normalizedProgress = Math.Max(0, Math.Min(1, progress));
                
                // Calculate exact width and round to nearest pixel for crisp rendering
                var width = Math.Round(totalWidth * normalizedProgress, MidpointRounding.AwayFromZero);
                
                LogDebug($"totalWidth={totalWidth}, progress={progress}, normalized={normalizedProgress}, result={width}");
                return width;
            }
            catch (Exception ex)
            {
                LogDebug($"Exception occurred: {ex.Message}");
                return 0.0;
            }
        }

        /// <summary>
        /// ConvertBack is not supported for this converter.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown as conversion back is not supported</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ProgressWidthConverter: ConvertBack is not supported");
        }
    }
}