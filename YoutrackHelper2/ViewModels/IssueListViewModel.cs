using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using YoutrackHelper2.Models;
using YoutrackHelper2.Models.Generics;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IssueListViewModel : BindableBase, INavigationAware
    {
        private readonly Connector connector;
        private readonly TimeCounter timeCounter = new () { TotalTimeTracking = true, };
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private bool uiEnabled = true;
        private IssueWrapper currentIssueWrapper = new ();
        private TimeSpan totalWorkingDuration = TimeSpan.Zero;

        public IssueListViewModel()
        {
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
            };
        }

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

        public TimeSpan TotalWorkingDuration
        {
            get => totalWorkingDuration;
            private set => SetProperty(ref totalWorkingDuration, value);
        }

        public AsyncDelegateCommand CreateIssueAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            var issue = CurrentIssueWrapper;
            if (issue == null || string.IsNullOrWhiteSpace(issue.Title))
            {
                return;
            }

            UiEnabled = false;
            await connector.CreateIssue(ProjectWrapper.ShortName, issue.Title, issue.Description);
            LoadIssueWrappersAsyncCommand.Execute(null);
            CurrentIssueWrapper = new IssueWrapper();
            UiEnabled = true;
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

        private List<IssueWrapper> ProgressingIssues { get; set; } = new ();

        private AsyncDelegateCommand LoadIssueWrappersAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            UiEnabled = false;
            await connector.LoadIssues(ProjectWrapper.ShortName);
            IssueWrappers = new ObservableCollection<IssueWrapper>(
                connector.IssueWrappers
                    .OrderBy(t => t.Completed)
                    .ThenByDescending(t => t.NumberInProject));
            await connector.LoadTimeTracking(IssueWrappers);

            ChangeTimerState();
            UiEnabled = true;

            RaisePropertyChanged(nameof(IssueWrappers));
        });

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
        /// メソッドを実行した際、 ProgressingIssues が更新されます。
        /// </summary>
        private void ChangeTimerState()
        {
            ProgressingIssues = IssueWrappers.Where(i => i.State == "作業中").ToList();
            if (ProgressingIssues.Count > 0)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
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
                    WorkType = "機能",
                },
                new IssueWrapper()
                {
                    Title = "２つ目のテスト課題のタイトル",
                    ShortName = "SHORTNAME-01",
                    Description = "課題の詳細説明が入力されます",
                    Comments = dummyComments,
                    Expanded = true,
                    WorkType = "バグ修正",
                },
                new IssueWrapper()
                {
                    Title = "３つ目のテスト課題のタイトル",
                    Description = "課題の説明\n 課題の説明２行目",
                    ShortName = "SHORTNAME-03",
                    Expanded = true,
                    Comments = dummyComments,
                    WorkType = "バグ修正",
                },
            });
        }
    }
}