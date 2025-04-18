using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YoutrackHelper2.Models.Tags;
using YouTrackSharp;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // DI により注入されるクラスであるため、警告を抑制する。
    public class Connector : IConnector
    {
        public Connector()
        {
        }

        public Connector(string url, string token)
        {
            Connection = new BearerTokenConnection(url, token);
            TagManager = new TagManager();
            TagManager.SetConnection(url, token);
        }

        public List<ProjectWrapper> ProjectWrappers { get; private set; }

        public List<IssueWrapper> IssueWrappers { get; private set; }

        public ITagManager TagManager { get; set; }

        public string ErrorMessage { get; set; }

        private int MaxResultCount { get; set; } = 40;

        private BearerTokenConnection Connection { get; set; }

        public void SetConnection(string uri, string token)
        {
            Connection = new BearerTokenConnection(uri, token);
            TagManager = new TagManager();
            TagManager.SetConnection(uri, token);
        }

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

        public async Task LoadIssues(string projectId)
        {
            try
            {
                var issueService = Connection.CreateIssuesService();
                var unresolvedSearchQuery = $"#Unresolved Sort by: Created desc Project:{projectId}";
                var resolvedSearchQuery = $"#Resolved Sort by: State Sort by:Created desc Project:{projectId}";

                // 取得される数を先に確認し、規定数よりも多ければ、取得数が規定数になるように takeCount の値を設定する
                int? takeCount = 40;

                var c = await issueService.GetIssueCount(unresolvedSearchQuery);
                if (c >= MaxResultCount)
                {
                    takeCount = MaxResultCount - (int)c;
                }

                var unresolved = await issueService.GetIssuesInProject(projectId, unresolvedSearchQuery, null, MaxResultCount);
                var resolved = await issueService.GetIssuesInProject(projectId, resolvedSearchQuery, null, takeCount);
                IssueWrappers = unresolved.Concat(resolved)
                    .Select(s =>
                    {
                        var issue = new IssueWrapper() { Issue = s, };
                        var etcNum = s.GetField("予測");
                        if (etcNum is { Value: not null, })
                        {
                            var val = etcNum.Value;
                            issue.EstimatedWorkTime = int.Parse(((List<string>)val).First());
                        }

                        issue.Issue = s;

                        return issue;
                    }).
                    ToList();
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
                    Tags = iw.Tags.Select(t => new SubValue<string>() { Value = t.Name, }),
                };

                var w = iw.WorkType.ToWorkTypeName();

                if (!string.IsNullOrEmpty(w))
                {
                    issue.SetField("Type", w);
                }

                if (iw.EstimatedWorkTime != 0)
                {
                    issue.SetField("予測", $"{iw.EstimatedWorkTime}m");
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

        public async Task<Issue> ChangeIssueState(string issueId, State state)
        {
            return await ApplyCommand(issueId, $"state {state.ToStateName()}", string.Empty);
        }

        public async Task<Issue> RemoveTagFromIssue(string issueId, string tag)
        {
            if (tag == "Star")
            {
                tag = "スター"; // 取得するときは英語だが、削除のリクエストのときは日本語じゃないとダメみたい
            }

            return await ApplyCommand(issueId, $"remove tag {tag}", string.Empty);
        }

        public async Task<Issue> UpdateIssueTexts(string issueId, string newTitle, string newDescription)
        {
            var issueService = Connection.CreateIssuesService();
            var issue = await issueService.GetIssue(issueId);
            issue.Summary = newTitle;
            issue.Description = newDescription;

            await issueService.UpdateIssue(issueId, issue.Summary, newDescription);
            return issue;
        }

        public async Task AddWorkingDuration(string issueId, int durationMinutes)
        {
            if (durationMinutes <= 0)
            {
                return;
            }

            await ApplyCommand(issueId, $"作業 {durationMinutes}m", string.Empty);
        }

        public async Task<List<Tag>> GetTags()
        {
            return await TagManager.GetTags();
        }

        public async Task CreateTag(Tag tag)
        {
            await TagManager.AddTag(tag);
        }
    }
}