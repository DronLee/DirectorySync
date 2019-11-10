using DirectorySync.Models;
using DirectorySync.Models.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using XUnitTestProject.Infrastructure;

namespace XUnitTestProject
{
    public class SynchronizedDirectoriesManagerTests
    {
        [Fact]
        public void Init()
        {
            var testItemFactory = new TestItemFactory();

            var testSettingsStorage = new TestSettingsStorage();
            testSettingsStorage.SettingsRows = new[]
            {
                new SettingsRow("1", "2", false, null),
                new SettingsRow("3", "4", true, null)
            };

            var synchronizedDirectoriesManager = new SynchronizedDirectoriesManager(testSettingsStorage, testItemFactory);

            Assert.Single(synchronizedDirectoriesManager.SynchronizedDirectories);
            var synchronizedDirectory = synchronizedDirectoriesManager.SynchronizedDirectories[0];
            Assert.Equal("3", synchronizedDirectory.LeftDirectory.FullPath);
            Assert.Equal("4", synchronizedDirectory.RightDirectory.FullPath);
        }

        /// <summary>
        /// Тест на акутализацию коллекции синхронизируемых директорий во время загрузки при изменении настроек.
        /// </summary>
        [Fact]
        public async Task Load()
        {
            var testItemFactory = new TestItemFactory();
            var testSettingsStorage = new TestSettingsStorage();
            ISynchronizedDirectories removedSynchronizedDirectories = null;

            using (var testDirectory = new TestDirectory())
            {
                var settingsRow1 = new SettingsRow(testDirectory.CreateDirectory("1"), testDirectory.CreateDirectory("2"), true, null);
                var settingsRow2 = new SettingsRow(testDirectory.CreateDirectory("3"), testDirectory.CreateDirectory("4"), false, null);
                var settingsRow3 = new SettingsRow(testDirectory.CreateDirectory("5"), testDirectory.CreateDirectory("6"), true, new[] { "tiff" });
                var settingsRow4 = new SettingsRow(testDirectory.CreateDirectory("7"), testDirectory.CreateDirectory("8"), true, new[] { "tiff" });

                testSettingsStorage.SettingsRows = new[]
                {
                    settingsRow1,
                    settingsRow2,
                    settingsRow3,
                    settingsRow4
                };

                var synchronizedDirectoriesManager = new SynchronizedDirectoriesManager(testSettingsStorage, testItemFactory);
                var oldSynchronizedDirectory2 = synchronizedDirectoriesManager.SynchronizedDirectories[1];
                var oldSynchronizedDirectory3 = synchronizedDirectoriesManager.SynchronizedDirectories[2];

                synchronizedDirectoriesManager.RemoveSynchronizedDirectoriesEvent += (ISynchronizedDirectories synchronizedDirectories) =>
                {
                    removedSynchronizedDirectories = synchronizedDirectories;
                };
                settingsRow1.IsUsed = false; // Эта строка при загрузке должна будет удалиться.
                settingsRow2.IsUsed = true; // Эта строка при загрузке должна будет добавиться.
                
                // Эта строка при загрузке должна будет инициализировать новую из-за изменения коллекции ExcludedExtensions.
                settingsRow3.ExcludedExtensions = new[] { "jpg" };

                await synchronizedDirectoriesManager.Load();

                Assert.Equal(3, synchronizedDirectoriesManager.SynchronizedDirectories.Length);

                // Проверим удалённую директорию.
                Assert.NotNull(removedSynchronizedDirectories);
                Assert.Equal(settingsRow1.LeftDirectory.DirectoryPath, removedSynchronizedDirectories.LeftDirectory.FullPath);
                Assert.Equal(settingsRow1.RightDirectory.DirectoryPath, removedSynchronizedDirectories.RightDirectory.FullPath);

                // Эта запись на синхронизируемую директорию должна быть новой, но соответсвовать изначальной третьей записи.
                var synchronizedDirectory1 = synchronizedDirectoriesManager.SynchronizedDirectories[0];
                Assert.NotEqual(oldSynchronizedDirectory2, synchronizedDirectory1);
                Assert.Equal(oldSynchronizedDirectory2.LeftDirectory.FullPath, synchronizedDirectory1.LeftDirectory.FullPath);
                Assert.Equal(oldSynchronizedDirectory2.RightDirectory.FullPath, synchronizedDirectory1.RightDirectory.FullPath);
                Assert.True(synchronizedDirectory1.IsLoaded);

                // Эта запись на синхронизируемую директорию не должна была обновляться.
                Assert.Equal(oldSynchronizedDirectory3, synchronizedDirectoriesManager.SynchronizedDirectories[1]);

                // Эта запись на синхронизируемую директорию должна быть добавленной из-за включения строки настройки.
                var synchronizedDirectory3 = synchronizedDirectoriesManager.SynchronizedDirectories[2];
                Assert.Equal(settingsRow2.LeftDirectory.DirectoryPath, synchronizedDirectory3.LeftDirectory.FullPath);
                Assert.Equal(settingsRow2.RightDirectory.DirectoryPath, synchronizedDirectory3.RightDirectory.FullPath);
                Assert.True(synchronizedDirectory3.IsLoaded);
            }
        }

