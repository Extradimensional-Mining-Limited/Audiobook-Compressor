/*
    Filename: Converters\RadioButtonToStringConverter.cs
    Last Updated: 2025-08-09 12:30 CEST
    Version: 1.2.G
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Converts between radio button selection state and string values for MVVM data binding per Focus 14.1.0 Phase 2.
*/

using System;
using System.Globalization;
using System.Windows.Data;

namespace Audiobook_Compressor.Converters
{
    /// <summary>
    /// Converts between boolean radio button state and string values
    /// Used for binding radio button groups to string-based properties
    /// </summary>
    public class RadioButtonToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string value to boolean based on parameter comparison
        /// </summary>
        /// <param name="value">The string value to compare</param>
        /// <param name="targetType">Target type (should be bool)</param>
        /// <param name="parameter">The string to compare against</param>
        /// <param name="culture">Culture info</param>
        /// <returns>True if value matches parameter, false otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString() == parameter.ToString();
        }

        /// <summary>
        /// Converts boolean back to string if true
        /// </summary>
        /// <param name="value">Boolean value indicating if radio button is selected</param>
        /// <param name="targetType">Target type (should be string)</param>
        /// <param name="parameter">The string value to return if true</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Parameter string if value is true, otherwise Binding.DoNothing</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                return parameter.ToString();
            }
            return System.Windows.Data.Binding.DoNothing;
        }
    }
}