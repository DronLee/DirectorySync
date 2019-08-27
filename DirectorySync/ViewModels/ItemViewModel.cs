using System;
using System.ComponentModel;
using System.Windows.Input;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class ItemViewModel : IItemViewModel
    {
        private readonly IItem _item;

        public ItemViewModel(string name)
        {
            _item = null;
            Name = name;
        }

        public ItemViewModel(IItem item)
        {
            _item = item;
            Name = item.Name;
        }

        public string Name { get; }

        public string IconPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICommand AcceptCommand => throw new NotImplementedException();

        public ItemStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}