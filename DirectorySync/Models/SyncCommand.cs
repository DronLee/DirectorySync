using System;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Команда синхронизации.
    /// </summary>
    public class SyncCommand
    {
        /// <summary>
        /// Действие команды синхронизации.
        /// </summary>
        public Func<Task> CommandAction { get; private set; }

        /// <summary>
        /// Событие изменения команды синхронизации.
        /// </summary>
        public event Action CommandActionChangedEvent;

        /// <summary>
        /// Событие запуска синхронизации.
        /// </summary>
        public event Action StartedSyncEvent;

        /// <summary>
        /// Событие завершения синхронизации.
        /// </summary>
        public event Action FinishedSyncEvent;

        /// <summary>
        /// Задание действия команды синхронизации.
        /// </summary>
        /// <param name="action">Действие команды синхронизации.</param>
        public void SetCommandAction(Func<Task> action)
        {
            if (CommandAction != action)
            {
                CommandAction = action;
                CommandActionChangedEvent?.Invoke();
            }
        }

        /// <summary>
        /// Выполнить команду синхронизации.
        /// </summary>
        public async Task Process()
        {
            if (CommandAction != null)
            {
                StartedSyncEvent?.Invoke();
                await CommandAction.Invoke();
                FinishedSyncEvent?.Invoke();
            }
        }
    }
}