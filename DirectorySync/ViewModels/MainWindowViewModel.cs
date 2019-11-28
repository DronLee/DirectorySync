using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
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
        private readonly IProcessScreenSaver _processScreenSaver;
        private readonly Dispatcher _dispatcher;

        private bool _menuButtonsIsEnabled;

        private ICommand _loadedFormCommand;
        private ICommand _selectedItemCommand;
        private ICommand _settingsCommand;
        private ICommand _clearLogCommand;
        private ICommand _refreshSynchronizedDirectoriesCommand;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsStorage">Хранилище настроек.</param>
        /// <param name="synchronizedDirectoriesManager">Менеджер синхронизируемых директорий.</param>
        /// <param name="rowViewModelFactory">Фабрика создания моделей представлений строк.</param>
        /// <param name="settingsViewModel">Модель представления окна настроек.</param>
        /// <param name="processScreenSaver">Объект, реализующий отображение заставки процесса.</param>
        public MainWindowViewModel(ISettingsStorage settingsStorage, ISynchronizedDirectoriesManager synchronizedDirectoriesManager, IRowViewModelFactory rowViewModelFactory,
            ISettingsViewModel settingsViewModel, IProcessScreenSaver processScreenSaver)
        {
            (_dispatcher, _processScreenSaver, _settingsStorage, _synchronizedDirectoriesManager, _settingsViewModel, _rowViewModelFactory) =
                (Dispatcher.CurrentDispatcher, processScreenSaver, settingsStorage, synchronizedDirectoriesManager, settingsViewModel, rowViewModelFactory);

            _synchronizedDirectoriesManager.AddSynchronizedDirectoriesEvent += AddSynchronizedDirectories;
            _synchronizedDirectoriesManager.RemoveSynchronizedDirectoriesEvent += RemoveSynchronizedDirectories;

            _rowViewModelFactory.AddRowEvent += AddRow;
            _rowViewModelFactory.DeleteRowEvent += DeleteRow;

            foreach (var row in _synchronizedDirectoriesManager.SynchronizedDirectories.Select(d => rowViewModelFactory.CreateRowViewModel(d)))
                AddRow(null, row);
        }

        /// <summary>
        /// Gif для отображения процесса синхронизации.
        /// </summary>
        public BitmapSource ProcessGifSource => _processScreenSaver.ProcessGifSource;

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
                        MenuButtonsIsEnabled = false;

                        _processScreenSaver.FrameUpdatedEvent += () => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessGifSource))); };
                        _processScreenSaver.Load(_dispatcher);

                        await LoadDirectories();
                        MenuButtonsIsEnabled = true;
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
                    _settingsCommand = new Command(action =>
                    {
                        if (ShowSettingsWindow(null))
                        {
                            MenuButtonsIsEnabled = false;
                            _synchronizedDirectoriesManager.Load();
                            MenuButtonsIsEnabled = true;
                        }
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
                if (_refreshSynchronizedDirectoriesCommand == null)
                {
                    _refreshSynchronizedDirectoriesCommand = new Command(action =>
                    {
                        MenuButtonsIsEnabled = false;
                        _synchronizedDirectoriesManager.Refresh();
                        MenuButtonsIsEnabled = true;
                    });
                }
                return _refreshSynchronizedDirectoriesCommand;
            }
        }

        /// <summary>
        /// Строки, отображающие отслеживание директорий.
        /// </summary>
        public ObservableCollection<IRowViewModel> Rows { get; } = new ObservableCollection<IRowViewModel>();

        /// <summary>
        /// Строки лога.
        /// </summary>
        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        /// <summary>
        /// True - кнопка очистки лога видна.
        /// </summary>
        public bool ClearLogButtonIsVisible => Log.Count > 0;

        /// <summary>
        /// True - кнопки меню доступны.
        /// </summary>
        public bool MenuButtonsIsEnabled
        {
            get
            {
                return _menuButtonsIsEnabled;
            }
            set
            {
                if (_menuButtonsIsEnabled != value)
                {
                    _menuButtonsIsEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MenuButtonsIsEnabled)));
                }
            }
        }

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
        }

        private bool ShowSettingsWindow(string comment)
        {
            _settingsViewModel.RefreshRows();
            _settingsViewModel.Comment = comment;
            var settingsWindow = new SettingsWindow(_settingsViewModel);
            settingsWindow.ShowDialog();
            return _settingsViewModel.Ok;
        }

        private void RemoveSynchronizedDirectories(ISynchronizedItems synchronizedDirectories)
        {
            var removingRow = Rows.Single(r => r.LeftItem.Directory == synchronizedDirectories.LeftDirectory &&
                r.RightItem.Directory == synchronizedDirectories.RightDirectory);
            DeleteRow(null, removingRow);
        }

        private void AddSynchronizedDirectories(ISynchronizedItems synchronizedDirectories)
        {
            AddRow(null, _rowViewModelFactory.CreateRowViewModel(synchronizedDirectories));
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
            _dispatcher.Invoke(() =>
            {
                Log.Add(message);
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Log)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ClearLogButtonIsVisible)));
            });
        }

        private void AddRow(IRowViewModel parentRow, IRowViewModel childRow)
        {
            if (parentRow == null)
            {
                _dispatcher.Invoke(() => Rows.Add(childRow));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rows)));
            }
            else
            {
                _dispatcher.Invoke(() => parentRow.ChildRows.Add(childRow));
                PropertyChanged?.Invoke(parentRow, new PropertyChangedEventArgs(nameof(parentRow.ChildRows)));
            }

            childRow.SyncErrorEvent += AddToLog;
        }

        private void DeleteRow(IRowViewModel parentRow, IRowViewModel childRow)
        {
            if (parentRow == null)
            {
                _dispatcher.Invoke(() => Rows.Remove(childRow));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rows)));
            }
            else
            {
                _dispatcher.Invoke(() => parentRow.ChildRows.Remove(childRow));
                PropertyChanged?.Invoke(parentRow, new PropertyChangedEventArgs(nameof(parentRow.ChildRows)));
            }

            childRow.SyncErrorEvent -= AddToLog;
        }
    }
}