using System;

namespace DirectorySync.Models
{
    internal class File : IItem
    {
        internal File(string fullPath)
        {
            FullPath = fullPath;
            var info = new System.IO.FileInfo(fullPath);
            Name = info.Name;
            LastUpdate = info.LastWriteTime;
        }

        public string Name { get; }

        public string FullPath { get; }

        public DateTime LastUpdate { get; }
    }
}