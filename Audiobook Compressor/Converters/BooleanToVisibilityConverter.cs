/*
    Filename: Converters\BooleanToVisibilityConverter.cs
    Last Updated: 2025-08-09 12:30 CEST
    Version: 1.2.G
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Standard WPF BooleanToVisibilityConverter for MVVM data binding per Focus 14.1.0 Phase 2.
    Note: This is available as a built-in converter in WPF, but provided here for completeness.
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Audiobook_Compressor.Converters
{
    /// <summary>
    /// Converts boolean values to Visibility values for XAML binding
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to Visibility
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts Visibility back to boolean
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}