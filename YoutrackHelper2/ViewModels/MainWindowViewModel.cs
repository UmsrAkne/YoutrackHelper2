using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using YoutrackHelper2.Views;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public string Title => "Prism Application";

        public DelegateCommand NavigateToIssueListPageCommand => new (() =>
        {
            regionManager.RequestNavigate("ContentRegion", nameof(IssueList));
        });

        public DelegateCommand NavigateToProjectListPageCommand => new (() =>
        {
            regionManager.RequestNavigate("ContentRegion", nameof(ProjectList));
        });
    }
}