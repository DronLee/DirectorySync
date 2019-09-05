using Autofac;
using System.Windows;
using DirectorySync.Views;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("XUnitTestProject")]
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
            containerBuilder.RegisterModule<AutofacRegisterModule>();
            var container = containerBuilder.Build();
            container.Resolve<MainWindow>().Show();
        }
    }
}