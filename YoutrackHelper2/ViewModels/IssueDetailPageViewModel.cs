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
        private Connector connector;

        public event Action<IDialogResult> RequestClose;

        public string Title => string.Empty;

        public IssueWrapper IssueWrapper { get; set; }

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
            connector = parameters.GetValue<Connector>(nameof(Connector));
            IssueWrapper = parameters.GetValue<IssueWrapper>(nameof(IssueWrapper));
            RaisePropertyChanged(nameof(IssueWrapper));
        }
    }
}