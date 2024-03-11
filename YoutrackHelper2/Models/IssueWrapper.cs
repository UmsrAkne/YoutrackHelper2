using Prism.Mvvm;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    public class IssueWrapper : BindableBase
    {
        private string title = string.Empty;
        private string shortName = string.Empty;
        private Issue issue;

        public Issue Issue
        {
            get => issue;
            set
            {
                if (value != null)
                {
                    Title = value.Summary;
                    ShortName = value.Id;
                }

                SetProperty(ref issue, value);
            }
        }

        public string Title { get => title; set => SetProperty(ref title, value); }

        public string ShortName { get => shortName; set => SetProperty(ref shortName, value); }
    }
}