﻿using DirectorySync.Models;
using DirectorySync.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class SynchronizedItemsTests
    {
        [Fact]
        public void CreateRowViewModel()
        {
            const string file1Name = "File1";
            const string file2Name = "File2";

            ISynchronizedItems loadedSynchronizedDirectories = null;

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

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                synchronizedDirectories.DirectoriesIsLoadedEvent += (ISynchronizedItems directoris) => { loadedSynchronizedDirectories = directoris; };

                // Пока не выполнялась загрузка, события завершения загрузки происходить не должно и дочерние элементы тоже должны отсутствовать.
                Assert.Null(loadedSynchronizedDirectories);
                Assert.Empty(synchronizedDirectories.ChildItems);
            }
        }

        /// <summary>
        /// Тест на срабатывание события DirectoriesIsLoadedEvent по завершению загрузки обоих директорий.
        /// </summary>
        [Fact]
        public async Task DirectoriesIsLoadedEvent()
        {
            List<ISynchronizedItems> loadedSynchronizedDirectoriesList = new List<ISynchronizedItems>();

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                synchronizedDirectories.DirectoriesIsLoadedEvent += (ISynchronizedItems loadedSynchronizedItems) =>
                {
                    loadedSynchronizedDirectoriesList.Add(loadedSynchronizedItems);
                };
                await synchronizedDirectories.Load();

                Assert.Single(loadedSynchronizedDirectoriesList);
                Assert.Equal(synchronizedDirectories, loadedSynchronizedDirectoriesList[0]);
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

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                Assert.Equal(5, synchronizedDirectories.ChildItems.Count);

                var directories = synchronizedDirectories.ChildItems[0];
                Assert.Equal(file1Name, directories.LeftItem.Name);
                Assert.Equal(file1Name, directories.RightItem.Name);
                Assert.Equal(ItemStatusEnum.ThereIs, directories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Missing, directories.RightItem.Status.StatusEnum);
                Assert.NotNull(directories.LeftItem.SyncCommand.CommandAction);
                Assert.NotNull(directories.RightItem.SyncCommand.CommandAction);

                directories = synchronizedDirectories.ChildItems[1];
                Assert.Equal(file2Name, directories.LeftItem.Name);
                Assert.Equal(file2Name, directories.RightItem.Name);
                Assert.Equal(ItemStatusEnum.Equally, directories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, directories.RightItem.Status.StatusEnum);
                Assert.Null(directories.LeftItem.SyncCommand.CommandAction);
                Assert.Null(directories.RightItem.SyncCommand.CommandAction);

                directories = synchronizedDirectories.ChildItems[2];
                Assert.Equal(file3Name, directories.LeftItem.Name);
                Assert.Equal(file3Name, directories.RightItem.Name);
                Assert.Equal(ItemStatusEnum.Newer, directories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Older, directories.RightItem.Status.StatusEnum);
                Assert.NotNull(directories.LeftItem.SyncCommand.CommandAction);
                Assert.NotNull(directories.RightItem.SyncCommand.CommandAction);

                directories = synchronizedDirectories.ChildItems[3];
                Assert.Equal(file4Name, directories.LeftItem.Name);
                Assert.Equal(file4Name, directories.RightItem.Name);
                Assert.Equal(ItemStatusEnum.Older, directories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Newer, directories.RightItem.Status.StatusEnum);
                Assert.NotNull(directories.LeftItem.SyncCommand.CommandAction);
                Assert.NotNull(directories.RightItem.SyncCommand.CommandAction);

                directories = synchronizedDirectories.ChildItems[4];
                Assert.Equal(file5Name, directories.LeftItem.Name);
                Assert.Equal(file5Name, directories.RightItem.Name);
                Assert.Equal(ItemStatusEnum.Missing, directories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.ThereIs, directories.RightItem.Status.StatusEnum);
                Assert.NotNull(directories.LeftItem.SyncCommand.CommandAction);
                Assert.NotNull(directories.RightItem.SyncCommand.CommandAction);
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
                
                // В директорию надо поместить хотя бы один файл, чтобы она была видна.
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directoryAndFileName), 
                    new Dictionary<string, DateTime> { { "1", DateTime.Now } });

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                Assert.Equal(2, synchronizedDirectories.ChildItems.Count);

                // Сначала директория, потом файл.
                var childDirectories1 = synchronizedDirectories.ChildItems[0];
                Assert.Equal(directoryAndFileName, childDirectories1.LeftItem.Name);
                Assert.Equal(directoryAndFileName, childDirectories1.RightItem.Name);
                Assert.NotNull(childDirectories1.RightItem.Directory);
                Assert.True(childDirectories1.RightItem.IsDirectory);

                // Даже если элемент отсутствует, а присутствующий является директорией, то и этот должен быть директорией.
                Assert.Null(childDirectories1.LeftItem.Directory);
                Assert.True(childDirectories1.LeftItem.IsDirectory);

                Assert.Equal(ItemStatusEnum.Missing, childDirectories1.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.ThereIs, childDirectories1.RightItem.Status.StatusEnum);
                var childDirectories2 = synchronizedDirectories.ChildItems[1];
                Assert.Equal(directoryAndFileName, childDirectories2.LeftItem.Name);
                Assert.Equal(directoryAndFileName, childDirectories2.RightItem.Name);
                Assert.Null(childDirectories2.LeftItem.Directory);
                Assert.False(childDirectories2.LeftItem.IsDirectory);
                Assert.Null(childDirectories2.RightItem.Directory);
                Assert.False(childDirectories2.RightItem.IsDirectory);
                Assert.Equal(ItemStatusEnum.ThereIs, childDirectories2.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Missing, childDirectories2.RightItem.Status.StatusEnum);
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

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                Assert.Single(synchronizedDirectories.ChildItems);

                var childDirectorie = synchronizedDirectories.ChildItems[0];
                Assert.Equal(directoryName, childDirectorie.LeftItem.Name);
                Assert.Equal(directoryName, childDirectorie.RightItem.Name);
                Assert.NotNull(childDirectorie.LeftItem.Directory);
                Assert.NotNull(childDirectorie.RightItem.Directory);
                Assert.Single(childDirectorie.ChildItems);

                // Это файлы.
                Assert.Null(childDirectorie.ChildItems[0].LeftItem.Directory);
                Assert.Null(childDirectorie.ChildItems[0].RightItem.Directory);

                // Файл правой новее, соответственно и статус правой Newer.
                Assert.Equal(ItemStatusEnum.Older, childDirectorie.ChildItems[0].LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Newer, childDirectorie.ChildItems[0].RightItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Older, childDirectorie.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Newer, childDirectorie.RightItem.Status.StatusEnum);
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

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                Assert.Single(synchronizedDirectories.ChildItems);

                var childDirectories = synchronizedDirectories.ChildItems[0];
                Assert.Equal(directoryName, childDirectories.LeftItem.Name);
                Assert.Equal(directoryName, childDirectories.RightItem.Name);
                Assert.NotNull(childDirectories.LeftItem.Directory);

                // Справа реально директории нет, потому что пустые директории не учитываются, но элемент помечен как директория.
                Assert.Null(childDirectories.RightItem.Directory);
                Assert.True(childDirectories.RightItem.IsDirectory);

                Assert.Empty(childDirectories.ChildItems);
            }
        }

        /// <summary>
        /// Проверка обновления статусов моделей синхронизируемых элементов на основе статусов дочерних элементов.
        /// </summary>
        /// <param name="leftStartStatus">Начальное значение статуса левого элемента.</param>
        /// <param name="rightStartStatus">Начальное значение статуса правого элемента.</param>
        /// <param name="leftItemsStatuses">Статусы левых дочерних элементов.</param>
        /// <param name="rightItemsStatuses">Статусы правых дочерних элементов.</param>
        /// <param name="leftExpectedStatus">Ожидаемое значение статуса левого элемента после обновления.</param>
        /// <param name="rightExpectedStatus">Ожидаемое значение статуса правого элемента после обновления.</param>
        [Theory]

        // Если нет дочерних элементов, то статус должен оставаться прежним.
        [InlineData("Equally", "Equally", new string[0], new string[0], "Equally", "Equally")]

        [InlineData("Equally", "Equally", new[] { "Newer" }, new[] { "Older" }, "Newer", "Older")]
        [InlineData("Equally", "Equally", new[] { "Missing" }, new[] { "ThereIs" }, "Missing", "ThereIs")]
        [InlineData("Unknown", "Unknown", new[] { "Equally" }, new[] { "Equally" }, "Equally", "Equally")]

        // Если дочерние элементы имеют один статус не считая Equally, то статус родительского будет такой же, как этот один.
        [InlineData("Equally", "Equally", new[] { "Missing", "Equally" }, new[] { "ThereIs", "Equally" }, "Missing", "ThereIs")]

        // Если дочерние элементы имеют разнообразные статусы, стутус родительского будет Unknown.
        [InlineData("Equally", "Equally", new[] { "Missing", "ThereIs" }, new[] { "ThereIs", "Missing" }, "Unknown", "Unknown")]
        public async Task RefreshStatusesFromChilds_CheckStatus(string leftStartStatus, string rightStartStatus,
            string[] leftItemsStatuses, string[] rightItemsStatuses,
            string leftExpectedStatus, string rightExpectedStatus)
        {
            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                var testFilesDictionary = new Dictionary<string, DateTime>();
                var updateData = DateTime.Now;
                for (byte i = 0; i < leftItemsStatuses.Length; i++)
                    testFilesDictionary.Add("File" + i.ToString(), updateData);
                leftDirectory.CreateFiles(testFilesDictionary);
                rightDirectory.CreateFiles(testFilesDictionary);

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                synchronizedDirectories.LeftItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftStartStatus));
                synchronizedDirectories.RightItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightStartStatus));

                await synchronizedDirectories.Load();
                for (byte i = 0; i < leftItemsStatuses.Length; i++)
                {
                    synchronizedDirectories.ChildItems[i].LeftItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftItemsStatuses[i]));
                    synchronizedDirectories.ChildItems[i].RightItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightItemsStatuses[i]));
                }

                synchronizedDirectories.RefreshStatusesFromChilds();
                
                Assert.Equal(leftExpectedStatus, synchronizedDirectories.LeftItem.Status.StatusEnum.ToString());
                Assert.Equal(rightExpectedStatus, synchronizedDirectories.RightItem.Status.StatusEnum.ToString());
            }
        }

        /// <summary>
        /// Проверка присвоения команд родительской записи от дочерних и их выполнения.
        /// </summary>
        /// <param name="strLeftStatus">Статус записей слева, тех которые не Equally.</param>
        /// <param name="strRightStatus">Статус записей справа, тех которые не Equally.</param>
        /// <param name="leftAccept">True - будет выполнена команда принятия левого элемента. Иначе - правого.</param>
        [Theory]
        [InlineData("Newer", "ThereIs", true)]
        [InlineData("ThereIs", "Newer", true)]
        [InlineData("Newer", "ThereIs", false)]
        [InlineData("ThereIs", "Newer", false)]
        public async Task RefreshStatusesFromChilds_CheckCommands(string strLeftStatus, string strRightStatus, bool leftAccept)
        {
            var leftStatus = Enum.Parse<ItemStatusEnum>(strLeftStatus);
            var rightStatus = Enum.Parse<ItemStatusEnum>(strRightStatus);

            var expectedCopyDestinationPathes = new List<string>();
            var resultCopyDestinationPathes = new List<string>();

            // Объект блокировки для списка resultCopyDestinationPathes. Так как в него запись может вестись с разных потоков.
            var resultCopyDestinationPathesLocker = new object();

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                var testFilesDictionary = new Dictionary<string, DateTime>();
                var updateData = DateTime.Now;
                for (byte i = 0; i < 4; i++)
                    testFilesDictionary.Add("Test" + i.ToString(), updateData);
                leftDirectory.CreateFiles(testFilesDictionary);
                rightDirectory.CreateFiles(testFilesDictionary);

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                for (byte i = 0; i < 4; i++)
                {
                    var leftSynchronizedItem = synchronizedDirectories.ChildItems[i].LeftItem;
                    var rightSynchronizedItem = synchronizedDirectories.ChildItems[i].RightItem;

                    // Запишем команды синхронизации для каждого элемента и подпишемся на их выполнение.
                    leftSynchronizedItem.SyncCommand.SetCommandAction(() => leftSynchronizedItem.Item.CopyTo(rightSynchronizedItem.FullPath));
                    leftSynchronizedItem.Item.CopiedFromToEvent += (IItem item1, string destinationPath) =>
                    {
                        lock (resultCopyDestinationPathesLocker)
                            resultCopyDestinationPathes.Add(destinationPath);
                    };
                    rightSynchronizedItem.SyncCommand.SetCommandAction(() => rightSynchronizedItem.Item.CopyTo(leftSynchronizedItem.FullPath));
                    rightSynchronizedItem.Item.CopiedFromToEvent += (IItem item1, string destinationPath) =>
                    {
                        lock (resultCopyDestinationPathesLocker)
                            resultCopyDestinationPathes.Add(destinationPath);
                    };

                    if (i % 2 == 0) // Половина элементов для синхронизации, вторая половина должна оставаться нетронутой.
                    {
                        expectedCopyDestinationPathes.Add(leftAccept ? rightSynchronizedItem.FullPath : leftSynchronizedItem.FullPath);
                        leftSynchronizedItem.UpdateStatus(leftStatus);
                        rightSynchronizedItem.UpdateStatus(rightStatus);
                    }
                    else
                    {
                        leftSynchronizedItem.UpdateStatus(ItemStatusEnum.Equally);
                        rightSynchronizedItem.UpdateStatus(ItemStatusEnum.Equally);
                    }
                }

                synchronizedDirectories.RefreshStatusesFromChilds();
                if (leftAccept)
                    await synchronizedDirectories.LeftItem.SyncCommand.Process();
                else
                    await synchronizedDirectories.RightItem.SyncCommand.Process();

                Assert.Equal(string.Join("|", expectedCopyDestinationPathes.ToArray()),
                    string.Join("|", resultCopyDestinationPathes.OrderBy(p => p).ToArray()));
            }
        }

        /// <summary>
        /// Проверка простановки статусов и отсутствия команд при выполнении RefreshStatusesFromChilds для строки,
        /// содержащей строку с неопределённым статусом.
        /// </summary>
        [Fact]
        public async Task RefreshStatusesFromChilds_UnknownChild()
        {
            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                const string directoryName = "Dir";
                var testFilesDictionary = new Dictionary<string, DateTime> { { "File1", DateTime.Now }, { "File2", DateTime.Now } };
                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(directoryName), testFilesDictionary);
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directoryName), testFilesDictionary);

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                var childSynchronizedDirectories = synchronizedDirectories.ChildItems[0];
                childSynchronizedDirectories.LeftItem.UpdateStatus(ItemStatusEnum.Unknown);
                childSynchronizedDirectories.RightItem.UpdateStatus(ItemStatusEnum.Unknown);

                var level2Child1 = childSynchronizedDirectories.ChildItems[0];
                level2Child1.LeftItem.UpdateStatus(ItemStatusEnum.Newer);
                level2Child1.LeftItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
                level2Child1.RightItem.UpdateStatus(ItemStatusEnum.Older);
                level2Child1.RightItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });

                var level2Child2 = childSynchronizedDirectories.ChildItems[1];
                level2Child1.LeftItem.UpdateStatus(ItemStatusEnum.Missing);
                level2Child1.LeftItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
                level2Child1.RightItem.UpdateStatus(ItemStatusEnum.ThereIs);
                level2Child1.RightItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });

                synchronizedDirectories.RefreshStatusesFromChilds();

                // У дочерней строки должен остаться неопределённый статус.
                Assert.Equal(ItemStatusEnum.Unknown, childSynchronizedDirectories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Unknown, childSynchronizedDirectories.RightItem.Status.StatusEnum);

                // Статусы родительской строки должны измениться на неопредёлённые.
                Assert.Equal(ItemStatusEnum.Unknown, synchronizedDirectories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Unknown, synchronizedDirectories.RightItem.Status.StatusEnum);

                // Команд не должно быть ни у дочерней строки, ни у родительской.
                Assert.Null(childSynchronizedDirectories.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childSynchronizedDirectories.RightItem.SyncCommand.CommandAction);
                Assert.Null(synchronizedDirectories.LeftItem.SyncCommand.CommandAction);
                Assert.Null(synchronizedDirectories.RightItem.SyncCommand.CommandAction);
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