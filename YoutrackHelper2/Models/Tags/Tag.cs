namespace YoutrackHelper2.Models.Tags
{
    public class Tag
    {
        public string Text { get; init; }

        public string ParentIssueId { get; init; } = string.Empty;
    }
}