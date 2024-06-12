using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutrackHelper2.Models.Tags
{
    public class DummyTagManager : ITagManager
    {
        public void SetConnection(string url, string tokenStr)
        {
        }

        public Task<List<Tag>> GetTags()
        {
            return Task.FromResult(new List<Tag>());
        }
    }
}