using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IssueDetailPageViewModel : BindableBase, IDialogAware
    {
        private IConnector connector;

        public event Action<IDialogResult> RequestClose;

        public string Title => string.Empty;

        public IssueWrapper IssueWrapper { get; set; }

        public TextWrapper Description { get; set; } = new TextWrapper();

        public TextWrapper IssueTitle { get; set; } = new TextWrapper();

        public AddWorkingDurationViewModel AddWorkingDurationViewModel { get; set; } = new ();

        public DelegateCommand CloseCommand => new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult());
        });

        public AsyncDelegateCommand UpdateDescriptionAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            if (!Description.TextChanged)
            {
                return;
            }

            IssueWrapper.Issue = await connector.UpdateDescriptionAsync(IssueWrapper.ShortName, Description.Text);
            Description.Text = IssueWrapper.Description;
            Description.TextChanged = false;
        });

        public AsyncDelegateCommand UpdateIssueTextsAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            if (!Description.TextChanged && !IssueTitle.TextChanged)
            {
                return;
            }

            IssueWrapper.Issue =
                await connector.UpdateIssueTexts(IssueWrapper.ShortName, IssueTitle.Text, Description.Text);

            Description.TextChanged = false;
            IssueTitle.TextChanged = false;
        });

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            connector = parameters.GetValue<IConnector>(nameof(Connector));
            IssueWrapper = parameters.GetValue<IssueWrapper>(nameof(IssueWrapper));
            Description.Text = IssueWrapper.Description;
            Description.TextChanged = false;
            IssueTitle.Text = IssueWrapper.Title;
            IssueTitle.TextChanged = false;
            AddWorkingDurationViewModel.Connector = connector;
            AddWorkingDurationViewModel.CurrentIssueWrapper = IssueWrapper;
            AddWorkingDurationViewModel.SetDefaultTexts();
            RaisePropertyChanged(nameof(IssueWrapper));
        }
    }
}