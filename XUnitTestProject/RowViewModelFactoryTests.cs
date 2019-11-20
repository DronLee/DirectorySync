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