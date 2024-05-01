using Prism.Mvvm;

namespace YoutrackHelper2.Models
{
    public class TitleBarText : BindableBase
    {
        private string text;

        public string Text { get => text; set => SetProperty(ref text, value); }
    }
}