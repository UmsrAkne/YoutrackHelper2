using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTrackSharp.Issues;
using YouTrackSharp.Projects;
using YouTrackSharp.TimeTracking;

namespace YoutrackHelper2.Models
{
    public class ConnectorMock : IConnector
    {
        private List<WorkItem> timeTracks = new List<WorkItem>();

        public List<ProjectWrapper> ProjectWrappers { get; private set; }

        public List<IssueWrapper> IssueWrappers { get; set; }

        public string ErrorMessage { get; set; }

        public void SetConnection(string uri, string token)
        {
        }

        public Task<Issue> ApplyCommand(string shortName, string command, string comment)
        {
            var target = IssueWrappers.FirstOrDefault(iw => iw.ShortName == shortName);
            if (target == null)
            {
                return null;
            }

            if (command == "state 作業中")
            {
                target.State = State.Progressing;
            }

            if (command == "state 未完了")
            {
                target.State = State.Incomplete;
            }

            if (command == "state 完了")
            {
                target.State = State.Completed;
                target.Completed = true;
            }

            var issue = new Issue()
            {
                Summary = target.Title,
                Id = target.ShortName,
            };

            issue.SetField(nameof(State), new List<string>() { target.State.ToStateName() });
            return Task.FromResult(issue);
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
            IssueWrappers = new List<IssueWrapper>()
            {
                new()
                {
                    Title = "テスト課題タイトル1",
                    ShortName = "ti-1",
                    Completed = false,
                    Description = "課題１の説明",
                    WorkType = WorkType.Feature,
                    WorkingDuration = default,
                    State = State.Incomplete,
                    Progressing = false,
                    Changes = null,
                    Tags = null,
                    NumberInProject = 1,
                },

                new()
                {
                    Title = "テスト課題タイトル2",
                    ShortName = "ti-2",
                    Completed = true,
                    Description = "課題2の説明",
                    WorkType = WorkType.Feature,
                    WorkingDuration = default,
                    State = State.Completed,
                    Progressing = false,
                    Changes = null,
                    Tags = null,
                    NumberInProject = 2,
                },

                new()
                {
                    Title = "テスト課題タイトル3 バグ ５分間作業済み",
                    ShortName = "ti-3",
                    Completed = false,
                    Description = "課題3の説明 バグの説明",
                    WorkType = WorkType.Bug,
                    WorkingDuration = default,
                    State = State.Incomplete,
                    Progressing = false,
                    Changes = null,
                    Tags = new List<Tag>() { new Tag() { Text = "Star", }, },
                    NumberInProject = 3,
                },
            };

            AddWorkingDuration("ti-3", 5);

            return Task.CompletedTask;
        }

        public Task<Issue> GetIssueAsync(string issueId)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadTimeTracking(IEnumerable<IssueWrapper> issues)
        {
            foreach (var issueWrapper in issues)
            {
                var wi = timeTracks.Where(iw => iw.Id == issueWrapper.ShortName);
                var d = TimeSpan.FromTicks(wi.Sum(w => w.Duration.Ticks));
                issueWrapper.WorkingDuration = issueWrapper.WorkingDuration.Add(d);
            }

            return Task.CompletedTask;
        }

        public Task LoadChangeHistory(IEnumerable<IssueWrapper> issues)
        {
            return Task.CompletedTask;
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

        public Task<Issue> ChangeIssueState(string issueId, State state)
        {
            var issue = new Issue();
            issue.SetField(nameof(State), new List<string>() { State.Obsolete.ToStateName(), });
            return Task.FromResult(issue);
        }

        public Task AddWorkingDuration(string issueId, int durationMinutes)
        {
            timeTracks.Add(new WorkItem
            {
                Id = issueId,
                Date = DateTime.Now,
                Duration = TimeSpan.FromMinutes(durationMinutes + 1),
                Created = DateTime.Now,
            });

            return Task.CompletedTask;
        }
    }
}