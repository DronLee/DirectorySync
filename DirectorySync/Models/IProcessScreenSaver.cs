using System;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс отображения заставки запущенного процеса в виде gif.
    /// </summary>
    public interface IProcessScreenSaver
    {
        /// <summary>
        /// Gif для отображения процесса синхронизации.
        /// </summary>
        BitmapSource ProcessGifSource { get; }

        /// <summary>
        /// True - отображение заставки остановлено.
        /// </summary>
        bool IsStopped { get; }

        /// <summary>
        /// Событие обновления отображенния заставки.
        /// </summary>
        event Action FrameUpdatedEvent;

        /// <summary>
        /// Запуск отображения заставки.
        /// </summary>
        void Start(Dispatcher dispatcher);

        /// <summary>
        /// Остановить отображение заставки.
        /// </summary>
        void Stop();
    }
}