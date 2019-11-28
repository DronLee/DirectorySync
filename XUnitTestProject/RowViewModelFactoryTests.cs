using DirectorySync.Models;
using DirectorySync.Models.Settings;
using DirectorySync.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class RowViewModelFactoryTests
    {
        /// <summary>
        /// Тестирование на простое создание модели представления строки без всякой загрузки.
        /// </summary>
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
                bool useAddRowEvent = false;

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new RowViewModelFactory();
                factory.AddRowEvent += (IRowViewModel parent, IRowViewModel child) => { useAddRowEvent = true; };
                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);

                // Пока не выполнялась загрузка, события добавления записи происходить не должны.
                Assert.False(useAddRowEvent);

                Assert.Empty(rowViewModel.ChildRows);
            }
        }

        /// <summary>
        /// Тестирование на построение дерева моделей представлений строк при загрузке.
        /// </summary>
        [Fact]
        public async Task CreateRowViewModel_Tree()
        {
            const string name1 = "1";
            const string name2 = "2";
            byte useAddRowsCount = 0;

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(name1), new Dictionary<string, DateTime>
                {
                    { name1, DateTime.Now }
                });
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(name1), new Dictionary<string, DateTime>
                {
                    { name1, DateTime.Now }
                });

                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(name2), new Dictionary<string, DateTime>
                {
                    { name1, DateTime.Now }
                });
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(name2), new Dictionary<string, DateTime>
                {
                    { name1, DateTime.Now }
                });

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                var factory = new RowViewModelFactory();
                factory.AddRowEvent += (IRowViewModel parent, IRowViewModel child) => { useAddRowsCount++; };
                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);

                await synchronizedDirectories.Load();

                Assert.Equal(4, useAddRowsCount); // Две директории, в каждой директории по одному файлу.

                Assert.Empty(rowViewModel.ChildRows); // А записей не прибавилось, потому что фабрика их не прибавляет.
            }
        }

        /// <summary>
        /// Проверка срабатывания события удаления строки при удалении синхронизируемых элементов директорий.
        /// </summary>
        [Fact]
        public async Task DeleteDirectoryRowEvent()
        {
            IRowViewModel deleteRowEventParent = null, deleteRowEventChild = null;

            using (var testDirectory = new Infrastructure.TestDirectory())
            {
                Infrastructure.TestDirectory.CreateFiles(testDirectory.CreateDirectory("Dir"), new Dictionary<string, DateTime>
                {
                    { "File", DateTime.Now }
                });

                var synchronizedDirectories = new TestSynchronizedItems(new Directory(testDirectory.FullPath, null, new ItemFactory()));
                var factory = new RowViewModelFactory();
                factory.DeleteRowEvent += (IRowViewModel parent, IRowViewModel child) =>
                {
                    deleteRowEventParent = parent;
                    deleteRowEventChild = child;
                };

                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);

                await synchronizedDirectories.Load();

                var level1Child = synchronizedDirectories.ChildItems[0];
                var level2Child = level1Child.ChildItems[0];

                ((TestSynchronizedItems)level1Child).Delete();

                Assert.NotNull(deleteRowEventParent);
                Assert.NotNull(deleteRowEventChild);
                Assert.Equal(synchronizedDirectories.LeftItem.Name, deleteRowEventParent.LeftItem.Name);
                Assert.Equal(level1Child.LeftItem.Name, deleteRowEventChild.LeftItem.Name);
            }
        }

        /// <summary>
        /// Проверка срабатывания события удаления строки при удалении синхронизируемых элементов файлов.
        /// </summary>
        [Fact]
        public async Task DeleteFileRowEvent()
        {
            IRowViewModel deleteRowEventParent = null, deleteRowEventChild = null;

            using (var testDirectory = new Infrastructure.TestDirectory())
            {
                Infrastructure.TestDirectory.CreateFiles(testDirectory.CreateDirectory("Dir"), new Dictionary<string, DateTime>
                {
                    { "File", DateTime.Now }
                });

                var synchronizedDirectories = new TestSynchronizedItems(new Directory(testDirectory.FullPath, null, new ItemFactory()));
                var factory = new RowViewModelFactory();
                factory.DeleteRowEvent += (IRowViewModel parent, IRowViewModel child) => 
                {
                    deleteRowEventParent = parent;
                    deleteRowEventChild = child;
                };

                var rowViewModel = factory.CreateRowViewModel(synchronizedDirectories);

                await synchronizedDirectories.Load();

                var level1Child = synchronizedDirectories.ChildItems[0];
                var level2Child = level1Child.ChildItems[0];

                ((TestSynchronizedItems)level2Child).Delete();

                Assert.NotNull(deleteRowEventParent);
                Assert.NotNull(deleteRowEventChild);
                Assert.Equal(level1Child.LeftItem.Name, deleteRowEventParent.LeftItem.Name);
                Assert.Equal(level2Child.LeftItem.Name, deleteRowEventChild.LeftItem.Name);
            }
        }

        /// <summary>
        /// Упрощённая модель синхронизируемых элементов, в которой не важно содержимое, главное,
        /// чтобы работала загрузка, создавая дочерние элементы, и была возможность активировать событие удаления.
        /// </summary>
        private class TestSynchronizedItems : ISynchronizedItems
        {
            private readonly IItem _item;

            public TestSynchronizedItems(IItem item)
            {
                _item = item;
                LeftItem = new SynchronizedItem(item.FullPath, item is IDirectory, item);
                RightItem = new SynchronizedItem(item.FullPath, item is IDirectory, item);
            }

            public bool IsLoaded => throw new NotImplementedException();

            public IDirectory LeftDirectory => throw new NotImplementedException();

            public IDirectory RightDirectory => throw new NotImplementedException();

            public ISynchronizedItem LeftItem { get; }

            public ISynchronizedItem RightItem { get; }

            public List<ISynchronizedItems> ChildItems { get; private set; } = new List<ISynchronizedItems>();

            public event Action<ISynchronizedItems> DirectoriesIsLoadedEvent;
            public event Action<ISynchronizedItems> DeleteEvent;

            public async Task Load()
            {
                await _item.Load();
                LoadChildItems();
                DirectoriesIsLoadedEvent?.Invoke(this);
            }

            public void LoadChildItems()
            {
                if (_item is IDirectory)
                    foreach (var item in ((IDirectory)_item).Items)
                    {
                        var child = new TestSynchronizedItems(item);
                        ChildItems.Add(child);
                        child.LoadChildItems();
                    }
            }

            public void LoadRequired()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Нужен, чтобы проверить реакцию на событие удаления синхронизируемых элементов.
            /// </summary>
            public void Delete()
            {
                DeleteEvent?.Invoke(this);
            }
        }

        private SynchronizedItems GetSynchronizedDirectories(string leftDirectoryPath, string rightDirectoryPath)
        {
            var settingsRow = new TestSettingsRow
            {
                LeftDirectory = new SettingsDirectory(leftDirectoryPath),
                RightDirectory = new SettingsDirectory(rightDirectoryPath)
            };

            return new SynchronizedItems(settingsRow, new SynchronizedItemFactory(new ItemFactory()), new SynchronizedItemMatcher());
        }

        private class TestSettingsRow : ISettingsRow
        {
            public SettingsDirectory LeftDirectory { get; set; }

            public SettingsDirectory RightDirectory { get; set; }

            public bool IsUsed { get; set; }

            public string[] ExcludedExtensions { get; set; }

            public void NotFoundRefresh()
            {
                throw new NotImplementedException();
            }
        }
    } 
}