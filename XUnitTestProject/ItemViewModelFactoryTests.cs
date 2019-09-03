using DirectorySync.Models;
using DirectorySync.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class ItemViewModelFactoryTests
    {
        [Fact]
        public void CreateSynchronizedDirectoriesViewModel()
        {
            const string file1Name = "File1";
            const string file2Name = "File2";

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                leftDirectory.CreateFiles(new Dictionary<string, DateTime>
                {
                    { file1Name, DateTime.Now },
                    { file2Name, DateTime.Now }
                });
                rightDirectory.CreateFiles(new Dictionary<string, DateTime>
                {
                    { file1Name, DateTime.Now },
                    { file2Name, DateTime.Now }
                });
                IRowViewModel loadedSynchronizedItemsViewModel = null;

                var synchronizedDirectories = new TestSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new ItemViewModelFactory();
                var synchronizedItemsViewModel = factory.CreateSynchronizedDirectoriesViewModel(synchronizedDirectories);
                synchronizedItemsViewModel.SynchronizedItemsViewModelIsLoadedEvent += new SynchronizedItemsViewModelIsLoaded(
                    delegate (IRowViewModel delegateSynchronizedItemsViewModel) { loadedSynchronizedItemsViewModel = delegateSynchronizedItemsViewModel; });

                // Пока не выполнялась загрузка, события завершения загрузки происходить не должно.
                Assert.Null(loadedSynchronizedItemsViewModel);

                Assert.Empty(synchronizedItemsViewModel.ChildRows);
            }
        }

        [Fact]
        public async Task LoadFiles()
        {
            const string file1Name = "File1";
            const string file2Name = "File2";
            const string file3Name = "File3";
            const string file4Name = "File4";
            const string file5Name = "File5";

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                leftDirectory.CreateFiles(new Dictionary<string, DateTime>
                {
                    { file1Name, DateTime.Now },
                    { file2Name, new DateTime(2019,1 ,1) },
                    { file3Name, new DateTime(2019, 1, 1) },
                    { file4Name, new DateTime(2019, 1, 1) }
                });
                rightDirectory.CreateFiles(new Dictionary<string, DateTime>
                {
                    { file2Name, new DateTime(2019,1 ,1) },
                    { file3Name, new DateTime(2018, 1, 1) },
                    { file4Name, new DateTime(2019, 5, 1) },
                    { file5Name, DateTime.Now }
                });
                IRowViewModel loadedSynchronizedItemsViewModel = null;

                var synchronizedDirectories = new TestSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new ItemViewModelFactory();
                var synchronizedItemsViewModel = factory.CreateSynchronizedDirectoriesViewModel(synchronizedDirectories);
                synchronizedItemsViewModel.SynchronizedItemsViewModelIsLoadedEvent += new SynchronizedItemsViewModelIsLoaded(
                    delegate (IRowViewModel delegateSynchronizedItemsViewModel) { loadedSynchronizedItemsViewModel = delegateSynchronizedItemsViewModel; });
                await synchronizedDirectories.Load();

                Assert.Equal(synchronizedItemsViewModel, loadedSynchronizedItemsViewModel);
                Assert.Equal(5, synchronizedItemsViewModel.ChildRows.Count);
                Assert.False(synchronizedItemsViewModel.Collapsed);

                Assert.Equal(file1Name, synchronizedItemsViewModel.ChildRows[0].LeftItem.Name);
                Assert.Equal(file1Name, synchronizedItemsViewModel.ChildRows[0].RightItem.Name);
                Assert.Equal(ItemStatusEnum.ThereIs, synchronizedItemsViewModel.ChildRows[0].LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Missing, synchronizedItemsViewModel.ChildRows[0].RightItem.Status.StatusEnum);

                Assert.Equal(file2Name, synchronizedItemsViewModel.ChildRows[1].LeftItem.Name);
                Assert.Equal(file2Name, synchronizedItemsViewModel.ChildRows[1].RightItem.Name);
                Assert.Equal(ItemStatusEnum.Equally, synchronizedItemsViewModel.ChildRows[1].LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, synchronizedItemsViewModel.ChildRows[1].RightItem.Status.StatusEnum);

                Assert.Equal(file3Name, synchronizedItemsViewModel.ChildRows[2].LeftItem.Name);
                Assert.Equal(file3Name, synchronizedItemsViewModel.ChildRows[2].RightItem.Name);
                Assert.Equal(ItemStatusEnum.Newer, synchronizedItemsViewModel.ChildRows[2].LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Older, synchronizedItemsViewModel.ChildRows[2].RightItem.Status.StatusEnum);

                Assert.Equal(file4Name, synchronizedItemsViewModel.ChildRows[3].LeftItem.Name);
                Assert.Equal(file4Name, synchronizedItemsViewModel.ChildRows[3].RightItem.Name);
                Assert.Equal(ItemStatusEnum.Older, synchronizedItemsViewModel.ChildRows[3].LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Newer, synchronizedItemsViewModel.ChildRows[3].RightItem.Status.StatusEnum);

                Assert.Equal(file5Name, synchronizedItemsViewModel.ChildRows[4].LeftItem.Name);
                Assert.Equal(file5Name, synchronizedItemsViewModel.ChildRows[4].RightItem.Name);
                Assert.Equal(ItemStatusEnum.Missing, synchronizedItemsViewModel.ChildRows[4].LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.ThereIs, synchronizedItemsViewModel.ChildRows[4].RightItem.Status.StatusEnum);
            }
        }

        private class TestSynchronizedDirectories : ISynchronizedDirectories
        {
            public TestSynchronizedDirectories(string leftDirectoryPath, string rightDirectoryPath)
            {
                LeftDirectory = new TestDirectory(leftDirectoryPath);
                RightDirectory = new TestDirectory(rightDirectoryPath);
            }

            public IDirectory LeftDirectory { get; }

            public IDirectory RightDirectory { get; }

            public async Task Load()
            {
                await LeftDirectory.Load();
                await RightDirectory.Load();
            }
        }

        private class TestDirectory : IDirectory
        {
            private readonly List<IItem> _files;

            public TestDirectory(string path)
            {
                FullPath = path;
                var directoryInfo = new System.IO.DirectoryInfo(FullPath);
                Name = directoryInfo.Name;
                LastUpdate = directoryInfo.LastWriteTime;
                _files = new List<IItem>();
            }

            public IItem[] Items => _files.ToArray();

            public bool IsLoaded { get; private set; }

            public string Name { get; }

            public string FullPath { get; }

            public DateTime LastUpdate { get; }

            public event LoadedDirectory LoadedDirectoryEvent;

            public async Task Load()
            {
                await Task.Run(() =>
                {
                    foreach (var file in System.IO.Directory.GetFiles(FullPath))
                        _files.Add(new TestFile(file));
                });
                IsLoaded = true;
                LoadedDirectoryEvent?.Invoke(this);
            }
        }

        private class TestFile : IItem
        {
            public TestFile(string path)
            {
                var fileInfo = new System.IO.FileInfo(path);
                FullPath = path;
                Name = fileInfo.Name;
                LastUpdate = fileInfo.LastWriteTime;
            }

            public string Name { get; }

            public string FullPath { get; }

            public DateTime LastUpdate { get; }
        }
    }
}