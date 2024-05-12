using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using YoutrackHelper2.Models;
using YoutrackHelper2.Models.Generics;
using YoutrackHelper2.Views;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IssueListViewModel : BindableBase, INavigationAware
    {
        private readonly Connector connector;
        private readonly TimeCounter timeCounter = new () { TotalTimeTracking = true, };
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly IDialogService dialogService;
        private bool uiEnabled = true;
        private IssueWrapper currentIssueWrapper = new ();
        private TimeSpan totalWorkingDuration = TimeSpan.Zero;
        private IssueWrapper selectedIssue;

        public IssueListViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            InjectDummies(); // Debugビルドの場合のみ実行される。ダミーの値をリストに入力する。

            var uri = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\uri.txt")
            .Replace("\n", string.Empty);

            var perm = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\perm.txt")
            .Replace("\n", string.Empty);

            connector = new Connector(uri, perm);
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
            };
        }

        public event EventHandler NavigationRequest;

        public ProjectWrapper ProjectWrapper { get; set; }

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
            set => SetProperty(ref currentIssueWrapper, value);
        }

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

        public AsyncDelegateCommand<TextBox> CreateIssueAsyncCommand => new AsyncDelegateCommand<TextBox>(async (textBox) =>
        {
            var issue = CurrentIssueWrapper;
            if (issue == null || string.IsNullOrWhiteSpace(issue.Title))
            {
                return;
            }

            UiEnabled = false;
            await connector.CreateIssue(
                ProjectWrapper.ShortName, issue.Title, issue.Description, CurrentIssueWrapper.WorkType);

            await LoadIssueWrappersAsyncCommand.ExecuteAsync();
            CurrentIssueWrapper = new IssueWrapper();
            UiEnabled = true;

            textBox?.Focus();
        });

        public AsyncDelegateCommand<IssueWrapper> CompleteIssueCommand => new AsyncDelegateCommand<IssueWrapper>(async (param) =>
        {
            if (param is { Completed: false, })
            {
                return;
            }

            UiEnabled = false;
            await param.Complete(connector, timeCounter);
            ChangeTimerState();
            UiEnabled = true;
        });

        public AsyncDelegateCommand<IssueWrapper> ToggleIssueStateCommand => new AsyncDelegateCommand<IssueWrapper>(async (param) =>
        {
            if (param is { Completed: true, })
            {
                return;
            }

            UiEnabled = false;
            await param.ToggleStatus(connector, timeCounter);
            ChangeTimerState();
            UiEnabled = true;
        });

        public AsyncDelegateCommand<IssueWrapper> PostCommentCommand => new (async (param) =>
        {
            if (param == null || string.IsNullOrWhiteSpace(param.TemporaryComment))
            {
                return;
            }

            UiEnabled = false;
            await connector.ApplyCommand(param.ShortName, "comment", param.TemporaryComment);
            param.Issue = await connector.GetIssueAsync(param.ShortName);
            param.TemporaryComment = string.Empty;
            UiEnabled = true;
        });

        public TitleBarText TitleBarText { get; set; }

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
                { nameof(Connector), connector },
            };

            dialogService.ShowDialog(nameof(IssuesPostPage), dialogParams, result =>
            {
                if (result.Parameters.GetValue<bool>(nameof(IssuesPostPageViewModel.IssuePosted)))
                {
                    LoadIssueWrappersAsyncCommand.Execute(null);
                }
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
        });

        public DelegateCommand NavigateToProjectListCommand => new DelegateCommand(() =>
        {
            NavigationRequest?.Invoke(this, new NavigationEventArgs(nameof(ProjectList)));
        });

        public bool Initialized { get; set; }

        public AsyncDelegateCommand LoadIssueWrappersAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            UiEnabled = false;
            await connector.LoadIssues(ProjectWrapper.ShortName);
            IssueWrappers = new ObservableCollection<IssueWrapper>(
                connector.IssueWrappers
                    .OrderBy(t => t.Completed)
                    .ThenByDescending(t => t.NumberInProject));
            await connector.LoadTimeTracking(IssueWrappers);

            ChangeTimerState();
            RaisePropertyChanged(nameof(IssueWrappers));

            UiEnabled = true;
        });

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
            ProgressingIssues = IssueWrappers.Where(i => i.State == "作業中").ToList();
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
            var dummyComments = new List<Comment>()
            {
                new () { Text = "テストコメントテストコメント", DateTime = new DateTime(2024, 4, 28, 12, 0, 0), },
                new () { Text = "テストコメントテストコメント\n改行コメント", DateTime = new DateTime(2024, 4, 28, 13, 0, 0), },
                new () { Text = "テストコメントテストコメント", DateTime = new DateTime(2024, 4, 28, 14, 0, 0), },
            };

            IssueWrappers.AddRange(new[]
            {
                new IssueWrapper()
                {
                    Title = "テスト課題のタイトル",
                    ShortName = "SHORTNAME-01",
                    Description = "課題の詳細説明が入力されます",
                    WorkType = WorkType.Feature,
                },
                new IssueWrapper()
                {
                    Title = "２つ目のテスト課題のタイトル",
                    ShortName = "SHORTNAME-01",
                    Description = "課題の詳細説明が入力されます",
                    Comments = dummyComments,
                    Expanded = true,
                    WorkType = WorkType.Bug,
                },
                new IssueWrapper()
                {
                    Title = "３つ目のテスト課題のタイトル",
                    Description = "課題の説明\n 課題の説明２行目",
                    ShortName = "SHORTNAME-03",
                    Expanded = true,
                    Comments = dummyComments,
                    WorkType = WorkType.Bug,
                },
            });
        }
    }
}