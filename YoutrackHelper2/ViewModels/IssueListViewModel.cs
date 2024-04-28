using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Prism.Mvvm;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IssueListViewModel : BindableBase
    {
        public IssueListViewModel()
        {
            InjectDummies(); // Debugビルドの場合のみ実行される。ダミーの値をリストに入力する。
        }

        public ObservableCollection<IssueWrapper> IssueWrappers { get; set; } = new ();

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
                    ShortName = "SHORTNAME-03",
                    Expanded = true,
                    Comments = dummyComments,
                    WorkType = "バグ修正",
                },
            });
        }
    }
}