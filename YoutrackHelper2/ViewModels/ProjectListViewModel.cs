using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using YoutrackHelper2.Models;
using YoutrackHelper2.Views;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ProjectListViewModel : BindableBase
    {
        private readonly IDialogService dialogService;
        private ObservableCollection<ProjectWrapper> projects = new ();
        private ProjectWrapper selectedProject;

        public ProjectListViewModel(IConnector connector, IDialogService dialogService)
        {
            Connector = connector;

            var uri = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\uri.txt")
            .Replace("\n", string.Empty);

            var perm = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\perm.txt")
            .Replace("\n", string.Empty);

            _ = GetProjectsAsync(uri, perm);

            this.dialogService = dialogService;
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
            var nea = new NavigationEventArgs(nameof(IssueList))
            {
                FavoriteProjects = Projects.Where(p => p.IsFavorite).ToList(),
            };

            NavigationRequest?.Invoke(this, nea);
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

        public DelegateCommand<ProjectWrapper> ToggleArchiveCommand => new DelegateCommand<ProjectWrapper>((param) =>
        {
            if (param == null)
            {
                return;
            }

            WriteJsonFile();
        });

        public DelegateCommand<ProjectWrapper> ChangeDefaultWorkTypeCommand => new DelegateCommand<ProjectWrapper>((param) =>
        {
            if (param == null)
            {
                return;
            }

            WriteJsonFile();
        });

        public DelegateCommand ShowTagManagementPageCommand => new DelegateCommand(() =>
        {
            var dialogParams = new DialogParameters
            {
                { nameof(Models.Connector), Connector },
            };

            dialogService.ShowDialog(nameof(TagManagementPage), dialogParams, _ => { });
        });

        public DelegateCommand OpenLogFileCommand => new DelegateCommand(() =>
        {
            FileService.OpenTextFile("log.txt");
        });

        public TitleBarText TitleBarText { get; set; }

        private IConnector Connector { get; set; }

        private async Task GetProjectsAsync(string uri, string perm)
        {
            Connector.SetConnection(uri, perm);
            await Connector.LoadProjects();
            var pws = Connector.ProjectWrappers;
            ReadJsonFile(pws);

            var favorites = pws.Where(p => p.IsFavorite).
                OrderBy(p => p.FullName).ToList();

            var archives = pws.Where(p => p.Archived).
                OrderBy(p => p.FullName).ToList();

            var other = pws.Except(favorites).Except(archives).OrderBy(p => p.FullName);

            Projects = new ObservableCollection<ProjectWrapper>(favorites.Concat(other).Concat(archives));
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
                    p.Archived = pw.Archived;
                    p.DefaultWorkType = pw.DefaultWorkType;
                }
            }
        }
    }
}