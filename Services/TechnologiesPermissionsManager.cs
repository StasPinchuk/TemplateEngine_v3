using System;
using TemplateEngine_v3.Interfaces;

using TemplateEngine_v3.Models.TemplateModels;
namespace TemplateEngine_v3.Services
{
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
