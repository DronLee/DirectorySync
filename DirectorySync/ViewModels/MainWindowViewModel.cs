using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Модель представления основного окна приложения.
    /// </summary>
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private readonly ISynchronizedDirectoriesManager _synchronizedDirectoriesManager;

        private ICommand _loadDirectoriesCommand;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="synchronizedDirectoriesManager">Менеджер синхронизируемых директорий.</param>
        /// <param name="itemViewModelFactory">Фабрика моделей представлений отслеживаемых элементов.</param>
        public MainWindowViewModel(ISynchronizedDirectoriesManager synchronizedDirectoriesManager, IItemViewModelFactory itemViewModelFactory)
        {
            _synchronizedDirectoriesManager = synchronizedDirectoriesManager;
            Rows = new ObservableCollection<IRowViewModel>(_synchronizedDirectoriesManager.SynchronizedDirectories.Select(d =>
                itemViewModelFactory.CreateSynchronizedDirectoriesViewModel(d)));
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
        /// Строки, отображающие отслеживание директорий.
        /// </summary>
        public ObservableCollection<IRowViewModel> Rows { get; private set; }

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private async void LoadDirectories()
        {
            await _synchronizedDirectoriesManager.Load();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rows)));
        }
    }
}