using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace YoutrackHelper2.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfirmationPageViewModel : BindableBase, IDialogAware
    {
        private string message;

        public event Action<IDialogResult> RequestClose;

        public string Title => string.Empty;

        public string Message { get => message; set => SetProperty(ref message, value); }

        public DelegateCommand YesButtonCommand => new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
        });

        public DelegateCommand NoButtonCommand => new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.No));
        });

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>(nameof(Message));
        }
    }
}