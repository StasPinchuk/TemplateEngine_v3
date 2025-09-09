using System;

namespace TemplateEngine_v3.Models.TemplateModels
{
    /// <summary>
    /// Перечисление прав доступа к шаблонам.
    /// </summary>
    [Flags]
    public enum TemplatePermissions
    {
        /// <summary>
        /// Нет прав доступа.
        /// </summary>
        None = 0,

        /// <summary>
        /// Право на редактирование шаблонов.
        /// </summary>
        Edit = 1 << 0,

        /// <summary>
        /// Право на удаление шаблонов.
        /// </summary>
        Delete = 1 << 1,

        /// <summary>
        /// Право на создание шаблонов.
        /// </summary>
        Create = 1 << 2,

        /// <summary>
        /// Право на копирование шаблонов.
        /// </summary>
        Copy = 1 << 3,

        /// <summary>
        /// Полный доступ ко всем операциям с шаблонами.
        /// </summary>
        All = Edit | Delete | Create | Copy
    }
}
