using System;
using System.Globalization;
using System.Windows.Data;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.Views
{
    public class WorkTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WorkType wt)
            {
                return wt switch
                {
                    WorkType.Feature => "機能",
                    WorkType.Appearance => "外観",
                    WorkType.Test => "テスト",
                    WorkType.Todo => "タスク",
                    WorkType.Bug => "バグ",
                    _ => string.Empty,
                };
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}