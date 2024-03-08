using Prism.Mvvm;
using YouTrackSharp.Projects;

namespace YoutrackHelper2.Models
{
    public class ProjectWrapper : BindableBase
    {
        private string fullName = string.Empty;
        private string shortName = string.Empty;
        private Project project;

        public Project Project
        {
            get => project;
            set
            {
                if (value != null)
                {
                    ShortName = value.ShortName;
                    FullName = value.Name;
                }

                SetProperty(ref project, value);
            }
        }

        public string ShortName { get => shortName; private set => SetProperty(ref shortName, value); }

        public string FullName { get => fullName; private set => SetProperty(ref fullName, value); }
    }
}