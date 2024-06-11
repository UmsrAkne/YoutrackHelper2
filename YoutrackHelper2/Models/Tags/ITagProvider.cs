namespace YoutrackHelper2.Models.Tags
{
    public interface ITagProvider
    {
        void SetConnection(string uri, string token);
    }
}