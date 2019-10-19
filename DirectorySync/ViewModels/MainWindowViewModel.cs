using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
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

        private ICommand _loadDirectoriesCommand;
        private ICommand _selectedItemCommand;
        private ICommand _settingsCommand;
        private ICommand _clearLogCommand;

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
        public ICommand LoadDirectoriesCommand
        {
            get
            {
                if (_loadDirectoriesCommand == null)
                    _loadDirectoriesCommand = new Command(x => LoadDirectories());
                return _loadDirectoriesCommand;
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
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private async void LoadDirectories()
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
    }
}