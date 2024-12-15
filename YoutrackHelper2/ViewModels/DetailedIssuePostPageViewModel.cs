using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DetailedIssuePostPageViewModel : BindableBase, IDialogAware
    {
        private IssueWrapper issueWrapper;
        private string tagsText;

        public event Action<IDialogResult> RequestClose;

        public string Title => string.Empty;

        public IssueWrapper IssueWrapper { get => issueWrapper; private set => SetProperty(ref issueWrapper, value); }

        public string TagsText { get => tagsText; set => SetProperty(ref tagsText, value); }

        public DelegateCommand CloseCommand => new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult());
        });

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            IssueWrapper = parameters.GetValue<IssueWrapper>(nameof(IssueWrapper));
        }
    }
}