using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    public interface IAddWorkingDuration
    {
        AsyncDelegateCommand AddWorkingDuration { get; }

        public IssueWrapper CurrentIssueWrapper { get; set; }
    }
}