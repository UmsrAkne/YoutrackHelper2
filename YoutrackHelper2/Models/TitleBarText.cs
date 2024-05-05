using Prism.Mvvm;

namespace YoutrackHelper2.Models
{
    public class TitleBarText : BindableBase
    {
        private string text;
        private string version;

        public string Text
        {
            get => $"{text} {Version}";
            set => SetProperty(ref text, value);
        }

        public string Version { get => version; set => SetProperty(ref version, value); }
    }
}