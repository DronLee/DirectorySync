using DirectorySync.Models.Settings;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace DirectorySync.ViewModels.Settings
{
    /// <summary>
    /// Модель представления строки настройки.
    /// </summary>
    public class SettingsRowViewModel : ISettingsRowViewModel
    {
        private const string _notFoundLeftDirectoryButtonStyleName = "NotFoundLeftDirectoryButton";
        private const string _notFoundRightDirectoryButtonStyleName = "NotFoundRightDirectoryButton";
        private const string _leftDirectoryButtonStyleName = "LeftDirectoryButton";
        private const string _rightDirectoryButtonStyleName = "RightDirectoryButton";

        private bool _isUsed = true; 

        private ISettingsRow _settingsRow;

        private ICommand _folderDialogCommand = null;
        private ICommand _deleteRowCommand = null;

        /// <summary>
        /// Конструктор создания модели пустой строки, нужной, чтобы пользователь мог добавить новые директории.
        /// </summary>
        public SettingsRowViewModel()
        {
            IsEmpty = true;
            LeftDirectory = new SettingsDirectoryViewModel();
            RightDirectory = new SettingsDirectoryViewModel();
        }

        /// <summary>
        /// Конструктор создания модели на основе строки настроек.
        /// </summary>
        /// <param name="settingsRow"></param>
        public SettingsRowViewModel(ISettingsRow settingsRow)
        {
            _settingsRow = settingsRow;
            IsEmpty = false;
            LeftDirectory = new SettingsDirectoryViewModel(settingsRow.LeftDirectory.DirectoryPath, settingsRow.LeftDirectory.NotFound,
                settingsRow.LeftDirectory.NotFound ? _notFoundLeftDirectoryButtonStyleName : _leftDirectoryButtonStyleName);
            RightDirectory = new SettingsDirectoryViewModel(settingsRow.RightDirectory.DirectoryPath, settingsRow.RightDirectory.NotFound,
                settingsRow.RightDirectory.NotFound ? _notFoundRightDirectoryButtonStyleName : _rightDirectoryButtonStyleName);
            _isUsed = settingsRow.IsUsed;
        }

        /// <summary>
        /// True - пустая строка, которая нужна, чтобы пользователь мог добавить новые директории.
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Левая директория.
        /// </summary>
        public ISettingsDirectoryViewModel LeftDirectory { get; set; }

        /// <summary>
        /// Правая директория.
        /// </summary>
        public ISettingsDirectoryViewModel RightDirectory { get; set; }

        /// <summary>
        /// Директории строки отслеживаются.
        /// </summary>
        public bool IsUsed
        {
            get { return _isUsed; }
            set
            {
                if (_isUsed != value)
                {
                    _isUsed = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUsed)));
                }
            }
        }

        /// <summary>
        /// Команда открытия диалога выбора директории.
        /// </summary>
        public ICommand FolderDialogCommand
        {
            get
            {
                if (_folderDialogCommand == null)
                    _folderDialogCommand = new Command<ISettingsDirectoryViewModel>((ISettingsDirectoryViewModel directory) =>
                    {
                        using (var folderDialog = new FolderBrowserDialog())
                            if (folderDialog.ShowDialog() == DialogResult.OK)
                            {
                                if (LeftDirectory == directory)
                                {
                                    LeftDirectory = new SettingsDirectoryViewModel(folderDialog.SelectedPath, false, _leftDirectoryButtonStyleName);
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftDirectory)));
                                }
                                else
                                {
                                    RightDirectory = new SettingsDirectoryViewModel(folderDialog.SelectedPath, false, _rightDirectoryButtonStyleName);
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RightDirectory)));
                                }
                                if(IsEmpty)
                                {
                                    SetEmptyDirectoryEvent?.Invoke();
                                    IsEmpty = false;
                                }
                            }

                    });
                return _folderDialogCommand;
            }
        }

        /// <summary>
        /// Команда удаления строки.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteRowCommand == null)
                    _deleteRowCommand = new Command(action => { DeleteRowEvent?.Invoke(this); });
                return _deleteRowCommand;
            }
        }

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие записи директории в пустую строку.
        /// </summary>
        public event Action SetEmptyDirectoryEvent;

        /// <summary>
        /// Событие удаления строки.
        /// </summary>
        public event Action<ISettingsRowViewModel> DeleteRowEvent;
    }
}