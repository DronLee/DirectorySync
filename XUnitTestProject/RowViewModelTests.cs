using DirectorySync.Models;
using DirectorySync.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
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

        // Если дочерние элементы имеют разнообразные статусы, стутус родительского будет Unknown.
        [InlineData("Equally", "Equally", new[] { "Missing", "Equally" }, new[] { "ThereIs", "Equally" }, "Unknown", "Unknown")]
        public void RefreshStatusesFromChilds_CheckStatus(string leftStartStatus, string rightStartStatus,
            string[] leftItemsStatuses, string[] rightItemsStatuses,
            string leftExpectedStatus, string rightExpectedStatus)
        {
            var leftItem = new TestItemViewModel("Left", (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftStartStatus));
            var rightItem = new TestItemViewModel("Right", (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightStartStatus));

            var rowViewModel = new RowViewModel(leftItem, rightItem);
            for (byte i = 0; i < leftItemsStatuses.Length; i++)
                rowViewModel.ChildRows.Add(new RowViewModel(
                    new TestItemViewModel("Left" + i.ToString(), (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftItemsStatuses[i])),
                    new TestItemViewModel("Right" + i.ToString(), (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightItemsStatuses[i]))));

            rowViewModel.RefreshStatusesFromChilds();

            Assert.Equal(leftExpectedStatus, rowViewModel.LeftItem.Status.StatusEnum.ToString());
            Assert.Equal(rightExpectedStatus, rowViewModel.RightItem.Status.StatusEnum.ToString());
        }

        /// <summary>
        /// Тест на выполнение команды принятия левого элемента.
        /// </summary>
        [Fact]
        public void LeftItemAcceptCommand()
        {
            var useAcceptCommand = false;
            var leftItemViewModel = new ItemViewModel("LeftItem", false, () => { useAcceptCommand = true; });
            var rightItemViewModel = new ItemViewModel("RightItem", false, () => { });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(10); //  Чтобы успела выполниться команда.

            Assert.True(useAcceptCommand);

            // Сразу после запуска выполнения команды должна удалиться.
            Assert.Null(rowViewModel.LeftItem.AcceptCommand);
            Assert.Null(rowViewModel.RightItem.AcceptCommand);
        }

        /// <summary>
        /// Тест на выполнение команды принятия правого элемента.
        /// </summary>
        [Fact]
        public void RightItemAcceptCommand()
        {
            var useAcceptCommand = false;
            var leftItemViewModel = new ItemViewModel("LeftItem", false, () => { });
            var rightItemViewModel = new ItemViewModel("RightItem", false, () => { useAcceptCommand = true; });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(10); //  Чтобы успела выполниться команда.

            Assert.True(useAcceptCommand);

            // Сразу после запуска выполнения команды должна удалиться.
            Assert.Null(rowViewModel.LeftItem.AcceptCommand);
            Assert.Null(rowViewModel.RightItem.AcceptCommand);
        }

        /// <summary>
        /// Тест на простановку свойств видимости при инициализации.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_Init()
        {
            var leftItemViewModel = new ItemViewModel("LeftItem", false, () => { });
            var rightItemViewModel = new TestItemViewModel("RightItem", ItemStatusEnum.Older);
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel);

            Assert.True(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.ProcessIconIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости сразу после запуска принятия левого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_StartedLeftItemAccept()
        {
            // Sleep, чтобы была возможность проверить свойства в процессе выполнения синхронизации.
            var leftItemViewModel = new ItemViewModel("LeftItem", false, () => { Thread.Sleep(70); });
            var rightItemViewModel = new TestItemViewModel("RightItem", ItemStatusEnum.Older);
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.True(rowViewModel.ProcessIconIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости сразу после запуска принятия правого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_StartedRightItemAccept()
        {
            var leftItemViewModel = new TestItemViewModel("LeftItem", ItemStatusEnum.Older);
            // Sleep, чтобы была возможность проверить свойства в процессе выполнения синхронизации.
            var rightItemViewModel = new ItemViewModel("RightItem", false, () => { Thread.Sleep(70); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.True(rowViewModel.ProcessIconIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости после завершения принятия левого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_FinishedLeftItemAccept()
        {
            var leftItemViewModel = new ItemViewModel("LeftItem", false, () => { });
            var rightItemViewModel = new TestItemViewModel("RightItem", ItemStatusEnum.Older);
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.ProcessIconIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости после завершения принятия правого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_FinishedRightItemAccept()
        {
            var leftItemViewModel = new TestItemViewModel("LeftItem", ItemStatusEnum.Older);
            var rightItemViewModel = new ItemViewModel("RightItem", false, () => { });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.ProcessIconIsVisible);
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

            public ICommand AcceptCommand { get; set; }

            public string IconPath => throw new NotImplementedException();

            public bool IsDirectory { get; }

            public event PropertyChangedEventHandler PropertyChanged;
            public event Action StartedSyncEvent;
            public event Action FinishedSyncEvent;

            public void SetActionCommand(Action action)
            {
                throw new NotImplementedException();
            }

            public void UpdateStatus(ItemStatusEnum statusEnum)
            {
                Status = new ItemStatus(statusEnum);
            }
        }
    }
}