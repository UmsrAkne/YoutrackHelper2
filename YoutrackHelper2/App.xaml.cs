using System.Windows;
using Prism.Ioc;
using YoutrackHelper2.ViewModels;
using YoutrackHelper2.Views;

namespace YoutrackHelper2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ProjectList>();
            containerRegistry.RegisterForNavigation<IssueList>();
            containerRegistry.RegisterDialog<IssuesPostPage, IssuesPostPageViewModel>();
            containerRegistry.RegisterDialog<IssueDetailPage, IssueDetailPageViewModel>();
        }
    }
}