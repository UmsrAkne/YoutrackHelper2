using System.Collections.Generic;
using System.Threading.Tasks;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    public class ConnectorMock : IConnector
    {
        public List<ProjectWrapper> ProjectWrappers { get; }

        public List<IssueWrapper> IssueWrappers { get; }

        public string ErrorMessage { get; set; }

        public Task<Issue> ApplyCommand(string shortName, string command, string comment)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadProjects()
        {
            throw new System.NotImplementedException();
        }

        public Task LoadIssues(string projectId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Issue> GetIssueAsync(string issueId)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadTimeTracking(IEnumerable<IssueWrapper> issues)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadChangeHistory(IEnumerable<IssueWrapper> issues)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadChangeHistory(IssueWrapper issue)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateIssue(string projectId, string title, string description, WorkType workType)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateIssue(string projectId, IssueWrapper iw)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteIssue(string issueId)
        {
            throw new System.NotImplementedException();
        }
    }
}