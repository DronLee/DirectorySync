using Autofac;
using System.Windows;
using DirectorySync.Views;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Runtime.InteropServices;
using System;

[assembly: InternalsVisibleTo("XUnitTestProject")]
namespace DirectorySync
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Mutex приложения. Нужен, чтобы убеждаться в единственном запуске приложения.
        /// </summary>
        private Mutex _applicationMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (InitAppMutex())
            {
                base.OnStartup(e);

                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterModule<AutofacRegisterModule>();
                var container = containerBuilder.Build();
                container.Resolve<MainWindow>().Show();
            }
            else
            {
                MessageBox.Show(
                    "Чтобы приложение работало корректно, его разрешается запускать лишь в единственном экземпляре.",
                    "Приложение уже запущено", MessageBoxButton.OK, MessageBoxImage.Warning);

                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Инициализация мьютекса приложения.
        /// </summary>
        /// <returns>True - мьютекс является единственным на данный момент.
        /// Значит приложение запущено в единственном экземпляре.</returns>
        private bool InitAppMutex()
        {
            bool result;

            var assembly = this.GetType().Assembly;
            var applicationGuid = ((GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;

            _applicationMutex = new Mutex(true, applicationGuid, out result);

            return result;
        }
    }
}