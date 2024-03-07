using Prism.Mvvm;
using YouTrackSharp.Projects;

namespace YoutrackHelper2.Models
{
    public class ProjectWrapper : BindableBase
    {
        public Project Project { get; set; }

        public string ShortName { get; private set; } = string.Empty;

        public string FullName { get; private set; } = string.Empty;
    }
}