using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using YoutrackHelper2.Models;
using YoutrackHelper2.Projects;
using YoutrackHelper2.Utils;
using YoutrackHelper2.Views;
using YouTrackSharp;

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
            this.dialogService = dialogService;
        }

        [Obsolete("このコンストラクタは xaml のプレビューの際にだけ使用します。")]
        public ProjectListViewModel()
        {
            SetDummies();
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

        public DelegateCommand NavigationRequestCommand => new (() =>
        {
            var nea = new NavigationEventArgs(nameof(IssueList))
            {
                FavoriteProjects = Projects.Where(p => p.IsFavorite).ToList(),
            };

            NavigationRequest?.Invoke(this, nea);
            WriteJsonFile();
        });

        public DelegateCommand<ProjectWrapper> ToggleFavoriteCommand => new ((param) =>
        {
            if (param == null)
            {
                return;
            }

            param.IsFavorite = !param.IsFavorite;
            WriteJsonFile();
        });

        public DelegateCommand<ProjectWrapper> ToggleArchiveCommand => new ((param) =>
        {
            if (param == null)
            {
                return;
            }

            WriteJsonFile();
        });

        public DelegateCommand<ProjectWrapper> ChangeDefaultWorkTypeCommand => new ((param) =>
        {
            if (param == null)
            {
                return;
            }

            WriteJsonFile();
        });

        public DelegateCommand ShowTagManagementPageCommand => new (() =>
        {
            var dialogParams = new DialogParameters
            {
                { nameof(Models.Connector), Connector },
            };

            dialogService.ShowDialog(nameof(TagManagementPage), dialogParams, _ => { });
        });

        public DelegateCommand OpenLogFileCommand => new (() =>
        {
            FileService.OpenTextFile("log.txt");
        });

        public TitleBarText TitleBarText { get; set; }

        private IProjectFetcher ProjectFetcher { get; set; }

        private IConnector Connector { get; set; }

        public async Task LoadProjectsAsync()
        {
            var authInfo = await AuthInfoLoader.GetAuthInfoAsync();
            ProjectFetcher ??= new ProjectFetcher(new BearerTokenConnection(authInfo.Uri, authInfo.Perm));

            var pws = await ProjectFetcher.LoadProjects();
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

        [Conditional("DEBUG")]
        private void SetDummies()
        {
            for (var i = 0; i < 10; i++)
            {
                Projects.Add(new ProjectWrapper { FullName = $"Project {i}", });
            }
        }
    }
}