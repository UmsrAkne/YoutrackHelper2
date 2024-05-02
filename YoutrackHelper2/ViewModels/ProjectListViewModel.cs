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
            var pws = Connector.ProjectWrappers;
            ReadJsonFile(pws);

            Projects = new ObservableCollection<ProjectWrapper>(
                    pws.OrderByDescending(p => p.IsFavorite)
                        .ThenBy(p => p.FullName));

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
        /// Projects.json ファイルを読み込み、引数のリストの各要素に、読み込んだデータを反映します。
        /// </summary>
        /// <param name="pws">中の要素を書き換える ProjectWrapper のリスト</param>
        private void ReadJsonFile(IEnumerable<ProjectWrapper> pws)
        {
            if (!File.Exists($"{nameof(Projects)}.json"))
            {
                return;
            }

            var json = File.ReadAllText($"{nameof(Projects)}.json");
            var list =
                JsonConvert.DeserializeObject<List<ProjectWrapper>>(json).ToDictionary(p => p.ShortName);

            foreach (var p in pws)
            {
                if (list.TryGetValue(p.ShortName, out var pw))
                {
                    p.IsFavorite = pw.IsFavorite;
                }
            }
        }
    }
}