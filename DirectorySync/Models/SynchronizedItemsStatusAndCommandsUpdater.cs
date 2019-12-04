using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Класс, отвечающий за обновление статусов и комманд синхронизируемых элементов. 
    /// </summary>
    public class SynchronizedItemsStatusAndCommandsUpdater : ISynchronizedItemsStatusAndCommandsUpdater
    {
        private static readonly Dictionary<ItemStatusEnum, string> _statusCommentsFromChildren = new Dictionary<ItemStatusEnum, string>
            {
                {ItemStatusEnum.Missing, "Не хватает тех элементов, что есть с другой стороны"},
                {ItemStatusEnum.ThereIs, "Содержит отсутствующие с другой стороны элементы"},
                {ItemStatusEnum.Older, "Содержит более старые"},
                {ItemStatusEnum.Newer, "Содержит более новые"}
            };

        private readonly ISynchronizedItemMatcher _synchronizedItemMatcher;

        public SynchronizedItemsStatusAndCommandsUpdater(ISynchronizedItemMatcher synchronizedItemMatcher)
        {
            _synchronizedItemMatcher = synchronizedItemMatcher;
        }

        /// <summary>
        /// Обновление статуса и команд левого элемента на основе дочерних.
        /// </summary>
        /// <param name="synchronizedItems">Синхронизируемые элементы, статусы и команды которых будут обновлены.</param>
        public void RefreshLeftItemStatusesAndCommandsFromChilds(ISynchronizedItems synchronizedItems)
        {
            if (synchronizedItems.ChildItemsCount > 0)
            {
                var notEquallyChilds = synchronizedItems.ChildItems.Where(r => r.LeftItem.Status.StatusEnum != ItemStatusEnum.Equally).ToArray();

                if (notEquallyChilds.Length == 0)
                {
                    // Если все дочерние строки имеют статус Equally, то и данная строка должна иметь такой сатус, и команд никаких быть при этом не должно.
                    synchronizedItems.LeftItem.UpdateStatus(ItemStatusEnum.Equally);
                    synchronizedItems.LeftItem.SyncCommand.SetCommandAction(null);
                }
                else if (notEquallyChilds.Any(r => r.LeftItem.Status.StatusEnum == ItemStatusEnum.Unknown))
                    ItemStatusUnknown(synchronizedItems.LeftItem);
                else
                {
                    var leftStatuses = notEquallyChilds.Select(r => r.LeftItem.Status.StatusEnum).Distinct().ToArray();

                    if (leftStatuses.Length == 1)
                        SetItemStatusAndCommands(synchronizedItems.LeftItem, leftStatuses.First(), notEquallyChilds.Select(r => r.LeftItem.SyncCommand.CommandAction));
                    else
                        ItemStatusUnknown(synchronizedItems.LeftItem);
                }
            }
        }

        /// <summary>
        /// Обновление статуса и команд правого элемента на основе дочерних.
        /// </summary>
        /// <param name="synchronizedItems">Синхронизируемые элементы, статусы и команды которых будут обновлены.</param>
        public void RefreshRightItemStatusesAndCommandsFromChilds(ISynchronizedItems synchronizedItems)
        {
            if (synchronizedItems.ChildItemsCount   > 0)
            {
                var notEquallyChilds = synchronizedItems.ChildItems.Where(r => r.RightItem.Status.StatusEnum != ItemStatusEnum.Equally).ToArray();

                if (notEquallyChilds.Length == 0)
                {
                    // Если все дочерние строки имеют статус Equally, то и данная строка должна иметь такой сатус, и команд никаких быть при этом не должно.
                    synchronizedItems.RightItem.UpdateStatus(ItemStatusEnum.Equally);
                    synchronizedItems.RightItem.SyncCommand.SetCommandAction(null);
                }
                else if (notEquallyChilds.Any(r => r.RightItem.Status.StatusEnum == ItemStatusEnum.Unknown))
                    ItemStatusUnknown(synchronizedItems.RightItem);
                else
                {
                    var rightStatuses = notEquallyChilds.Select(r => r.RightItem.Status.StatusEnum).Distinct().ToArray();

                    if (rightStatuses.Length == 1)
                        SetItemStatusAndCommands(synchronizedItems.RightItem, rightStatuses.First(), notEquallyChilds.Select(r => r.RightItem.SyncCommand.CommandAction));
                    else
                        ItemStatusUnknown(synchronizedItems.RightItem);
                }
            }
        }

        /// <summary>
        /// Простановка в модели синхронизируемых элементов статусов и команд синхронизации.
        /// </summary>
        /// <param name="item1">Модель одного элемента.</param>
        /// <param name="item2">Модель второго элемента.</param>
        public void UpdateStatusesAndCommands(ISynchronizedItem item1, ISynchronizedItem item2)
        {
            _synchronizedItemMatcher.UpdateStatusesAndCommands(item1, item2);
        }

        private void ItemStatusUnknown(ISynchronizedItem item)
        {
            item.UpdateStatus(ItemStatusEnum.Unknown);
            item.SyncCommand.SetCommandAction(null);
        }

        /// <summary>
        /// Задание статуса и комманд синхронизации для синхронизируемого элемента, исходя из дочерних неидентичных строк.
        /// </summary>
        /// <param name="synchronizedItem">Синхронизируемый элемент, для которого задаётся статус и команды.</param>
        /// <param name="status">Задаваемй статус.</param>
        /// <param name="actionCommands">Команды синхронизации.</param>
        private void SetItemStatusAndCommands(ISynchronizedItem synchronizedItem, ItemStatusEnum status, IEnumerable<Func<Task>> actionCommands)
        {
            synchronizedItem.UpdateStatus(status, _statusCommentsFromChildren.ContainsKey(status) ?
                            _statusCommentsFromChildren[status] : null);

            // Если нет, команды, но должна быть, исходя из дочерних элементов,
            // то можно команду представить как последовательное выпонения команд дочерних элементов. 
            if (status != ItemStatusEnum.Equally)
                synchronizedItem.SyncCommand.SetCommandAction(async () =>
                {
                    foreach (var actionCommand in actionCommands)
                        await actionCommand.Invoke();
                });
        }
    }
}