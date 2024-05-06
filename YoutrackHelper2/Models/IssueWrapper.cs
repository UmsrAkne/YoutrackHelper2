using System;
using System.Collections.Generic;
using System.Linq;
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
        private string state = string.Empty;
        private string temporaryComment;
        private DateTime startedAt1;
        private bool progressing;

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
                    State = ValueGetter.GetString(value, "State");
                    CreationDateTime = DateTimeOffset.FromUnixTimeMilliseconds(ValueGetter.GetLong(value, "created")).DateTime;
                    Resolved = DateTimeOffset.FromUnixTimeMilliseconds(ValueGetter.GetLong(value, "resolved")).DateTime;
                    NumberInProject = ValueGetter.GetLong(value, "numberInProject");
                    Progressing = State == "作業中";
                    Expanded = Progressing;
                    Comments = value.Comments.
                        Select(c => new Comment()
                        {
                            Text = c.Text,
                            DateTime = c.Created ?? default,
                        })
                        .OrderByDescending(c => c.DateTime)
                        .ToList();
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

        public string State { get => state; set => SetProperty(ref state, value); }

        public DateTime Resolved { get; set; }

        public long NumberInProject { get; set; }

        public DateTime StartedAt { get => startedAt1; set => SetProperty(ref startedAt1, value); }

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

        public bool Progressing { get => progressing; set => SetProperty(ref progressing, value); }

        public DelegateCommand ToggleExpandedCommand => new DelegateCommand(() =>
        {
            Expanded = !Expanded;
        });

        public List<Comment> Comments { get => comments; set => SetProperty(ref comments, value); }

        public string TemporaryComment
        {
            get => temporaryComment;
            set => SetProperty(ref temporaryComment, value);
        }

        public async Task ToggleStatus(Connector connector, TimeCounter counter)
        {
            // Logger.WriteMessageToFile($"課題の状態を変更 {FullName} 現在の状態 : {Status}");
            if (State == "未完了")
            {
                counter.StartTimeTracking(shortName, DateTime.Now);
            }

            var comment = string.Empty;

            if (counter.IsTrackingNameRegistered(ShortName) && State == "作業中")
            {
                var now = DateTime.Now;
                var duration = counter.FinishTimeTracking(shortName, now);
                var startedAt = now - duration;
                const string f = "yyyy/MM/dd HH:mm";
                comment = $"中断 作業時間 {(int)duration.TotalMinutes} min ({startedAt.ToString(f)} - {now.ToString(f)})";
                if (duration.TotalSeconds > 60)
                {
                    await connector.ApplyCommand(ShortName, $"作業 {(int)duration.TotalMinutes}m", string.Empty);
                }
            }

            switch (State)
            {
                case "未完了":
                    Issue = await connector.ApplyCommand(ShortName, "state 作業中", comment);
                    StartedAt = DateTime.Now;
                    return;
                case "作業中":
                    Issue = await connector.ApplyCommand(ShortName, "state 未完了", comment);
                    StartedAt = default;
                    // UpdateWorkingDuration(DateTime.Now);
                    break;
            }
        }

        public async Task Complete(Connector connector, TimeCounter counter)
        {
            // Logger.WriteMessageToFile($"課題を完了 {FullName}");
            var comment = string.Empty;

            if (counter.IsTrackingNameRegistered(ShortName))
            {
                var now = DateTime.Now;
                var duration = counter.FinishTimeTracking(shortName, now);
                var startedAt = now - duration;
                const string f = "yyyy/MM/dd HH:mm";
                comment = $"完了 作業時間 {(int)duration.TotalMinutes} min ({startedAt.ToString(f)} - {now.ToString(f)})";
                if (duration.TotalSeconds > 60)
                {
                    await connector.ApplyCommand(ShortName, $"作業 {(int)duration.TotalMinutes}m", string.Empty);
                }
            }

            Issue = await connector.ApplyCommand(ShortName, "state 完了", comment);
            StartedAt = DateTime.MinValue;

            // UpdateWorkingDuration(DateTime.Now);
        }
    }
}