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
                new SettingsRow("1", "2", false),
                new SettingsRow("3", "4", true)
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
                var settingsRow1 = new SettingsRow(testDirectory.CreateDirectory("1"), testDirectory.CreateDirectory("2"), true);
                var settingsRow2 = new SettingsRow(testDirectory.CreateDirectory("3"), testDirectory.CreateDirectory("4"), false);

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

        private class TestItemFactory : IItemFactory
        {
            public IDirectory CreateDirectory(string directoryPath)
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

            public event Action<IDirectory> LoadedDirectoryEvent;
            public event Action DeletedEvent;
            public event Action<string> SyncErrorEvent;

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

            public ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed)
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
