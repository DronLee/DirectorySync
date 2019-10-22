using DirectorySync.Models;
using DirectorySync.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class RowViewModelFactoryTests
    {
        [Fact]
        public void CreateRowViewModel()
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
                IRowViewModel loadedRowViewModel = null;

                var synchronizedDirectories = new TestSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new RowViewModelFactory(null);
                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);
                rowViewModel.RowViewModelIsLoadedEvent += (IRowViewModel delegateRowViewModel) => 
                    { loadedRowViewModel = delegateRowViewModel; };

                // Пока не выполнялась загрузка, события завершения загрузки происходить не должно.
                Assert.Null(loadedRowViewModel);

                Assert.Empty(rowViewModel.ChildRows);
            }
        }

        /// <summary>
        /// Проверка создания моделей представлений элементов при загрузке файлов.
        /// </summary>
        /// <returns></returns>
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
                IRowViewModel loadedRowViewModel = null;

                var synchronizedDirectories = new TestSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new RowViewModelFactory(new ItemViewModelMatcher());
                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);
                rowViewModel.RowViewModelIsLoadedEvent += (IRowViewModel delegateRowViewModel) =>
                    { loadedRowViewModel = delegateRowViewModel; };
                await synchronizedDirectories.Load();

                Assert.Equal(rowViewModel, loadedRowViewModel);
                Assert.Equal(5, rowViewModel.ChildRows.Count);
                Assert.False(rowViewModel.IsExpanded);

                var row = rowViewModel.ChildRows[0];
                Assert.Equal(file1Name, row.LeftItem.Name);
                Assert.Equal(file1Name, row.RightItem.Name);
                Assert.Equal(ItemStatusEnum.ThereIs, row.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Missing, row.RightItem.Status.StatusEnum);
                Assert.NotNull(row.LeftItem.AcceptCommand);
                Assert.NotNull(row.RightItem.AcceptCommand);

                row = rowViewModel.ChildRows[1];
                Assert.Equal(file2Name, row.LeftItem.Name);
                Assert.Equal(file2Name, row.RightItem.Name);
                Assert.Equal(ItemStatusEnum.Equally, row.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, row.RightItem.Status.StatusEnum);
                Assert.Null(row.LeftItem.AcceptCommand);
                Assert.Null(row.RightItem.AcceptCommand);

                row = rowViewModel.ChildRows[2];
                Assert.Equal(file3Name, row.LeftItem.Name);
                Assert.Equal(file3Name, row.RightItem.Name);
                Assert.Equal(ItemStatusEnum.Newer, row.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Older, row.RightItem.Status.StatusEnum);
                Assert.NotNull(row.LeftItem.AcceptCommand);
                Assert.NotNull(row.RightItem.AcceptCommand);

                row = rowViewModel.ChildRows[3];
                Assert.Equal(file4Name, row.LeftItem.Name);
                Assert.Equal(file4Name, row.RightItem.Name);
                Assert.Equal(ItemStatusEnum.Older, row.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Newer, row.RightItem.Status.StatusEnum);
                Assert.NotNull(row.LeftItem.AcceptCommand);
                Assert.NotNull(row.RightItem.AcceptCommand);

                row = rowViewModel.ChildRows[4];
                Assert.Equal(file5Name, row.LeftItem.Name);
                Assert.Equal(file5Name, row.RightItem.Name);
                Assert.Equal(ItemStatusEnum.Missing, row.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.ThereIs, row.RightItem.Status.StatusEnum);
                Assert.NotNull(row.LeftItem.AcceptCommand);
                Assert.NotNull(row.RightItem.AcceptCommand);
            }
        }

        /// <summary>
        /// Проверка создания моделей представлений элементов при загрузке,
        /// один из которых файл, второй директория, и имеют одинаковые наименования. 
        /// </summary>
        [Fact]
        public async Task LoadDirectoryAndFile()
        {
            const string directoryAndFileName = "Item";

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                leftDirectory.CreateFiles(new Dictionary<string, DateTime>
                {
                    { directoryAndFileName, DateTime.Now }
                });
                rightDirectory.CreateDirectory(directoryAndFileName);

                var synchronizedDirectories = new TestSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new RowViewModelFactory(new ItemViewModelMatcher());
                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);

                await synchronizedDirectories.Load();


                Assert.Equal(2, rowViewModel.ChildRows.Count);
                Assert.False(rowViewModel.IsExpanded);

                // Сначала директория, потом файл.
                var childRow1 = rowViewModel.ChildRows[0];
                Assert.Equal(directoryAndFileName, childRow1.LeftItem.Name);
                Assert.Equal(directoryAndFileName, childRow1.RightItem.Name);
                Assert.NotNull(childRow1.RightItem.Directory);
                Assert.True(childRow1.RightItem.IsDirectory);

                // Даже если элемент отсутствует, а присутствующий является директорией, то и этот должен быть директорией.
                Assert.Null(childRow1.LeftItem.Directory);
                Assert.True(childRow1.LeftItem.IsDirectory);

                Assert.Equal(ItemStatusEnum.Missing, childRow1.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.ThereIs, childRow1.RightItem.Status.StatusEnum);
                var childRow2 = rowViewModel.ChildRows[1];
                Assert.Equal(directoryAndFileName, childRow2.LeftItem.Name);
                Assert.Equal(directoryAndFileName, childRow2.RightItem.Name);
                Assert.Null(childRow2.LeftItem.Directory);
                Assert.False(childRow2.LeftItem.IsDirectory);
                Assert.Null(childRow2.RightItem.Directory);
                Assert.False(childRow2.RightItem.IsDirectory);
                Assert.Equal(ItemStatusEnum.ThereIs, childRow2.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Missing, childRow2.RightItem.Status.StatusEnum);
            }
        }

        /// <summary>
        /// Проверка создания моделей представлений элементов при загрузке пустых директорий.
        /// Проверяется, что у директорий проставились статусы в соответствии с их отношением друг к другу.
        /// </summary>
        [Theory]
        [InlineData("2019-01-01", "2018-01-01", "Newer", "Older")]
        [InlineData("2018-01-01", "2019-01-01", "Older", "Newer")]
        [InlineData("2019-01-01", "2019-01-01", "Equally", "Equally")]
        public async Task LoadEmptyDirectories(string strLeftDirectoryLastUpdate, string strRightDirectoryLastUpdate,
            string expectedLeftStratus, string expectedRightStratus)
        {
            const string directoryName = "Directory";

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                leftDirectory.CreateDirectory(directoryName, DateTime.Parse(strLeftDirectoryLastUpdate));
                rightDirectory.CreateDirectory(directoryName, DateTime.Parse(strRightDirectoryLastUpdate));

                var synchronizedDirectories = new TestSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new RowViewModelFactory(new ItemViewModelMatcher());
                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);

                await synchronizedDirectories.Load();

                Assert.Single(rowViewModel.ChildRows);
                Assert.False(rowViewModel.IsExpanded);

                var childRow = rowViewModel.ChildRows[0];
                Assert.Equal(directoryName, childRow.LeftItem.Name);
                Assert.Equal(directoryName, childRow.RightItem.Name);
                Assert.NotNull(childRow.LeftItem.Directory);
                Assert.NotNull(childRow.RightItem.Directory);
                Assert.Equal(expectedLeftStratus, childRow.LeftItem.Status.StatusEnum.ToString());
                Assert.Equal(expectedRightStratus, childRow.RightItem.Status.StatusEnum.ToString());
            }
        }

        /// <summary>
        /// Проверка создания моделей представлений элементов при загрузке директорий.
        /// В частности проверяется, что статусы дочерних элементов влияют на статусы родительских.
        /// </summary>
        [Fact]
        public async Task LoadDirectories_RefreshStatusesFromChilds()
        {
            const string directoryName = "Directory";
            const string fileName = "File";

            var newerDate = new DateTime(2019, 1, 1);
            var olderDate = new DateTime(2018, 1, 1);

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                var childLeftDirectoryPath = leftDirectory.CreateDirectory(directoryName, newerDate);
                var childRightDirectoryPath = rightDirectory.CreateDirectory(directoryName, olderDate);

                // Хотя левая директория новее, но содержимое её будет старше.
                // Получается после загрузки левая директория должна будте получить статус Older.  
                Infrastructure.TestDirectory.CreateFiles(childLeftDirectoryPath, new Dictionary<string, DateTime> {
                    { fileName, olderDate } });
                Infrastructure.TestDirectory.CreateFiles(childRightDirectoryPath, new Dictionary<string, DateTime> {
                    { fileName, newerDate } });

                var synchronizedDirectories = new TestSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new RowViewModelFactory(new ItemViewModelMatcher());
                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);

                await synchronizedDirectories.Load();

                Assert.Single(rowViewModel.ChildRows);
                Assert.False(rowViewModel.IsExpanded);

                var childRow = rowViewModel.ChildRows[0];
                Assert.Equal(directoryName, childRow.LeftItem.Name);
                Assert.Equal(directoryName, childRow.RightItem.Name);
                Assert.NotNull(childRow.LeftItem.Directory);
                Assert.NotNull(childRow.RightItem.Directory);
                Assert.Single(childRow.ChildRows);

                // Это файлы.
                Assert.Null(childRow.ChildRows[0].LeftItem.Directory);
                Assert.Null(childRow.ChildRows[0].RightItem.Directory);

                // Файл правой новее, соответственно и статус правой Newer.
                Assert.Equal(ItemStatusEnum.Older, childRow.ChildRows[0].LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Newer, childRow.ChildRows[0].RightItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Older, childRow.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Newer, childRow.RightItem.Status.StatusEnum);
            }
        }

        /// <summary>
        /// Проверка создания моделей представлений элементов при загрузке директорий.
        /// Одна директория пустая, вторая - нет.
        /// </summary>
        [Fact]
        public async Task LoadDirectories_OneEmptyDirectory()
        {
            const string directoryName = "Directory";
            const string fileName = "File";

            var newerDate = new DateTime(2019, 1, 1);
            var olderDate = new DateTime(2018, 1, 1);

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                var childLeftDirectoryPath = leftDirectory.CreateDirectory(directoryName, newerDate);
                Infrastructure.TestDirectory.CreateFiles(childLeftDirectoryPath, new Dictionary<string, DateTime> {
                    { fileName, DateTime.Now } });

                rightDirectory.CreateDirectory(directoryName, olderDate);

                var synchronizedDirectories = new TestSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new RowViewModelFactory(new ItemViewModelMatcher());
                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);

                await synchronizedDirectories.Load();

                Assert.Single(rowViewModel.ChildRows);
                Assert.False(rowViewModel.IsExpanded);

                var childRow = rowViewModel.ChildRows[0];
                Assert.Equal(directoryName, childRow.LeftItem.Name);
                Assert.Equal(directoryName, childRow.RightItem.Name);
                Assert.NotNull(childRow.LeftItem.Directory);
                Assert.NotNull(childRow.RightItem.Directory);
                Assert.Single(childRow.ChildRows);

                // Это файлы.
                Assert.Null(childRow.ChildRows[0].LeftItem.Directory);
                Assert.Null(childRow.ChildRows[0].RightItem.Directory);

                // В првавой директории файла нет, соответственно и статус правой Missing.
                Assert.Equal(ItemStatusEnum.ThereIs, childRow.ChildRows[0].LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Missing, childRow.ChildRows[0].RightItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.ThereIs, childRow.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Missing, childRow.RightItem.Status.StatusEnum);
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

            public bool IsLoaded => throw new NotImplementedException();

            public async Task Load()
            {
                await LeftDirectory.Load();
                await RightDirectory.Load();
            }
        }

        private class TestDirectory : IDirectory
        {
            private readonly List<IItem> _items;

            public TestDirectory(string path)
            {
                FullPath = path;
                var directoryInfo = new System.IO.DirectoryInfo(FullPath);
                Name = directoryInfo.Name;
                LastUpdate = directoryInfo.LastWriteTime;
                _items = new List<IItem>();
            }

            public IItem[] Items => _items.ToArray();

            public bool IsLoaded { get; private set; }

            public string Name { get; }

            public string FullPath { get; }

            public DateTime LastUpdate { get; }

            public string LastLoadError { get; }

            public event Action<IDirectory> LoadedDirectoryEvent;
            public event Action<IItem> DeletedEvent;
            public event Action<string> SyncErrorEvent;
            public event Action<IItem, string> CopiedFromToEvent;

            public Task CopyTo(string destinationPath)
            {
                throw new NotImplementedException();
            }

            public Task Delete()
            {
                throw new NotImplementedException();
            }

            public async Task Load()
            {
                await Task.Run(() =>
                {
                    foreach (var directoryPath in System.IO.Directory.GetDirectories(FullPath))
                        _items.Add(new TestDirectory(directoryPath));
                });

                await Task.Run(() =>
                {
                    foreach (var file in System.IO.Directory.GetFiles(FullPath))
                        _items.Add(new TestFile(file));
                });

                foreach (IDirectory directory in _items.Where(i => i is IDirectory))
                    await directory.Load();

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

            public event Action<IItem> DeletedEvent;
            public event Action<string> SyncErrorEvent;
            public event Action<IItem, string> CopiedFromToEvent;

            public Task CopyTo(string destinationPath)
            {
                throw new NotImplementedException();
            }

            public Task Delete()
            {
                throw new NotImplementedException();
            }
        }
    }
}