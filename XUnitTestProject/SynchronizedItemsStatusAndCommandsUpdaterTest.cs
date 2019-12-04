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
                testSynchronizedItems.ChildItemsList.Add(childSynchronizedItems);

                // Запишем команды синхронизации для каждого элемента.
                childSynchronizedItems.LeftItem.SyncCommand.SetCommandAction(() =>
                {
                    return Task.Run(() => usedCommands.Add(childSynchronizedItems.LeftItem.Name));
                });

                // Лишь половина элементов будет иметь статус для синхронизации и лишь команды этих элементов должны быть выполнены.
                childSynchronizedItems.LeftItem.UpdateStatus(i % 2 == 0 ? leftStatus : ItemStatusEnum.Equally);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater(null);
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
                testSynchronizedItems.ChildItemsList.Add(childSynchronizedItems);

                // Запишем команды синхронизации для каждого элемента.
                childSynchronizedItems.RightItem.SyncCommand.SetCommandAction(() =>
                {
                    return Task.Run(() => usedCommands.Add(childSynchronizedItems.RightItem.Name));
                });

                // Лишь половина элементов будет иметь статус для синхронизации и лишь команды этих элементов должны быть выполнены.
                childSynchronizedItems.RightItem.UpdateStatus(i % 2 == 0 ? rightStatus : ItemStatusEnum.Equally);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater(null);
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
                testSynchronizedItems.ChildItemsList.Add(childSynchronizedItems);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater(null);
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
                testSynchronizedItems.ChildItemsList.Add(childSynchronizedItems);
            }

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater(null);
            synchronizedItemsStatusAndCommandsUpdater.RefreshRightItemStatusesAndCommandsFromChilds(testSynchronizedItems);

            Assert.Equal(rightExpectedStatus, testSynchronizedItems.RightItem.Status.StatusEnum.ToString());
        }

        /// <summary>
        /// Проверка простановки статусов и отсутствия команд слева при дочерних элементах с неопределённым статусом.
        /// </summary>
        [Fact]
        public void RefreshLeftItemStatusesAndCommandsFromChilds_UnknownChild()
        {
            var testSynchronizedItems = new TestSynchronizedItems(null);

            var childSynchronizedDirectories = new TestSynchronizedItems(null);
            childSynchronizedDirectories.LeftItem.UpdateStatus(ItemStatusEnum.Unknown);
            testSynchronizedItems.ChildItemsList.Add(childSynchronizedDirectories);

            var level2Child1 = new TestSynchronizedItems(null);
            level2Child1.LeftItem.UpdateStatus(ItemStatusEnum.Newer);
            level2Child1.LeftItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            childSynchronizedDirectories.ChildItemsList.Add(level2Child1);

            var level2Child2 = new TestSynchronizedItems(null);
            level2Child2.LeftItem.UpdateStatus(ItemStatusEnum.Missing);
            level2Child2.LeftItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            childSynchronizedDirectories.ChildItemsList.Add(level2Child2);

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater(null);
            synchronizedItemsStatusAndCommandsUpdater.RefreshLeftItemStatusesAndCommandsFromChilds(testSynchronizedItems);

            // У дочерней строки должен остаться неопределённый статус.
            Assert.Equal(ItemStatusEnum.Unknown, childSynchronizedDirectories.LeftItem.Status.StatusEnum);

            // Статусы родительской строки должны измениться на неопредёлённые.
            Assert.Equal(ItemStatusEnum.Unknown, testSynchronizedItems.LeftItem.Status.StatusEnum);

            // Команд не должно быть ни у дочерней строки, ни у родительской.
            Assert.Null(childSynchronizedDirectories.LeftItem.SyncCommand.CommandAction);
            Assert.Null(testSynchronizedItems.LeftItem.SyncCommand.CommandAction);
        }

        /// <summary>
        /// Проверка простановки статусов и отсутствия команд справа при дочерних элементах с неопределённым статусом.
        /// </summary>
        [Fact]
        public void RefreshRightItemStatusesAndCommandsFromChilds_UnknownChild()
        {
            var testSynchronizedItems = new TestSynchronizedItems(null);

            var childSynchronizedDirectories = new TestSynchronizedItems(null);
            childSynchronizedDirectories.RightItem.UpdateStatus(ItemStatusEnum.Unknown);
            testSynchronizedItems.ChildItemsList.Add(childSynchronizedDirectories);

            var level2Child1 = new TestSynchronizedItems(null);
            level2Child1.RightItem.UpdateStatus(ItemStatusEnum.Older);
            level2Child1.RightItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            childSynchronizedDirectories.ChildItemsList.Add(level2Child1);

            var level2Child2 = new TestSynchronizedItems(null);
            level2Child2.RightItem.UpdateStatus(ItemStatusEnum.ThereIs);
            level2Child2.RightItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            childSynchronizedDirectories.ChildItemsList.Add(level2Child2);

            var synchronizedItemsStatusAndCommandsUpdater = new SynchronizedItemsStatusAndCommandsUpdater(null);
            synchronizedItemsStatusAndCommandsUpdater.RefreshRightItemStatusesAndCommandsFromChilds(testSynchronizedItems);

            // У дочерней строки должен остаться неопределённый статус.
            Assert.Equal(ItemStatusEnum.Unknown, childSynchronizedDirectories.RightItem.Status.StatusEnum);

            // Статусы родительской строки должны измениться на неопредёлённые.
            Assert.Equal(ItemStatusEnum.Unknown, testSynchronizedItems.RightItem.Status.StatusEnum);

            // Команд не должно быть ни у дочерней строки, ни у родительской.
            Assert.Null(childSynchronizedDirectories.RightItem.SyncCommand.CommandAction);
            Assert.Null(testSynchronizedItems.RightItem.SyncCommand.CommandAction);
        }

        /// <summary>
        /// Упрощённая модель синхронизируемых элементов для тестирования. Предоставляет список дочерних элементов и имеет упрощённый конструктор,
        /// добавляющий пару оодинаковых элементов.
        /// </summary>
        private class TestSynchronizedItems : ISynchronizedItems
        {
            public readonly List<ISynchronizedItems> ChildItemsList = new List<ISynchronizedItems>();

            public TestSynchronizedItems(string itemName)
            {
                LeftItem = new SynchronizedItem(itemName, true, null);
                RightItem = new SynchronizedItem(itemName, true, null);
            }

            public IDirectory LeftDirectory => LeftItem as IDirectory;

            public IDirectory RightDirectory => RightItem as IDirectory;

            public ISynchronizedItem LeftItem { get; }

            public ISynchronizedItem RightItem { get; }

            public ISynchronizedItems[] ChildItems => ChildItemsList.ToArray();

            public bool InProcess => throw new NotImplementedException();

            public int ChildItemsCount => ChildItemsList.Count;

            public event Action StartLoadDirectoriesEvent;
            public event Action<ISynchronizedItems> DirectoriesIsLoadedEvent;
            public event Action<ISynchronizedItems> DeleteEvent;
            public event Action DeletedEvent;
            public event Action<bool> InProcessChangedEvent;

            public void InProcessChange(bool inProcess)
            {
                throw new NotImplementedException();
            }

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