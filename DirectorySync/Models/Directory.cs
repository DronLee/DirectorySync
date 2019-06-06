using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    internal class Directory : IDirectory
    {
        private readonly IItemFactory _itemFactory;
        private readonly List<IItem> items;

        internal Directory(string fullPath, IItemFactory itemFactory)
        {
            FullPath = fullPath;
            var info = new System.IO.DirectoryInfo(fullPath);
            Name = info.Name;
            LastUpdate = info.LastWriteTime;

            _itemFactory = itemFactory;
        }

        public IItem[] Items => items.ToArray();

        public string Name { get; }

        public string FullPath { get; }

        public DateTime LastUpdate { get; }

        public async Task Load()
        {
            await Task.Run(() =>
            {
                foreach (var directoryPath in System.IO.Directory.GetDirectories(FullPath))
                    items.Add(_itemFactory.CreateDirectory(directoryPath));
            });

            await Task.Run(() =>
            {
                foreach (var filePath in System.IO.Directory.GetFiles(FullPath))
                    items.Add(_itemFactory.CreateFile(filePath));
            });
        }
    }
}