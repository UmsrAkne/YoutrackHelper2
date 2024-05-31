namespace YoutrackHelper2.Models
{
    public class Tag
    {
        public string Text { get; set; }

        public string ParentIssueId { get; set; } = string.Empty;
    }
}