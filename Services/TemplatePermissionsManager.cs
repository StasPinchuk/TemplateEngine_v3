using System;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.TemplateModels;

namespace TemplateEngine_v3.Services
{
    /// <summary>
    /// Менеджер прав доступа для шаблонов.
    /// </summary>
    public class TemplatePermissionsManager : IPermissions
    {
        private readonly TemplatePermissions _permissions;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TemplatePermissionsManager"/>.
        /// </summary>
        /// <param name="permissions">Права доступа к шаблону.</param>
        public TemplatePermissionsManager(TemplatePermissions permissions)
        {
            _permissions = permissions;
        }

        /// <summary>
        /// Проверяет наличие определённого права.
        /// </summary>
        /// <param name="permission">Право доступа, приведённое к Enum.</param>
        /// <returns>true, если право имеется; иначе false.</returns>
        public bool HasPermission(Enum permission)
        {
            return (_permissions & (TemplatePermissions)permission) == (TemplatePermissions)permission;
        }
    }
}
