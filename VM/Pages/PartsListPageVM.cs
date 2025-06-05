using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.UserControls;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    public class PartsListPageVM : BaseNotifyPropertyChanged
    {
        private readonly Frame _nodePage;
        private readonly INodeManager _nodeManager;
        private readonly ITechnologiesManager _technologiesManager;
        private readonly ITemplateManager _templateManager;
        public ObservableCollection<Node> Nodes { get; set; } = [];
        public ObservableCollection<PageModel> PageCollection { get; set; } = [];
        public ObservableCollection<NodeGroup> NodeGroups { get; set; } = new();
        private HashSet<string> _expandedGroups = new HashSet<string>();

        private PageModel _currentPage;

        private int _columnCount = 4;
        public int ColumnCount
        {
            get => _columnCount;
            set => SetValue(ref _columnCount, value, nameof(ColumnCount));
        }

        public Node SelectedNode
        {
            get => _nodeManager?.CurrentNode;
            set
            {
                if (value != null)
                {
                    _nodeManager.CurrentNode = value;
                    _technologiesManager.CurrentTechnologies = value.Technologies;
                    SetPageCollection();
                }
            }
        }

        private string _filterString = string.Empty;
        public string FilterString
        {
            get => _filterString;
            set
            {
                SetValue(ref _filterString, value, nameof(FilterString));
                FilterNode();
            }
        }

        public ICommand NextPageCommand { get; set; }
        public ICommand DeleteNodeCommand { get; set; }
        public ICommand CopyNodeCommand { get; set; }
        public ICommand AddNodeCommand { get; set; }
        public ICommand CopyNodeCurrentTemplateCommand { get; set; }
        public ICommand CopyNodeAllTemplateCommand { get; set; }
        public ICommand FilterNodeCommand { get; set; }

        public PartsListPageVM(ITechnologiesManager technologiesManager, INodeManager nodeManager, ITemplateManager templateManager, Frame nodePage)
        {
            _nodeManager = nodeManager;
            _technologiesManager = technologiesManager;
            _templateManager = templateManager; 
            _technologiesManager.MenuHelper = _nodeManager.MenuHelper;
            Nodes = _nodeManager.Nodes;
            _nodePage = nodePage;

            SelectedNode = Nodes.FirstOrDefault();
            SetNodeGroup();
            SetNodePage(PageCollection.FirstOrDefault());

            InitializeCommand();
        }

        private void InitializeCommand()
        {
            NextPageCommand = new RelayCommand(SetNodePage);
            DeleteNodeCommand = new RelayCommand(DeleteNode);
            CopyNodeCommand = new RelayCommand(CopyNode);
            AddNodeCommand = new RelayCommand(AddNode);
            CopyNodeCurrentTemplateCommand = new RelayCommand(CopyNodeCurrentTemplate);
            CopyNodeAllTemplateCommand = new RelayCommand(CopyNodeAllTemplate);
            FilterNodeCommand = new RelayCommand(FilterNode);
        }

        private void SetNodeGroup()
        {
            // Сохраняем, какие группы были развёрнуты
            _expandedGroups = new HashSet<string>(
                NodeGroups.Where(g => g.IsExpanded).Select(g => g.Key));

            var grouped = Nodes
                .GroupBy(n => n.Type)
                .Select(g => new NodeGroup
                {
                    Key = g.Key,
                    Nodes = new ObservableCollection<Node>(g.ToList()),
                    IsExpanded = _expandedGroups.Contains(g.Key) // восстанавливаем состояние
                })
                .ToList();

            NodeGroups = new(grouped);
        }


        private void SetNodePage(object parameter)
        {
            if (parameter is PageModel page)
            {
                if (page.Title.Equals("Детали"))
                {
                    PageCollection[0].IsSelected = true;
                    MenuHistory.NextPage(page);
                }
                else
                    _nodePage.Navigate(page.ModelPage);

                _currentPage = page;
            }
        }

        private void DeleteNode(object parameter)
        {
            if (parameter is Node node)
            {
                var removeNode = Nodes.FirstOrDefault(findNode => findNode.Id.Equals(node.Id));

                if (removeNode != null)
                {
                    Nodes.Remove(removeNode);
                    var currentGroup = NodeGroups.FirstOrDefault(g => g.Key.Equals(node.Type));
                    var removeNodeInGroup = currentGroup.Nodes.FirstOrDefault(findNode => findNode.Id.Equals(node.Id));
                    currentGroup.Nodes.Remove(removeNodeInGroup);
                    if (currentGroup.Nodes.Count == 0)
                    {
                        NodeGroups.Remove(currentGroup);
                    }
                    SelectedNode = Nodes.FirstOrDefault();
                }
            }
        }

        private void CopyNode(object parameter)
        {
            if (parameter is Node node)
            {
                var copyNode = Nodes.FirstOrDefault(findNode => findNode.Id.Equals(node.Id));

                Node newNode = new();
                newNode = copyNode.DeepCopyWithNewIds();

                Nodes.Add(newNode);
                var currentGroup = NodeGroups.FirstOrDefault(g => g.Key.Equals(newNode.Type));
                currentGroup?.Nodes.Add(newNode);
                SelectedNode = Nodes.Last();
                newNode = null;
                copyNode = null;
            }
        }

        private void AddNode()
        {
            var newNode = new Node()
            {
                Name = "Новая деталь",
                Type = "Сборочная единица",
            };
            Nodes.Add(newNode);

            var currentGroup = NodeGroups.FirstOrDefault(g => g.Key.Equals(newNode.Type));
            if (currentGroup == null)
            {
                NodeGroups.Add(new() { Key = newNode.Type, Nodes = new() { newNode } });
            }
            else
                currentGroup.Nodes.Add(newNode);

            SelectedNode = Nodes.Last();
            newNode = null;
        }

        private void SetPageCollection()
        {
            bool isAssembly = SelectedNode?.Type == "Сборочная единица";
            bool isDetail = SelectedNode?.Type == "Деталь";
            bool needParamsAndTech = isAssembly || isDetail;

            if (PageCollection.Count == 0)
            {
                InitializePages(isAssembly, needParamsAndTech);
            }
            else
            {

                INodeManager nodeManager = new NodeManager()
                {
                    MenuHelper = _nodeManager.MenuHelper,
                    TableManager = _nodeManager.TableManager,
                    EvaluatorManager = _nodeManager.EvaluatorManager,
                };

                if (SelectedNode != null)
                {
                    nodeManager.CurrentNode = SelectedNode;
                    nodeManager.Nodes = SelectedNode.Nodes;
                }
                UpdatePage(Title: "Основная информация", required: true, typeofPage: typeof(MainNodePage), PackIconKind.NoteText, new object[] { nodeManager, SetPageCollection, SetNodeGroup });
                UpdatePage(Title: "Формулы и условия", required: true, typeofPage: typeof(FormulasAndTermsPage), PackIconKind.Function, new object[] { nodeManager });
                UpdatePage(Title: "Параметры", required: needParamsAndTech, typeofPage: typeof(ParametersPage), PackIconKind.Tune, new object[] { nodeManager });
                UpdatePage(Title: "Тех. процесс", required: needParamsAndTech, typeofPage: typeof(TechnologiesPage), PackIconKind.Cogs, new object[] { _technologiesManager });
                UpdatePage(Title: "Детали", required: isAssembly, typeofPage: typeof(PartsListPage), PackIconKind.Cube, new object[] { _technologiesManager, nodeManager, _templateManager });
            }

            ColumnCount = PageCollection.Count;

            if (!PageCollection.Any(page => page.Title.Equals(_currentPage?.Title)))
            {
                _currentPage = PageCollection.FirstOrDefault();
                _currentPage.IsSelected = true;
                SetNodePage(_currentPage);
            }
        }

        private void InitializePages(bool isAssembly, bool needParamsAndTech)
        {
            INodeManager nodeManager = new NodeManager() 
            {
                MenuHelper = _nodeManager.MenuHelper,
                TableManager = _nodeManager.TableManager,
                EvaluatorManager = _nodeManager.EvaluatorManager,
            };

            if(SelectedNode != null)
            {
                nodeManager.CurrentNode = SelectedNode;
                nodeManager.Nodes = SelectedNode.Nodes;
            }
            PageCollection.Clear();

            PageCollection.Add(new PageModel("Основная информация", typeof(MainNodePage), true, PackIconKind.NoteText, new object[] { nodeManager, SetPageCollection, SetNodeGroup }));
            PageCollection.Add(new PageModel("Формулы и условия", typeof(FormulasAndTermsPage), PackIconKind.Function, new object[] { nodeManager }));

            if (needParamsAndTech)
            {
                PageCollection.Add(new PageModel("Параметры", typeof(ParametersPage), PackIconKind.Tune, new object[] { nodeManager }));
                PageCollection.Add(new PageModel("Тех. процесс", typeof(TechnologiesPage), PackIconKind.Cogs, new object[] { _technologiesManager }));
            }

            if (isAssembly)
            {
                PageCollection.Add(new PageModel("Детали", typeof(PartsListPage), PackIconKind.Cube, new object[] { _technologiesManager, nodeManager, _templateManager }));
            }
        }

        private void UpdatePage(string Title, bool required, Type typeofPage, PackIconKind icon, object[] parameters)
        {
            var existingPage = PageCollection.FirstOrDefault(p => p.Title == Title);

            if(existingPage != null && existingPage.ConstructorParameters.Any(param => param is INodeManager))
            {
                var nodeManager = existingPage.ConstructorParameters.FirstOrDefault(param => param is INodeManager) as INodeManager;
                nodeManager.CurrentNode = SelectedNode;
            }

            if (required && existingPage == null)
            {
                PageCollection.Add(new PageModel(Title, typeofPage, icon, parameters));
            }
            else if (!required && existingPage != null)
            {
                existingPage.ClearPage();
                PageCollection.Remove(existingPage);
            }
        }

        private async void CopyNodeCurrentTemplate()
        {
            var dialog = new CopyNodeChoiceDialog(_templateManager.GetSelectedTemplate(), _nodeManager);
            await DialogHost.Show(dialog, "MainDialog");
            SetNodeGroup();
        }

        private async void CopyNodeAllTemplate()
        {
            var dialog = new CopyNodeChoiceDialog(_templateManager.GetReadyTemplate(), _nodeManager);
            await DialogHost.Show(dialog, "MainDialog");
            SetNodeGroup();
        }

        private void FilterNode()
        {
            if (string.IsNullOrEmpty(FilterString))
                Nodes = _nodeManager.Nodes;
            else
                Nodes = new(Nodes.Where(node => node.Name.Contains(FilterString)));
            OnPropertyChanged(nameof(Nodes));
            SetNodeGroup();
            OnPropertyChanged(nameof(NodeGroups));
        }

    }
}
