using System;
using System.Collections.Generic;
using System.Linq;
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
            set => SetProperty(ref creationDateTime, value);
        }

        public DelegateCommand ToggleExpandedCommand => new DelegateCommand(() =>
        {
            Expanded = !Expanded;
        });

        public List<Comment> Comments { get => comments; set => SetProperty(ref comments, value); }
    }
}