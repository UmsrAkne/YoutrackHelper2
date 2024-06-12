using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutrackHelper2.Models.Tags
{
    public class DummyTagManager : ITagManager
    {
        private List<Tag> Tags { get; set; } = new ()
        {
            new Tag() { Name = "testTag1", },
            new Tag() { Name = "testTag2", },
            new Tag() { Name = "testTag3", },
            new Tag() { Name = "testTag4", },
            new Tag() { Name = "testTag5", },
        };

        public void SetConnection(string url, string tokenStr)
        {
        }

        public Task<List<Tag>> GetTags()
        {
            return Task.FromResult(Tags);
        }

        public Task AddTag(Tag tag)
        {
            Tags.Add(tag);
            return Task.CompletedTask;
        }
    }
}