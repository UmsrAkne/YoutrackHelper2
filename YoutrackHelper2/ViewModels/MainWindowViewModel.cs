using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using YoutrackHelper2.Models;
using YoutrackHelper2.Views;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainWindowViewModel : BindableBase
    {
        private const string RegionName = "ContentRegion";
        private static bool initialized;
        private readonly IRegionManager regionManager;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public string Title => "Prism Application";

        public DelegateCommand<ProjectWrapper> NavigateToIssueListPageCommand => new ((param) =>
        {
            var parameters = new NavigationParameters
            {
                { nameof(IssueListViewModel.ProjectWrapper), param },
            };
            regionManager.RequestNavigate(RegionName, nameof(IssueList), parameters);
        });

        public DelegateCommand NavigateToProjectListPageCommand => new (() =>
        {
            regionManager.RequestNavigate(RegionName, nameof(ProjectList));
        });

        public DelegateCommand AppInitializeCommand => new (() =>
        {
            if (initialized)
            {
                return;
            }

            regionManager.RequestNavigate(RegionName, nameof(ProjectList));

            initialized = true;
            var projectsView = regionManager.Regions[RegionName].ActiveViews.FirstOrDefault() as ProjectList;

            if (projectsView?.DataContext is ProjectListViewModel vm)
            {
                vm.NavigationRequest += (_, _) =>
                {
                    NavigateToIssueListPageCommand.Execute(vm.SelectedProject);
                };
            }
        });
    }
}