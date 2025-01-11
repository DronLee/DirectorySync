using Autofac;
using DirectorySync.Models;
using DirectorySync.ViewModels;
using DirectorySync.Views;
using DirectorySync.Models.Settings;
using DirectorySync.ViewModels.Settings;
using Serilog;

namespace DirectorySync
{
    internal sealed class AutofacRegisterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsStorage>().As<ISettingsStorage>().SingleInstance()
                .WithParameter("settingsFile", "Settings");

            RegisterLogger(builder);

            builder.RegisterType<SettingsViewModel>().As<ISettingsViewModel>().SingleInstance();

            builder.RegisterType<ProcessScreenSaver>().As<IProcessScreenSaver>().SingleInstance();

            builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();
            builder.RegisterType<MainWindow>().SingleInstance();

            builder.RegisterType<ItemFactory>().As<IItemFactory>().SingleInstance();
            builder.RegisterType<SynchronizedItemFactory>().As<ISynchronizedItemFactory>().SingleInstance();
            builder.RegisterType<SynchronizedItemMatcher>().As<ISynchronizedItemMatcher>().SingleInstance();
            builder.RegisterType<SynchronizedItemsStatusAndCommandsUpdater>()
                .As<ISynchronizedItemsStatusAndCommandsUpdater>().SingleInstance();
            builder.RegisterType<SynchronizedDirectoriesManager>().As<ISynchronizedDirectoriesManager>().SingleInstance();
            builder.RegisterType<RowViewModelFactory>().As<IRowViewModelFactory>().SingleInstance();
        }

        private void RegisterLogger(ContainerBuilder builder)
        {
            var configuration = new LoggerConfiguration().ReadFrom.AppSettings();
            var logger = configuration.CreateLogger();
            builder.Register(r => logger).As<ILogger>().SingleInstance();
        }
    }
}