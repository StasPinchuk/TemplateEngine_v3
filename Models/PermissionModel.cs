using System.Collections.Generic;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет модель прав доступа к определённой странице.
    /// </summary>
    public class PermissionModel : BaseNotifyPropertyChanged
    {
        private string _permissionPageName = string.Empty;

        /// <summary>
        /// Название страницы, для которой назначены права.
        /// </summary>
        public string PermissionPageName
        {
            get => _permissionPageName;
            set => SetValue(ref _permissionPageName, value, nameof(PermissionPageName));
        }

        private List<UserPermission> _permissionList = new();

        /// <summary>
        /// Список прав пользователей, связанных с этой страницей.
        /// </summary>
        public List<UserPermission> PermissionList
        {
            get => _permissionList;
            set => SetValue(ref _permissionList, value, nameof(PermissionList));
        }

        private bool _isSelected = false;

        /// <summary>
        /// Флаг, указывающий, выбрана ли данная модель (например, для отображения или редактирования).
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetValue(ref _isSelected, value, nameof(IsSelected));
        }
    }
}
