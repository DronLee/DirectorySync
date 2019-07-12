using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Items = new ObservableCollection<IItemViewModel>();
        }

        public ItemViewModel(IItem item)
        {
            _item = item;
            Name = item.Name;
            Items = new ObservableCollection<IItemViewModel>();
        }

        public string Name { get; }

        public ObservableCollection<IItemViewModel> Items { get; }

        public string IconPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICommand AcceptCommand => throw new NotImplementedException();

        public ItemStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddItems(IItemViewModel[] items)
        {
            foreach (var item in items)
                Items.Add(item);
        }
    }
}