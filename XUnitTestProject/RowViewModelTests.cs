using DirectorySync.Models;
using DirectorySync.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace XUnitTestProject
{
    public class RowViewModelTests
    {
        /// <summary>
        /// Проверка обновления статусов моделей представлений отслеживаемых элементов
        /// на основе статусов дочерних элементов.
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
        public void RefreshStatusesFromChilds_CheckStatus(string leftStartStatus, string rightStartStatus,
            string[] leftItemsStatuses, string[] rightItemsStatuses,
            string leftExpectedStatus, string rightExpectedStatus)
        {
            var leftItem = new TestItemViewModel("Left", (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftStartStatus));
            var rightItem = new TestItemViewModel("Right", (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightStartStatus));

            var rowViewModel = new RowViewModel(leftItem, rightItem, null);
            for (byte i = 0; i < leftItemsStatuses.Length; i++)
                rowViewModel.ChildRows.Add(new RowViewModel(
                    new TestItemViewModel("Left" + i.ToString(), (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftItemsStatuses[i])),
                    new TestItemViewModel("Right" + i.ToString(), (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightItemsStatuses[i])), null));

            rowViewModel.RefreshStatusesFromChilds();

            Assert.Equal(leftExpectedStatus, rowViewModel.LeftItem.Status.StatusEnum.ToString());
            Assert.Equal(rightExpectedStatus, rowViewModel.RightItem.Status.StatusEnum.ToString());
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
        public void RefreshStatusesFromChilds_CheckCommands(string strLeftStatus, string strRightStatus, bool leftAccept)
        {
            var leftStatus = Enum.Parse<ItemStatusEnum>(strLeftStatus);
            var rightStatus = Enum.Parse<ItemStatusEnum>(strRightStatus);

            var expectedCopyDestinationPathes = new List<string>();
            var resultCopyDestinationPathes = new List<string>();

            // Объект блокировки для списка resultCopyDestinationPathes. Так как в него запись может вестись с разных потоков.
            var resultCopyDestinationPathesLocker = new Object();

            var rowViewModel = new RowViewModel(new ItemViewModel(null, true, new TestItem()), new ItemViewModel(null, true, new TestItem()), null);

            for (byte i = 0; i < 4; i++)
            {
                var leftItemViewModel = new ItemViewModel("Test" + i.ToString(), false, new TestItem());
                var rightItemViewModel = new ItemViewModel("Test" + i.ToString(), false, new TestItem());

                // Запишем команды синхронизации для каждого элемента и подпишемся на их выполнение.
                leftItemViewModel.SetActionCommand(() => leftItemViewModel.Item.CopyTo(rightItemViewModel.FullPath));
                leftItemViewModel.Item.CopiedFromToEvent += (IItem item1, string destinationPath) =>
                {
                    lock (resultCopyDestinationPathesLocker)
                        resultCopyDestinationPathes.Add(destinationPath);
                };
                rightItemViewModel.SetActionCommand(() => rightItemViewModel.Item.CopyTo(leftItemViewModel.FullPath));
                rightItemViewModel.Item.CopiedFromToEvent += (IItem item1, string destinationPath) =>
                {
                    lock (resultCopyDestinationPathesLocker)
                        resultCopyDestinationPathes.Add(destinationPath);
                };

                if (i % 2 == 0) // Половина элементов для синхронизации, вторая половина должна оставаться нетронутой.
                {
                    expectedCopyDestinationPathes.Add(rightItemViewModel.FullPath);
                    leftItemViewModel.UpdateStatus(leftStatus);
                    rightItemViewModel.UpdateStatus(rightStatus);
                }
                else
                {
                    leftItemViewModel.UpdateStatus(ItemStatusEnum.Equally);
                    rightItemViewModel.UpdateStatus(ItemStatusEnum.Equally);
                }

                rowViewModel.ChildRows.Add(new RowViewModel(leftItemViewModel, rightItemViewModel, rowViewModel));
            }

            rowViewModel.RefreshStatusesFromChilds();
            if (leftAccept)
                rowViewModel.LeftItem.AcceptCommand.Execute(null);
            else
                rowViewModel.RightItem.AcceptCommand.Execute(null);

            Thread.Sleep(20); // Чтобы успели выполниться команды.

            Assert.Equal(string.Join("|", expectedCopyDestinationPathes.ToArray()),
                string.Join("|", resultCopyDestinationPathes.OrderBy(p => p).ToArray()));
        }

        /// <summary>
        /// Тест на выполнение команды принятия левого элемента.
        /// </summary>
        [Fact]
        public void LeftItemAcceptCommand()
        {
            var useAcceptCommand = false;
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            var rightItemViewModel = new ItemViewModel("RightItem", false, null);
            leftItemViewModel.SetActionCommand(() => Task.Run(() => { useAcceptCommand = true; }) );
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(15); //  Чтобы успела выполниться команда.

            Assert.True(useAcceptCommand);
        }

        /// <summary>
        /// Тест на выполнение команды принятия правого элемента.
        /// </summary>
        [Fact]
        public void RightItemAcceptCommand()
        {
            var useAcceptCommand = false;
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            var rightItemViewModel = new ItemViewModel("RightItem", false, null);
            rightItemViewModel.SetActionCommand(() => Task.Run(() => { useAcceptCommand = true; }));
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(15); //  Чтобы успела выполниться команда.

            Assert.True(useAcceptCommand);
        }

        /// <summary>
        /// Тест на простановку свойств видимости при инициализации.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_Init()
        {
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            leftItemViewModel.SetActionCommand(() => Task.Run(() => { }));
            var rightItemViewModel = new ItemViewModel("RightItem", false, null);
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);

            // Когда строка только создана, она считается ещё в процессе загрузки. Этот признак должен быть потом убран.
            Assert.True(rowViewModel.InProcess);
            // Соответсвенно, пока строка "в процессе", кнопки синхронизации не должны быть доступны.
            Assert.False(rowViewModel.CommandButtonIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости сразу после запуска принятия левого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_StartedLeftItemAccept()
        {
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            var rightItemViewModel = new TestItemViewModel("RightItem", ItemStatusEnum.Older);
            // Sleep, чтобы была возможность проверить свойства в процессе выполнения синхронизации.
            leftItemViewModel.SetActionCommand(() => { Thread.Sleep(70); return Task.FromResult(true); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.True(rowViewModel.InProcess);
        }

        /// <summary>
        /// Тест на простановку свойств видимости сразу после запуска принятия правого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_StartedRightItemAccept()
        {
            var leftItemViewModel = new TestItemViewModel("LeftItem", ItemStatusEnum.Older);
            var rightItemViewModel = new ItemViewModel("RightItem", false, null);
            // Sleep, чтобы была возможность проверить свойства в процессе выполнения синхронизации.
            rightItemViewModel.SetActionCommand(() => { Thread.Sleep(70); return Task.FromResult(true); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.True(rowViewModel.InProcess);
        }

        /// <summary>
        /// Тест на простановку свойств видимости после завершения принятия левого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_FinishedLeftItemAccept()
        {
            var leftItemViewModel = new ItemViewModel("LeftItem", false, new TestItem());
            leftItemViewModel.SetActionCommand(() => { return Task.FromResult(true); });
            var rightItemViewModel = new ItemViewModel("RightItem", false, new TestItem());
            rightItemViewModel.SetActionCommand(() => { return Task.FromResult(true); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.True(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.InProcess);
        }

        /// <summary>
        /// Тест на простановку свойств видимости после завершения принятия правого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_FinishedRightItemAccept()
        {
            var leftItemViewModel = new ItemViewModel("LeftItem", false, new TestItem());
            leftItemViewModel.SetActionCommand(() => { return Task.FromResult(true); });
            var rightItemViewModel = new ItemViewModel("RightItem", false, new TestItem());
            rightItemViewModel.SetActionCommand(() => { return Task.FromResult(true); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); // Чтобы успели обновиться свойства.

            Assert.True(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.InProcess);
        }

        private class TestItemViewModel : IItemViewModel
        {
            public TestItemViewModel(string name, ItemStatusEnum status)
            {
                Name = name;
                Status = new ItemStatus(status);
            }

            public string Name { get; }

            public IDirectory Directory => null;

            public ItemStatus Status { get; private set; }

            public Func<Task> CommandAction { get; }

            public ICommand AcceptCommand { get; set; }

            public string IconPath => throw new NotImplementedException();

            public bool IsDirectory { get; }

            public IItem Item => throw new NotImplementedException();

            public string FullPath => throw new NotImplementedException();

            public event Action AcceptCommandChangedEvent;
            public event Action StartedSyncEvent;
            public event Action<IItemViewModel> FinishedSyncEvent;
            public event Action<IItemViewModel, IItemViewModel> CopiedFromToEvent;
            public event PropertyChangedEventHandler PropertyChanged;
            public event Action<string> SyncErrorEvent;

            public void SetActionCommand(Func<Task> action) { }

            public void UpdateStatus(ItemStatusEnum statusEnum, string comment = null)
            {
                Status = new ItemStatus(statusEnum);
            }
        }

        private class TestItem : IItem
        {
            public string Name { get; private set; }

            public string FullPath => throw new NotImplementedException();

            public DateTime LastUpdate => throw new NotImplementedException();

            public event Action<IItem> DeletedEvent;
            public event Action<string> SyncErrorEvent;
            public event Action<IItem, string> CopiedFromToEvent;

            public Task CopyTo(string destinationPath)
            {
                return Task.Run(() => CopiedFromToEvent?.Invoke(null, destinationPath));
            }

            public Task Delete()
            {
                throw new NotImplementedException();
            }
        }
    }
}