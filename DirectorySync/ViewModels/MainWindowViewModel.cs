using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DirectorySync.Models;
using DirectorySync.Models.Settings;
using DirectorySync.ViewModels.Settings;
using DirectorySync.Views;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Модель представления основного окна приложения.
    /// </summary>
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private readonly ISettingsStorage _settingsStorage;
        private readonly ISynchronizedDirectoriesManager _synchronizedDirectoriesManager;
        private readonly ISettingsViewModel _settingsViewModel;
        private readonly IRowViewModelFactory _rowViewModelFactory;
        private readonly Dispatcher _dispatcher;

        private Bitmap _processGifBitmap;

        private ICommand _loadedFormCommand;
        private ICommand _selectedItemCommand;
        private ICommand _settingsCommand;
        private ICommand _clearLogCommand;
        private ICommand _refreshSynchronizedDirectoriesCommand;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="synchronizedDirectoriesManager">Менеджер синхронизируемых директорий.</param>
        /// <param name="rowViewModelFactory">Фабрика моделей представлений отслеживаемых элементов.</param>
        public MainWindowViewModel(ISettingsStorage settingsStorage, ISynchronizedDirectoriesManager synchronizedDirectoriesManager, IRowViewModelFactory rowViewModelFactory,
            ISettingsViewModel settingsViewModel)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _settingsStorage = settingsStorage;
            _synchronizedDirectoriesManager = synchronizedDirectoriesManager;
            _synchronizedDirectoriesManager.AddSynchronizedDirectoriesEvent += AddSynchronizedDirectories;
            _synchronizedDirectoriesManager.RemoveSynchronizedDirectoriesEvent += RemoveSynchronizedDirectories;
            _settingsViewModel = settingsViewModel;
            _rowViewModelFactory = rowViewModelFactory;
            _rowViewModelFactory.RefreshedRowEvent += (IRowViewModel refreshedRow) => { SubscribeOnErrors(refreshedRow); };
            Rows = new ObservableCollection<IRowViewModel>(_synchronizedDirectoriesManager.SynchronizedDirectories.Select(d =>
                rowViewModelFactory.CreateRowViewModel(d)));
            Log = new ObservableCollection<string>();
        }

        /// <summary>
        /// Команда загрузки директорий.
        /// </summary>
        public ICommand LoadedFormCommand
        {
            get
            {
                if (_loadedFormCommand == null)
                    _loadedFormCommand = new Command(async x =>
                    {
                        ProcessGifSource = GetProcessGifSource();
                        ImageAnimator.Animate(_processGifBitmap, OnFrameChanged);
                        await LoadDirectories();
                    });
                return _loadedFormCommand;
            }
        }

        /// <summary>
        /// Комнада выбора строки.
        /// </summary>
        public ICommand SelectedItemCommand
        {
            get
            {
                if (_selectedItemCommand == null)
                    _selectedItemCommand = new Command<TreeView>((TreeView treeView) =>
                    {
                        if (treeView.SelectedItem != null)
                            ((IRowViewModel)treeView.SelectedItem).IsSelected = true;
                    });
                return _selectedItemCommand;
            }
        }

        /// <summary>
        /// Команда вызова окна настроек.
        /// </summary>
        public ICommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                    _settingsCommand = new Command(async action =>
                    {
                        if (ShowSettingsWindow(null))
                            await _synchronizedDirectoriesManager.Load();
                    });
                return _settingsCommand;
            }
        }

        /// <summary>
        /// Команда на очистку окна сообщений.
        /// </summary>
        public ICommand ClearLogCommand
        {
            get
            {
                if (_clearLogCommand == null)
                    _clearLogCommand = new Command(action =>
                    {
                        Log.Clear();
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Log)));
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ClearLogButtonIsVisible)));
                    });
                return _clearLogCommand;
            }
        }

        /// <summary>
        /// Команда обновления представлений синхронизируемых директорий.
        /// </summary>
        public ICommand RefreshSynchronizedDirectoriesCommand 
        { 
            get
            {
                if(_refreshSynchronizedDirectoriesCommand == null)
                {
                    _refreshSynchronizedDirectoriesCommand = new Command(async action =>
                    {

                        foreach (var row in Rows)
                            row.ShowInProcess();
                        await _synchronizedDirectoriesManager.Refresh();
                    });
                }
                return _refreshSynchronizedDirectoriesCommand;
            }
        }

        /// <summary>
        /// Строки, отображающие отслеживание директорий.
        /// </summary>
        public ObservableCollection<IRowViewModel> Rows { get; }

        /// <summary>
        /// Строки лога.
        /// </summary>
        public ObservableCollection<string> Log { get; }

        /// <summary>
        /// True - кнопка очистки лога видна.
        /// </summary>
        public bool ClearLogButtonIsVisible => Log.Count > 0;

        /// <summary>
        /// Gif для отображения процесса синхронизации.
        /// </summary>
        public BitmapSource ProcessGifSource { get; private set; }

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private async Task LoadDirectories()
        {
            bool checkDirectory = true;

            if (_settingsStorage.SettingsRows.Count(r => r.IsUsed) == 0)
                checkDirectory = ShowSettingsWindow("Чтобы начать работу укажите пары синхронизируемых между собой директорий.");
            else if (_settingsStorage.SettingsRows.Where(r => r.IsUsed).Any(r => r.LeftDirectory.NotFound || r.RightDirectory.NotFound))
                checkDirectory = ShowSettingsWindow("Среди указаных директорий есть те, которые не удаётся найти. Отключите их или удалите из списка.");

            if (!checkDirectory)
                Environment.Exit(-1);

            await _synchronizedDirectoriesManager.Load();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rows)));
        }

        private bool ShowSettingsWindow(string comment)
        {
            _settingsViewModel.Comment = comment;
            var settingsWindow = new SettingsWindow(_settingsViewModel);
            settingsWindow.ShowDialog();
            return _settingsViewModel.Ok;
        }

        private void RemoveSynchronizedDirectories(ISynchronizedDirectories synchronizedDirectories)
        {
            var removingRow = Rows.Single(r => r.LeftItem.Directory == synchronizedDirectories.LeftDirectory &&
                r.RightItem.Directory == synchronizedDirectories.RightDirectory);
            Rows.Remove(removingRow);
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Rows)));
        }

        private void AddSynchronizedDirectories(ISynchronizedDirectories synchronizedDirectories)
        {
            Rows.Add(_rowViewModelFactory.CreateRowViewModel(synchronizedDirectories));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Rows)));
        }

        private void SubscribeOnErrors(IRowViewModel row)
        {
            row.SyncErrorEvent += (string error) =>
            {
                _dispatcher.Invoke(() =>
                {
                    AddToLog(error);
                });
            };
            foreach (var childRow in row.ChildRows)
                SubscribeOnErrors(childRow);
        }

        private void AddToLog(string message)
        {
            Log.Add(message);
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Log)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ClearLogButtonIsVisible)));
        }

        #region Методы, реализующие анимацию gif для отрображения процесса синхронизации.
        private BitmapSource GetProcessGifSource()
        {
            if (_processGifBitmap == null)
                _processGifBitmap = new Bitmap(@"c:\MyProgramms\Своё\DirectorySync\DirectorySync\Icons\SyncProcess.gif"); //System.IO.Path.Combine(Environment.CurrentDirectory, "Icons", "SyncProcess.gif"));
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessGifSource)));
        }
        #endregion
    }
}