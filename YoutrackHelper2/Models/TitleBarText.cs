using System;
using Prism.Mvvm;

namespace YoutrackHelper2.Models
{
    public class TitleBarText : BindableBase
    {
        private string text;
        private bool progressing;
        private TimeSpan currentWorkingDuration;
        private string projectName;
        private string issueTitle;

        public string Text
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ProjectName))
                {
                    return text;
                }

                if (!string.IsNullOrWhiteSpace(IssueTitle))
                {
                    return Progressing ? $"[{(int)CurrentWorkingDuration.TotalMinutes}m] {IssueTitle}" : $"{ProjectName}";
                }

                return text;
            }
            set => SetProperty(ref text, value);
        }

        public string ProjectName { get => projectName; set => SetProperty(ref projectName, value); }

        public string IssueTitle { get => issueTitle; set => SetProperty(ref issueTitle, value); }

        public bool Progressing
        {
            get => progressing;
            set
            {
                if (SetProperty(ref progressing, value))
                {
                    RaisePropertyChanged(nameof(Text));
                }
            }
        }

        public TimeSpan CurrentWorkingDuration
        {
            get => currentWorkingDuration;
            set
            {
                if (SetProperty(ref currentWorkingDuration, value))
                {
                    RaisePropertyChanged(nameof(Text));
                }
            }
        }
    }
}