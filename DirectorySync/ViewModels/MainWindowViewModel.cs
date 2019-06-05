using System;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        public IDirectory[] LeftDirectories => throw new NotImplementedException();

        public IDirectory[] RightDirectories => throw new NotImplementedException();
    }
}