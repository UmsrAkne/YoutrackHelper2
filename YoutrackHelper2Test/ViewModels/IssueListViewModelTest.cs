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
        public void Test()
        {
            var vm = new IssueListViewModel(null, new ConnectorMock());
        }
    }
}