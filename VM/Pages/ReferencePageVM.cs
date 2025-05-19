using System.Collections.ObjectModel;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    public enum TemplateClass
    {
        Ready,
        Draft,
        TrashCan
    }

    public class ReferencePageVM
    {
        public ObservableCollection<ReferenceModelInfo> ReferencesList { get; set; } = new();
        private readonly ITemplateManager _templateManager;
        private readonly IBranchManager _branchManager;
        private readonly ITechnologiesManager _technologiesManager;
        private readonly TemplateClass _templateClass;
        private readonly ColumnDefinition _sideBar;

        public ICommand RemoveCommand { get; set; }
        public ICommand CloneCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand CreateCommand { get; set; }

        public ReferencePageVM(ITemplateManager templateManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            _templateManager = templateManager;
            _templateClass = templateClass;
            _sideBar = sideBar;
            SetReferenceList();
            InitializeTemplateCommand();
        }

        public ReferencePageVM(ITemplateManager templateManager, ITechnologiesManager technologiesManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            _templateManager = templateManager;
            _technologiesManager = technologiesManager;
            _templateClass = templateClass;
            _sideBar = sideBar;
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

        public ReferencePageVM(IBranchManager branchManager, ColumnDefinition sideBar)
        {
            _branchManager = branchManager;
            _sideBar = sideBar;
            InitializeBranchCommand();

            SetBranchesList();
        }

        private void InitializeBranchCommand()
        {
            RemoveCommand = new RelayCommand(RemoveBranch);
            CloneCommand = new RelayCommand(CloneBranch);
            EditCommand = new RelayCommand(CloneBranch);
        }

        public ReferencePageVM(ITechnologiesManager technologiesManager, ColumnDefinition sideBar)
        {
            _technologiesManager = technologiesManager;
            _sideBar = sideBar;
            InitializeTechnologiesCommand();

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
            var referencesList = new ObservableCollection<ReferenceModelInfo>(_templateManager.GetReadyTemplate());
            ReferencesList.Clear();
            foreach(ReferenceModelInfo reference in referencesList)
                ReferencesList.Add(reference);
            referencesList = null;
        }

        private void SetDraftTemplateList()
        {
            var referencesList = new ObservableCollection<ReferenceModelInfo>(_templateManager.GetDraftTemplates());
            ReferencesList.Clear();
            foreach (var template in referencesList)
            {
                ReferencesList.Add(template);
            }
        }

        private void SetTrashCanTemplateList()
        {
            var referencesList = new ObservableCollection<ReferenceModelInfo>(_templateManager.GetTrashCanTemplates());
            ReferencesList.Clear();
            foreach (var template in referencesList)
            {
                ReferencesList.Add(template);
            }
        }

        private void SetBranchesList()
        {
            var referencesList = new ObservableCollection<ReferenceModelInfo>(_branchManager.GetAllBranches());
            ReferencesList.Clear();
            foreach (var branch in referencesList)
            {
                ReferencesList.Add(branch);
            }
        }

        private void SetTechnologiesList()
        {
            var referencesList = new ObservableCollection<ReferenceModelInfo>(_technologiesManager.GetAllTechnologies());
            ReferencesList.Clear();
            foreach (var branch in referencesList)
            {
                ReferencesList.Add(branch);
            }
        }

        private async void RemoveTemplate(object parameter)
        {
            if(parameter is ReferenceModelInfo referenceModel)
            {
                bool isRemove = await _templateManager.RemoveTemplateAsync(referenceModel);
                if (isRemove)
                {
                    SetReferenceList();
                }
            }
        }

        private async void RemoveBranch(object parameter)
        {
            if(parameter is ReferenceModelInfo referenceModel)
            {
                bool isRemove = await _branchManager.RemoveBranch(referenceModel);
                if (isRemove)
                {
                    SetBranchesList();
                }
            }
        }

        private async void RemoveTechnologies(object parameter)
        {
            if(parameter is ReferenceModelInfo referenceModel)
            {
                bool isRemove = await _technologiesManager.RemoveTechnologies(referenceModel);
                if (isRemove)
                {
                    SetTechnologiesList();
                }
            }
        }

        private async void CloneTemplate(object parameter)
        {
            if(parameter is ReferenceModelInfo referenceModel)
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
            if(parameter is ReferenceModelInfo referenceModel)
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
            if(parameter is ReferenceModelInfo referenceModel)
            {
                ReferencesList.Remove(referenceModel);
                bool isClone = await _technologiesManager.CloneTechnologies(referenceModel);
                if (isClone)
                {
                    SetTechnologiesList();
                }
            }
        }

        private void EditTemplate(object parameter)
        {
            if(parameter is ReferenceModelInfo referenceModel)
            {
                _templateManager.SetTemplate(referenceModel);

                var templateEditPage = new PageModel(referenceModel.Name, typeof(TemplateEditPage), new object[]{ _technologiesManager, _templateManager});

                MenuHistory.NextPage(templateEditPage, true);
                _sideBar.Width = new GridLength(80);
            }
        }

        private void EditTechnologies(object parameter)
        {
            if(parameter is ReferenceModelInfo referenceModel)
            {
                _technologiesManager.CurrentTechnologies = new JsonSerializer().Deserialize<Technologies>(referenceModel.ObjectStruct);

                var technologiesEditPage = new PageModel(referenceModel.Name, typeof(TechnologiesPage), new object[]{ _technologiesManager});

                MenuHistory.NextPage(technologiesEditPage, true);
                _sideBar.Width = new GridLength(80);
            }
        }

        private void CreateTemplate(object parameter)
        {
            var template = new Template() { Name = "Новый шаблон" };

            _templateManager.SetTemplate(template);

            var templateEditPage = new PageModel(template.Name, typeof(TemplateEditPage), new object[] { _technologiesManager, _templateManager });

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
    }
}
