using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutrackHelper2.Models.Tags;
using YouTrackSharp;
using YouTrackSharp.Issues;
using YouTrackSharp.Projects;
using YouTrackSharp.TimeTracking;

namespace YoutrackHelper2.Models
{
    public class ConnectorMock : IConnector
    {
        private List<WorkItem> timeTracks = new List<WorkItem>();

        public ConnectorMock()
        {
            DummyIssues = new List<Issue>()
            {
                new Issue
                {
                    Id = "test-1",
                    Summary = "テスト課題01",
                    Description = "テスト課題 01 の説明",
                },
                new Issue
                {
                    Id = "test-2",
                    Summary = "テスト課題02長い名前の課題テスト長い名前の課題テスト長い名前の課題テスト長い名前の課題テスト",
                    Description = "テスト課題 02 の説明",
                },
                new Issue
                {
                    Id = "test-3",
                    Summary = "テスト課題03長い名前の課題テスト長い名前の課題テスト長い名前の課題テスト長い名前の課題テスト",
                    Description = "テスト課題 03 の説明",
                },
                new Issue
                {
                    Id = "test-4",
                    Summary = "テスト課題04",
                    Description = "テスト課題 04 の説明",
                },
            };

            for (var i = 0; i < 20; i++)
            {
                var w = new Issue()
                {
                    Summary = $"{i}つ目のテスト課題のタイトル",
                    Description = "課題の説明\n 課題の説明２行目",
                    Id = $"test-{i + 5}",
                };

                w.SetField(nameof(State), new List<string>() { State.Incomplete.ToStateName(), });
                w.SetField("Type", new List<string>() { WorkType.Feature.ToWorkTypeName(), });
                w.SetField("numberInProject", 5 + i);

                DummyIssues.Add(w);
            }

            DummyIssues[0].SetField(nameof(State), new List<string>() { State.Completed.ToStateName(), });
            DummyIssues[0].SetField("Type", new List<string>() { WorkType.Feature.ToWorkTypeName(), });
            DummyIssues[0].SetField("numberInProject", 1);

            DummyIssues[1].SetField(nameof(State), new List<string>() { State.Incomplete.ToStateName(), });
            DummyIssues[1].SetField("Type", new List<string>() { WorkType.Bug.ToWorkTypeName(), });
            DummyIssues[1].SetField("numberInProject", 2);
            DummyIssues[1].Tags = new List<SubValue<string>>() { new() { Value = "Star", }, };

            for (var i = 0; i < 15; i++)
            {
                DummyIssues[1].Comments.Add(new YouTrackSharp.Issues.Comment { Text = $"{i} : コメントコメントコメントコメントコメントコメントコメントコメントコメントコメントコメントコメント", });
            }

            DummyIssues[2].SetField(nameof(State), new List<string>() { State.Obsolete.ToStateName(), });
            DummyIssues[2].SetField("Type", new List<string>() { WorkType.Test.ToWorkTypeName(), });
            DummyIssues[2].SetField("numberInProject", 3);

            DummyIssues[3].SetField(nameof(State), new List<string>() { State.Incomplete.ToStateName(), });
            DummyIssues[3].SetField("Type", new List<string>() { WorkType.Feature.ToWorkTypeName(), });
            DummyIssues[3].SetField("numberInProject", 4);

            IssueWrappers = DummyIssues.Select(i => new IssueWrapper() { Issue = i, }).ToList();
            IssueWrappers[1].Expanded = true;
        }

        public List<ProjectWrapper> ProjectWrappers { get; private set; }

        public List<IssueWrapper> IssueWrappers { get; set; }

        public ITagManager TagManager { get; set; }

        public string ErrorMessage { get; set; }

        private List<Issue> DummyIssues { get; set; }

        public void SetConnection(string uri, string token)
        {
            TagManager = new DummyTagManager();
            TagManager.SetConnection(uri, token);
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

            target.Issue ??= new Issue()
            {
                Summary = target.Title,
                Id = target.ShortName,
            };

            var issue = target.Issue;
            if (command == "comment")
            {
                issue.Comments.Add(new YouTrackSharp.Issues.Comment { Text = comment, });
            }

            issue.SetField(nameof(State), new List<string>() { target.State.ToStateName(), });
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
            IssueWrappers = DummyIssues.Select(i => new IssueWrapper() { Issue = i, }).ToList();
            AddWorkingDuration("test-3", 5);

            return Task.CompletedTask;
        }

        public Task<Issue> GetIssueAsync(string issueId)
        {
            throw new NotImplementedException();
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
            return Task.CompletedTask;
        }

        public Task CreateIssue(string projectId, string title, string description, WorkType workType)
        {
            throw new NotImplementedException();
        }

        public Task CreateIssue(string projectId, IssueWrapper iw)
        {
            return Task.CompletedTask;
        }

        public Task DeleteIssue(string issueId)
        {
            throw new NotImplementedException();
        }

        public Task<Issue> ChangeIssueState(string issueId, State state)
        {
            var issue = DummyIssues.FirstOrDefault(i => i.Id == issueId);
            if (issue == null)
            {
                throw new ArgumentException($"指定された issueId が不正です。 id:{issueId}");
            }

            issue.SetField(nameof(State), new List<string>() { State.Obsolete.ToStateName(), });
            return Task.FromResult(issue);
        }

        public Task<Issue> RemoveTagFromIssue(string issueId, string tag)
        {
            var target = DummyIssues.FirstOrDefault(issue => issue.Id == issueId);
            if (target == null)
            {
                throw new ArgumentException($"指定された issueId が不正です。 id:{issueId}");
            }

            target.Tags = target.Tags.Where(t => t.Value != tag);
            return Task.FromResult(target);
        }

        public Task<Issue> UpdateIssueTexts(string issueId, string newTitle, string newDescription)
        {
            var target = DummyIssues.FirstOrDefault(issue => issue.Id == issueId);
            if (target != null)
            {
                target.Description = newDescription;
                target.Summary = newTitle;
            }

            return Task.FromResult(target);
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

        public Task<List<Tag>> GetTags()
        {
            return TagManager.GetTags();
        }

        public async Task CreateTag(Tag tag)
        {
            await TagManager.AddTag(tag);
        }
    }
}