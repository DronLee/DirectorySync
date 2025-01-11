using Serilog;
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
        private readonly object _startAndStopLocker = new object();

        private readonly ILogger _logger;

        private Dispatcher _dispatcher;
        private Bitmap _processGifBitmap;

        public ProcessScreenSaver(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gif для отображения процесса синхронизации.
        /// </summary>
        public BitmapSource ProcessGifSource { get; private set; }

        /// <inheritdoc/>
        public bool IsStopped { get; private set; } = true;

        public event Action FrameUpdatedEvent;

        /// <inheritdoc/>
        public void Start(Dispatcher dispatcher)
        {
            lock (_startAndStopLocker)
            {
                _dispatcher = dispatcher;
                _dispatcher.Invoke(() => { ProcessGifSource = GetProcessGifSource(); });
                ImageAnimator.Animate(_processGifBitmap, OnFrameChanged);
                IsStopped = false;
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            lock (_startAndStopLocker)
            {
                ImageAnimator.StopAnimate(_processGifBitmap, OnFrameChanged);
                IsStopped = true;
            }
        }

        private BitmapSource GetProcessGifSource()
        {
            if (_processGifBitmap is null)
            {
                _processGifBitmap = Resources.SyncProcess;
            }

            try
            {
                var handle = _processGifBitmap.GetHbitmap();
                return Imaging.CreateBitmapSourceFromHBitmap(
                        handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Не удалось получить gif для отображения работающего процесса (потребляемая память: {0} МБ).", GC.GetTotalMemory(true) / 1024 / 1024);
                throw;
            }
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