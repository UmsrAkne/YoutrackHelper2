using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ProjectListViewModel : BindableBase
    {
        private ObservableCollection<ProjectWrapper> projects = new ();
        private ProjectWrapper selectedProject;

        public ProjectListViewModel()
        {
            var uri = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\uri.txt")
            .Replace("\n", string.Empty);

            var perm = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\perm.txt")
            .Replace("\n", string.Empty);

            _ = GetProjectsAsync(uri, perm);
        }

        public event EventHandler NavigationRequest;

        public ProjectWrapper SelectedProject
        {
            get => selectedProject;
            set => SetProperty(ref selectedProject, value);
        }

        public ObservableCollection<ProjectWrapper> Projects
        {
            get => projects;
            private set => SetProperty(ref projects, value);
        }

        public DelegateCommand NavigationRequestCommand => new DelegateCommand(() =>
        {
            NavigationRequest?.Invoke(this, EventArgs.Empty);
            WriteJsonFile();
        });

        public DelegateCommand<ProjectWrapper> ToggleFavoriteCommand => new DelegateCommand<ProjectWrapper>((param) =>
        {
            if (param == null)
            {
                return;
            }

            param.IsFavorite = !param.IsFavorite;
            WriteJsonFile();
        });

        private Connector Connector { get; set; }

        public TitleBarText TitleBarText { get; set; }

        private async Task GetProjectsAsync(string uri, string perm)
        {
            Connector = new Connector(uri, perm);
            await Connector.LoadProjects();
            Projects = new ObservableCollection<ProjectWrapper>(Connector.ProjectWrappers);
            ReadJsonFile();

            // foreach (var p in ps)
            // {
            //     var issueList = await Connector.GetIssues(p.Name);
            //     p.IssueCount = issueList.Count;
            //     p.IncompleteIssueCount = issueList.Count(i => !new IssueWrapper(i).Completed);
            // }
        }

        private void WriteJsonFile()
        {
            var json = JsonConvert.SerializeObject(Projects, Formatting.Indented);
            File.WriteAllText($"{nameof(Projects)}.json", json);
        }

        /// <summary>
        /// Projects.json ファイルを読み込み、現在の Projects の各要素に、読み込んだデータを反映します。
        /// </summary>
        private void ReadJsonFile()
        {
            if (!File.Exists($"{nameof(Projects)}.json"))
            {
                return;
            }

            var json = File.ReadAllText($"{nameof(Projects)}.json");
            var list =
                JsonConvert.DeserializeObject<List<ProjectWrapper>>(json).ToDictionary(p => p.ShortName);

            foreach (var p in Projects)
            {
                if (list.TryGetValue(p.ShortName, out var pw))
                {
                    p.IsFavorite = pw.IsFavorite;
                }
            }
        }
    }
}