using Prism.Mvvm;

namespace YoutrackHelper2.Models
{
    public class TextWrapper : BindableBase
    {
        private string text = string.Empty;
        private bool textChanged;

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

        public bool TextChanged { get => textChanged; set => SetProperty(ref textChanged, value); }
    }
}