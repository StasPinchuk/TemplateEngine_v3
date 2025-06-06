using System;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Interfaces;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Перечисление для прав доступа к шаблонам.
    /// </summary>
    [Flags]
    public enum TemplatePermissions
    {
        None = 0,              // Нет прав
        Edit = 1 << 0,         // Право на редактирование
        Delete = 1 << 1,       // Право на удаление
        Create = 1 << 2,       // Право на создание
        Copy = 1 << 3,         // Право на копирование
        All = Edit | Delete | Create | Copy  // Все права
    }

    /// <summary>
    /// Перечисление для прав доступа к филиалам.
    /// </summary>
    [Flags]
    public enum BranchPermissions
    {
        None = 0,              // Нет прав
        Edit = 1 << 0,         // Право на редактирование
        Delete = 1 << 1,       // Право на удаление
        Create = 1 << 2,       // Право на создание
        Copy = 1 << 3,         // Право на копирование
        All = Edit | Delete | Create | Copy  // Все права
    }

    /// <summary>
    /// Перечисление для прав доступа к технологическим процессам.
    /// </summary>
    [Flags]
    public enum TechnologiesPermissions
    {
        None = 0,              // Нет прав
        Edit = 1 << 0,         // Право на редактирование
        Delete = 1 << 1,       // Право на удаление
        Create = 1 << 2,       // Право на создание
        Copy = 1 << 3,         // Право на копирование
        All = Edit | Delete | Create | Copy  // Все права
    }

    /// <summary>
    /// Представляет пользователя с различными правами доступа.
    /// </summary>
    public class UserModel : BaseNotifyPropertyChanged
    {
        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set => SetValue(ref _id, value, nameof(Id));
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetValue(ref _lastName, value, nameof(LastName));
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetValue(ref _firstName, value, nameof(FirstName));
        }

        private string _patronymic = string.Empty;
        public string Patronymic
        {
            get => _patronymic;
            set => SetValue(ref _patronymic, value, nameof(Patronymic));
        }

        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value, nameof(Description));
        }

        private TemplatePermissions _templatePermission = TemplatePermissions.None;
        public TemplatePermissions TemplatePermission
        {
            get => _templatePermission;
            set
            {
                SetValue(ref _templatePermission, value, nameof(TemplatePermission));
            }
        }

        private BranchPermissions _branchPermission = BranchPermissions.None;
        public BranchPermissions BranchPermission
        {
            get => _branchPermission;
            set
            {
                SetValue(ref _branchPermission, value, nameof(BranchPermission));
            }
        }

        private TechnologiesPermissions _technologiesPermission = TechnologiesPermissions.None;
        public TechnologiesPermissions TechnologiesPermission
        {
            get => _technologiesPermission;
            set
            {
                SetValue(ref _technologiesPermission, value, nameof(TechnologiesPermission));
            }
        }

        private bool _isAdmin = false;
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetValue(ref _isAdmin, value, nameof(IsAdmin));
        }

        // Менеджеры прав для проверки прав пользователя
        private IPermissions _templatePermissionsManager;
        private IPermissions _branchPermissionsManager;
        private IPermissions _technologiesPermissionsManager;

        // Конструктор 
        public UserModel()
        {
            _templatePermissionsManager = new TemplatePermissionsManager(TemplatePermission);
            _branchPermissionsManager = new BranchPermissionsManager(BranchPermission);
            _technologiesPermissionsManager = new TechnologiesPermissionsManager(TechnologiesPermission);
        }

        // Проверки прав
        public bool HasTemplatePermission(TemplatePermissions requiredPermission)
        {
            return _templatePermissionsManager.HasPermission(requiredPermission);
        }

        public bool HasBranchPermission(BranchPermissions requiredPermission)
        {
            return _branchPermissionsManager.HasPermission(requiredPermission);
        }

        public bool HasTechnologiesPermission(TechnologiesPermissions requiredPermission)
        {
            return _technologiesPermissionsManager.HasPermission(requiredPermission);
        }
    }


}
