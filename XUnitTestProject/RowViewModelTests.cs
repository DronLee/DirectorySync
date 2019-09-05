using DirectorySync.Models;
using DirectorySync.ViewModels;
using System;
using System.ComponentModel;
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

            public ICommand AcceptCommand => throw new NotImplementedException();

            public event PropertyChangedEventHandler PropertyChanged;

            public void UpdateStatus(ItemStatusEnum statusEnum)
            {
                Status = new ItemStatus(statusEnum);
            }
        }
    }
}