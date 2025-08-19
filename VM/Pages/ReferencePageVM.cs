using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Services.UsersServices;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// Типы классов шаблонов для отображения.
    /// </summary>
    public enum TemplateClass
    {
        /// <summary>Готовые шаблоны</summary>
        Ready,
        /// <summary>Черновики</summary>
        Draft,
        /// <summary>Корзина (удалённые)</summary>
        TrashCan
    }

    /// <summary>
    /// ViewModel для страницы со списком ссылок на шаблоны, филиалы или технологии.
    /// Обеспечивает команды для создания, удаления, клонирования и редактирования элементов.
    /// </summary>
    public class ReferencePageVM : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Список ссылок на модели (шаблоны, филиалы, технологии).
        /// </summary>
        public ObservableCollection<ReferenceModelInfo> ReferencesList { get; set; } = new();

        private readonly TemplateManager _templateManager;
        private readonly BranchManager _branchManager;
        private readonly TechnologiesManager _technologiesManager;
        private readonly TemplateClass _templateClass;
        private readonly ColumnDefinition _sideBar;
        private readonly Action _setListAction;

        private object _permission = null;

        /// <summary>
        /// Разрешения пользователя для текущей страницы.
        /// </summary>
        public object Permission
        {
            get => _permission;
            set => SetValue(ref _permission, value, nameof(Permission));
        }

        private bool _canCreate = false;

        public bool CanCreate
        {
            get => _canCreate;
            set => SetValue(ref _canCreate, value, nameof(CanCreate));
        }

        private TemplateStageService _stageService;
        public TemplateStageService StageService
        {
            get => _stageService;
            set => SetValue(ref _stageService, value, nameof(StageService));
        }

        /// <summary>
        /// Команда удаления элемента.
        /// </summary>
        public ICommand RemoveCommand { get; set; }

        /// <summary>
        /// Команда клонирования элемента.
        /// </summary>
        public ICommand CloneCommand { get; set; }

        /// <summary>
        /// Команда редактирования элемента.
        /// </summary>
        public ICommand EditCommand { get; set; }

        /// <summary>
        /// Команда создания нового элемента.
        /// </summary>
        public ICommand CreateCommand { get; set; }

        /// <summary>
        /// Конструктор для страницы с шаблонами (без технологий).
        /// </summary>
        public ReferencePageVM(TemplateManager templateManager, BranchManager branchManager, UserManager userManager, TemplateClass templateClass, TemplateStageService stageService, ColumnDefinition sideBar)
        {
            _templateManager = templateManager;
            _templateClass = templateClass;
            _branchManager = branchManager;
            StageService = stageService;
            _sideBar = sideBar;

            Permission = userManager.CurrentUser.TemplatePermission;

            if (Permission is TemplatePermissions templatePermission)
                CanCreate = (templatePermission.HasFlag(TemplatePermissions.All) || templatePermission.HasFlag(TemplatePermissions.Create));

            SetReferenceList();

            _setListAction += SetReferenceList;

            InitializeTemplateCommand();
        }

        /// <summary>
        /// Конструктор для страницы с шаблонами и технологиями.
        /// </summary>
        public ReferencePageVM(TemplateManager templateManager, TechnologiesManager technologiesManager, BranchManager branchManager, UserManager userManager, TemplateClass templateClass, TemplateStageService stageService, ColumnDefinition sideBar)
        {
            _templateManager = templateManager;
            _technologiesManager = technologiesManager;
            _templateClass = templateClass;
            _branchManager = branchManager;
            StageService = stageService;
            _sideBar = sideBar;

            Permission = userManager.CurrentUser.TemplatePermission;

            if (Permission is TemplatePermissions templatePermission)
                CanCreate = (templatePermission.HasFlag(TemplatePermissions.All) || templatePermission.HasFlag(TemplatePermissions.Create));

            SetReferenceList();
            _setListAction += SetReferenceList;
            InitializeTemplateCommand();
        }

        /// <summary>
        /// Инициализация команд для работы с шаблонами.
        /// </summary>
        private void InitializeTemplateCommand()
        {
            RemoveCommand = new RelayCommand(RemoveTemplate);
            CloneCommand = new RelayCommand(CloneTemplate);
            EditCommand = new RelayCommand(EditTemplate);
            CreateCommand = new RelayCommand(CreateTemplate);
        }

        /// <summary>
        /// Конструктор для страницы с филиалами.
        /// </summary>
        public ReferencePageVM(BranchManager branchManager, UserManager userManager, TemplateStageService stageService, ColumnDefinition sideBar)
        {
            _branchManager = branchManager;
            StageService = stageService;
            _sideBar = sideBar;
            InitializeBranchCommand();

            Permission = userManager.CurrentUser.BranchPermission;
            if (Permission is BranchPermissions branchPermissions)
                CanCreate = (branchPermissions == BranchPermissions.All || branchPermissions.HasFlag(BranchPermissions.Create));

            SetBranchesList();
            _setListAction += SetBranchesList;
        }

        /// <summary>
        /// Инициализация команд для работы с филиалами.
        /// </summary>
        private void InitializeBranchCommand()
        {
            RemoveCommand = new RelayCommand(RemoveBranch);
            CloneCommand = new RelayCommand(CloneBranch);
            EditCommand = new RelayCommand(EditBranch);
            CreateCommand = new RelayCommand(CreateBranch);
        }

        /// <summary>
        /// Конструктор для страницы с технологиями.
        /// </summary>
        public ReferencePageVM(TechnologiesManager technologiesManager, UserManager userManager, TemplateStageService stageService, ColumnDefinition sideBar)
        {
            _technologiesManager = technologiesManager;
            StageService = stageService;
            _sideBar = sideBar;
            InitializeTechnologiesCommand();

            Permission = userManager.CurrentUser.TechnologiesPermission;

            if (Permission is TechnologiesPermissions technologiesPermissions)
                CanCreate = (technologiesPermissions == TechnologiesPermissions.All || technologiesPermissions.HasFlag(TechnologiesPermissions.Create));

            SetTechnologiesList();
            _setListAction += SetTechnologiesList;
        }

        /// <summary>
        /// Инициализация команд для работы с технологиями.
        /// </summary>
        private void InitializeTechnologiesCommand()
        {
            RemoveCommand = new RelayCommand(RemoveTechnologies);
            CloneCommand = new RelayCommand(CloneTechnologies);
            EditCommand = new RelayCommand(EditTechnologies);
            CreateCommand = new RelayCommand(CreateTechnologies);
        }

        /// <summary>
        /// Устанавливает список ссылок в зависимости от класса шаблона.
        /// </summary>
        private void SetReferenceList()
        {
            switch (_templateClass)
            {
                case TemplateClass.Ready:
                    SetReadyTemplateList();
                    break;
                case TemplateClass.TrashCan:
                    SetTrashCanTemplateList();
                    break;
            }
        }

        private readonly object _lockObject = new object();

        /// <summary>
        /// Загружает список готовых шаблонов.
        /// </summary>
        private void SetReadyTemplateList()
        {
            ReferencesList = _templateManager.GetReadyTemplate();
            OnPropertyChanged(nameof(ReferencesList));
        }

        /// <summary>
        /// Загружает список шаблонов из корзины.
        /// </summary>
        private void SetTrashCanTemplateList()
        {
            ReferencesList = _templateManager.GetTrashCanTemplates();
            OnPropertyChanged(nameof(ReferencesList));
        }

        /// <summary>
        /// Загружает список всех филиалов.
        /// </summary>
        private void SetBranchesList()
        {
            ReferencesList = _branchManager.GetAllBranches();
            OnPropertyChanged(nameof(ReferencesList));
        }

        /// <summary>
        /// Загружает список всех технологий.
        /// </summary>
        private void SetTechnologiesList()
        {
            ReferencesList = _technologiesManager.GetAllTechnologies();
            OnPropertyChanged(nameof(ReferencesList));
        }

        /// <summary>
        /// Удаляет шаблон после подтверждения.
        /// </summary>
        /// <param name="parameter">Ссылка на шаблон для удаления.</param>
        private async void RemoveTemplate(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                var result = System.Windows.MessageBox.Show($"Вы действительно хотите удалить шаблон \"{referenceModel.Name}\"?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool isRemove = await _templateManager.RemoveTemplateAsync(referenceModel, StageService);
                    if (isRemove)
                    {
                        _templateManager.ClearTemplate();
                        SetReferenceList();
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет филиал после подтверждения.
        /// </summary>
        /// <param name="parameter">Ссылка на филиал для удаления.</param>
        private async void RemoveBranch(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                var result = System.Windows.MessageBox.Show($"Вы действительно хотите удалить ветку \"{referenceModel.Name}\"?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool isRemove = await _branchManager.RemoveBranch(referenceModel);
                    if (isRemove)
                    {
                        SetBranchesList();
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет технологию после подтверждения.
        /// </summary>
        /// <param name="parameter">Ссылка на технологию для удаления.</param>
        private async void RemoveTechnologies(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                var result = System.Windows.MessageBox.Show($"Вы действительно хотите удалить технологию \"{referenceModel.Name}\"?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool isRemove = await _technologiesManager.RemoveTechnologies(referenceModel);
                    if (isRemove)
                    {
                        SetTechnologiesList();
                    }
                }
            }
        }

        /// <summary>
        /// Клонирует шаблон.
        /// </summary>
        /// <param name="parameter">Ссылка на шаблон для клонирования.</param>
        private async void CloneTemplate(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                ReferencesList.Remove(referenceModel);
                bool isClone = await _templateManager.CopyTemplateAsync(referenceModel);
                if (isClone)
                {
                    SetReferenceList();
                }
            }
        }

        /// <summary>
        /// Клонирует филиал.
        /// </summary>
        /// <param name="parameter">Ссылка на филиал для клонирования.</param>
        private async void CloneBranch(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                ReferencesList.Remove(referenceModel);
                bool isClone = await _branchManager.CloneBranch(referenceModel);
                if (isClone)
                {
                    SetBranchesList();
                }
            }
        }

        /// <summary>
        /// Клонирует технологию.
        /// </summary>
        /// <param name="parameter">Ссылка на технологию для клонирования.</param>
        private async void CloneTechnologies(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                ReferencesList.Remove(referenceModel);
                bool isClone = await _technologiesManager.CloneTechnologies(referenceModel);
                if (isClone)
                {
                    SetTechnologiesList();
                }
            }
        }

        /// <summary>
        /// Редактирует шаблон. Если шаблон в корзине, восстанавливает его.
        /// </summary>
        /// <param name="parameter">Ссылка на шаблон для редактирования.</param>
        private async void EditTemplate(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                if (referenceModel.Type.Name.Equals("Корзина"))
                {
                    var isRestory = await _templateManager.RestoreTemplateAsync(referenceModel);
                    string msg = isRestory ? "Шаблон успешно восстановлен" : "Шаблон не восстановлен";

                    System.Windows.MessageBox.Show(msg, "Восстановление шаблона");

                    return;
                }

                LogManager.CreateLogObjectGroup(referenceModel.Name, "Шаблоны");

                var templateManager = _templateManager.Clone();

                var isSetTemplate = await templateManager.SetTemplateAsync(referenceModel);

                if (!isSetTemplate)
                {
                    return;
                }

                NavigationService.RenameSelectedTab(referenceModel.Name);

                var templateEditPage = new PageModel(referenceModel.Name, typeof(TemplateEditPage), new object[] { templateManager, _technologiesManager, _branchManager, StageService });

                NavigationService.SetPageInMainFrame(templateEditPage);

                _sideBar.Width = new GridLength(80);
            }
        }

        /// <summary>
        /// Редактирует технологию.
        /// </summary>
        /// <param name="parameter">Ссылка на технологию для редактирования.</param>
        private void EditTechnologies(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                _technologiesManager.CurrentTechnologies = new JsonSerializer().Deserialize<Technologies>(referenceModel.ObjectStruct);

                var technologiesManager = _technologiesManager.DeepCopy();

                NavigationService.RenameSelectedTab(referenceModel.Name);

                var technologiesEditPage = new PageModel(referenceModel.Name, typeof(TechnologiesPage), new object[] { technologiesManager });

                NavigationService.SetPageInMainFrame(technologiesEditPage);

                _sideBar.Width = new GridLength(80);
            }
        }

        /// <summary>
        /// Редактирует филиал.
        /// </summary>
        /// <param name="parameter">Ссылка на филиал для редактирования.</param>
        private void EditBranch(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                Branch branch = new JsonSerializer().Deserialize<Branch>(referenceModel.ObjectStruct);

                var branchManager = _branchManager.DeepCopy();

                NavigationService.RenameSelectedTab(referenceModel.Name);

                var branchEditPage = new PageModel(referenceModel.Name, typeof(BranchMainPage), new object[] { branchManager, branch });

                NavigationService.SetPageInMainFrame(branchEditPage);

                _sideBar.Width = new GridLength(80);
            }
        }

        /// <summary>
        /// Создаёт новый шаблон и открывает страницу редактирования.
        /// </summary>
        /// <param name="parameter">Параметры команды (не используются).</param>
        private async void CreateTemplate(object parameter)
        {
            LogManager.CreateLogObjectGroup("Новый шаблон", "Шаблоны");
            var template = new Template() { Name = "Новый шаблон" };

            var templateManager = _templateManager.Clone();

            await templateManager.SetTemplateAsync(template);

            NavigationService.RenameSelectedTab(template.Name);

            var templateEditPage = new PageModel(template.Name, typeof(TemplateEditPage), new object[] { templateManager, _technologiesManager, _branchManager, StageService });

            NavigationService.SetPageInMainFrame(templateEditPage);

            _sideBar.Width = new GridLength(80);
        }

        /// <summary>
        /// Создаёт новую технологию и открывает страницу редактирования.
        /// </summary>
        /// <param name="parameter">Параметры команды (не используются).</param>
        private void CreateTechnologies(object parameter)
        {
            _technologiesManager.CurrentTechnologies = new Technologies() { Name = "Новое ТП" };

            var technologiesManager = _technologiesManager.DeepCopy();

            NavigationService.RenameSelectedTab(_technologiesManager.CurrentTechnologies.Name);

            var technologiesEditPage = new PageModel(technologiesManager.CurrentTechnologies.Name, typeof(TechnologiesPage), new object[] { technologiesManager });

            NavigationService.SetPageInMainFrame(technologiesEditPage);

            _sideBar.Width = new GridLength(80);
        }

        /// <summary>
        /// Создаёт новый филиал и открывает страницу редактирования.
        /// </summary>
        /// <param name="parameter">Параметры команды (не используются).</param>
        private void CreateBranch(object parameter)
        {
            var newBranch = new Branch()
            {
                Name = "Новый филиал"
            };

            var branchManager = _branchManager.DeepCopy();

            NavigationService.RenameSelectedTab(newBranch.Name);

            var branchCreatePage = new PageModel(newBranch.Name, typeof(BranchMainPage), new object[] { branchManager, newBranch });

            NavigationService.SetPageInMainFrame(branchCreatePage);

            _sideBar.Width = new GridLength(80);
        }

        public void SearchReferenceModel(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                _setListAction?.Invoke();
            else
            {
                _setListAction?.Invoke();
                ReferencesList = new(ReferencesList.Where(reference => reference.Name.StartsWith(searchString,StringComparison.OrdinalIgnoreCase)));
                OnPropertyChanged(nameof(ReferencesList));
            }
        }
    }
}
