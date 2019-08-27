using Autofac;
using System.Windows;
using DirectorySync.Views;
using DirectorySync.ViewModels;
using DirectorySync.Models;
using DirectorySync.Properties;

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
            containerBuilder.RegisterType<MainWindow>().SingleInstance();
            containerBuilder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();
            containerBuilder.RegisterType<ItemFactory>().As<IItemFactory>().SingleInstance();

            containerBuilder.RegisterType<SynchronizedDirectoriesManager>().As<ISynchronizedDirectoriesManager>().SingleInstance()
                .WithParameter("leftDirectories", Settings.Default.LeftDirectories)
                .WithParameter("rightDirectories", Settings.Default.RightDirectories);

            containerBuilder.RegisterType<ItemViewModelFactory>().As<IItemViewModelFactory>().SingleInstance();

            var container = containerBuilder.Build();
            container.Resolve<MainWindow>().Show();
        }
    }
}