using Prism.Mvvm;

namespace YoutrackHelper2.Models
{
    public class TitleBarText : BindableBase
    {
        private string text;
        private string version;
        private bool progressing;

        public string Text
        {
            get => Progressing ? $"[w] {text} {Version}" : $"{text} {Version}";
            set => SetProperty(ref text, value);
        }

        public string Version { get => version; set => SetProperty(ref version, value); }

        public bool Progressing
        {
            get => progressing;
            set
            {
                if (SetProperty(ref progressing, value))
                {
                    RaisePropertyChanged(nameof(Text));
                }
            }
        }
    }
}