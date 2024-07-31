using System;
using System.Globalization;
using System.Windows.Data;

namespace YoutrackHelper2.Views.Converters
{
    public class ShortNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string stringValue)
            {
                return value;
            }

            // ハイフンで文字列を分割
            var parts = stringValue.Split('-');
            if (parts.Length == 2)
            {
                var prefix = parts[0]; // XXX 部分
                var numberPart = parts[1]; // 0 部分

                // 数値部分を4桁にパディング
                var formattedNumber = numberPart.PadLeft(3, '0');
                return $"{prefix}-{formattedNumber}";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}