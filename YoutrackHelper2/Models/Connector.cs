using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YouTrackSharp;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    public class Connector
    {
        public Connector(string url, string token)
        {
            Connection = new BearerTokenConnection(url, token);
        }

        public List<ProjectWrapper> ProjectWrappers { get; private set; }

        public List<IssueWrapper> IssueWrappers { get; private set; }

        public string ErrorMessage { get; set; }

        private BearerTokenConnection Connection { get; set; }

        public async Task LoadProjects()
        {
            try
            {
                var projectsService = Connection.CreateProjectsService();
                var projects = await projectsService.GetAccessibleProjects();
                ProjectWrappers = projects.Select(p => new ProjectWrapper() { Project = p, }).ToList();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}(Connector : 46)");
                ErrorMessage = "接続に失敗しました";
            }
        }

        public async Task LoadIssues(string projectId)
        {
            try
            {
                var issueService = Connection.CreateIssuesService();
                var dtFrom = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
                var dtTo = DateTime.Now.ToString("yyyy-MM");
                var searchQuery = $"Project:{projectId} and (State:UnResolved or Created:{dtFrom} .. {dtTo})";

                var issues = await issueService.GetIssuesInProject(projectId, searchQuery);
                IssueWrappers = issues.Select(s => new IssueWrapper() { Issue = s, }).ToList();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}(Connector : 46)");
                throw;
            }
        }

        public async Task LoadTimeTracking(IEnumerable<IssueWrapper> issues)
        {
            try
            {
                var ttService = Connection.CreateTimeTrackingService();

                foreach (var issue in issues)
                {
                    var list = await ttService.GetWorkItemsForIssue(issue.ShortName);
                    issue.WorkingDuration = TimeSpan.FromTicks(list.Sum(t => t.Duration.Ticks));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}(Connector : 46)");
                throw;
            }
        }

        public async Task CreateIssue(string projectId, string title, string description)
        {
            try
            {
                var issuesService = Connection.CreateIssuesService();
                var issue = new Issue
                {
                    Summary = title,
                    Description = description,
                };

                await issuesService.CreateIssue(projectId, issue);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}(Connector : 94)");
                ErrorMessage = "接続に失敗しました";
            }
        }
    }
}