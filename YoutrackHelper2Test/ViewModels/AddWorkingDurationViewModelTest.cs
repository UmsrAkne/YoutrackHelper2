using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using YoutrackHelper2.Models;
using YoutrackHelper2.ViewModels;

namespace YoutrackHelper2Test.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [TestFixture]
    public class AddWorkingDurationViewModelTest
    {
        [Test]
        public async Task AddWorkingDurationAsyncCommandTest()
        {
            var con = new ConnectorMock();
            await con.LoadIssues("projectId");
            var issue = con.IssueWrappers.First();

            var vm = new AddWorkingDurationViewModel
            {
                Connector = con,
                CurrentIssueWrapper = issue,
                DurationText = 10.ToString(),
                TimeText = 200010010010.ToString(),
                TimeRangeDirection = TimeRangeDirection.To,
            };

            await vm.AddWorkingDurationAsyncCommand.ExecuteAsync();
            Assert.That(issue.Comments.Count, Is.EqualTo(1));
            Assert.That(issue.Comments.First().Text, Is.EqualTo("作業時間を追加 2000/10/01 00:00 - 2000/10/01 00:10"));

            vm.TimeRangeDirection = TimeRangeDirection.From;
            vm.DurationText = 10.ToString();
            await vm.AddWorkingDurationAsyncCommand.ExecuteAsync();
            Assert.That(issue.Comments.Count, Is.EqualTo(2));
            Assert.That(issue.Comments[1].Text, Is.EqualTo("作業時間を追加 2000/10/01 00:10 - 2000/10/01 00:20"));
        }
    }
}