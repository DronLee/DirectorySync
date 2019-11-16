using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DirectorySync.Models
{
    /// <summary>
    /// Класс отображающий заставку запущенного процеса в виде gif.
    /// </summary>
    public class ProcessScreenSaver: IProcessScreenSaver
    {
        private Dispatcher _dispatcher;
        private Bitmap _processGifBitmap;

        /// <summary>
        /// Gif для отображения процесса синхронизации.
        /// </summary>
        public BitmapSource ProcessGifSource { get; private set; }

        public event Action FrameUpdatedEvent;

        /// <summary>
        /// Загрузка изображения заставки.
        /// </summary>
        public void Load(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            ProcessGifSource = GetProcessGifSource();
            ImageAnimator.Animate(_processGifBitmap, OnFrameChanged);
        }

        private BitmapSource GetProcessGifSource()
        {
            if (_processGifBitmap == null)
                _processGifBitmap = Resources.SyncProcess;
            var handle = _processGifBitmap.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(
                    handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(FrameUpdatedCallback));
        }

        private void FrameUpdatedCallback()
        {
            ImageAnimator.UpdateFrames();
            if (ProcessGifSource != null)
                ProcessGifSource.Freeze();
            ProcessGifSource = GetProcessGifSource();
            FrameUpdatedEvent?.Invoke();
        }
    }
}