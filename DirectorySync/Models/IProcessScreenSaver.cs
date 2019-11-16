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
        /// Событие обновления отображенния заставки.
        /// </summary>
        event Action FrameUpdatedEvent;

        /// <summary>
        /// Загрузка gif-файла заставки.
        /// </summary>
        void Load(Dispatcher dispatcher);
    }
}