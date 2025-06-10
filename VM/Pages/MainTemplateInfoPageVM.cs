using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    public class MainTemplateInfoPageVM : BaseNotifyPropertyChanged
    {
        private readonly ITechnologiesManager _technologiesManager;
        private readonly INodeManager _nodeManager;
        private readonly ITemplateManager _templateManager;
        private Template _currentTemplate;
        public Template CurrentTemplate
        {
            get => _currentTemplate;
            set
            {
                SetValue(ref _currentTemplate, value, nameof(CurrentTemplate));
                if (_currentTemplate != null)
                    CurrentRelation = _currentTemplate.TemplateRelations.FirstOrDefault();
            }
        }

        private TemplateRelations _currentRelation;
        public TemplateRelations CurrentRelation
        {
            get => _currentRelation;
            set => SetValue(ref _currentRelation, value, nameof(CurrentRelation));
        }

        public ContextMenu TextBoxMenu => _nodeManager.MenuHelper.GetContextMenu() ?? new ContextMenu();

        public ObservableCollection<PageModel> PagesCollection { get; private set; } = new();

        public ICommand NextPageCommand { get; private set; }
        public ICommand SetRelationsCommand { get; private set; }
        public ICommand AddDesignationCommand { get; private set; }
        public ICommand RemoveDesignationCommand { get; private set; }
        public ICommand AddExampleMarkingsCommand { get; private set; }
        public ICommand RemoveExampleMarkingsCommand { get; private set; }

        public MainTemplateInfoPageVM(ITechnologiesManager technologiesManager, ITemplateManager templateManager)
        {
            _technologiesManager = technologiesManager;
            _templateManager = templateManager;
            _nodeManager = new NodeManager()
            {
                MenuHelper = _templateManager.MenuHelper,
                TableManager = templateManager.TableService
            };
            CurrentTemplate = _templateManager.GetSelectedTemplate();

            technologiesManager.CurrentTechnologies = CurrentRelation?.Technologies ?? null;
            technologiesManager.MenuHelper = _templateManager.MenuHelper;

            var pageCollection = new ObservableCollection<PageModel>()
            {
                new PageModel("Процесс изготовления", typeof(TechnologiesPage), new object[] { technologiesManager }),
                new PageModel("Состав изделия", typeof(PartsListPage), new object[] { _technologiesManager, _nodeManager, _templateManager }),
            };

            foreach (PageModel page in pageCollection)
            {
                PagesCollection.Add(page);
            }

            InitializeCommand();
        }

        private void InitializeCommand()
        {
            NextPageCommand = new RelayCommand(NextPage, CanNextPage);
            SetRelationsCommand = new RelayCommand(SetRelations);
            AddDesignationCommand = new RelayCommand(AddDesignation);
            RemoveDesignationCommand = new RelayCommand(RemoveDesignationAsync);
            AddExampleMarkingsCommand = new RelayCommand(AddExampleMarkingsAsync);
            RemoveExampleMarkingsCommand = new RelayCommand(RemoveExampleMarkingsAsync);
        }

        private bool CanNextPage(object parameters)
        {
            return CurrentRelation != null;
        }

        private void NextPage(object parameters)
        {
            if (parameters is PageModel nextPage)
            {
                _technologiesManager.CurrentTechnologies = CurrentRelation.Technologies;
                _nodeManager.Nodes = CurrentRelation.Nodes;
                IEvaluatorManager evaluatorManager = new EvaluatorManager(_templateManager.GetSelectedTemplate(), CurrentRelation);

                _nodeManager.EvaluatorManager = evaluatorManager;

                nextPage.IsSelected = false;
                PagesCollection.First().IsSelected = false;
                MenuHistory.NextPage(nextPage);
            }
        }

        private void SetRelations(object parameters)
        {
            if (parameters is TemplateRelations relations)
            {
                CurrentRelation = relations;
                _technologiesManager.CurrentTechnologies = relations.Technologies;
                _nodeManager.Nodes = relations.Nodes;
            }
        }

        private void AddDesignation(object parameters)
        {
            CurrentTemplate.TemplateRelations.Add(new());
            UpdateContextMenu();
        }

        private void RemoveDesignationAsync(object parameters)
        {
            if (parameters is TemplateRelations relations)
                CurrentTemplate.TemplateRelations.Remove(relations);
            if (CurrentTemplate.TemplateRelations.Count == 0)
                CurrentRelation = null;
            else
            {
                CurrentRelation = CurrentTemplate.TemplateRelations.LastOrDefault();
            }
            UpdateContextMenu();
        }

        private void AddExampleMarkingsAsync(object parameters)
        {
            CurrentTemplate.ExampleMarkings.Add(string.Empty);
            UpdateContextMenu();
        }

        private void RemoveExampleMarkingsAsync(object parameters)
        {
            if (parameters is string exampleMarkings)
                CurrentTemplate.ExampleMarkings.Remove(exampleMarkings);
            UpdateContextMenu();
        }

        public async void UpdateContextMenu()
        {
            await _nodeManager.MenuHelper.UpdateContextMenuAsync();

        }
    }
}
