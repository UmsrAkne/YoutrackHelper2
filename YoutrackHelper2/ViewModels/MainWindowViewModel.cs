﻿using System.Diagnostics;
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
            TitleBarText.Text = "Projects";

            SetVersion();
        }

        public TitleBarText TitleBarText { get; private set; } = new ();

        public DelegateCommand NavigateToProjectListPageCommand => new (() =>
        {
            regionManager.RequestNavigate(RegionName, nameof(ProjectList));
            TitleBarText.Text = "Projects";
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
                vm.TitleBarText = TitleBarText;
                vm.NavigationRequest += (_, e) =>
                {
                    if (e is not NavigationEventArgs ne)
                    {
                        return;
                    }

                    if (ne.DestViewName == nameof(IssueList))
                    {
                        NavigateToIssueListPageCommand.Execute(vm.SelectedProject);
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
            if (v?.DataContext as IssueListViewModel is { TitleBarText: null, } vm)
            {
                vm.TitleBarText = TitleBarText;
                TitleBarText.Text = param.FullName;

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

        [Conditional("RELEASE")]
        private void SetVersion()
        {
            // リリースビルドの場合のみ実行するコード
            TitleBarText.Version = "version : " + "20240510" + "a";
        }
    }
}