using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YouTrackSharp;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    public class Connector : IConnector
    {
        public Connector(string url, string token)
        {
            Connection = new BearerTokenConnection(url, token);
        }

        public List<ProjectWrapper> ProjectWrappers { get; private set; }

        public List<IssueWrapper> IssueWrappers { get; private set; }

        public string ErrorMessage { get; set; }

        private int MaxResultCount { get; set; } = 40;

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
                Logger.WriteMessageToFile("プロジェクトのロードに失敗しました");
                Logger.WriteMessageToFile(e.ToString());
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
                var searchQuery = $"Sort by: State Sort by:Created asc Project:{projectId} and (State:UnResolved or Created:{dtFrom} .. {dtTo})";

                // 取得される数を先に確認し、規定数よりも多ければ、取得数が規定数になるように skip の値を設定する
                int? skipCount = 0;
                var c = await issueService.GetIssueCount(searchQuery);
                if (c >= MaxResultCount)
                {
                    skipCount = (int)c - MaxResultCount;
                }

                var issues = await issueService.GetIssuesInProject(projectId, searchQuery, skipCount);
                IssueWrappers = issues.Select(s => new IssueWrapper() { Issue = s, }).ToList();
            }
            catch (Exception e)
            {
                Logger.WriteMessageToFile("課題リストのロードに失敗しました");
                Logger.WriteMessageToFile(e.ToString());
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
                Logger.WriteMessageToFile("課題のロードに失敗しました");
                Logger.WriteMessageToFile(e.ToString());
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
                Logger.WriteMessageToFile("タイムトラッキング情報のロードに失敗しました");
                Logger.WriteMessageToFile(e.ToString());
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
            var issueWrappers = issues.ToList();
            if (!issueWrappers.Any())
            {
                return;
            }

            try
            {
                var issuesService = Connection.CreateIssuesService();
                foreach (var issue in issueWrappers)
                {
                    var changes = await issuesService.GetChangeHistoryForIssue(issue.ShortName);
                    issue.Changes = changes.ToList();
                }
            }
            catch (Exception e)
            {
                Logger.WriteMessageToFile("変更ログ情報のロードに失敗しました");
                Logger.WriteMessageToFile(e.ToString());
                Debug.WriteLine($"{e}(Connector : 125)");
                throw;
            }
        }

        /// <summary>
        /// 入力した課題の状態変更記録をロードし、入力します
        /// </summary>
        /// <param name="issue">状態変更記録を入力する課題</param>
        /// <exception cref="ArgumentException">IssueWrapper.Issue が null の時にスローされます</exception>
        /// <returns>非同期操作を表すタスク</returns>
        public async Task LoadChangeHistory(IssueWrapper issue)
        {
            if (issue.Issue == null)
            {
                throw new ArgumentException("IssueWrapper.Issue が null です");
            }

            try
            {
                var issuesService = Connection.CreateIssuesService();
                var changes = await issuesService.GetChangeHistoryForIssue(issue.ShortName);
                issue.Changes = changes.ToList();
            }
            catch (Exception e)
            {
                Logger.WriteMessageToFile("変更ログ情報のロードに失敗しました");
                Logger.WriteMessageToFile(e.ToString());
                Debug.WriteLine($"{e}(Connector : 152)");
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

                var w = workType.ToWorkTypeName();

                if (!string.IsNullOrEmpty(w))
                {
                    issue.SetField("Type", w);
                }

                await issuesService.CreateIssue(projectId, issue);
            }
            catch (Exception e)
            {
                Logger.WriteMessageToFile("課題の新規作成に失敗しました");
                Logger.WriteMessageToFile(e.ToString());
                Debug.WriteLine($"{e}(Connector : 94)");
                ErrorMessage = "接続に失敗しました";
            }
        }

        public async Task CreateIssue(string projectId, IssueWrapper iw)
        {
            try
            {
                var issuesService = Connection.CreateIssuesService();
                var issue = new Issue
                {
                    Summary = iw.Title,
                    Description = iw.Description,
                    Tags = iw.Tags.Select(t => new SubValue<string>() { Value = t.Text, }),
                };

                var w = iw.WorkType.ToWorkTypeName();

                if (!string.IsNullOrEmpty(w))
                {
                    issue.SetField("Type", w);
                }

                await issuesService.CreateIssue(projectId, issue);
            }
            catch (Exception e)
            {
                Logger.WriteMessageToFile("課題の新規作成に失敗しました");
                Logger.WriteMessageToFile(e.ToString());
                Debug.WriteLine($"{e}(Connector : 94)");
                ErrorMessage = "接続に失敗しました";
            }
        }

        public async Task DeleteIssue(string issueId)
        {
            Logger.WriteMessageToFile($"{issueId} を削除します");

            try
            {
                var issueService = Connection.CreateIssuesService();
                await issueService.DeleteIssue(issueId);
            }
            catch (Exception e)
            {
                Logger.WriteMessageToFile("課題の削除に失敗しました");
                Logger.WriteMessageToFile(e.ToString());
                Debug.WriteLine($"{e}(Connector : 263)");
                ErrorMessage = "課題の削除に失敗しました";
            }
        }
    }
}