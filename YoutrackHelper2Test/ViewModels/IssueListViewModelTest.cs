using NUnit.Framework;
using Prism.Mvvm;
using YoutrackHelper2.Models;
using YoutrackHelper2.ViewModels;

namespace YoutrackHelper2Test.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [TestFixture]
    public class IssueListViewModelTest : BindableBase
    {
        [Test]
        public void 連番の課題名を生成するテスト()
        {
            var vm = new IssueListViewModel(null, new ConnectorMock());
            var issue = new IssueWrapper() { Title = "連番課題[10]", };
            // vm.IssueWrappers.Add(issue);
            // vm.SelectedIssue = issue;

            Assert.That(vm.GetNumberedIssueTitle(issue.Title), Is.EqualTo("連番課題[11]"));
        }

        [Test]
        public void グローバルな連番の課題名を生成するテスト()
        {
            var vm = new IssueListViewModel(null, new ConnectorMock());
            var issue10 = new IssueWrapper() { Title = "連番課題[10]", };
            var issue11 = new IssueWrapper() { Title = "連番課題[11]", };
            vm.IssueWrappers.Add(issue10);
            vm.IssueWrappers.Add(issue11);

            Assert.That(vm.GetNumberedIssueTitle(issue10.Title, true), Is.EqualTo("連番課題[12]"));
        }
    }
}