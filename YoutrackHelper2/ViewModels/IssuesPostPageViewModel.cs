using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private string issuesText;

        public event Action<IDialogResult> RequestClose;

        public bool UiEnabled { get => uiEnabled; set => SetProperty(ref uiEnabled, value); }

        public string IssuesText { get => issuesText; set => SetProperty(ref issuesText, value); }

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

        public DelegateCommand ConvertToIssuesCommand => new DelegateCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(IssuesText))
            {
                return;
            }

            IssueWrappers.AddRange(IssuesText
                .Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(t => new IssueWrapper() { Title = t, }));
        });

        public DelegateCommand<IssueWrapper> RemoveIssueCommand => new DelegateCommand<IssueWrapper>((param) =>
        {
            IssueWrappers.Remove(param);
        });

        public DelegateCommand ClearIssuesCommand => new DelegateCommand(() =>
        {
            IssueWrappers.Clear();
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