using System;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.Helpers
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

    /// <summary>
    /// Менеджер прав доступа для технологических процессов.
    /// </summary>
    public class TechnologiesPermissionsManager : IPermissions
    {
        private readonly TechnologiesPermissions _permissions;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TechnologiesPermissionsManager"/>.
        /// </summary>
        /// <param name="permissions">Права доступа к технологиям.</param>
        public TechnologiesPermissionsManager(TechnologiesPermissions permissions)
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
            return (_permissions & (TechnologiesPermissions)permission) == (TechnologiesPermissions)permission;
        }
    }
}
