using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class SynchronizedDirectoryViewModel : ISynchronizedDirectoryViewModel
    {
        private readonly IItemViewModelFactory _itemViewModelFactory;
        private readonly IDirectory _directory;

        public SynchronizedDirectoryViewModel(IItemViewModelFactory itemViewModelFactory, IDirectory directory)
        {
            _itemViewModelFactory = itemViewModelFactory;
            _directory = directory;
            _directory.ItemCollectionChangedEvent += ItemCollectionChanged;
        }

        public bool Collapsed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISynchronizedItemViewModel[] Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => throw new NotImplementedException();

        public string IconPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICommand AcceptCommand => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;

        private void ItemCollectionChanged()
        {
            Items = _directory.Items.Select(i => _itemViewModelFactory.CreateItemViewModel(i)).ToArray();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Items"));
        }
    }
}