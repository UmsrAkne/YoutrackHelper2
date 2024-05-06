using System;
using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IssuesPostPageViewModel : BindableBase, IDialogAware
    {
        private ProjectWrapper projectWrapper;
        private Connector connector;
        private ObservableCollection<IssueWrapper> issueWrappers = new ObservableCollection<IssueWrapper>();
        private bool uiEnabled = true;

        public event Action<IDialogResult> RequestClose;

        public bool UiEnabled { get => uiEnabled; set => SetProperty(ref uiEnabled, value); }

        public string Title => string.Empty;

        public ObservableCollection<IssueWrapper> IssueWrappers
        {
            get => issueWrappers;
            set => SetProperty(ref issueWrappers, value);
        }

        public DelegateCommand CloseCommand => new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult());
        });

        public AsyncDelegateCommand CreateIssuesAsyncCommand => new (async () =>
        {
            UiEnabled = false;
            foreach (var issue in IssueWrappers)
            {
                await connector.CreateIssue(projectWrapper.ShortName, issue.Title, string.Empty);
            }

            IssueWrappers.Clear();
            UiEnabled = true;
        });

        public DelegateCommand AddIssueCommand => new DelegateCommand(() =>
        {
            IssueWrappers.Add(new IssueWrapper());
        });

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            connector = parameters.GetValue<Connector>(nameof(Connector));
            projectWrapper = parameters.GetValue<ProjectWrapper>(nameof(ProjectWrapper));
        }
    }
}