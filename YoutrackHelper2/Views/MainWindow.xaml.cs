using System.Windows;
using Prism.Regions;

namespace YoutrackHelper2.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IRegionManager regionManager;

        public MainWindow(IRegionManager regionManager)
        {
            InitializeComponent();
            this.regionManager = regionManager;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            regionManager.RequestNavigate("ContentRegion", nameof(ProjectList));
        }
    }
}