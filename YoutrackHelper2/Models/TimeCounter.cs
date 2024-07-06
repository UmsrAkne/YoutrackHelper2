using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutrackHelper2.Models
{
    public class TimeCounter
    {
        private const string TotalTimeTrackingKey = "total-duration-key";
        private readonly Dictionary<string, DateTime> trackingTimeDictionary = new ();
        private readonly TimeCounter totalTimeCounter;
        private TimeSpan totalTimeSpan = TimeSpan.Zero;

        public bool TotalTimeTracking
        {
            get => totalTimeCounter != null;
            init
            {
                if (value)
                {
                    totalTimeCounter ??= new TimeCounter();
                }
            }
        }

        /// <summary>
        ///     指定したキーで時間計測の開始時刻を設定します。
        ///     同一のキーで複数回の値時刻の登録は無効です。最初に登録した時刻のみ有効です。
        /// </summary>
        /// <param name="trackingName">時間計測の情報を判別する文字列</param>
        /// <param name="dateTime">計測の開始時刻</param>
        public void StartTimeTracking(string trackingName, DateTime dateTime)
        {
            trackingTimeDictionary.TryAdd(trackingName, dateTime);
            if (!TotalTimeTracking)
            {
                return;
            }

            totalTimeCounter.StartTimeTracking(TotalTimeTrackingKey, dateTime);
        }

        /// <summary>
        ///     StartTimeTracking() で登録した開始時刻から、このメソッドで入力した終了時刻までの時間を取得します。
        /// </summary>
        /// <param name="trackingName">時間計測の情報を判別する文字列</param>
        /// <param name="dateTime">計測の終了時刻</param>
        /// <returns>
        ///     指定したキーで登録されている開始時刻から、dateTime までの時間を返します。
        ///     指定したキーで開始時刻の登録がない場合、また開始時刻よりも終了時刻が前である場合は TimeSpan.Zero を返します。
        /// </returns>
        public TimeSpan FinishTimeTracking(string trackingName, DateTime dateTime)
        {
            if (!trackingTimeDictionary.ContainsKey(trackingName))
            {
                return TimeSpan.Zero;
            }

            var result = dateTime > trackingTimeDictionary[trackingName]
                ? dateTime - trackingTimeDictionary[trackingName]
                : TimeSpan.Zero;

            trackingTimeDictionary.Remove(trackingName);

            if (TotalTimeTracking && trackingTimeDictionary.Keys.Count == 0)
            {
                // ここまでの処理ですべてのトラッキングが終了したら、トータルの集計も終了する。
                // trackingTimeDictionary に一つでもキーが残っている場合は、継続中のタスクが存在するため、トータルの計測を継続する。
                totalTimeSpan += totalTimeCounter.FinishTimeTracking(TotalTimeTrackingKey, dateTime);
            }

            return result;
        }

        public IEnumerable<string> GetTrackingNames()
        {
            return trackingTimeDictionary.Keys.Select(s => s);
        }

        public bool IsTrackingNameRegistered(string trackingName)
        {
            return trackingTimeDictionary.ContainsKey(trackingName);
        }

        public TimeSpan GetTotalWorkingDuration(DateTime now)
        {
            if (!TotalTimeTracking)
            {
                return TimeSpan.Zero;
            }

            return totalTimeCounter.IsTrackingNameRegistered(TotalTimeTrackingKey)
                ? totalTimeSpan + (now - totalTimeCounter.GetStartedDateTime(TotalTimeTrackingKey))
                : totalTimeSpan;
        }

        /// <summary>
        /// 引数に入力したキーで計測中の作業の時間を取得します。
        /// </summary>
        /// <param name="trackingName">取得した作業のキー</param>
        /// <param name="now">現在の時刻を入力します</param>
        /// <returns>キーに紐づけられた作業時間を取得します。入力されたキーが不正な値だった場合は TimeSpan.Zero を返します。</returns>
        public TimeSpan GetWorkingDuration(string trackingName, DateTime now)
        {
            if (trackingTimeDictionary.TryGetValue(trackingName, out var dt))
            {
                return now - dt;
            }

            return TimeSpan.Zero;
        }

        private DateTime GetStartedDateTime(string trackingName)
        {
            return trackingTimeDictionary.GetValueOrDefault(trackingName);
        }
    }
}