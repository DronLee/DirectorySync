using DirectorySync.Models.Settings;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace DirectorySync.ViewModels.Settings
{
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

        public SettingsRowViewModel()
        {
            IsEmpty = true;
            LeftDirectory = new SettingsDirectoryViewModel();
            RightDirectory = new SettingsDirectoryViewModel();
        }

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

        public bool IsEmpty { get; set; }
        public ISettingsDirectoryViewModel LeftDirectory { get; set; }
        public ISettingsDirectoryViewModel RightDirectory { get; set; }
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

        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteRowCommand == null)
                    _deleteRowCommand = new Command(action => { DeleteRowEvent?.Invoke(this); });
                return _deleteRowCommand;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action SetEmptyDirectoryEvent;
        public event Action<ISettingsRowViewModel> DeleteRowEvent;
    }
}