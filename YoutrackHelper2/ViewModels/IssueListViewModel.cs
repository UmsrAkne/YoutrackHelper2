using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Prism.Mvvm;
using Prism.Regions;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IssueListViewModel : BindableBase, INavigationAware
    {
        private readonly Connector connector;
        private bool uiEnabled = true;

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
        }

        public string ProjectName { get; private set; }

        public ObservableCollection<IssueWrapper> IssueWrappers { get; set; } = new ();

        public bool UiEnabled { get => uiEnabled; set => SetProperty(ref uiEnabled, value); }

        public AsyncDelegateCommand LoadIssueWrappersAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            UiEnabled = false;
            await connector.LoadIssues(ProjectName);
            IssueWrappers = new ObservableCollection<IssueWrapper>(
                connector.IssueWrappers
                    .OrderBy(t => t.Completed)
                    .ThenByDescending(t => t.CreationDateTime));
            await connector.LoadTimeTracking(IssueWrappers);

            UiEnabled = true;

            RaisePropertyChanged(nameof(IssueWrappers));
        });

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!navigationContext.Parameters.TryGetValue(nameof(ProjectName), out string parameterValue))
            {
                return;
            }

            if (parameterValue != null)
            {
                ProjectName = parameterValue;
                LoadIssueWrappersAsyncCommand.Execute(null);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
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