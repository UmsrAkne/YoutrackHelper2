using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutrackHelper2.Models.Tags
{
    public interface ITagManager
    {
        void SetConnection(string url, string tokenStr);

        Task<List<Tag>> GetTags();

        Task AddTag(Tag tag);
    }
}