using System;
using System.Globalization;
using System.Windows.Data;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.Views
{
    /// <summary>
    /// IssueWrapper.State の値を変換して、課題の状態欄に表示するためのコンバーターです。
    /// </summary>
    public class IssueStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IssueWrapper issueWrapper)
            {
                return string.Empty;
            }

            if (issueWrapper.Progressing)
            {
                return issueWrapper.WorkingDuration.ToString(@"hh\:mm\:ss");
            }

            if (issueWrapper.Completed || issueWrapper.WorkingDuration == TimeSpan.Zero)
            {
                return issueWrapper.State;
            }

            return $"{(int)issueWrapper.WorkingDuration.TotalMinutes} min";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 現状では実装の必要はないが、念の為空文字を返す。
            return string.Empty;
        }
    }
}