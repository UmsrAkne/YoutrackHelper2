using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutrackHelper2.Models.Tags
{
    public interface ITagProvider
    {
        void SetConnection(string url, string tokenStr);

        Task<List<Tag>> GetTags();
    }
}