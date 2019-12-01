using DirectorySync.Models;
using DirectorySync.Models.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class SynchronizedItemsTests
    {
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
        /// Проверка создания моделей синхронизируемых элементов при загрузке файлов.
        /// </summary>
        [Fact]
        public async Task LoadFiles()
        {
            const string file1Name = "File1";
            const string file2Name = "File2";
            const string file3Name = "File3";
            var updateFileDate = DateTime.Now;
            var filesDirectoinaries = new Dictionary<string, DateTime>
            {
                { file1Name, updateFileDate },
                { file2Name, updateFileDate },
                { file3Name, updateFileDate }
            };

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                leftDirectory.CreateFiles(filesDirectoinaries);
                rightDirectory.CreateFiles(filesDirectoinaries);

                var settingsRow = new TestSettingsRow
                {
                    LeftDirectory = new SettingsDirectory(leftDirectory.FullPath),
                    RightDirectory = new SettingsDirectory(rightDirectory.FullPath)
                };
                var testSynchronizedItemsStatusAndCommandsUpdater = new TestSynchronizedItemsStatusAndCommandsUpdater();
                var synchronizedDirectories = new SynchronizedItems(settingsRow, new SynchronizedItemFactory(new ItemFactory()),
                    testSynchronizedItemsStatusAndCommandsUpdater);

                await synchronizedDirectories.Load();

                Assert.Equal(3, synchronizedDirectories.ChildItems.Count);

                var directories = synchronizedDirectories.ChildItems[0];
                Assert.Equal(file1Name, directories.LeftItem.Name);
                Assert.Equal(file1Name, directories.RightItem.Name);

                // Это файлы.
                Assert.False(directories.LeftItem.IsDirectory);
                Assert.Null(directories.LeftItem.Directory);
                Assert.False(directories.RightItem.IsDirectory);
                Assert.Null(directories.RightItem.Directory);

                directories = synchronizedDirectories.ChildItems[1];
                Assert.Equal(file2Name, directories.LeftItem.Name);
                Assert.Equal(file2Name, directories.RightItem.Name);

                // Это файлы.
                Assert.False(directories.LeftItem.IsDirectory);
                Assert.Null(directories.LeftItem.Directory);
                Assert.False(directories.RightItem.IsDirectory);
                Assert.Null(directories.RightItem.Directory);

                directories = synchronizedDirectories.ChildItems[2];
                Assert.Equal(file3Name, directories.LeftItem.Name);
                Assert.Equal(file3Name, directories.RightItem.Name);

                // Это файлы.
                Assert.False(directories.LeftItem.IsDirectory);
                Assert.Null(directories.LeftItem.Directory);
                Assert.False(directories.RightItem.IsDirectory);
                Assert.Null(directories.RightItem.Directory);
            }
        }

        /// <summary>
        /// Проверка создания моделей синхронизируемых элементов при загрузке,
        /// один из которых файл, второй директория, и имеют одинаковые наименования. 
        /// </summary>
        [Fact]
        public async Task LoadDirectoryAndFile()
        {
            const string directoryAndFileName = "Item";

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                var updateDate = DateTime.Now;

                leftDirectory.CreateFiles(new Dictionary<string, DateTime>
                {
                    { directoryAndFileName, updateDate }
                });
                
                // В директорию надо поместить хотя бы один файл, чтобы она была видна.
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directoryAndFileName), 
                    new Dictionary<string, DateTime> { { "1", updateDate } });

                var settingsRow = new TestSettingsRow
                {
                    LeftDirectory = new SettingsDirectory(leftDirectory.FullPath),
                    RightDirectory = new SettingsDirectory(rightDirectory.FullPath)
                };
                var testSynchronizedItemsStatusAndCommandsUpdater = new TestSynchronizedItemsStatusAndCommandsUpdater();
                var synchronizedDirectories = new SynchronizedItems(settingsRow, new SynchronizedItemFactory(new ItemFactory()),
                    testSynchronizedItemsStatusAndCommandsUpdater);

                await synchronizedDirectories.Load();

                Assert.Equal(2, synchronizedDirectories.ChildItems.Count); // Одна модель на директории, одна модель на файлы.

                // Сначала директория, потом файл.
                var childDirectories1 = synchronizedDirectories.ChildItems[0];
                Assert.Equal(directoryAndFileName, childDirectories1.LeftItem.Name);
                Assert.Equal(directoryAndFileName, childDirectories1.RightItem.Name);
                Assert.NotNull(childDirectories1.RightItem.Directory);
                Assert.True(childDirectories1.RightItem.IsDirectory);

                // Даже если элемент отсутствует, а присутствующий является директорией, то и этот должен быть директорией.
                Assert.Null(childDirectories1.LeftItem.Directory);
                Assert.True(childDirectories1.LeftItem.IsDirectory);

                var childDirectories2 = synchronizedDirectories.ChildItems[1];
                Assert.Equal(directoryAndFileName, childDirectories2.LeftItem.Name);
                Assert.Equal(directoryAndFileName, childDirectories2.RightItem.Name);
                Assert.Null(childDirectories2.LeftItem.Directory);
                Assert.False(childDirectories2.LeftItem.IsDirectory);
                Assert.Null(childDirectories2.RightItem.Directory);
                Assert.False(childDirectories2.RightItem.IsDirectory);
            }
        }

        /// <summary>
        /// Проверка выполнения метода обновления статусов и команд на основе дочерних элементов при загрузке.
        /// </summary>
        [Fact]
        public async Task Load_RefreshStatusAndCommandsFromChilds()
        {
            const string directoryName = "Directory";
            const string fileName = "File";
            var updateDate = DateTime.Now;

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                var childLeftDirectoryPath = leftDirectory.CreateDirectory(directoryName, updateDate);
                var childRightDirectoryPath = rightDirectory.CreateDirectory(directoryName, updateDate);

                Infrastructure.TestDirectory.CreateFiles(childLeftDirectoryPath, new Dictionary<string, DateTime> {
                    { fileName, updateDate } });
                Infrastructure.TestDirectory.CreateFiles(childRightDirectoryPath, new Dictionary<string, DateTime> {
                    { fileName, updateDate } });

                var settingsRow = new TestSettingsRow
                {
                    LeftDirectory = new SettingsDirectory(leftDirectory.FullPath),
                    RightDirectory = new SettingsDirectory(rightDirectory.FullPath)
                };

                var testSynchronizedItemsStatusAndCommandsUpdater = new TestSynchronizedItemsStatusAndCommandsUpdater();
                var synchronizedDirectories = new SynchronizedItems(settingsRow, new SynchronizedItemFactory(new ItemFactory()),
                    testSynchronizedItemsStatusAndCommandsUpdater);
                await synchronizedDirectories.Load();

                Assert.Single(synchronizedDirectories.ChildItems);

                var childDirectories = synchronizedDirectories.ChildItems[0];

                var refreshedLeftItemSynchronizedItemsList = testSynchronizedItemsStatusAndCommandsUpdater.RefreshedLeftItemSynchronizedItemsList;
                var refreshedRightItemSynchronizedItemsList = testSynchronizedItemsStatusAndCommandsUpdater.RefreshedRightItemSynchronizedItemsList;
                Assert.Equal(2, refreshedLeftItemSynchronizedItemsList.Count);
                Assert.Equal(2, refreshedRightItemSynchronizedItemsList.Count);
                Assert.Equal(childDirectories, refreshedLeftItemSynchronizedItemsList[0]);
                Assert.Equal(childDirectories, refreshedRightItemSynchronizedItemsList[0]);
                Assert.Equal(synchronizedDirectories, refreshedLeftItemSynchronizedItemsList[1]);
                Assert.Equal(synchronizedDirectories, refreshedRightItemSynchronizedItemsList[1]);
            }
        }

        /// <summary>
        /// Проверка выполнения метода обновления статусов и команд синхронизируемых элементов при загрузке дочерних элементов.
        /// </summary>
        [Fact]
        public async void LoadChildItems_RefreshStatusAndCommandsFromChilds()
        {
            const string dir1Name = "Dir1";
            const string dir2Name = "Dir2";
            const string file1Name = "File1";
            const string file2Name = "File2";
            const string file3Name = "File3";
            var updateDate = DateTime.Now;

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(dir1Name),
                    new Dictionary<string, DateTime> { { file1Name, updateDate } });
                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(dir2Name),
                    new Dictionary<string, DateTime> { { file2Name, updateDate }, { file3Name, updateDate } });

                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(dir1Name),
                    new Dictionary<string, DateTime> { { file1Name, updateDate } });
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(dir2Name),
                    new Dictionary<string, DateTime> { { file2Name, updateDate }, { file3Name, updateDate } });

                var settingsRow = new TestSettingsRow
                {
                    LeftDirectory = new SettingsDirectory(leftDirectory.FullPath),
                    RightDirectory = new SettingsDirectory(rightDirectory.FullPath)
                };

                var testSynchronizedItemsStatusAndCommandsUpdater = new TestSynchronizedItemsStatusAndCommandsUpdater();
                var synchronizedItems = new SynchronizedItems(settingsRow, new SynchronizedItemFactory(new ItemFactory()),
                    testSynchronizedItemsStatusAndCommandsUpdater);

                await Task.WhenAll(synchronizedItems.LeftDirectory.Load(), synchronizedItems.RightDirectory.Load());

                synchronizedItems.LoadChildItems();

                var refreshedLeftItemSynchronizedItemsList = testSynchronizedItemsStatusAndCommandsUpdater.RefreshedLeftItemSynchronizedItemsList;
                var refreshedRightItemSynchronizedItemsList = testSynchronizedItemsStatusAndCommandsUpdater.RefreshedRightItemSynchronizedItemsList;

                // Каждая директория должна была обновить свои статусы и команды.
                Assert.Equal(3, refreshedLeftItemSynchronizedItemsList.Count);
                Assert.Equal(3, refreshedRightItemSynchronizedItemsList.Count);

                // В таком порядке должны были обновляться статусы элементов.
                Assert.Equal(refreshedLeftItemSynchronizedItemsList[0], synchronizedItems.ChildItems[0]);
                Assert.Equal(refreshedLeftItemSynchronizedItemsList[1], synchronizedItems.ChildItems[1]);
                Assert.Equal(refreshedLeftItemSynchronizedItemsList[2], synchronizedItems);
                Assert.Equal(refreshedRightItemSynchronizedItemsList[0], synchronizedItems.ChildItems[0]);
                Assert.Equal(refreshedRightItemSynchronizedItemsList[1], synchronizedItems.ChildItems[1]);
                Assert.Equal(refreshedRightItemSynchronizedItemsList[2], synchronizedItems);
            }
        }

        /// <summary>
        /// Проверка создания моделей синхронизируемых элементов при загрузке директорий.
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

                var settingsRow = new TestSettingsRow
                {
                    LeftDirectory = new SettingsDirectory(leftDirectory.FullPath),
                    RightDirectory = new SettingsDirectory(rightDirectory.FullPath)
                };

                var testSynchronizedItemsStatusAndCommandsUpdater = new TestSynchronizedItemsStatusAndCommandsUpdater();
                var synchronizedDirectories = new SynchronizedItems(settingsRow, new SynchronizedItemFactory(new ItemFactory()),
                    testSynchronizedItemsStatusAndCommandsUpdater);

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
        /// Проверка изменений синхронизируемых директорий после удаления отслеживаемых элементов через команду корневого элемента слева.
        /// </summary>
        [Fact]
        public async Task SynchronizedDirectoriesAfterDeleteItem_ThroughLeftRoot()
        {
            const string directoryName = "Dir1";
            const string fileName = "File1";
            var fileUpdateDate = DateTime.Now;

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(directoryName),
                    new Dictionary<string, DateTime> { { fileName, fileUpdateDate } });
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directoryName),
                    new Dictionary<string, DateTime> { { fileName, fileUpdateDate }, { "File2", fileUpdateDate } });

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                // Эта команда должна выполнить удаление файла "File2" справа.
                await synchronizedDirectories.LeftItem.SyncCommand.Process();

                // По прежнему должна остваться одна дочерняя запись на директории.
                Assert.Single(synchronizedDirectories.ChildItems);

                await Task.Delay(20); // Чтобы успели обновиться статусы и команды.

                // Дочерние элементы теперь должны быть идентичные и команды синхронизации им не нужны.
                var childItem = synchronizedDirectories.ChildItems[0];
                Assert.Equal(ItemStatusEnum.Equally, childItem.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, childItem.RightItem.Status.StatusEnum);
                Assert.Null(childItem.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childItem.RightItem.SyncCommand.CommandAction);

                // И в ней одна запись на оставшуюся пару файлов.
                Assert.Single(childItem.ChildItems);

                // Эти тоже теперь должны быть идентичные и команды синхронизации им не нужны.
                childItem = childItem.ChildItems[0];
                Assert.Equal(ItemStatusEnum.Equally, childItem.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, childItem.RightItem.Status.StatusEnum);
                Assert.Null(childItem.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childItem.RightItem.SyncCommand.CommandAction);

                // И корневые элементы теперь должны быть идентичные и команды синхронизации им тоже не нужны.
                Assert.Equal(ItemStatusEnum.Equally, synchronizedDirectories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, synchronizedDirectories.RightItem.Status.StatusEnum);
                Assert.Null(synchronizedDirectories.LeftItem.SyncCommand.CommandAction);
                Assert.Null(synchronizedDirectories.RightItem.SyncCommand.CommandAction);
            }
        }

        /// <summary>
        /// Проверка изменений синхронизируемых директорий после удаления отслеживаемых элементов через команду одного из дочерних элементов слева.
        /// </summary>
        /// <param name="childItemName">Наименование элемента слева, через команду которого будет осуществляться удаление.</param>
        [Theory]
        [InlineData("Dir1")]
        [InlineData("File2")]
        public async Task SynchronizedDirectoriesAfterDeleteItem_ThroughLeftChild(string childItemName)
        {
            const string directoryName = "Dir1";
            const string fileName = "File1";
            var fileUpdateDate = DateTime.Now;

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {

                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(directoryName),
                    new Dictionary<string, DateTime> { { fileName, fileUpdateDate } });
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directoryName),
                    new Dictionary<string, DateTime> { { fileName, fileUpdateDate }, { "File2", fileUpdateDate } });

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                // Эта команда должна выполнить удаление файла "File2" справа.
                await GetChildItemByName(synchronizedDirectories, true, childItemName).SyncCommand.Process();

                // По прежнему должна остваться одна дочерняя запись на директории.
                Assert.Single(synchronizedDirectories.ChildItems);

                await Task.Delay(20); // Чтобы успели обновиться статусы и команды.

                // Дочерние элементы теперь должны быть идентичные и команды синхронизации им не нужны.
                var childItem = synchronizedDirectories.ChildItems[0];
                Assert.Equal(ItemStatusEnum.Equally, childItem.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, childItem.RightItem.Status.StatusEnum);
                Assert.Null(childItem.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childItem.RightItem.SyncCommand.CommandAction);

                // И в ней одна запись на оставшуюся пару файлов.
                Assert.Single(childItem.ChildItems);

                // Эти тоже теперь должны быть идентичные и команды синхронизации им не нужны.
                childItem = childItem.ChildItems[0];
                Assert.Equal(ItemStatusEnum.Equally, childItem.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, childItem.RightItem.Status.StatusEnum);
                Assert.Null(childItem.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childItem.RightItem.SyncCommand.CommandAction);

                // И корневые элементы теперь должны быть идентичные и команды синхронизации им тоже не нужны.
                Assert.Equal(ItemStatusEnum.Equally, synchronizedDirectories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, synchronizedDirectories.RightItem.Status.StatusEnum);
                Assert.Null(synchronizedDirectories.LeftItem.SyncCommand.CommandAction);
                Assert.Null(synchronizedDirectories.RightItem.SyncCommand.CommandAction);
            }
        }

        /// <summary>
        /// Проверка изменений синхронизируемых директорий после удаления отслеживаемых элементов через команду корневого элемента справа.
        /// </summary>
        [Fact]
        public async Task SynchronizedDirectoriesAfterDeleteItem_ThroughRightRoot()
        {
            const string directoryName = "Dir1";
            const string fileName = "File1";
            var fileUpdateDate = DateTime.Now;

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(directoryName),
                    new Dictionary<string, DateTime> { { fileName, fileUpdateDate }, { "File2", fileUpdateDate } });
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directoryName),
                    new Dictionary<string, DateTime> { { fileName, fileUpdateDate } });

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                // Эта команда должна выполнить удаление файла "File2" слева.
                await synchronizedDirectories.RightItem.SyncCommand.Process();

                // По прежнему должна остваться одна дочерняя запись на директории.
                Assert.Single(synchronizedDirectories.ChildItems);

                await Task.Delay(20); // Чтобы успели обновиться статусы и команды.

                // Дочерние элементы теперь должны быть идентичные и команды синхронизации им не нужны.
                var childItem = synchronizedDirectories.ChildItems[0];
                Assert.Equal(ItemStatusEnum.Equally, childItem.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, childItem.RightItem.Status.StatusEnum);
                Assert.Null(childItem.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childItem.RightItem.SyncCommand.CommandAction);

                // И в ней одна запись на оставшуюся пару файлов.
                Assert.Single(childItem.ChildItems);

                // Эти тоже теперь должны быть идентичные и команды синхронизации им не нужны.
                childItem = childItem.ChildItems[0];
                Assert.Equal(ItemStatusEnum.Equally, childItem.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, childItem.RightItem.Status.StatusEnum);
                Assert.Null(childItem.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childItem.RightItem.SyncCommand.CommandAction);

                // И корневые элементы теперь должны быть идентичные и команды синхронизации им тоже не нужны.
                Assert.Equal(ItemStatusEnum.Equally, synchronizedDirectories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, synchronizedDirectories.RightItem.Status.StatusEnum);
                Assert.Null(synchronizedDirectories.LeftItem.SyncCommand.CommandAction);
                Assert.Null(synchronizedDirectories.RightItem.SyncCommand.CommandAction);
            }
        }

        /// <summary>
        /// Проверка изменений синхронизируемых директорий после удаления отслеживаемых элементов через команду одного из дочерних элементов справа.
        /// </summary>
        /// <param name="childItemName">Наименование элемента справа, через команду которого будет осуществляться удаление.</param>
        [Theory]
        [InlineData("Dir1")]
        [InlineData("File2")]
        public async Task SynchronizedDirectoriesAfterDeleteItem_ThroughRightChild(string childItemName)
        {
            const string directoryName = "Dir1";
            const string fileName = "File1";
            var fileUpdateDate = DateTime.Now;

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(directoryName),
                    new Dictionary<string, DateTime> { { fileName, fileUpdateDate }, { "File2", fileUpdateDate } });
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directoryName),
                    new Dictionary<string, DateTime> { { fileName, fileUpdateDate } });

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                // Эта команда должна выполнить удаление файла "File2" слева.
                await GetChildItemByName(synchronizedDirectories, false, childItemName).SyncCommand.Process();

                // По прежнему должна остваться одна дочерняя запись на директории.
                Assert.Single(synchronizedDirectories.ChildItems);

                await Task.Delay(20); // Чтобы успели обновиться статусы и команды.

                // Дочерние элементы теперь должны быть идентичные и команды синхронизации им не нужны.
                var childItem = synchronizedDirectories.ChildItems[0];
                Assert.Equal(ItemStatusEnum.Equally, childItem.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, childItem.RightItem.Status.StatusEnum);
                Assert.Null(childItem.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childItem.RightItem.SyncCommand.CommandAction);

                // И в ней одна запись на оставшуюся пару файлов.
                Assert.Single(childItem.ChildItems);

                // Эти тоже теперь должны быть идентичные и команды синхронизации им не нужны.
                childItem = childItem.ChildItems[0];
                Assert.Equal(ItemStatusEnum.Equally, childItem.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, childItem.RightItem.Status.StatusEnum);
                Assert.Null(childItem.LeftItem.SyncCommand.CommandAction);
                Assert.Null(childItem.RightItem.SyncCommand.CommandAction);

                // И корневые элементы теперь должны быть идентичные и команды синхронизации им тоже не нужны.
                Assert.Equal(ItemStatusEnum.Equally, synchronizedDirectories.LeftItem.Status.StatusEnum);
                Assert.Equal(ItemStatusEnum.Equally, synchronizedDirectories.RightItem.Status.StatusEnum);
                Assert.Null(synchronizedDirectories.LeftItem.SyncCommand.CommandAction);
                Assert.Null(synchronizedDirectories.RightItem.SyncCommand.CommandAction);
            }
        }

        /// <summary>
        /// Проверка удаления оставшихся пустыми синхронизируемых директорий после удаления отслеживаемых элементов.
        /// </summary>
        [Fact]
        public async Task RemoveEmptySynchronizedDirectoriesAfterDeleteItem()
        {
            const string directoryName = "Dir1";

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                Infrastructure.TestDirectory.CreateDirectory(leftDirectory.CreateDirectory(directoryName), directoryName);
                Infrastructure.TestDirectory.CreateFiles(
                    Infrastructure.TestDirectory.CreateDirectory(rightDirectory.CreateDirectory(directoryName), directoryName),
                    new Dictionary<string, DateTime> { { "File1", DateTime.Now } });

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                await synchronizedDirectories.LeftItem.SyncCommand.Process(); // Эта команда должна выполнить удаление файла справа.

                // Корневые элементы должны по прежнему остваться директориями.
                Assert.NotNull(synchronizedDirectories.LeftDirectory);
                Assert.NotNull(synchronizedDirectories.RightDirectory);

                await Task.Delay(20); // Подождём, чтобы успела обновиться коллекция дочерних элементов.

                // Но синхронизируемых элементов в них быть не должно, так как никаких файлов не осталось.
                Assert.Empty(synchronizedDirectories.ChildItems);
            }
        }

        /// <summary>
        /// Проверка появления команд в директориях после выполнения синхронизации дочерних элементов.
        /// </summary>
        [Fact]
        public async Task AddCommandsAfterSyncProcess()
        {
            const string directory1Name = "Dir1";
            const string directory2Name = "Dir2";
            const string file1Name = "File1";
            var updateDate = DateTime.Now;

            using (var leftDirectory = new Infrastructure.TestDirectory())
            using (var rightDirectory = new Infrastructure.TestDirectory())
            {
                leftDirectory.CreateDirectory(directory1Name);
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directory1Name),
                    new Dictionary<string, DateTime> { { file1Name, updateDate } });

                Infrastructure.TestDirectory.CreateFiles(leftDirectory.CreateDirectory(directory2Name),
                    new Dictionary<string, DateTime> { { file1Name, updateDate }, { "File2", updateDate} });
                Infrastructure.TestDirectory.CreateFiles(rightDirectory.CreateDirectory(directory2Name),
                    new Dictionary<string, DateTime> { { file1Name, updateDate } });

                var synchronizedDirectories = GetSynchronizedDirectories(leftDirectory.FullPath, rightDirectory.FullPath);
                await synchronizedDirectories.Load();

                // После того, как Dir2 слева начнёт соответствовать Dir2 справа,
                // у корневой строки должны появиться команды для синхронизации Dir1.
                await synchronizedDirectories.ChildItems[1].LeftItem.SyncCommand.Process();
                await Task.Delay(20); // Чтобы успели обновиться статусы и команды.
                Assert.NotNull(synchronizedDirectories.LeftItem.SyncCommand.CommandAction);
                Assert.NotNull(synchronizedDirectories.RightItem.SyncCommand.CommandAction);
            }
        }

        private ISynchronizedItem GetChildItemByName(ISynchronizedItems synchronizedItems, bool isLeft, string childItemName)
        {
            var item = isLeft ? synchronizedItems.LeftItem : synchronizedItems.RightItem;
            if (item.Name == childItemName)
                return item;
            foreach (var childItem in synchronizedItems.ChildItems)
            {
                var result = GetChildItemByName(childItem, isLeft, childItemName);
                if (result != null)
                    return result;
            }

            return null;
        }

        private SynchronizedItems GetSynchronizedDirectories(string leftDirectoryPath, string rightDirectoryPath)
        {
            var settingsRow = new TestSettingsRow
            {
                LeftDirectory = new SettingsDirectory(leftDirectoryPath),
                RightDirectory = new SettingsDirectory(rightDirectoryPath)
            };

            return new SynchronizedItems(settingsRow, new SynchronizedItemFactory(new ItemFactory()),
                new SynchronizedItemsStatusAndCommandsUpdater(new SynchronizedItemMatcher()));
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

        private class TestSynchronizedItemsStatusAndCommandsUpdater : ISynchronizedItemsStatusAndCommandsUpdater
        {
            /// <summary>
            /// Список синхронизируемых элементов, для которых выполнялось обновление статусов и команд на основе дочерних элементов слева.
            /// </summary>
            public readonly List<ISynchronizedItems> RefreshedLeftItemSynchronizedItemsList = new List<ISynchronizedItems>();

            /// <summary>
            /// Список синхронизируемых элементов, для которых выполнялось обновление статусов и команд на основе дочерних элементов справа.
            /// </summary>
            public readonly List<ISynchronizedItems> RefreshedRightItemSynchronizedItemsList = new List<ISynchronizedItems>();

            public void RefreshLeftItemStatusesAndCommandsFromChilds(ISynchronizedItems synchronizedItems)
            {
                RefreshedLeftItemSynchronizedItemsList.Add(synchronizedItems);
            }

            public void RefreshRightItemStatusesAndCommandsFromChilds(ISynchronizedItems synchronizedItems)
            {
                RefreshedRightItemSynchronizedItemsList.Add(synchronizedItems);
            }

            public void UpdateStatusesAndCommands(ISynchronizedItem item1, ISynchronizedItem item2)
            {
                return;
            }
        }
    }
}