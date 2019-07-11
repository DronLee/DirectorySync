using System;
using System.ComponentModel;
using System.Windows.Input;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    internal class SynchronizedFileViewModel : ISynchronizedItemViewModel
    {
        private readonly IItem _file;

        internal SynchronizedFileViewModel(IItem file)
        {
            _file = file;
        }

        public string Name => throw new NotImplementedException();

        public string IconPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICommand AcceptCommand => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}