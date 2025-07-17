/*
    Last Updated: 2025-07-17 00:06 CEST
    Version: 1.0.2
    State: Stable
    Signed: User
    
    Synopsis:
    Value converter for progress bar width calculation.
    Converts progress value (0-1) and total width into actual pixel width for status bar progress.
    Updated file header structure and documented in Summary.md.
*/
using System;
using System.Globalization;
using System.Windows.Data;

namespace Audiobook_Compressor
{
    public class ProgressWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 &&
                values[0] is double totalWidth &&
                values[1] is double progress)
            {
                // Ensure progress is between 0 and 1
                var normalizedProgress = Math.Max(0, Math.Min(1, progress));
                // Calculate exact width
                return Math.Round(totalWidth * normalizedProgress);
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}