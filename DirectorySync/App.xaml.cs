using Autofac;
using System.Windows;
using DirectorySync.Views;

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

            var container = containerBuilder.Build();
            container.Resolve<MainWindow>().Show();
        }
    }
}