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

                testSettingsStorage.SettingsRows = new[]
                {
                    settingsRow1,
                    settingsRow2
                };

                var synchronizedDirectoriesManager = new SynchronizedDirectoriesManager(testSettingsStorage, testItemFactory);
                synchronizedDirectoriesManager.RemoveSynchronizedDirectoriesEvent += (ISynchronizedDirectories synchronizedDirectories) =>
                {
                    removedSynchronizedDirectories = synchronizedDirectories;
                };
                settingsRow1.IsUsed = false; // Это строка при загрузке должна будет удалиться.
                settingsRow2.IsUsed = true; // Это строка при загрузке должна будет добавиться.
                await synchronizedDirectoriesManager.Load();

                Assert.Single(synchronizedDirectoriesManager.SynchronizedDirectories);
                var synchronizedDirectory = synchronizedDirectoriesManager.SynchronizedDirectories.Single();
                Assert.Equal(settingsRow2.LeftDirectory.DirectoryPath, synchronizedDirectory.LeftDirectory.FullPath);
                Assert.Equal(settingsRow2.RightDirectory.DirectoryPath, synchronizedDirectory.RightDirectory.FullPath);
                Assert.True(synchronizedDirectory.IsLoaded);

                // Проверим удалённую директорию.
                Assert.NotNull(removedSynchronizedDirectories);
                Assert.Equal(settingsRow1.LeftDirectory.DirectoryPath, removedSynchronizedDirectories.LeftDirectory.FullPath);
                Assert.Equal(settingsRow1.RightDirectory.DirectoryPath, removedSynchronizedDirectories.RightDirectory.FullPath);
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
                return new TestDirectoryModel(directoryPath);
            }

            public IItem CreateFile(string filePath)
            {
                throw new NotImplementedException();
            }
        }

        private class TestDirectoryModel : IDirectory
        {
            public TestDirectoryModel(string directoryPath)
            {
                FullPath = directoryPath;
            }

            public IItem[] Items => throw new NotImplementedException();

            public bool IsLoaded { get; private set; }

            public string Name => throw new NotImplementedException();

            public string FullPath { get; private set; }

            public DateTime LastUpdate => throw new NotImplementedException();

            public string LastLoadError => throw new NotImplementedException();

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
