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

            var testSynchronizedItems = new TestSynchronizedItems(null);

            for (byte i = 0; i < 4; i++)
            {
                var childSynchronizedItems = new TestSynchronizedItems("Test" + i.ToString());
                testSynchronizedItems.ChildItems.Add(childSynchronizedItems);

                // Запишем команды синхронизации для каждого элемента.
                childSynchronizedItems.LeftItem.SyncCommand.SetCommandAction(() =>
                {
                    return Task.Run(() => usedCommands.Add(childSynchronizedItems.LeftItem.Name));
                });

                // Лишь половина элементов будет иметь статус для синхронизации и лишь команды этих элементов должны быть выполнены.
                childSynchronizedItems.LeftItem.UpdateStatus(i % 2 == 0 ? leftStatus : ItemStatusEnum.Equally);
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

            var testSynchronizedItems = new TestSynchronizedItems(null);

            for (byte i = 0; i < 4; i++)
            {
                var childSynchronizedItems = new TestSynchronizedItems("Test" + i.ToString());
                testSynchronizedItems.ChildItems.Add(childSynchronizedItems);

                // Запишем команды синхронизации для каждого элемента.
                childSynchronizedItems.RightItem.SyncCommand.SetCommandAction(() =>
                {
                    return Task.Run(() => usedCommands.Add(childSynchronizedItems.RightItem.Name));
                });

                // Лишь половина элементов будет иметь статус для синхронизации и лишь команды этих элементов должны быть выполнены.
                childSynchronizedItems.RightItem.UpdateStatus(i % 2 == 0 ? rightStatus : ItemStatusEnum.Equally);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater();
            synchronizedItemsStatusAndCommandsUpdater.RefreshRightItemStatusesAndCommandsFromChilds(testSynchronizedItems);
            await testSynchronizedItems.RightItem.SyncCommand.Process();

            Assert.Equal(2, usedCommands.Count);
            Assert.Equal("Test0", usedCommands[0]);
            Assert.Equal("Test2", usedCommands[1]);
        }

        /// <summary>
        /// Проверка обновления статусов моделей синхронизируемых элементов слева на основе дочерних элементов.
        /// </summary>
        /// <param name="leftStartStatus">Начальное значение статуса левого элемента.</param>
        /// <param name="leftItemsStatuses">Статусы левых дочерних элементов.</param>
        /// <param name="leftExpectedStatus">Ожидаемое значение статуса левого элемента после обновления.</param>
        [Theory]

        // Если нет дочерних элементов, то статус должен оставаться прежним.
        [InlineData("Equally", new string[0], "Equally")]

        [InlineData("Equally", new[] { "Newer" }, "Newer")]
        [InlineData("Equally", new[] { "Missing" }, "Missing")]
        [InlineData("Unknown", new[] { "Equally" }, "Equally")]

        // Если дочерние элементы имеют один статус не считая Equally, то статус родительского будет такой же, как этот один.
        [InlineData("Equally", new[] { "Missing", "Equally" }, "Missing")]

        // Если дочерние элементы имеют разнообразные статусы, стутус родительского будет Unknown.
        [InlineData("Equally", new[] { "Missing", "ThereIs" }, "Unknown")]
        public void RefreshLeftItemStatusesAndCommandsFromChilds_CheckStatus(string leftStartStatus, string[] leftItemsStatuses, string leftExpectedStatus)
        {
            var testSynchronizedItems = new TestSynchronizedItems(null);
            testSynchronizedItems.LeftItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftStartStatus));

            foreach (var leftItemStatus in leftItemsStatuses)
            {
                var childSynchronizedItems = new TestSynchronizedItems(null);
                childSynchronizedItems.LeftItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftItemStatus));
                testSynchronizedItems.ChildItems.Add(childSynchronizedItems);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater();
            synchronizedItemsStatusAndCommandsUpdater.RefreshLeftItemStatusesAndCommandsFromChilds(testSynchronizedItems);

            Assert.Equal(leftExpectedStatus, testSynchronizedItems.LeftItem.Status.StatusEnum.ToString());
        }

        /// <summary>
        /// Проверка обновления статусов моделей синхронизируемых элементов справа на основе дочерних элементов.
        /// </summary>
        /// <param name="rightStartStatus">Начальное значение статуса правого элемента.</param>
        /// <param name="rightItemsStatuses">Статусы правых дочерних элементов.</param>
        /// <param name="rightExpectedStatus">Ожидаемое значение статуса правого элемента после обновления.</param>
        [Theory]

        // Если нет дочерних элементов, то статус должен оставаться прежним.
        [InlineData("Equally", new string[0], "Equally")]

        [InlineData("Equally", new[] { "Newer" }, "Newer")]
        [InlineData("Equally", new[] { "Missing" }, "Missing")]
        [InlineData("Unknown", new[] { "Equally" }, "Equally")]

        // Если дочерние элементы имеют один статус не считая Equally, то статус родительского будет такой же, как этот один.
        [InlineData("Equally", new[] { "Missing", "Equally" }, "Missing")]

        // Если дочерние элементы имеют разнообразные статусы, стутус родительского будет Unknown.
        [InlineData("Equally", new[] { "Missing", "ThereIs" }, "Unknown")]
        public void RefreshRightItemStatusesAndCommandsFromChilds_CheckStatus(string rightStartStatus, string[] rightItemsStatuses, string rightExpectedStatus)
        {
            var testSynchronizedItems = new TestSynchronizedItems(null);
            testSynchronizedItems.RightItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightStartStatus));

            foreach(var rightItemStatus in rightItemsStatuses)
            {
                var childSynchronizedItems = new TestSynchronizedItems(null);
                childSynchronizedItems.RightItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightItemStatus));
                testSynchronizedItems.ChildItems.Add(childSynchronizedItems);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater();
            synchronizedItemsStatusAndCommandsUpdater.RefreshRightItemStatusesAndCommandsFromChilds(testSynchronizedItems);

            Assert.Equal(rightExpectedStatus, testSynchronizedItems.RightItem.Status.StatusEnum.ToString());
        }

        private class TestSynchronizedItems : ISynchronizedItems
        {
            public TestSynchronizedItems(string itemName)
            {
                LeftItem = new SynchronizedItem(itemName, true, null);
                RightItem = new SynchronizedItem(itemName, true, null);
            }

            public IDirectory LeftDirectory => LeftItem as IDirectory;

            public IDirectory RightDirectory => RightItem as IDirectory;

            public ISynchronizedItem LeftItem { get; }

            public ISynchronizedItem RightItem { get; }

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
