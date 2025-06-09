using System.Collections.Generic;

namespace TemplateEngine_v3.Models
{
    public class PermissionModel : BaseNotifyPropertyChanged
    {
        private string _permissionPageName = string.Empty;
        public string PermissionPageName
        {
            get => _permissionPageName;
            set => SetValue(ref _permissionPageName, value, nameof(PermissionPageName));
        }

        private List<UserPermission> _permissionList = new();
        public List<UserPermission> PermissionList
        {
            get => _permissionList;
            set => SetValue(ref _permissionList, value, nameof(PermissionList));
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetValue(ref _isSelected, value, nameof(IsSelected));
        }
    }
}
