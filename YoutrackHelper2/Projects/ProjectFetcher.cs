using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutrackHelper2.Models;
using YouTrackSharp;

namespace YoutrackHelper2.Projects
{
    public class ProjectFetcher : IProjectFetcher
    {
        public ProjectFetcher(BearerTokenConnection connection)
        {
            Connection = connection;
        }

        private BearerTokenConnection Connection { get; set; }

        /// <summary>
        /// サーバーからプロジェクトをロードし、それをラップして返す非同期メソッドです。
        /// </summary>
        /// <returns>ロードした `Project` をラップした `ProjectWrapper` のリストを返します。</returns>
        public async Task<List<ProjectWrapper>> LoadProjects()
        {
            try
            {
                var projectsService = Connection.CreateProjectsService();
                var projects = await projectsService.GetAccessibleProjects();
                return projects.Select(p => new ProjectWrapper() { Project = p, }).ToList();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"プロジェクトサービスへの接続に失敗(ProjectFetcher : 26)");
                Logger.WriteMessageToFile("プロジェクトのロードに失敗しました");
                Logger.WriteMessageToFile(e.ToString());
            }

            return new List<ProjectWrapper>();
        }
    }
}