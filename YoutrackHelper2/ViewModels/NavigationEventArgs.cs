using System;
using System.Collections.Generic;
using YoutrackHelper2.Models;

namespace YoutrackHelper2.ViewModels
{
    public class NavigationEventArgs : EventArgs
    {
        public NavigationEventArgs(string destViewName)
        {
            DestViewName = destViewName;
        }

        public string DestViewName { get; }

        public List<ProjectWrapper> FavoriteProjects { get; set; }
    }
}