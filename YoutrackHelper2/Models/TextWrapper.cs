using System;
using Prism.Mvvm;

namespace YoutrackHelper2.Models
{
    public class TextWrapper : BindableBase
    {
        private string text = string.Empty;
        private bool textChanged;

        public event EventHandler TextChangedEvent;

        public string Text
        {
            get => text;
            set
            {
                if (SetProperty(ref text, value))
                {
                    TextChanged = true;
                }
            }
        }

        public bool TextChanged
        {
            get => textChanged;
            set
            {
                if (SetProperty(ref textChanged, value))
                {
                    OnTextChanged();
                }
            }
        }

        private void OnTextChanged()
        {
            TextChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}