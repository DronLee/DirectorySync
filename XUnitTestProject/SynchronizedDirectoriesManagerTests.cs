using DirectorySync.Models;
using DirectorySync.Models.Settings;
using System;
using System.Collections.Generic;
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
            var testSettingsStorage = new TestSettingsStorage();
            testSettingsStorage.SettingsRows = new[]
            {
                new SettingsRow("1", "2", false, null),
                new SettingsRow("3", "4", true, null)
            };

            var synchronizedDirectoriesManager = new SynchronizedDirectoriesManager(testSettingsStorage, new SynchronizedItemFactory(new ItemFactory()), null);

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
            var testSettingsStorage = new TestSettingsStorage();
            ISynchronizedItems removedSynchronizedDirectories = null;
            var loadedDirectories = new List<IDirectory>();

            using (var testDirectory = new TestDirectory())
            {
                var settingsRow1 = new SettingsRow(testDirectory.CreateDirectory("1"), testDirectory.CreateDirectory("2"), true, null);
                var settingsRow2 = new SettingsRow(testDirectory.CreateDirectory("3"), testDirectory.CreateDirectory("4"), false, null);
                var settingsRow3 = new SettingsRow(testDirectory.CreateDirectory("5"), testDirectory.CreateDirectory("6"), true, new[] { "tiff" });
                var settingsRow4 = new SettingsRow(testDirectory.CreateDirectory("7"), testDirectory.CreateDirectory("8"), true, new[] { "tiff" });
                var settingsRow5 = new SettingsRow(testDirectory.CreateDirectory("9"), testDirectory.CreateDirectory("10"), true, null);

                testSettingsStorage.SettingsRows = new[]
                {
                    settingsRow1,
                    settingsRow2,
                    settingsRow3,
                    settingsRow4,
                    settingsRow5
                };

                var synchronizedDirectoriesManager = new SynchronizedDirectoriesManager(testSettingsStorage, new SynchronizedItemFactory(new ItemFactory()), null);

                await synchronizedDirectoriesManager.Load(); // Загрузка до изменения настроек.

                var oldSynchronizedDirectory2 = synchronizedDirectoriesManager.SynchronizedDirectories[1];
                var oldSynchronizedDirectory3 = synchronizedDirectoriesManager.SynchronizedDirectories[2];
                var oldSynchronizedDirectory4 = synchronizedDirectoriesManager.SynchronizedDirectories[3];

                foreach(var synchronizedDirectory in synchronizedDirectoriesManager.SynchronizedDirectories)
                {
                    synchronizedDirectory.LeftDirectory.LoadedDirectoryEvent += (IDirectory loadedDirecory) => { loadedDirectories.Add(loadedDirecory); };
                    synchronizedDirectory.RightDirectory.LoadedDirectoryEvent += (IDirectory loadedDirecory) => { loadedDirectories.Add(loadedDirecory); };
                }

                synchronizedDirectoriesManager.RemoveSynchronizedDirectoriesEvent += (ISynchronizedItems synchronizedDirectories) =>
                {
                    removedSynchronizedDirectories = synchronizedDirectories;
                };
                settingsRow1.IsUsed = false; // Эта строка при загрузке должна будет удалиться.
                settingsRow2.IsUsed = true; // Эта строка при загрузке должна будет добавиться.
                
                // Эта строка при загрузке должна будет инициализировать новую из-за изменения коллекции ExcludedExtensions.
                settingsRow3.ExcludedExtensions = new[] { "jpg" };

                await synchronizedDirectoriesManager.Load(); // Загрузка после изменения настроек.

                Assert.Equal(4, synchronizedDirectoriesManager.SynchronizedDirectories.Length);

                // Проверим удалённую директорию.
                Assert.NotNull(removedSynchronizedDirectories);
                Assert.Equal(settingsRow1.LeftDirectory.DirectoryPath, removedSynchronizedDirectories.LeftDirectory.FullPath);
                Assert.Equal(settingsRow1.RightDirectory.DirectoryPath, removedSynchronizedDirectories.RightDirectory.FullPath);

                // Записи на синхронизируемые директории должны оставаться прежними.
                Assert.Equal(oldSynchronizedDirectory2, synchronizedDirectoriesManager.SynchronizedDirectories[0]);
                Assert.Equal(oldSynchronizedDirectory3, synchronizedDirectoriesManager.SynchronizedDirectories[1]);
                Assert.Equal(oldSynchronizedDirectory4, synchronizedDirectoriesManager.SynchronizedDirectories[2]);

                // Лишь две директории одной записи должны были обновиться.
                Assert.Equal(2, loadedDirectories.Count);
                Assert.Contains(oldSynchronizedDirectory2.LeftDirectory, loadedDirectories);
                Assert.Contains(oldSynchronizedDirectory2.RightDirectory, loadedDirectories);

                // И массивы исключаемых из рассмотрения расширений файлов тоже должны были обновиться.
                Assert.Equal(settingsRow3.ExcludedExtensions, oldSynchronizedDirectory2.LeftDirectory.ExcludedExtensions);
                Assert.Equal(settingsRow3.ExcludedExtensions, oldSynchronizedDirectory2.RightDirectory.ExcludedExtensions);

                // Эта запись на синхронизируемую директорию должна быть добавленной из-за включения строки настройки.
                var synchronizedDirectory3 = synchronizedDirectoriesManager.SynchronizedDirectories[3];
                Assert.Equal(settingsRow2.LeftDirectory.DirectoryPath, synchronizedDirectory3.LeftDirectory.FullPath);
                Assert.Equal(settingsRow2.RightDirectory.DirectoryPath, synchronizedDirectory3.RightDirectory.FullPath);
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
                TestDirectory.CreateFiles(leftDirectory, new Dictionary<string, DateTime>
                {
                    { "1.tiff", fileLastUpdate },
                    { "2.tiff", fileLastUpdate },
                    { "3.tiff", fileLastUpdate },
                    { "4.jpg", fileLastUpdate },
                    { "5.jpg", fileLastUpdate },
                    { "6.png", fileLastUpdate }
                });

                var rightDirectory = testDirectory.CreateDirectory("2");
                TestDirectory.CreateFiles(rightDirectory, new Dictionary<string, DateTime>
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

                var synchronizedDirectoriesManager = new SynchronizedDirectoriesManager(testSettingsStorage, new SynchronizedItemFactory(new ItemFactory()), new SynchronizedItemMatcher());
                await synchronizedDirectoriesManager.Load();

                Assert.Single(synchronizedDirectoriesManager.SynchronizedDirectories);
                var synchronizedDirectory = synchronizedDirectoriesManager.SynchronizedDirectories.Single();
                Assert.Equal(loadedFilesCount, synchronizedDirectory.LeftDirectory.Items.Length);
                Assert.Equal(loadedFilesCount, synchronizedDirectory.RightDirectory.Items.Length);
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
