using System;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Interfaces;

namespace TemplateEngine_v3.Models
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

    /// <summary>
    /// Перечисление прав доступа к технологическим процессам.
    /// </summary>
    [Flags]
    public enum TechnologiesPermissions
    {
        /// <summary>
        /// Нет прав доступа.
        /// </summary>
        None = 0,

        /// <summary>
        /// Право на редактирование технологических процессов.
        /// </summary>
        Edit = 1 << 0,

        /// <summary>
        /// Право на удаление технологических процессов.
        /// </summary>
        Delete = 1 << 1,

        /// <summary>
        /// Право на создание технологических процессов.
        /// </summary>
        Create = 1 << 2,

        /// <summary>
        /// Право на копирование технологических процессов.
        /// </summary>
        Copy = 1 << 3,

        /// <summary>
        /// Полный доступ ко всем операциям с технологическими процессами.
        /// </summary>
        All = Edit | Delete | Create | Copy
    }

    /// <summary>
    /// Представляет пользователя с правами доступа к различным модулям.
    /// </summary>
    public class UserModel : BaseNotifyPropertyChanged
    {
        private string _id = string.Empty;

        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        public string Id
        {
            get => _id;
            set => SetValue(ref _id, value, nameof(Id));
        }

        private string _lastName = string.Empty;

        /// <summary>
        /// Фамилия пользователя.
        /// </summary>
        public string LastName
        {
            get => _lastName;
            set => SetValue(ref _lastName, value, nameof(LastName));
        }

        private string _firstName = string.Empty;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string FirstName
        {
            get => _firstName;
            set => SetValue(ref _firstName, value, nameof(FirstName));
        }

        private string _patronymic = string.Empty;

        /// <summary>
        /// Отчество пользователя.
        /// </summary>
        public string Patronymic
        {
            get => _patronymic;
            set => SetValue(ref _patronymic, value, nameof(Patronymic));
        }

        /// <summary>
        /// Полное имя пользователя (ФИО).
        /// </summary>
        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();

        private string _description = string.Empty;

        /// <summary>
        /// Описание или должность пользователя.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value, nameof(Description));
        }

        private TemplatePermissions _templatePermission = TemplatePermissions.None;

        /// <summary>
        /// Права пользователя на работу с шаблонами.
        /// </summary>
        public TemplatePermissions TemplatePermission
        {
            get => _templatePermission;
            set => SetValue(ref _templatePermission, value, nameof(TemplatePermission));
        }

        private BranchPermissions _branchPermission = BranchPermissions.None;

        /// <summary>
        /// Права пользователя на работу с филиалами.
        /// </summary>
        public BranchPermissions BranchPermission
        {
            get => _branchPermission;
            set => SetValue(ref _branchPermission, value, nameof(BranchPermission));
        }

        private TechnologiesPermissions _technologiesPermission = TechnologiesPermissions.None;

        /// <summary>
        /// Права пользователя на работу с технологическими процессами.
        /// </summary>
        public TechnologiesPermissions TechnologiesPermission
        {
            get => _technologiesPermission;
            set => SetValue(ref _technologiesPermission, value, nameof(TechnologiesPermission));
        }

        private bool _isAdmin = false;

        /// <summary>
        /// Признак, является ли пользователь администратором.
        /// </summary>
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetValue(ref _isAdmin, value, nameof(IsAdmin));
        }

        // Приватные менеджеры прав
        private IPermissions _templatePermissionsManager;
        private IPermissions _branchPermissionsManager;
        private IPermissions _technologiesPermissionsManager;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UserModel"/>.
        /// </summary>
        public UserModel()
        {
            _templatePermissionsManager = new TemplatePermissionsManager(TemplatePermission);
            _branchPermissionsManager = new BranchPermissionsManager(BranchPermission);
            _technologiesPermissionsManager = new TechnologiesPermissionsManager(TechnologiesPermission);
        }

        /// <summary>
        /// Проверяет наличие указанного права на шаблоны.
        /// </summary>
        /// <param name="requiredPermission">Необходимое право.</param>
        /// <returns>True, если право есть, иначе False.</returns>
        public bool HasTemplatePermission(TemplatePermissions requiredPermission)
        {
            return _templatePermissionsManager.HasPermission(requiredPermission);
        }

        /// <summary>
        /// Проверяет наличие указанного права на филиалы.
        /// </summary>
        /// <param name="requiredPermission">Необходимое право.</param>
        /// <returns>True, если право есть, иначе False.</returns>
        public bool HasBranchPermission(BranchPermissions requiredPermission)
        {
            return _branchPermissionsManager.HasPermission(requiredPermission);
        }

        /// <summary>
        /// Проверяет наличие указанного права на технологические процессы.
        /// </summary>
        /// <param name="requiredPermission">Необходимое право.</param>
        /// <returns>True, если право есть, иначе False.</returns>
        public bool HasTechnologiesPermission(TechnologiesPermissions requiredPermission)
        {
            return _technologiesPermissionsManager.HasPermission(requiredPermission);
        }
    }
}
