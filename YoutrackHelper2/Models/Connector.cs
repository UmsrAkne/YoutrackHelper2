using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YouTrackSharp;

namespace YoutrackHelper2.Models
{
    public class Connector
    {
        public Connector(string url, string token)
        {
            Connection = new BearerTokenConnection(url, token);
        }

        private BearerTokenConnection Connection { get; set; }

        public List<ProjectWrapper> ProjectWrappers { get; set; }

        public List<IssueWrapper> IssueWrappers { get; set; }

        public string ErrorMessage { get; set; }

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
                var issues = await issueService.GetIssuesInProject(projectId);
                IssueWrappers = issues.Select(s => new IssueWrapper() { Issue = s, }).ToList();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}(Connector : 46)");
                throw;
            }
        }
    }
}