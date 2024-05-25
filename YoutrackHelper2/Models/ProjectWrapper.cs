using Newtonsoft.Json;
using Prism.Mvvm;
using YouTrackSharp.Projects;

namespace YoutrackHelper2.Models
{
    public class ProjectWrapper : BindableBase
    {
        private string fullName = string.Empty;
        private string shortName = string.Empty;
        private Project project;
        private bool isFavorite;
        private bool archived;

        [JsonIgnore]
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

        // ReSharper disable once MemberCanBePrivate.Global
        // JsonConverter でのデシリアライズ時に get, set が必要
        public string ShortName { get => shortName; set => SetProperty(ref shortName, value); }

        // ReSharper disable once MemberCanBePrivate.Global
        // JsonConverter でのデシリアライズ時に get, set が必要
        public string FullName { get => fullName; set => SetProperty(ref fullName, value); }

        public bool IsFavorite { get => isFavorite; set => SetProperty(ref isFavorite, value); }

        public bool Archived { get => archived; set => SetProperty(ref archived, value); }
    }
}