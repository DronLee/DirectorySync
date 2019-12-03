using Autofac;
using DirectorySync.Models;
using DirectorySync.Models.Settings;

namespace XUnitTestProject
{
    internal class TestAutofacRegisterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsStorage>().As<ISettingsStorage>().SingleInstance()
                .WithParameter("settingsFile", "Settings");

            builder.RegisterType<ItemFactory>().As<IItemFactory>().SingleInstance();
            builder.RegisterType<SynchronizedItemFactory>().As<ISynchronizedItemFactory>().SingleInstance();
            builder.RegisterType<SynchronizedItemMatcher>().As<ISynchronizedItemMatcher>().SingleInstance();
            builder.RegisterType<SynchronizedItemsStatusAndCommandsUpdater>()
                .As<ISynchronizedItemsStatusAndCommandsUpdater>().SingleInstance();
            builder.RegisterType<SynchronizedDirectoriesManager>().As<ISynchronizedDirectoriesManager>().SingleInstance();
        }
    }
}