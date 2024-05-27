using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    public interface IConnector
    {
        List<ProjectWrapper> ProjectWrappers { get; }

        List<IssueWrapper> IssueWrappers { get; }

        string ErrorMessage { get; set; }

        void SetConnection(string uri, string token);

        Task<Issue> ApplyCommand(string shortName, string command, string comment);

        Task LoadProjects();

        Task LoadIssues(string projectId);

        /// <summary>
        /// id から Issue を検索して返します。
        /// 複数の issue を取得する処理の場合は LoadIssues(string) を使用してください。
        /// このメソッドは単一の Issue を取得するためのメソッドです。
        /// </summary>
        /// <param name="issueId">検索する issue の shortName を入力します</param>
        /// <returns>検索された Issue </returns>
        Task<Issue> GetIssueAsync(string issueId);

        Task LoadTimeTracking(IEnumerable<IssueWrapper> issues);

        /// <summary>
        /// リスト内の要素の Changes に値を入力します。
        /// </summary>
        /// <param name="issues">Changes を入力したい IssueWrapper のリスト</param>
        /// <returns>非同期操作を表すタスク</returns>
        Task LoadChangeHistory(IEnumerable<IssueWrapper> issues);

        /// <summary>
        /// 入力した課題の状態変更記録をロードし、入力します
        /// </summary>
        /// <param name="issue">状態変更記録を入力する課題</param>
        /// <exception cref="ArgumentException">IssueWrapper.Issue が null の時にスローされます</exception>
        /// <returns>非同期操作を表すタスク</returns>
        Task LoadChangeHistory(IssueWrapper issue);

        Task CreateIssue(string projectId, string title, string description, WorkType workType);

        Task CreateIssue(string projectId, IssueWrapper iw);

        Task DeleteIssue(string issueId);

        Task<Issue> ChangeIssueState(string issueId, State state);

        /// <summary>
        /// 指定した Issue に作業時間を追加します
        /// </summary>
        /// <param name="issueId">作業時間を追加する課題の id</param>
        /// <param name="durationMinutes">追加する作業時間。分単位で入力</param>
        /// <returns>非同期操作を表すタスク</returns>
        Task AddWorkingDuration(string issueId, int durationMinutes);
    }
}