        [Theory]
        [InlineData(null, 6)]
        [InlineData(new[] { "tiff" }, 3)]
        [InlineData(new[] { "jpg" }, 4)]
        [InlineData(new[] { "jpg", "tiff" }, 1)]
        public async Task LoadWithExcludedExtensions(string[] excludedExtensions, byte loadedFilesCount)
        {
            var testSettingsStorage = new TestSettingsStorage();

            using (var testDirectory = new TestDirectory())
            {
                var fileLastUpdate = DateTime.Now;

                var leftDirectory = testDirectory.CreateDirectory("1");
                TestDirectory.CreateFiles(leftDirectory, new System.Collections.Generic.Dictionary<string, DateTime>
                {
                    { "1.tiff", fileLastUpdate },
                    { "2.tiff", fileLastUpdate },
                    { "3.tiff", fileLastUpdate },
                    { "4.jpg", fileLastUpdate },
                    { "5.jpg", fileLastUpdate },
                    { "6.png", fileLastUpdate }
                });

                var rightDirectory = testDirectory.CreateDirectory("2");
                TestDirectory.CreateFiles(rightDirectory, new System.Collections.Generic.Dictionary<string, DateTime>
                {
                    { "1.tiff", fileLastUpdate },
                    { "2.tiff", fileLastUpdate },
                    { "3.tiff", fileLastUpdate },
                    { "4.jpg", fileLastUpdate },
                    { "5.jpg", fileLastUpdate },
                    { "6.png", fileLastUpdate }
                });

                var settingsRow1 = new SettingsRow(leftDirectory, rightDirectory, true, excludedExtensions);
                testSettingsStorage.SettingsRows = new[] { settingsRow1 };

                var synchronizedDirectoriesManager = new SynchronizedDirectoriesManager(testSettingsStorage, new ItemFactory());
                await synchronizedDirectoriesManager.Load();

                Assert.Single(synchronizedDirectoriesManager.SynchronizedDirectories);
                var synchronizedDirectory = synchronizedDirectoriesManager.SynchronizedDirectories.Single();
                Assert.Equal(loadedFilesCount, synchronizedDirectory.LeftDirectory.Items.Length);
                Assert.Equal(loadedFilesCount, synchronizedDirectory.RightDirectory.Items.Length);
            }
        }

        private class TestItemFactory : IItemFactory
        {
            public IDirectory CreateDirectory(string directoryPath, string[] excludedExtensions)
            {
                return new TestDirectoryModel(directoryPath, excludedExtensions);
            }

            public IItem CreateFile(string filePath)
            {
                throw new NotImplementedException();
            }
        }

        private class TestDirectoryModel : IDirectory
        {
            public TestDirectoryModel(string directoryPath, string[] excludedExtensions)
            {
                FullPath = directoryPath;
                ExcludedExtensions = excludedExtensions;
            }

            public IItem[] Items => throw new NotImplementedException();

            public bool IsLoaded { get; private set; }

            public string Name => throw new NotImplementedException();

            public string FullPath { get; private set; }

            public DateTime LastUpdate => throw new NotImplementedException();

            public string LastLoadError => throw new NotImplementedException();

            public string[] ExcludedExtensions { get; }

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

            public Task Load()
            {
                IsLoaded = true;
                return Task.CompletedTask;
            }
        }

        private class TestSettingsStorage : ISettingsStorage
        {
            public ISettingsRow[] SettingsRows { get; set; }

            public ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed, string[] excludedExtensions)
            {
                throw new NotImplementedException();
            }

            public void Save()
            {
                throw new NotImplementedException();
            }
        }
    }
}
