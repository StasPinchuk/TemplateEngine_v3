using System;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для проверки наличия прав доступа.
    /// </summary>
    public interface IPermissions
    {
        /// <summary>
        /// Проверяет, есть ли указанное разрешение.
        /// </summary>
        /// <param name="permission">Перечисление, представляющее проверяемое разрешение.</param>
        /// <returns>True, если разрешение есть, иначе — false.</returns>
        bool HasPermission(Enum permission);
    }
}
