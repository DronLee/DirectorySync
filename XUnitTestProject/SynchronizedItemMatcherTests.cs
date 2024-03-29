﻿using DirectorySync.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class SynchronizedItemMatcherTests
    {
        /// <summary>
        /// Тест на простановку статусов при различии сравниваемых элементов.
        /// </summary>
        /// <param name="date1">Дата последнего обновления первого элемента.</param>
        /// <param name="date2">Дата последнего обновления второго элемента.</param>
        /// <param name="strExpextedStatus1">Ожидаемый статус первого элемента.</param>
        /// <param name="strExpextedStatus2">Ожидаемый статус второго элемента.</param>
        [Theory]
        [InlineData("2019-01-01", "2018-01-01", "Newer", "Older")]
        [InlineData("2018-01-01", "2019-01-01", "Older", "Newer")]
        [InlineData("2019-01-01", null, "ThereIs", "Missing")]
        [InlineData(null, "2019-01-01", "Missing", "ThereIs")]
        public void UpdateStatusesAndCommands(string date1, string date2, string strExpextedStatus1, string strExpextedStatus2)
        {
            var expextedStatus1 = Enum.Parse<ItemStatusEnum>(strExpextedStatus1);
            var expextedStatus2 = Enum.Parse<ItemStatusEnum>(strExpextedStatus2);

            var item1 = new SynchronizedItem(null, false, date1 == null ? null :
                 new TestItem(DateTime.Parse(date1)));
            var item2 = new SynchronizedItem(null, false, date2 == null ? null :
                 new TestItem(DateTime.Parse(date2)));

            var matcher = new SynchronizedItemMatcher();
            matcher.UpdateStatusesAndCommands(item1, item2);

            Assert.Equal(expextedStatus1, item1.Status.StatusEnum);
            Assert.Equal(expextedStatus2, item2.Status.StatusEnum);
            Assert.NotNull(item1.SyncCommand.CommandAction);
            Assert.NotNull(item2.SyncCommand.CommandAction);
        }

        /// <summary>
        /// Тест на простановку статуса ошибки загрузки.
        /// </summary>
        /// <param name="loadError1">Текст ошибки для первого элемента.</param>
        /// <param name="loadError2">Текст ошибки для второго элемента.</param>
        [Theory]
        [InlineData("Error1", null)]
        [InlineData(null, "Error2")]
        public void UpdateStatusesAndCommandsForLoadError(string loadError1, string loadError2)
        {
            var itemViewModel1 = new SynchronizedItem(null, false, new TestItem(DateTime.Now) { LastLoadError = loadError1 });
            var itemViewModel2 = new SynchronizedItem(null, false, new TestItem(DateTime.Now) { LastLoadError = loadError2 });

            // Чтобы потом проверить, что команд не стало.
            itemViewModel1.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            itemViewModel2.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });

            var matcher = new SynchronizedItemMatcher();
            matcher.UpdateStatusesAndCommands(itemViewModel1, itemViewModel2);

            Assert.Equal(ItemStatusEnum.LoadError, itemViewModel1.Status.StatusEnum);
            Assert.Equal(ItemStatusEnum.LoadError, itemViewModel2.Status.StatusEnum);
            Assert.Null(itemViewModel1.SyncCommand.CommandAction);
            Assert.Null(itemViewModel2.SyncCommand.CommandAction);
        }

        /// <summary>
        /// Тест на проостановку статуса Equally.
        /// </summary>
        [Fact]
        public void UpdateStatusesAndCommandsForEquals()
        {
            var lastUpdate = DateTime.Now;

            var item1 = new SynchronizedItem(null, false, new TestItem(lastUpdate));
            var item2 = new SynchronizedItem(null, false, new TestItem(lastUpdate));

            // Чтобы потом проверить, что команд не стало.
            item1.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            item2.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });

            var matcher = new SynchronizedItemMatcher();
            matcher.UpdateStatusesAndCommands(item1, item2);

            Assert.Equal(ItemStatusEnum.Equally, item1.Status.StatusEnum);
            Assert.Equal(ItemStatusEnum.Equally, item2.Status.StatusEnum);
            Assert.Null(item1.SyncCommand.CommandAction);
            Assert.Null(item2.SyncCommand.CommandAction);
        }

        private class TestItem : IDirectory
        {
            public TestItem(DateTime lastUpdate)
            {
                LastUpdate = lastUpdate;
            }

            public string Name => throw new NotImplementedException();

            public string FullPath => throw new NotImplementedException();

            public DateTime LastUpdate { get; }

            public IItem[] Items => throw new NotImplementedException();

            public bool IsLoaded => throw new NotImplementedException();

            public string LastLoadError { get; set; }

            public string[] ExcludedExtensions { get; set; }

            public event Action<IItem> DeletedEvent;
            public event Action<IItem, string> CopiedFromToEvent;
            public event Action<string> SyncErrorEvent;
            public event Action<IDirectory> LoadedDirectoryEvent;

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
                throw new NotImplementedException();
            }
        }
    }
}