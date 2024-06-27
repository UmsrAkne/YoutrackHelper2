using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using YoutrackHelper2.Models;
using YoutrackHelper2.Models.Generics;
using YoutrackHelper2.Models.Tags;
using YoutrackHelper2.Views;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IssueListViewModel : BindableBase, INavigationAware
    {
        private readonly TimeCounter timeCounter = new () { TotalTimeTracking = true, };
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly IDialogService dialogService;
        private bool uiEnabled = true;
        private IssueWrapper currentIssueWrapper = new ();
        private TimeSpan totalWorkingDuration = TimeSpan.Zero;
        private IssueWrapper selectedIssue;
        private string tagText = "#";
        private string commandText;
        private ProjectWrapper projectWrapper;
        private List<ProjectWrapper> favoriteProjects;

        public IssueListViewModel(IDialogService dialogService, IConnector connector)
        {
            this.dialogService = dialogService;

            var uri = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\uri.txt")
            .Replace("\n", string.Empty);

            var perm = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\perm.txt")
            .Replace("\n", string.Empty);

            Connector = connector;
            Connector.SetConnection(uri, perm);

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (_, _) =>
            {
                TotalWorkingDuration = timeCounter.GetTotalWorkingDuration(DateTime.Now);
                if (ProgressingIssues.Count != 0)
                {
                    foreach (var progressingIssue in ProgressingIssues)
                    {
                        progressingIssue.WorkingDuration =
                        timeCounter.GetWorkingDuration(progressingIssue.ShortName, DateTime.Now);
                    }
                }

                if (ProgressingIssues.Count == 1)
                {
                    TitleBarText.CurrentWorkingDuration =
                        timeCounter.GetWorkingDuration(ProgressingIssues.FirstOrDefault()!.ShortName, DateTime.Now);
                }
            };
        }

        // ReSharper disable once UnusedMember.Global
        // デザイン確認用に呼び出される。デバッグ・リリース共に、通常のビルド時には呼び出されない。
        public IssueListViewModel()
        {
            InjectDummies();
        }

        public event EventHandler NavigationRequest;

        public ProjectWrapper ProjectWrapper
        {
            get => projectWrapper;
            set
            {
                projectWrapper = value;
                if (value != null)
                {
                    CurrentIssueWrapper.WorkType = projectWrapper.DefaultWorkType;
                }
            }
        }

        public ObservableCollection<IssueWrapper> IssueWrappers { get; set; } = new ();

        /// <summary>
        /// 課題情報入力欄のテキストを Binding して保持するためのプロパティです。
        /// </summary>
        /// <value>
        /// 課題情報入力欄の情報を保持する IssueWrapper
        /// </value>
        public IssueWrapper CurrentIssueWrapper
        {
            get => currentIssueWrapper;
            private set => SetProperty(ref currentIssueWrapper, value);
        }

        public string CommandText { get => commandText; set => SetProperty(ref commandText, value); }

        public bool UiEnabled { get => uiEnabled; set => SetProperty(ref uiEnabled, value); }

        public IssueWrapper SelectedIssue
        {
            get => selectedIssue;
            set
            {
                if (SetProperty(ref selectedIssue, value))
                {
                    RaisePropertyChanged(nameof(SelectedItemIsNotNull));
                }
            }
        }

        public bool SelectedItemIsNotNull => SelectedIssue != null;

        public TimeSpan TotalWorkingDuration
        {
            get => totalWorkingDuration;
            private set => SetProperty(ref totalWorkingDuration, value);
        }

        public string TagText { get => tagText; set => SetProperty(ref tagText, value); }

        public IConnector Connector { get; set; }

        public AsyncDelegateCommand<TextBox> CreateIssueAsyncCommand => new AsyncDelegateCommand<TextBox>(async (textBox) =>
        {
            var issue = CurrentIssueWrapper;
            if (issue == null || string.IsNullOrWhiteSpace(issue.Title))
            {
                return;
            }

            Logger.WriteMessageToFile("課題を新規作成します");
            Logger.WriteIssueInfoToFile(CurrentIssueWrapper);

            UiEnabled = false;

            if (!string.IsNullOrWhiteSpace(TagText) && TagText.Contains('#'))
            {
                issue.Tags = TagText
                    .Split("#", StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => new Tag() { Name = t.Trim(), });
            }

            await Connector.CreateIssue(ProjectWrapper.ShortName, issue);
            await LoadIssueWrappersAsyncCommand.ExecuteAsync();
            CurrentIssueWrapper = new IssueWrapper() { WorkType = ProjectWrapper.DefaultWorkType, };
            UiEnabled = true;

            textBox?.Focus();
        });

        public AsyncDelegateCommand<TextBox> CreateIssueFromStrAsyncCommand => new AsyncDelegateCommand<TextBox>(async (param) =>
        {
            if (string.IsNullOrWhiteSpace(CommandText))
            {
                return;
            }

            var iw = !CommandText.Contains(',')
                ? new IssueWrapper() { Title = CommandText.Trim(), }
                : IssueWrapper.ToIssueWrapper(CommandText);

            Logger.WriteMessageToFile("課題をコマンドの文字列から新規作成します");
            Logger.WriteIssueInfoToFile(CurrentIssueWrapper);

            UiEnabled = false;

            await Connector.CreateIssue(ProjectWrapper.ShortName, iw);
            await LoadIssueWrappersAsyncCommand.ExecuteAsync();
            CurrentIssueWrapper = new IssueWrapper();
            CommandText = string.Empty;
            UiEnabled = true;

            param?.Focus();
        });

        public DelegateCommand CreateNumberedIssueCommand => new DelegateCommand(() =>
        {
            if (SelectedIssue == null)
            {
                return;
            }

            var issueTitle = SelectedIssue.Title;
            const string pattern = @"^(.+)\[(\d{2})\]$";
            var match = Regex.Match(issueTitle, pattern);

            // 連番課題のフォーマットに沿ったタイトルであるかを確認し、フォーマット通りであれば連番をインクリメントする。
            if (match.Success)
            {
                var prefix = match.Groups[1].Value;
                var numberString = match.Groups[2].Value;

                // 二桁の数値を取得して+1
                var number = int.Parse(numberString);
                number++;

                CurrentIssueWrapper.Title = $"{prefix}[{number:00}]";
            }
            else
            {
                CurrentIssueWrapper.Title = SelectedIssue.Title;
            }

            CurrentIssueWrapper.Description = SelectedIssue.Description;
            CurrentIssueWrapper.WorkType = SelectedIssue.WorkType;
        });

        public AsyncDelegateCommand<IssueWrapper> CompleteIssueCommand => new AsyncDelegateCommand<IssueWrapper>(async (param) =>
        {
            if (param == null)
            {
                return;
            }

            UiEnabled = false;
            await param.Complete(Connector, timeCounter);
            param.Issue = await Connector.RemoveTagFromIssue(param.ShortName, "スター");
            ChangeTimerState();
            UiEnabled = true;
        });

        public AsyncDelegateCommand RevertToIncompleteAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            if (!IssueWrappers.Any(iw => iw.IsSelected))
            {
                return;
            }

            UiEnabled = false;

            var candidates = IssueWrappers
                .Where(iw => iw.IsSelected)
                .Where(iw => iw.Completed)
                .ToList();

            foreach (var issueWrapper in candidates)
            {
                await issueWrapper.ToIncomplete(Connector);
            }

            if (candidates.Count != 0)
            {
                LoadIssueWrappersAsyncCommand.Execute(null);
            }

            UiEnabled = true;
        });

        public AsyncDelegateCommand CompleteIssueListCommand => new AsyncDelegateCommand(async () =>
        {
            if (!IssueWrappers.Any(iw => iw.IsSelected))
            {
                return;
            }

            UiEnabled = false;
            foreach (var issueWrapper in IssueWrappers.Where(iw => iw.IsSelected))
            {
                await issueWrapper.Complete(Connector, timeCounter);
            }

            ChangeTimerState();
            UiEnabled = true;
        });

        public AsyncDelegateCommand DeleteIssueListAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            if (!IssueWrappers.Any(iw => iw.IsSelected))
            {
                return;
            }

            UiEnabled = false;
            foreach (var issueWrapper in IssueWrappers.Where(iw => iw.IsSelected))
            {
                await Connector.DeleteIssue(issueWrapper.ShortName);
            }

            LoadIssueWrappersAsyncCommand.Execute(null);
            UiEnabled = true;
        });

        public AsyncDelegateCommand<IssueWrapper> ToggleIssueStateCommand => new AsyncDelegateCommand<IssueWrapper>(async (param) =>
        {
            if (param is { Completed: true, })
            {
                return;
            }

            UiEnabled = false;
            await param.ToggleStatus(Connector, timeCounter);
            ChangeTimerState();
            UiEnabled = true;
        });

        public AsyncDelegateCommand<TextBox> PostCommentCommand => new (async (param) =>
        {
            if (param?.DataContext is not IssueWrapper iw || string.IsNullOrWhiteSpace(iw.TemporaryComment))
            {
                return;
            }

            Logger.WriteMessageToFile($"コメントを投稿します {iw.TemporaryComment}");

            UiEnabled = false;
            await Connector.ApplyCommand(iw.ShortName, "comment", iw.TemporaryComment);
            iw.Issue = await Connector.GetIssueAsync(iw.ShortName);
            iw.TemporaryComment = string.Empty;
            UiEnabled = true;

            param.Focus();
        });

        public TitleBarText TitleBarText { get; set; } = new TitleBarText();

        public DelegateCommand<IssueWrapper> CopyIssueTitleCommand => new ((param) =>
        {
            if (param == null)
            {
                return;
            }

            Clipboard.SetText(param.Title);
        });

        public DelegateCommand ShowIssuesPostPageCommand => new DelegateCommand(() =>
        {
            var dialogParams = new DialogParameters
            {
                { nameof(ProjectWrapper), ProjectWrapper },
                { nameof(Models.Connector), Connector },
            };

            dialogService.ShowDialog(nameof(IssuesPostPage), dialogParams, result =>
            {
                if (result.Parameters.GetValue<bool>(nameof(IssuesPostPageViewModel.IssuePosted)))
                {
                    LoadIssueWrappersAsyncCommand.Execute(null);
                }
            });
        });

        public DelegateCommand<IssueWrapper> ShowIssueDetailPageCommand => new DelegateCommand<IssueWrapper>((param) =>
        {
            if (param == null)
            {
                return;
            }

            var dialogParams = new DialogParameters
            {
                { nameof(IssueWrapper), param },
                { nameof(Models.Connector), Connector },
            };

            dialogService.ShowDialog(nameof(IssueDetailPage), dialogParams, _ =>
            {
                LoadIssueWrappersAsyncCommand.Execute(null);
            });
        });

        public DelegateCommand InputIssueInfoCommand => new DelegateCommand(() =>
        {
            if (SelectedIssue == null)
            {
                return;
            }

            CurrentIssueWrapper.Title = SelectedIssue.Title;
            CurrentIssueWrapper.Description = SelectedIssue.Description;
            CurrentIssueWrapper.WorkType = SelectedIssue.WorkType;
        });

        public DelegateCommand NavigateToProjectListCommand => new DelegateCommand(() =>
        {
            NavigationRequest?.Invoke(this, new NavigationEventArgs(nameof(ProjectList)));
            IssueWrappers.Clear();
        });

        public bool Initialized { get; set; }

        public AsyncDelegateCommand LoadIssueWrappersAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            UiEnabled = false;
            await Connector.LoadIssues(ProjectWrapper.ShortName);
            IssueWrappers = new ObservableCollection<IssueWrapper>(
                Connector.IssueWrappers
                    .OrderBy(t => t.Completed)
                    .ThenByDescending(t => t.NumberInProject));
            await Connector.LoadTimeTracking(IssueWrappers.Where(w => !w.Completed));
            await Connector.LoadChangeHistory(IssueWrappers.Where(w => w.Expanded));

            ChangeTimerState();
            RaisePropertyChanged(nameof(IssueWrappers));

            UiEnabled = true;
        });

        public AsyncDelegateCommand<IssueWrapper> LoadChangeHistoriesAsyncCommand => new AsyncDelegateCommand<IssueWrapper>(async (param) =>
        {
            UiEnabled = false;
            await Connector.LoadChangeHistory(param);
            UiEnabled = true;
        });

        public DelegateCommand CollapsedAllIssueCommand => new DelegateCommand(() =>
        {
            foreach (var iw in IssueWrappers.Where(i => i.Expanded))
            {
                iw.Expanded = false;
            }
        });

        public AsyncDelegateCommand ToObsoleteCommand => new AsyncDelegateCommand(async () =>
        {
            if (SelectedIssue == null)
            {
                return;
            }

            UiEnabled = false;
            SelectedIssue.Issue = await Connector.ChangeIssueState(SelectedIssue.ShortName, State.Obsolete);
            UiEnabled = true;
        });

        public AsyncDelegateCommand<Tag> RemoveTagAsyncCommand => new AsyncDelegateCommand<Tag>(async (param) =>
        {
            var iw = IssueWrappers.FirstOrDefault(w => w.ShortName == param.ParentIssueId);
            if (iw == null)
            {
                return;
            }

            iw.Issue = await Connector.RemoveTagFromIssue(iw.ShortName, param.Name);
        });

        public DelegateCommand<ProjectWrapper> ChangeProjectCommand => new DelegateCommand<ProjectWrapper>( (param) =>
        {
            if (param == null)
            {
                return;
            }

            ProjectWrapper = param;
            LoadIssueWrappersAsyncCommand.Execute(null);
            TitleBarText.Text = param.FullName;
        });

        public List<ProjectWrapper> FavoriteProjects
        {
            get => favoriteProjects;
            set => SetProperty(ref favoriteProjects, value);
        }

        private List<IssueWrapper> ProgressingIssues { get; set; } = new ();

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!navigationContext.Parameters.TryGetValue(nameof(ProjectWrapper), out ProjectWrapper parameterValue))
            {
                return;
            }

            if (parameterValue != null)
            {
                ProjectWrapper = parameterValue;
                LoadIssueWrappersAsyncCommand.Execute(null);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// 現在作業中の課題がプロジェクト内に存在すれば timer を On に。そうでなければ Off に設定します。
        /// メソッドを実行した際、 ProgressingIssues と、 TitleBarText.Progressing が更新されます。
        /// </summary>
        private void ChangeTimerState()
        {
            ProgressingIssues = IssueWrappers.Where(i => i.State == State.Progressing).ToList();
            ProgressingIssues.ForEach(i => i.Progressing = true);
            if (ProgressingIssues.Count > 0)
            {
                timer.Start();
                TitleBarText.Progressing = true;
            }
            else
            {
                timer.Stop();
                TitleBarText.Progressing = false;
            }
        }

        [Conditional("DEBUG")]
        private void InjectDummies()
        {
            IssueWrappers = new ObservableCollection<IssueWrapper>(
                new ConnectorMock().IssueWrappers);
        }
    }
}