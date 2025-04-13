using System;
using Prism.Mvvm;

namespace YoutrackHelper2.Models
{
    public class TitleBarText : BindableBase
    {
        private string text;
        private bool progressing;
        private TimeSpan currentWorkingDuration;

        public string Text
        {
            get => Progressing ? $"[{(int)CurrentWorkingDuration.TotalMinutes}m] {text}" : $"{text}";
            set => SetProperty(ref text, value);
        }

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

        public TimeSpan CurrentWorkingDuration
        {
            get => currentWorkingDuration;
            set
            {
                if (SetProperty(ref currentWorkingDuration, value))
                {
                    RaisePropertyChanged(nameof(Text));
                }
            }
        }
    }
}