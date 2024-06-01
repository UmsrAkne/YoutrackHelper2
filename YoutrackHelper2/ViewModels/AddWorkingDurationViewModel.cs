using System;
using System.Globalization;
using Prism.Mvvm;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AddWorkingDurationViewModel : BindableBase
    {
        private const string DateTimeFormat = "yyyy/MM/dd HH:mm";
        private string timeText;
        private string durationText;
        private TimeRangeDirection timeRangeDirection;
        private bool enabledUi = true;

        public IConnector Connector { get; set; }

        public IssueWrapper CurrentIssueWrapper { get; set; }

        public TimeRangeDirection TimeRangeDirection
        {
            get => timeRangeDirection;
            set => SetProperty(ref timeRangeDirection, value);
        }

        public string TimeText { get => timeText; set => SetProperty(ref timeText, value); }

        public string DurationText { get => durationText; set => SetProperty(ref durationText, value); }

        public bool EnabledUi { get => enabledUi; set => SetProperty(ref enabledUi, value); }

        public AsyncDelegateCommand AddWorkingDurationAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            // DurationText が変換不可能なら、そもそも処理の必要がない。また、常識に考えて異常に大きな時間が入力された場合も不正な値とする。
            if (!int.TryParse(DurationText, out var dur) || dur >= 1200)
            {
                return;
            }

            EnabledUi = false;

            // 時刻のテキストボックスが空白の場合は、時刻指定なしとして扱う
            if (string.IsNullOrWhiteSpace(TimeText) && dur > 0)
            {
                await Connector.AddWorkingDuration(CurrentIssueWrapper.ShortName, dur);
                CurrentIssueWrapper.Issue = await Connector.ApplyCommand(
                    CurrentIssueWrapper.ShortName, "comment", "時刻指定なしで作業時間を追加");

                DurationText = string.Empty;
                EnabledUi = true;
                return;
            }

            var tt = TimeText!.Replace(" ", string.Empty);

            // 空白ではないが、時刻が変換できない場合は入力をミスと判定し、キャンセルする
            if (!IsValidDateTime(tt, out var dt))
            {
                EnabledUi = true;
                return;
            }

            var comment = TimeRangeDirection switch
            {
                TimeRangeDirection.From =>
                    $"作業時間を追加 {dt.ToString(DateTimeFormat)} - {dt.AddMinutes(dur).ToString(DateTimeFormat)}",
                TimeRangeDirection.To =>
                    $"作業時間を追加 {dt.AddMinutes(dur * -1).ToString(DateTimeFormat)} - {dt.ToString(DateTimeFormat)}",
                _ => string.Empty,
            };

            await Connector.AddWorkingDuration(CurrentIssueWrapper.ShortName, dur);
            CurrentIssueWrapper.Issue = await Connector.ApplyCommand(CurrentIssueWrapper.ShortName, "comment", comment);

            DurationText = string.Empty;
            EnabledUi = true;
        });

        /// <summary>
        /// このビューモデルのテキストプロパティの値を初期化します
        /// </summary>
        public void SetDefaultTexts()
        {
            DurationText = string.Empty;
            TimeText = DateTime.Now.ToString("yyyy MMdd HHmm");
        }

        private bool IsValidDateTime(string input, out DateTime dateTime)
        {
            return DateTime.TryParseExact(input, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
        }
    }
}