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

        public async Task<Issue> ApplyCommand(string shortName, string command, string comment)
        {
            var issueService = Connection.CreateIssuesService();
            if (string.IsNullOrWhiteSpace(comment))
            {
                await issueService.ApplyCommand(shortName, command);
            }
            else
            {
                await issueService.ApplyCommand(shortName, command, comment);
            }

            return await issueService.GetIssue(shortName);
        }

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

        /// <summary>
        /// id から Issue を検索して返します。
        /// 複数の issue を取得する処理の場合は LoadIssues(string) を使用してください。
        /// このメソッドは単一の Issue を取得するためのメソッドです。
        /// </summary>
        /// <param name="issueId">検索する issue の shortName を入力します</param>
        /// <returns>検索された Issue </returns>
        public async Task<Issue> GetIssueAsync(string issueId)
        {
            try
            {
                var issueService = Connection.CreateIssuesService();
                var issue = await issueService.GetIssue(issueId);
                return issue;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}(Connector : 84)");
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

        /// <summary>
        /// リスト内の要素の Changes に値を入力します。
        /// </summary>
        /// <param name="issues">Changes を入力したい IssueWrapper のリスト</param>
        /// <returns>非同期操作を表すタスク</returns>
        public async Task LoadChangeHistory(IEnumerable<IssueWrapper> issues)
        {
            try
            {
                var issuesService = Connection.CreateIssuesService();
                foreach (var issue in issues)
                {
                    var changes = await issuesService.GetChangeHistoryForIssue(issue.ShortName);
                    issue.Changes = changes.ToList();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}(Connector : 125)");
                throw;
            }
        }

        public async Task CreateIssue(string projectId, string title, string description, WorkType workType)
        {
            try
            {
                var issuesService = Connection.CreateIssuesService();
                var issue = new Issue
                {
                    Summary = title,
                    Description = description,
                };

                var w = IssueWrapper.ConvertWorkType(workType);

                if (!string.IsNullOrEmpty(w))
                {
                    issue.SetField("Type", w);
                }

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