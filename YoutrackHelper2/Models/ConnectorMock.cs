using System.Collections.Generic;
using System.Threading.Tasks;
using YouTrackSharp.Issues;
using YouTrackSharp.Projects;

namespace YoutrackHelper2.Models
{
    public class ConnectorMock : IConnector
    {
        public List<ProjectWrapper> ProjectWrappers { get; private set; }

        public List<IssueWrapper> IssueWrappers { get; }

        public string ErrorMessage { get; set; }

        public void SetConnection(string uri, string token)
        {
        }

        public Task<Issue> ApplyCommand(string shortName, string command, string comment)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadProjects()
        {
            ProjectWrappers = new List<ProjectWrapper>()
            {
                new ProjectWrapper() { Project = new Project() { Name = "TestProject0", ShortName = "TestP0", }, },
                new ProjectWrapper() { Project = new Project() { Name = "TestProject1", ShortName = "TestP1", }, },
                new ProjectWrapper() { Project = new Project() { Name = "TestProject2", ShortName = "TestP2", }, },
                new ProjectWrapper() { Project = new Project() { Name = "TestProject3", ShortName = "TestP3", }, },
                new ProjectWrapper() { Project = new Project() { Name = "TestProject4", ShortName = "TestP4", }, },
            };

            return Task.CompletedTask;
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