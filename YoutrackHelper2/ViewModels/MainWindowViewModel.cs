using System.Collections.Generic;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using YoutrackHelper2.Models;
using YoutrackHelper2.Utils;
using YoutrackHelper2.Views;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainWindowViewModel : BindableBase
    {
        private const string RegionName = "ContentRegion";
        private const string RegionTitle = "YoutrackHelper2 Projects";
        private static bool initialized;
        private readonly IRegionManager regionManager;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            TitleBarText.Text = AppVersionInfo.Title;
        }

        public TitleBarText TitleBarText { get; set; } = new ();

        public AsyncDelegateCommand AppInitializeCommandAsync => new (async () =>
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
                await vm.LoadProjectsAsync();
                vm.TitleBarText = new TitleBarText();
                vm.NavigationRequest += (_, e) =>
                {
                    if (e is not NavigationEventArgs ne)
                    {
                        return;
                    }

                    if (ne.DestViewName == nameof(IssueList))
                    {
                        NavigateToIssueListPageCommand.Execute(vm.SelectedProject);
                        NavigateToIssueListPage(vm.SelectedProject, ((NavigationEventArgs)e).FavoriteProjects);
                    }
                };
            }
        });

        private DelegateCommand<ProjectWrapper> NavigateToIssueListPageCommand => new ((param) =>
        {
            var parameters = new NavigationParameters
            {
                { nameof(IssueListViewModel.ProjectWrapper), param },
            };

            regionManager.RequestNavigate(RegionName, nameof(IssueList), parameters);

            var v = regionManager.Regions[RegionName].ActiveViews.FirstOrDefault(v => v is IssueList) as IssueList;
            if (v?.DataContext as IssueListViewModel is { } vm)
            {
                TitleBarText.Text = param.FullName;
                vm.TitleBarText = TitleBarText;

                if (vm.Initialized)
                {
                    return;
                }

                vm.Initialized = true;
                vm.NavigationRequest += (_, e) =>
                {
                    if (e is not NavigationEventArgs ne)
                    {
                        return;
                    }

                    if (ne.DestViewName == nameof(ProjectList))
                    {
                        NavigateToProjectListPageCommand.Execute();
                    }
                };
            }
        });

        private AppVersionInfo AppVersionInfo { get; set; } = new ();

        private DelegateCommand NavigateToProjectListPageCommand => new (() =>
        {
            regionManager.RequestNavigate(RegionName, nameof(ProjectList));
            AppVersionInfo.Title = RegionTitle;
        });

        private void NavigateToIssueListPage(ProjectWrapper projectWrapper, List<ProjectWrapper> favorites)
        {
            var parameters = new NavigationParameters
            {
                { nameof(IssueListViewModel.ProjectWrapper), projectWrapper },
            };

            regionManager.RequestNavigate(RegionName, nameof(IssueList), parameters);

            var v = regionManager.Regions[RegionName].ActiveViews.FirstOrDefault(v => v is IssueList) as IssueList;
            if (v?.DataContext as IssueListViewModel is { } vm)
            {
                TitleBarText.Text = projectWrapper.FullName;
                vm.TitleBarText = TitleBarText;

                vm.FavoriteProjects = favorites.Where(p => p.FullName != projectWrapper.FullName).ToList();

                if (vm.Initialized)
                {
                    return;
                }

                vm.Initialized = true;
                vm.NavigationRequest += (_, e) =>
                {
                    if (e is not NavigationEventArgs ne)
                    {
                        return;
                    }

                    if (ne.DestViewName == nameof(ProjectList))
                    {
                        NavigateToProjectListPageCommand.Execute();
                    }
                };
            }
        }
    }
}