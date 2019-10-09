using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
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
        private readonly IRowViewModelFactory _itemViewModelFactory;

        private ICommand _loadDirectoriesCommand;
        private ICommand _selectedItemCommand;
        private ICommand _settingsCommand;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="synchronizedDirectoriesManager">Менеджер синхронизируемых директорий.</param>
        /// <param name="itemViewModelFactory">Фабрика моделей представлений отслеживаемых элементов.</param>
        public MainWindowViewModel(ISettingsStorage settingsStorage, ISynchronizedDirectoriesManager synchronizedDirectoriesManager, IRowViewModelFactory itemViewModelFactory,
            ISettingsViewModel settingsViewModel)
        {
            _settingsStorage = settingsStorage;
            _synchronizedDirectoriesManager = synchronizedDirectoriesManager;
            _synchronizedDirectoriesManager.AddSynchronizedDirectoriesEvent += AddSynchronizedDirectories;
            _synchronizedDirectoriesManager.RemoveSynchronizedDirectoriesEvent += RemoveSynchronizedDirectories;
            _settingsViewModel = settingsViewModel;
            _itemViewModelFactory = itemViewModelFactory;
            Rows = new ObservableCollection<IRowViewModel>(_synchronizedDirectoriesManager.SynchronizedDirectories.Select(d =>
                itemViewModelFactory.CreateRowViewModel(d)));
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
        /// Строки, отображающие отслеживание директорий.
        /// </summary>
        public ObservableCollection<IRowViewModel> Rows { get; private set; }

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private async void LoadDirectories()
        {
            bool checkDirectory = true;

            if(_settingsStorage.SettingsRows.Count(r => r.IsUsed) == 0)
                checkDirectory = ShowSettingsWindow("Чтобы начать работу укажите пары синхронизируемых между собой директорий.");
            else if(_settingsStorage.SettingsRows.Where(r=>r.IsUsed).Any(r => r.LeftDirectory.NotFound || r.RightDirectory.NotFound))
                checkDirectory = ShowSettingsWindow("Среди указаных директорий есть те, которые не удаётся найти. Отключите их или удалите из списка.");

            if(!checkDirectory)
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
            var removingRow = Rows.Single(r => r.LeftItem.Directory.FullPath == synchronizedDirectories.LeftDirectory.FullPath &&
                r.RightItem.Directory.FullPath == synchronizedDirectories.RightDirectory.FullPath);
            Rows.Remove(removingRow);
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Rows)));
        }

        private void AddSynchronizedDirectories(ISynchronizedDirectories synchronizedDirectories)
        {
            Rows.Add(_itemViewModelFactory.CreateRowViewModel(synchronizedDirectories));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Rows)));
        }
    }
}