namespace YoutrackHelper2.Models
{
    public class Tag
    {
        public string Text { get; init; }

        public string ParentIssueId { get; init; } = string.Empty;
    }
}