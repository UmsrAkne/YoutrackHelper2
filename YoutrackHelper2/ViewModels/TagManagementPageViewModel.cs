using System;
using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using YoutrackHelper2.Models;
using YoutrackHelper2.Models.Tags;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TagManagementPageViewModel : BindableBase, IDialogAware
    {
        private IConnector connector;

        public event Action<IDialogResult> RequestClose;

        public string Title => string.Empty;

        public TitleBarText TitleBarText { get; } = new () { Text = "Tag management page", };

        public ObservableCollection<Tag> Tags { get; set; }

        public DelegateCommand CloseCommand => new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult());
        });

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            connector = parameters.GetValue<IConnector>(nameof(Connector));
            if (connector != null)
            {
                Tags = new ObservableCollection<Tag>(await connector.GetTags());
            }
        }
    }
}