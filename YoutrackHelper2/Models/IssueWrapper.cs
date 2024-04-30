using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    public class IssueWrapper : BindableBase
    {
        private string title = string.Empty;
        private string shortName = string.Empty;
        private Issue issue;
        private bool completed;
        private string description = string.Empty;
        private bool expanded;
        private List<Comment> comments;
        private string workType = string.Empty;
        private DateTime creationDateTime;
        private TimeSpan workingDuration = TimeSpan.Zero;

        public Issue Issue
        {
            get => issue;
            set
            {
                if (value != null)
                {
                    Title = value.Summary;
                    ShortName = value.Id;
                    Description = value.Description;
                    WorkType = ValueGetter.GetString(value, "Type");
                    Completed = ValueGetter.GetString(value, "State") == "完了";
                    CreationDateTime = DateTimeOffset.FromUnixTimeMilliseconds(ValueGetter.GetLong(value, "created")).DateTime;
                }

                SetProperty(ref issue, value);
            }
        }

        public bool Expanded { get => expanded; set => SetProperty(ref expanded, value); }

        public string Title { get => title; set => SetProperty(ref title, value); }

        public string ShortName { get => shortName; set => SetProperty(ref shortName, value); }

        public string Description { get => description; set => SetProperty(ref description, value); }

        public bool Completed { get => completed; set => SetProperty(ref completed, value); }

        public string WorkType { get => workType; set => SetProperty(ref workType, value); }

        public TimeSpan WorkingDuration
        {
            get => workingDuration;
            set => SetProperty(ref workingDuration, value);
        }

        public DateTime CreationDateTime
        {
            get => creationDateTime;
            private set => SetProperty(ref creationDateTime, value);
        }

        public DelegateCommand ToggleExpandedCommand => new DelegateCommand(() =>
        {
            Expanded = !Expanded;
        });

        public List<Comment> Comments { get => comments; init => SetProperty(ref comments, value); }

        public async Task Complete(Connector connector)
        {
            // Logger.WriteMessageToFile($"課題を完了 {FullName}");
            var comment = string.Empty;

            // if (counter.IsTrackingNameRegistered(ShortName))
            // {
            //     var now = DateTime.Now;
            //     var duration = counter.FinishTimeTracking(shortName, now);
            //     var startedAt = now - duration;
            //     const string f = "yyyy/MM/dd HH:mm";
            //     comment = $"完了 作業時間 {(int)duration.TotalMinutes} min ({startedAt.ToString(f)} - {now.ToString(f)})";
            //     if (duration.TotalSeconds > 60)
            //     {
            //         await connector.ApplyCommand(ShortName, $"作業 {(int)duration.TotalMinutes}m", string.Empty);
            //     }
            // }
            Issue = await connector.ApplyCommand(ShortName, "state 完了", comment);

            // StartedAt = DateTime.MinValue;
            // UpdateWorkingDuration(DateTime.Now);
        }
    }
}