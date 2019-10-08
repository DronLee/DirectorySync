using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DirectorySync.Models.Settings;

namespace DirectorySync.ViewModels.Settings
{
    public class SettingsViewModel : ISettingsViewModel
    {
        private readonly ISettingsStorage _settingsStorage;

        private ICommand _okCommand = null;

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

        private string _comment;

        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comment)));
            }
        }
        public bool Ok { get; set; }

        public ObservableCollection<ISettingsRowViewModel> SettingsRows { get; set; }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                    _okCommand = new Command<Window>((Window window) =>
                    {
                        if (SettingsRows.Any(r => !r.IsEmpty && (r.LeftDirectory.DirectoryPath == null || r.RightDirectory.DirectoryPath == null)))
                        {
                            CommentType = MessageTypeEnum.Warning;
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommentType)));
                            Comment = "Не для всех директорий указаны пары. Уберите директории без пары из списка или отключите их.";
                        }
                        else
                        {
                            _settingsStorage.SettingsRows = SettingsRows.Where(r => !r.IsEmpty).Select(r =>
                                  _settingsStorage.CreateSettingsRow(r.LeftDirectory.DirectoryPath, r.RightDirectory.DirectoryPath, r.IsUsed)
                            ).ToArray();
                            _settingsStorage.Save();
                            Ok = true;
                            window.Close();
                        }
                    });
                return _okCommand;
            }
        }
        public MessageTypeEnum CommentType { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

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