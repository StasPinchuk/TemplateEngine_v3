using System;

namespace TemplateEngine_v3.Models.TemplateModels
{
    /// <summary>
    /// Перечисление прав доступа к технологическим процессам.
    /// </summary>
    [Flags]
    public enum TechnologiesPermissions
    {
        /// <summary>
        /// Нет прав доступа.
        /// </summary>
        None = 0,

        /// <summary>
        /// Право на редактирование технологических процессов.
        /// </summary>
        Edit = 1 << 0,

        /// <summary>
        /// Право на удаление технологических процессов.
        /// </summary>
        Delete = 1 << 1,

        /// <summary>
        /// Право на создание технологических процессов.
        /// </summary>
        Create = 1 << 2,

        /// <summary>
        /// Право на копирование технологических процессов.
        /// </summary>
        Copy = 1 << 3,

        /// <summary>
        /// Полный доступ ко всем операциям с технологическими процессами.
        /// </summary>
        All = Edit | Delete | Create | Copy
    }
}
