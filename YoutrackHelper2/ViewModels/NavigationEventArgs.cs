using System;

namespace YoutrackHelper2.ViewModels
{
    public class NavigationEventArgs : EventArgs
    {
        public NavigationEventArgs(string destViewName)
        {
            DestViewName = destViewName;
        }

        public string DestViewName { get; }
    }
}