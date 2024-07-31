using System;
using System.Globalization;
using System.Windows.Data;

namespace YoutrackHelper2.Views.Converters
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var format = (string)parameter ?? @"hh\:mm\:ss";
                var ts = (TimeSpan)value;
                return ts != TimeSpan.Zero
                    ? TimeSpan.FromSeconds(Math.Floor(((TimeSpan)value).TotalSeconds)).ToString(format)
                    : string.Empty;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}