using Autofac;
using System.Windows;
using DirectorySync.Views;
using DirectorySync.ViewModels;

namespace DirectorySync
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<MainWindow>();
            containerBuilder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>();

            var container = containerBuilder.Build();
            container.Resolve<MainWindow>().Show();
        }
    }
}