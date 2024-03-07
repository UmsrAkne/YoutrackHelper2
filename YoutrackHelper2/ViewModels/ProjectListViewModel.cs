using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Prism.Mvvm;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ProjectListViewModel : BindableBase
    {
        private ObservableCollection<ProjectWrapper> projects = new ();

        public ProjectListViewModel()
        {
            var uri = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\uri.txt")
            .Replace("\n", string.Empty);

            var perm = File.ReadAllText(
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\perm.txt")
            .Replace("\n", string.Empty);

            _ = GetProjectsAsync(uri, perm);
        }

        public ObservableCollection<ProjectWrapper> Projects
        {
            get => projects;
            private set => SetProperty(ref projects, value);
        }

        private Connector Connector { get; set; }

        private async Task GetProjectsAsync(string uri, string perm)
        {
            Connector = new Connector(uri, perm);
            await Connector.LoadProjects();
            Projects = new ObservableCollection<ProjectWrapper>(Connector.ProjectWrappers);

            // foreach (var p in ps)
            // {
            //     var issueList = await Connector.GetIssues(p.Name);
            //     p.IssueCount = issueList.Count;
            //     p.IncompleteIssueCount = issueList.Count(i => !new IssueWrapper(i).Completed);
            // }
        }
    }
}