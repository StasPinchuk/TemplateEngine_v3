using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.UsersServices;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    public enum TemplateClass
    {
        Ready,
        Draft,
        TrashCan
    }

    public class ReferencePageVM : BaseNotifyPropertyChanged
    {
        public ObservableCollection<ReferenceModelInfo> ReferencesList { get; set; } = new();
        private readonly ITemplateManager _templateManager;
        private readonly IBranchManager _branchManager;
        private readonly ITechnologiesManager _technologiesManager;
        private readonly TemplateClass _templateClass;
        private readonly ColumnDefinition _sideBar;

        private object _permission = null;
        public object Permission
        {
            get => _permission;
            set => SetValue(ref _permission, value, nameof(Permission));
        }

        public ICommand RemoveCommand { get; set; }
        public ICommand CloneCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand CreateCommand { get; set; }

        public ReferencePageVM(ITemplateManager templateManager, IBranchManager branchManager, UserManager userManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            _templateManager = templateManager;
            _templateClass = templateClass;
            _branchManager = branchManager;
            _sideBar = sideBar;

            Permission = userManager.CurrentUser.TemplatePermission;

            SetReferenceList();
            InitializeTemplateCommand();
        }

        public ReferencePageVM(ITemplateManager templateManager, ITechnologiesManager technologiesManager, IBranchManager branchManager, UserManager userManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            _templateManager = templateManager;
            _technologiesManager = technologiesManager;
            _templateClass = templateClass;
            _branchManager = branchManager;
            _sideBar = sideBar;

            Permission = userManager.CurrentUser.TemplatePermission;

            SetReferenceList();
            InitializeTemplateCommand();
        }

        private void InitializeTemplateCommand()
        {
            RemoveCommand = new RelayCommand(RemoveTemplate);
            CloneCommand = new RelayCommand(CloneTemplate);
            EditCommand = new RelayCommand(EditTemplate);
            CreateCommand = new RelayCommand(CreateTemplate);
        }

        public ReferencePageVM(IBranchManager branchManager, UserManager userManager, ColumnDefinition sideBar)
        {
            _branchManager = branchManager;
            _sideBar = sideBar;
            InitializeBranchCommand();

            Permission = userManager.CurrentUser.BranchPermission;

            SetBranchesList();
        }

        private void InitializeBranchCommand()
        {
            RemoveCommand = new RelayCommand(RemoveBranch);
            CloneCommand = new RelayCommand(CloneBranch);
            EditCommand = new RelayCommand(EditBranch);
            CreateCommand = new RelayCommand(CreateBranch);
        }

        public ReferencePageVM(ITechnologiesManager technologiesManager, UserManager userManager, ColumnDefinition sideBar)
        {
            _technologiesManager = technologiesManager;
            _sideBar = sideBar;
            InitializeTechnologiesCommand();

            Permission = userManager.CurrentUser.TechnologiesPermission;

            SetTechnologiesList();
        }

        private void InitializeTechnologiesCommand()
        {
            RemoveCommand = new RelayCommand(RemoveTechnologies);
            CloneCommand = new RelayCommand(CloneTechnologies);
            EditCommand = new RelayCommand(EditTechnologies);
            CreateCommand = new RelayCommand(CreateTechnologies);
        }

        private void SetReferenceList()
        {
            switch (_templateClass)
            {
                case TemplateClass.Ready:
                    SetReadyTemplateList();
                    break;
                case TemplateClass.Draft:
                    SetDraftTemplateList();
                    break;
                case TemplateClass.TrashCan:
                    SetTrashCanTemplateList();
                    break;
            }
        }

        private readonly object _lockObject = new object();

        private void SetReadyTemplateList()
        {
            ReferencesList = _templateManager.GetReadyTemplate();
            OnPropertyChanged(nameof(ReferencesList));
        }

        private void SetDraftTemplateList()
        {
            ReferencesList = _templateManager.GetDraftTemplates();

            OnPropertyChanged(nameof(ReferencesList));
        }

        private void SetTrashCanTemplateList()
        {
            ReferencesList = _templateManager.GetTrashCanTemplates();

            OnPropertyChanged(nameof(ReferencesList));
        }

        private void SetBranchesList()
        {
            ReferencesList = _branchManager.GetAllBranches();

            OnPropertyChanged(nameof(ReferencesList));
        }

        private void SetTechnologiesList()
        {
            ReferencesList = _technologiesManager.GetAllTechnologies();

            OnPropertyChanged(nameof(ReferencesList));
        }

        private async void RemoveTemplate(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                var result = MessageBox.Show($"Вы действительно хотите удалить шаблон \"{referenceModel.Name}\"?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool isRemove = await _templateManager.RemoveTemplateAsync(referenceModel);
                    if (isRemove)
                    {
                        SetReferenceList();
                    }
                }
            }
        }

        private async void RemoveBranch(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                var result = MessageBox.Show($"Вы действительно хотите удалить ветку \"{referenceModel.Name}\"?",
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

        private async void RemoveTechnologies(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                var result = MessageBox.Show($"Вы действительно хотите удалить технологию \"{referenceModel.Name}\"?",
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

        private async void EditTemplate(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                if (referenceModel.Type.Name.Equals("Корзина"))
                {
                    var isRestory = await _templateManager.RestoreTemplateAsync(referenceModel);
                    string msg = isRestory ? "Шаблон успешно восстановлен" : "Шаблон не восстановлен";

                    MessageBox.Show(msg, "Восстановление шаблона");

                    return;
                }

                var isSetTemplate = await _templateManager.SetTemplateAsync(referenceModel);
                if (!isSetTemplate)
                {
                    return;
                }

                var templateEditPage = new PageModel(referenceModel.Name, typeof(TemplateEditPage), new object[] { _technologiesManager, _templateManager, _branchManager });

                MenuHistory.NextPage(templateEditPage, true);
                _sideBar.Width = new GridLength(80);
            }
        }

        private void EditTechnologies(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                _technologiesManager.CurrentTechnologies = new JsonSerializer().Deserialize<Technologies>(referenceModel.ObjectStruct);

                var technologiesEditPage = new PageModel(referenceModel.Name, typeof(TechnologiesPage), new object[] { _technologiesManager });

                MenuHistory.NextPage(technologiesEditPage, true);
                _sideBar.Width = new GridLength(80);
            }
        }

        private void EditBranch(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                Branch branch = new JsonSerializer().Deserialize<Branch>(referenceModel.ObjectStruct);

                var branchEditPage = new PageModel(referenceModel.Name, typeof(BranchMainPage), new object[] { _branchManager, branch });

                MenuHistory.NextPage(branchEditPage, true);
                _sideBar.Width = new GridLength(80);
            }
        }

        private void CreateTemplate(object parameter)
        {
            var template = new Template() { Name = "Новый шаблон" };

            _templateManager.SetTemplateAsync(template);

            var templateEditPage = new PageModel(template.Name, typeof(TemplateEditPage), new object[] { _technologiesManager, _templateManager, _branchManager });

            MenuHistory.NextPage(templateEditPage, true);
            _sideBar.Width = new GridLength(80);
        }

        private void CreateTechnologies(object parameter)
        {
            _technologiesManager.CurrentTechnologies = new Technologies() { Name = "Новое ТП" };

            var technologiesEditPage = new PageModel(_technologiesManager.CurrentTechnologies.Name, typeof(TechnologiesPage), new object[] { _technologiesManager });

            MenuHistory.NextPage(technologiesEditPage, true);
            _sideBar.Width = new GridLength(80);
        }

        private void CreateBranch(object parameter)
        {
            var newBranch = new Branch()
            {
                Name = "Новый филиал"
            };

            var branchCreatePage = new PageModel(newBranch.Name, typeof(BranchMainPage), new object[] { _branchManager, newBranch });

            MenuHistory.NextPage(branchCreatePage, true);
            _sideBar.Width = new GridLength(80);
        }
    }
}
