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
        private ObservableCollection<IssueWrapper> issueWrappers = new ObservableCollection<IssueWrapper>();

        public event Action<IDialogResult> RequestClose;

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

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}