using DirectorySync.ViewModels;
using System.Windows;
using System.Windows.Controls;

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

        private void LeftTreeViewScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TreeViewScrollChanged(RightTreeViewScroll, e);
        }

        private void RightTreeViewScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TreeViewScrollChanged(LeftTreeViewScroll, e);
        }

        private void TreeViewScrollChanged(ScrollViewer scrollViewer, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
                scrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void TreeViewScroll_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Не важно левый или правый двигать скролл, всё равно они синхронизированы.
            LeftTreeViewScroll.ScrollToVerticalOffset(LeftTreeViewScroll.VerticalOffset - e.Delta);
        }
    }
}