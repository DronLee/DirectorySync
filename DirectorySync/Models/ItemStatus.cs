﻿namespace DirectorySync.Models
{
    /// <summary>
    /// Статус элемента.
    /// </summary>
    internal enum ItemStatus : byte
    {
        /// <summary>
        /// Неизвестно.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Имеется.
        /// </summary>
        ThereIs = 1,
        /// <summary>
        /// Отсутствует.
        /// </summary>
        Missing = 2,
        /// <summary>
        /// Более новый.
        /// </summary>
        Newer = 3,
        /// <summary>
        /// Более старый.
        /// </summary>
        Older = 4,
        /// <summary>
        /// Идентичный.
        /// </summary>
        Equally = 5
    }
}