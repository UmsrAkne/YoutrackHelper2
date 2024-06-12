using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private string tagNameText = string.Empty;
        private ObservableCollection<Tag> tags;

        public TagManagementPageViewModel()
        {
            SetDummies();
        }

        public event Action<IDialogResult> RequestClose;

        public string Title => string.Empty;

        public TitleBarText TitleBarText { get; } = new () { Text = "Tag management page", };

        public string TagNameText { get => tagNameText; set => SetProperty(ref tagNameText, value); }

        public ObservableCollection<Tag> Tags { get => tags; private set => SetProperty(ref tags, value); }

        public DelegateCommand CloseCommand => new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult());
        });

        public AsyncDelegateCommand CreateTagAsyncCommand => new AsyncDelegateCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(TagNameText))
            {
                return;
            }

            await connector.CreateTag(new Tag() { Name = TagNameText, });
            TagNameText = string.Empty;
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

        [Conditional("DEBUG")]
        private void SetDummies()
        {
            var ts = new DummyTagManager().GetTags().Result;
            Tags = new ObservableCollection<Tag>(ts);
        }
    }
}