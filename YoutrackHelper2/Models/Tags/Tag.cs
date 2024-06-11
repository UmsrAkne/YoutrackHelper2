namespace YoutrackHelper2.Models.Tags
{
    public class Tag
    {
        public string Name { get; init; }

        public string ParentIssueId { get; init; } = string.Empty;
    }
}