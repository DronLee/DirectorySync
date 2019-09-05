using Autofac;
using DirectorySync.Models;
using DirectorySync.Properties;
using DirectorySync.ViewModels;
using DirectorySync.Views;

namespace DirectorySync
{
    internal class AutofacRegisterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();
            builder.RegisterType<ItemFactory>().As<IItemFactory>().SingleInstance();

            builder.RegisterType<SynchronizedDirectoriesManager>().As<ISynchronizedDirectoriesManager>().SingleInstance()
                .WithParameter("leftDirectories", Settings.Default.LeftDirectories)
                .WithParameter("rightDirectories", Settings.Default.RightDirectories);

            builder.RegisterType<RowViewModelFactory>().As<IRowViewModelFactory>().SingleInstance();
        }
    }
}