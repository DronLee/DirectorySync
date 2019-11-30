using DirectorySync.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class SynchronizedItemsStatusAndCommandsUpdaterTest
    {
        /// <summary>
        /// Проверка присвоения команд родительской записи от дочерних для левого элемента.
        /// </summary>
        /// <param name="strLeftStatus">Статус записей слева, тех которые не Equally.</param>
        [Theory]
        [InlineData("Missing")]
        [InlineData("ThereIs")]
        [InlineData("Older")]
        [InlineData("Newer")]
        public async Task RefreshLeftItemStatusesAndCommandsFromChilds_CheckCommands(string strLeftStatus)
        {
            var leftStatus = Enum.Parse<ItemStatusEnum>(strLeftStatus);

            var usedCommands = new List<string>(); // Наименования элемнтов, чьи команды будут выполнены.

            var testSynchronizedItems = new TestSynchronizedItems();
            testSynchronizedItems.LeftItem = new SynchronizedItem(null, true, null);
            testSynchronizedItems.RightItem = new SynchronizedItem(null, true, null);

            for (byte i = 0; i < 4; i++)
            {
                var childSynchronizedItems = new TestSynchronizedItems();
                var leftSynchronizedItem = childSynchronizedItems.LeftItem = new SynchronizedItem("Test" + i.ToString(), false, null);
                var rightSynchronizedItem = childSynchronizedItems.RightItem = new SynchronizedItem("Test" + i.ToString(), false, null);
                testSynchronizedItems.ChildItems.Add(childSynchronizedItems);

                // Запишем команды синхронизации для каждого элемента.
                leftSynchronizedItem.SyncCommand.SetCommandAction(() =>
                {
                    return Task.Run(() => usedCommands.Add(leftSynchronizedItem.Name));
                });

                // Лишь половина элементов будет иметь статус для синхронизации и лишь команды этих элементов должны быть выполнены.
                leftSynchronizedItem.UpdateStatus(i % 2 == 0 ? leftStatus : ItemStatusEnum.Equally);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater();
            synchronizedItemsStatusAndCommandsUpdater.RefreshLeftItemStatusesAndCommandsFromChilds(testSynchronizedItems);
            await testSynchronizedItems.LeftItem.SyncCommand.Process();

            Assert.Equal(2, usedCommands.Count);
            Assert.Equal("Test0", usedCommands[0]);
            Assert.Equal("Test2", usedCommands[1]);
        }

        /// <summary>
        /// Проверка присвоения команд родительской записи от дочерних для правого элемента.
        /// </summary>
        /// <param name="strRightStatus">Статус записей справа, тех которые не Equally.</param>
        [Theory]
        [InlineData("Missing")]
        [InlineData("ThereIs")]
        [InlineData("Older")]
        [InlineData("Newer")]
        public async Task RefreshRightItemStatusesAndCommandsFromChilds_CheckCommands(string strRightStatus)
        {
            var rightStatus = Enum.Parse<ItemStatusEnum>(strRightStatus);

            var usedCommands = new List<string>(); // Наименования элемнтов, чьи команды будут выполнены.

            var testSynchronizedItems = new TestSynchronizedItems();
            testSynchronizedItems.LeftItem = new SynchronizedItem(null, true, null);
            testSynchronizedItems.RightItem = new SynchronizedItem(null, true, null);

            for (byte i = 0; i < 4; i++)
            {
                var childSynchronizedItems = new TestSynchronizedItems();
                childSynchronizedItems.LeftItem = new SynchronizedItem("Test" + i.ToString(), false, null);
                var rightSynchronizedItem = childSynchronizedItems.RightItem = new SynchronizedItem("Test" + i.ToString(), false, null);
                testSynchronizedItems.ChildItems.Add(childSynchronizedItems);

                // Запишем команды синхронизации для каждого элемента.
                rightSynchronizedItem.SyncCommand.SetCommandAction(() =>
                {
                    return Task.Run(() => usedCommands.Add(rightSynchronizedItem.Name));
                });

                // Лишь половина элементов будет иметь статус для синхронизации и лишь команды этих элементов должны быть выполнены.
                rightSynchronizedItem.UpdateStatus(i % 2 == 0 ? rightStatus : ItemStatusEnum.Equally);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater();
            synchronizedItemsStatusAndCommandsUpdater.RefreshRightItemStatusesAndCommandsFromChilds(testSynchronizedItems);
            await testSynchronizedItems.RightItem.SyncCommand.Process();

            Assert.Equal(2, usedCommands.Count);
            Assert.Equal("Test0", usedCommands[0]);
            Assert.Equal("Test2", usedCommands[1]);
        }

        private class TestSynchronizedItems : ISynchronizedItems
        {
            public IDirectory LeftDirectory => LeftItem as IDirectory;

            public IDirectory RightDirectory => RightItem as IDirectory;

            public ISynchronizedItem LeftItem { get; set; }

            public ISynchronizedItem RightItem { get; set; }

            public List<ISynchronizedItems> ChildItems { get; } = new List<ISynchronizedItems>();

            public event Action StartLoadDirectoriesEvent;
            public event Action<ISynchronizedItems> DirectoriesIsLoadedEvent;
            public event Action<ISynchronizedItems> DeleteEvent;
            public event Action DeletedEvent;

            public void IsDeleted()
            {
                throw new NotImplementedException();
            }

            public Task Load()
            {
                throw new NotImplementedException();
            }

            public void LoadChildItems()
            {
                throw new NotImplementedException();
            }
        }
    }
}
