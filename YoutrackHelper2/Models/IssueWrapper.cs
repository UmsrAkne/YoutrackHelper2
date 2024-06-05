using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    public class IssueWrapper : BindableBase
    {
        private string title = string.Empty;
        private string shortName = string.Empty;
        private Issue issue;
        private bool completed;
        private string description = string.Empty;
        private bool expanded;
        private List<Comment> comments;
        private WorkType workType = WorkType.Feature;
        private DateTime creationDateTime;
        private TimeSpan workingDuration = TimeSpan.Zero;
        private State state = State.Incomplete;
        private string temporaryComment;
        private DateTime startedAt1;
        private bool progressing;
        private List<Change> changes = new ();
        private IEnumerable<Tag> tags = new List<Tag>();
        private bool isSelected;
        private DateTime resolved;

        public Issue Issue
        {
            get => issue;
            set
            {
                if (value != null)
                {
                    Title = value.Summary;
                    ShortName = value.Id;
                    Description = value.Description;

                    WorkType = WorkTypeExtension.FromString(ValueGetter.GetString(value, "Type"));

                    State = StateExtension.FromString(ValueGetter.GetString(value, "State"));
                    Completed = State is State.Completed or State.Obsolete;

                    CreationDateTime =
                        ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(ValueGetter.GetLong(value, "created")));

                    Resolved =
                        ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(ValueGetter.GetLong(value, "resolved")));

                    NumberInProject = ValueGetter.GetLong(value, "numberInProject");
                    Progressing = State == State.Progressing;
                    if (!Expanded)
                    {
                        Expanded = Progressing;
                    }

                    Comments = value.Comments.
                        Select(c => new Comment()
                        {
                            Text = c.Text,
                            DateTime = (c.Created ?? default).ToLocalTime(),
                        })
                        .OrderByDescending(c => c.DateTime)
                        .ToList();

                    Tags = value.Tags.Select(t => new Tag()
                    {
                        Text = t.Value,
                        ParentIssueId = ShortName,
                    });
                }

                SetProperty(ref issue, value);
            }
        }

        public IEnumerable<Tag> Tags { get => tags; set => SetProperty(ref tags, value); }

        public bool Expanded { get => expanded; set => SetProperty(ref expanded, value); }

        public string Title { get => title; set => SetProperty(ref title, value); }

        public string ShortName { get => shortName; set => SetProperty(ref shortName, value); }

        public string Description { get => description; set => SetProperty(ref description, value); }

        public bool Completed { get => completed; set => SetProperty(ref completed, value); }

        public WorkType WorkType { get => workType; set => SetProperty(ref workType, value); }

        public State State
        {
            get => state;
            set
            {
                if (SetProperty(ref state, value))
                {
                    RaisePropertyChanged(nameof(Self));
                }
            }
        }

        public DateTime Resolved { get => resolved; private set => SetProperty(ref resolved, value); }

        public long NumberInProject { get; private set; }

        public DateTime StartedAt { get => startedAt1; set => SetProperty(ref startedAt1, value); }

        public List<Change> Changes
        {
            get => changes;
            set => SetProperty(ref changes, value);
        }

        public TimeSpan WorkingDuration
        {
            get => workingDuration;
            set
            {
                if (SetProperty(ref workingDuration, value))
                {
                    RaisePropertyChanged(nameof(Self));
                }
            }
        }

        public DateTime CreationDateTime
        {
            get => creationDateTime;
            private set => SetProperty(ref creationDateTime, value);
        }

        public bool Progressing
        {
            get => progressing;
            set
            {
                if (SetProperty(ref progressing, value))
                {
                    RaisePropertyChanged(nameof(Self));
                }
            }
        }

        public DelegateCommand ToggleExpandedCommand => new DelegateCommand(() =>
        {
            Expanded = !Expanded;
        });

        public List<Comment> Comments { get => comments; set => SetProperty(ref comments, value); }

        public string TemporaryComment
        {
            get => temporaryComment;
            set => SetProperty(ref temporaryComment, value);
        }

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

        /// <summary>
        /// このオブジェクト自身を取得します。以下にこのプロパティの役割を記述。
        /// このオブジェクトを IssueStatusConverter に渡すことで、状態と作業時間の表示を行っている。
        /// 変換に必要な情報が多いため、MultiValueConverter を使わないことにした。
        /// しかし、直接オブジェクトを Binding した場合、変更の通知を飛ばすことができない。
        /// (単一のオブジェクトなら可能だが、このオブジェクトはリストの要素の一つであるため無理）
        /// そこで、自身を返すプロパティを実装し、これを Binding。
        /// 状態の表示を変更する必要が出た場合は、都度別の setter で RaisePropertyChanged() を呼び出す。
        /// </summary>
        /// <value> このオブジェクト自身 </value>
        public IssueWrapper Self => this;

        /// <summary>
        /// カンマで区切られたテキストから IssueWrapper を生成します。
        /// </summary>
        /// <remarks>
        /// カンマで区切られたテキストの変換の仕様は以下です。
        /// 区切られたテキストが WorkType に変換可能なテキストであれば、 WorkType をその値にセットします。
        /// それ以外ならば、最初に出現したテキストを Title に、その次に出現したテキストを Description にセットします。
        /// Description の入力は任意です。
        /// </remarks>
        /// <param name="text">変換するテキスト</param>
        /// <returns>変換された IssueWrapper。引数がnullや空文字だった場合は、new IssueWrapper() を返します</returns>
        public static IssueWrapper ToIssueWrapper(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new IssueWrapper();
            }

            var w = new IssueWrapper();
            var texts = text.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var t in texts.Select(s => s.TrimEnd().TrimStart()))
            {
                // # から始まる文字列の要素はタグとみなす
                if (t.StartsWith("#"))
                {
                    w.Tags = t.Split("#", StringSplitOptions.RemoveEmptyEntries)
                        .Select(tt => new Tag() { Text = tt.Trim(), }).ToList();
                    continue;
                }

                if (WorkTypeExtension.CanConvert(t))
                {
                    w.WorkType = WorkTypeExtension.FromString(t);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(w.Title))
                {
                    w.Title = t;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(w.Description))
                {
                    w.Description = t;
                }
            }

            return w;
        }

        public async Task ToggleStatus(IConnector connector, TimeCounter counter)
        {
            Logger.WriteMessageToFile($"課題の状態を変更 {ShortName} 現在の状態 : {State}");

            if (State == State.Incomplete)
            {
                counter.StartTimeTracking(shortName, DateTime.Now);
            }

            var comment = string.Empty;

            if (counter.IsTrackingNameRegistered(ShortName) && State == State.Progressing)
            {
                var now = DateTime.Now;
                var duration = counter.FinishTimeTracking(shortName, now);
                var startedAt = now - duration;
                const string f = "yyyy/MM/dd HH:mm";
                comment = $"中断 作業時間 {(int)duration.TotalMinutes} min ({startedAt.ToString(f)} - {now.ToString(f)})";
                await connector.AddWorkingDuration(ShortName, (int)duration.TotalMinutes);

                await connector.LoadTimeTracking(new List<IssueWrapper>() { this, });
            }

            switch (State)
            {
                case State.Incomplete:
                    Issue = await connector.ApplyCommand(ShortName, "state 作業中", comment);
                    StartedAt = DateTime.Now;
                    return;
                case State.Progressing:
                    Issue = await connector.ApplyCommand(ShortName, "state 未完了", comment);
                    StartedAt = default;
                    break;
            }
        }

        public async Task Complete(IConnector connector, TimeCounter counter)
        {
            Logger.WriteMessageToFile($"課題を完了 {ShortName}");
            var comment = string.Empty;

            if (counter.IsTrackingNameRegistered(ShortName))
            {
                var now = DateTime.Now;
                var duration = counter.FinishTimeTracking(shortName, now);
                var startedAt = now - duration;
                const string f = "yyyy/MM/dd HH:mm";
                comment = $"完了 作業時間 {(int)duration.TotalMinutes} min ({startedAt.ToString(f)} - {now.ToString(f)})";
                await connector.AddWorkingDuration(ShortName, (int)duration.TotalMinutes);
            }

            Issue = await connector.ApplyCommand(ShortName, "state 完了", comment);
            StartedAt = DateTime.MinValue;
        }

        public async Task ToIncomplete(IConnector connector)
        {
            Logger.WriteMessageToFile($"課題を未完了に戻します {ShortName}");
            Issue = await connector.ApplyCommand(ShortName, "state 未完了", string.Empty);
        }

        private DateTime ConvertTime(DateTimeOffset dto)
        {
            return TimeZoneInfo.ConvertTime(dto, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).DateTime;
        }
    }
}