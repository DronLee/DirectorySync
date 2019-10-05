using DirectorySync.ViewModels;
using System.Windows;

namespace DirectorySync.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IMainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void LeftTreeViewScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            RightTreeViewScroll.ScrollToVerticalOffset(e.VerticalOffset);
            RightTreeViewScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void RightTreeViewScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            LeftTreeViewScroll.ScrollToVerticalOffset(e.VerticalOffset);
            LeftTreeViewScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
        }
    }
}