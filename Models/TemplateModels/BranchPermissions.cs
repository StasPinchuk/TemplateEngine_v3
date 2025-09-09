using System;

namespace TemplateEngine_v3.Models.TemplateModels
{
    /// <summary>
    /// Перечисление прав доступа к филиалам.
    /// </summary>
    [Flags]
    public enum BranchPermissions
    {
        /// <summary>
        /// Нет прав доступа.
        /// </summary>
        None = 0,

        /// <summary>
        /// Право на редактирование филиалов.
        /// </summary>
        Edit = 1 << 0,

        /// <summary>
        /// Право на удаление филиалов.
        /// </summary>
        Delete = 1 << 1,

        /// <summary>
        /// Право на создание филиалов.
        /// </summary>
        Create = 1 << 2,

        /// <summary>
        /// Право на копирование филиалов.
        /// </summary>
        Copy = 1 << 3,

        /// <summary>
        /// Полный доступ ко всем операциям с филиалами.
        /// </summary>
        All = Edit | Delete | Create | Copy
    }
}
