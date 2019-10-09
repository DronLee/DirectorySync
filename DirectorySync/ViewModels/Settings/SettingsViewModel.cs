using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DirectorySync.Models.Settings;

namespace DirectorySync.ViewModels.Settings
{
    /// <summary>
    /// Модель представления настроек.
    /// </summary>
    public class SettingsViewModel : ISettingsViewModel
    {
        private readonly ISettingsStorage _settingsStorage;

        private string _comment;
        private ICommand _okCommand = null;

        /// <summary>
        /// Конструктор. 
        /// </summary>
        /// <param name="settingsStorage">Хранилище настроек.</param>
        public SettingsViewModel(ISettingsStorage settingsStorage)
        {
            _settingsStorage = settingsStorage;

            SettingsRows = new ObservableCollection<ISettingsRowViewModel>(
                settingsStorage.SettingsRows.Select(r =>
                {
                    var row = new SettingsRowViewModel(r);
                    row.DeleteRowEvent += DeleteRow;
                    return row;
                }));

            AddEmptyRow();
            CommentType = MessageTypeEnum.Default;
        }

        /// <summary>
        /// Сообщение для пользователя в окне настроек.
        /// </summary>
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comment)));
                }
            }
        }

        /// <summary>
        /// True - окно закрыто с принятием настроек.
        /// </summary>
        public bool Ok { get; set; }

        /// <summary>
        /// Модели представлений строк настроек.
        /// </summary>
        public ObservableCollection<ISettingsRowViewModel> SettingsRows { get; set; }

        /// <summary>
        /// Команда принятия настроек.
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                    _okCommand = new Command<Window>((Window window) =>
                    {
                        var activeSettingsRows = SettingsRows.Where(r => !r.IsEmpty && r.IsUsed).ToArray();
                        Comment = GetWarningMessage();
                        if (Comment == null)
                        {
                            _settingsStorage.SettingsRows = SettingsRows.Where(r => !r.IsEmpty).Select(r =>
                                  _settingsStorage.CreateSettingsRow(r.LeftDirectory.DirectoryPath, r.RightDirectory.DirectoryPath, r.IsUsed)
                            ).ToArray();
                            _settingsStorage.Save();
                            Ok = true;
                            window.Close();
                        }
                        else
                        {
                            CommentType = MessageTypeEnum.Warning;
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommentType)));
                        }
                    });
                return _okCommand;
            }
        }

        /// <summary>
        /// Тип сообщения для пользователя в окне настроек.
        /// </summary>
        public MessageTypeEnum CommentType { get; private set; }

        /// <summary>
        /// Событие изменения одного из свойств.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private string GetWarningMessage()
        {
            var activeSettingsRows = SettingsRows.Where(r => !r.IsEmpty && r.IsUsed).ToArray();
            if (activeSettingsRows.Length == 0)
                return "Не задано ни одной пары директорий для синхронизации.";
            if (activeSettingsRows.Any(r => r.LeftDirectory.DirectoryPath == null || r.RightDirectory.DirectoryPath == null))
                return "Не для всех директорий указаны пары. Удалите директории без пары из списка или отключите их.";
            else if (activeSettingsRows.Any(r => r.LeftDirectory.NotFound || r.RightDirectory.NotFound))
                return "Не все директории удаётся найти. Удалите строки, где есть отсутствующие директории или отключите их.";
            return null;
        }

        private void DeleteRow(ISettingsRowViewModel row)
        {
            if (!row.IsEmpty)
            {
                SettingsRows.Remove(row);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsRows)));
            }
        }
        private void AddEmptyRow()
        {
            var emptyRow = new SettingsRowViewModel();
            emptyRow.SetEmptyDirectoryEvent += () => { AddEmptyRow(); };
            emptyRow.DeleteRowEvent += DeleteRow;
            SettingsRows.Add(emptyRow);
        }
    }
}