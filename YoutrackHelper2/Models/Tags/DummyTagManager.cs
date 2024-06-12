using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutrackHelper2.Models.Tags
{
    public class DummyTagManager : ITagManager
    {
        private List<Tag> Tags { get; set; } = new ();

        public void SetConnection(string url, string tokenStr)
        {
        }

        public Task<List<Tag>> GetTags()
        {
            return Task.FromResult(new List<Tag>());
        }

        public Task AddTag(Tag tag)
        {
            Tags.Add(tag);
            return Task.CompletedTask;
        }
    }
}