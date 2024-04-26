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
            IssueWrappers.AddRange(new[]
            {
                new IssueWrapper()
                {
                    Title = "テスト課題のタイトル",
                    ShortName = "SHORTNAME-01",
                },
                new IssueWrapper()
                {
                    Title = "２つ目のテスト課題のタイトル",
                    ShortName = "SHORTNAME-02",
                },
            });
        }
    }
}