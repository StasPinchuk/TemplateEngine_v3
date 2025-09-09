using System;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models.TemplateModels;

namespace TemplateEngine_v3.Services
{
    /// <summary>
    /// Менеджер прав доступа для филиалов.
    /// </summary>
    public class BranchPermissionsManager : IPermissions
    {
        private readonly BranchPermissions _permissions;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="BranchPermissionsManager"/>.
        /// </summary>
        /// <param name="permissions">Права доступа к филиалу.</param>
        public BranchPermissionsManager(BranchPermissions permissions)
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
            return (_permissions & (BranchPermissions)permission) == (BranchPermissions)permission;
        }
    }
}
