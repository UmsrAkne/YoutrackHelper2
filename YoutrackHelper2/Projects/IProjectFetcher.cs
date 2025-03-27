using System.Collections.Generic;
using System.Threading.Tasks;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.Projects
{
    public interface IProjectFetcher
    {
        public Task<List<ProjectWrapper>> LoadProjects();
    }
}