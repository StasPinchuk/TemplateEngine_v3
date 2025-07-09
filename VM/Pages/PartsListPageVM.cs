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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace TemplateEngine_v3.VM.Pages
{
    public class PartsListPageVM : BaseNotifyPropertyChanged
    {
        private readonly Frame _nodePage;
        private readonly NodeManager _nodeManager;
        private readonly TechnologiesManager _technologiesManager;
        private readonly TemplateManager _templateManager;

        private ObservableCollection<Node> _nodes;
        public ObservableCollection<Node> Nodes
        {
            get => _nodes;
            set => SetValue(ref _nodes, value, nameof(Nodes));
        }

        private List<PageModel> _pageCollection = new();
        public List<PageModel> PageCollection
        {
            get => _pageCollection;
            set => SetValue(ref _pageCollection, value, nameof(PageCollection));
        }

        private ObservableCollection<PageModel> _visibilitePageCollection = new();
        public ObservableCollection<PageModel> VisibilitePageCollection
        {
            get => _visibilitePageCollection;
            set => SetValue(ref _visibilitePageCollection, value, nameof(VisibilitePageCollection));
        }

        private ObservableCollection<NodeGroup> _nodeGroups = new();
        public ObservableCollection<NodeGroup> NodeGroups
        {
            get => _nodeGroups;
            set => SetValue(ref _nodeGroups, value, nameof(NodeGroups));
        }

        private HashSet<string> _expandedGroups = new();

        private PageModel _currentPage;

        public Node SelectedNode
        {
            get => _nodeManager?.CurrentNode;
            set
            {
                _nodeManager.CurrentNode = value;
                if (value != null)
                {
                    _technologiesManager.CurrentTechnologies = value.Technologies;
                    SetPageCollection();
                }
                else
                {
                    PageCollection.Clear();
                    _currentPage = null;
                    SetNodePage(_currentPage);
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

        public PartsListPageVM(TechnologiesManager technologiesManager, NodeManager nodeManager, TemplateManager templateManager, Frame nodePage)
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
            _expandedGroups = new(NodeGroups.Where(g => g.IsExpanded).Select(g => g.Key));

            var grouped = Nodes
                .GroupBy(n => n.Type)
                .Select(g => new NodeGroup
                {
                    Key = g.Key,
                    Nodes = new ObservableCollection<Node>(g.ToList()),
                    IsExpanded = _expandedGroups.Contains(g.Key)
                })
                .ToList();

            NodeGroups.Clear();

            foreach (var group in grouped)
                NodeGroups.Add(group);

        }

        private void SetNodePage(object parameter)
        {
            if (parameter is PageModel page)
            {
                page.ClearPage();

                if (page.Title.Equals("Детали"))
                {
                    PageCollection[0].IsSelected = true;

                    NavigationService.AddPageToPageHistory(page);
                    NavigationService.SetPageInSecondaryFrame();
                }
                else
                {
                    _nodePage.Navigate(page.ModelPage);
                }
                _currentPage = page;
            }
        }

        private void DeleteNode(object parameter)
        {
            if (parameter is Node node)
            {
                var removeNode = Nodes.FirstOrDefault(n => n.Id == node.Id);
                if (removeNode == null) return;

                Nodes.Remove(removeNode);
                var group = NodeGroups.FirstOrDefault(g => g.Key == node.Type);
                group?.Nodes.Remove(removeNode);
                if (group?.Nodes.Count == 0) NodeGroups.Remove(group);

                SelectedNode = Nodes.FirstOrDefault();
            }
        }

        private void CopyNode(object parameter)
        {
            if (parameter is not Node node) return;
            var copy = node.DeepCopyWithNewIds();
            Nodes.Add(copy);

            var group = NodeGroups.FirstOrDefault(g => g.Key == copy.Type);
            if (group == null)
                NodeGroups.Add(new NodeGroup { Key = copy.Type, Nodes = new() { copy } });
            else
                group.Nodes.Add(copy);

            SelectedNode = copy;
        }

        private void AddNode()
        {
            var newNode = new Node { Name = "Новая деталь", Type = "Сборочная единица" };
            Nodes.Add(newNode);

            var group = NodeGroups.FirstOrDefault(g => g.Key == newNode.Type);
            if (group == null)
                NodeGroups.Add(new NodeGroup { Key = newNode.Type, Nodes = new() { newNode } });
            else
                group.Nodes.Add(newNode);

            SelectedNode = newNode;
        }

        private void SetPageCollection()
        {
            bool isAssembly = SelectedNode?.Type == "Сборочная единица";
            bool isDetail = SelectedNode?.Type == "Деталь";
            bool needParamsAndTech = !isAssembly && !isDetail;

            if (PageCollection.Count == 0)
            {
                InitializePages(isAssembly, needParamsAndTech);
            }
            else
            {
                var nodeManager = new NodeManager
                {
                    MenuHelper = _nodeManager.MenuHelper,
                    TableManager = _nodeManager.TableManager,
                    EvaluatorManager = _nodeManager.EvaluatorManager,
                    CurrentNode = SelectedNode.Nodes.FirstOrDefault() ?? SelectedNode,
                    Nodes = SelectedNode.Nodes
                };

                VisibilitePageCollection.Clear();

                if (isAssembly)
                {
                    foreach(var page in PageCollection)
                        VisibilitePageCollection.Add(page);
                }

                if (isDetail)
                {
                    foreach(var page in PageCollection.Take(PageCollection.Count-1))
                        VisibilitePageCollection.Add(page);
                }

                if (needParamsAndTech)
                {
                    foreach(var page in PageCollection.Take(2))
                        VisibilitePageCollection.Add(page);
                }

                UpdatePage("Детали", isAssembly, typeof(PartsListPage), PackIconKind.Cube, new object[] { _technologiesManager, nodeManager, _templateManager });
            }

            if (!PageCollection.Any(p => p.Title == _currentPage?.Title))
            {
                _currentPage = PageCollection.FirstOrDefault();
                _currentPage.IsSelected = true;
            }
        }

        private void InitializePages(bool isAssembly, bool needParamsAndTech)
        {
            var nodeManager = new NodeManager
            {
                MenuHelper = _nodeManager.MenuHelper,
                TableManager = _nodeManager.TableManager,
                EvaluatorManager = _nodeManager.EvaluatorManager,
                CurrentNode = SelectedNode.Nodes.FirstOrDefault() ?? SelectedNode,
                Nodes = SelectedNode.Nodes
            };

            PageCollection = new();
            PageCollection.Add(new PageModel("Основная информация", typeof(MainNodePage), true, PackIconKind.NoteText, new object[] { _nodeManager, SetPageCollection, SetNodeGroup }));
            PageCollection.Add(new PageModel("Формулы и условия", typeof(FormulasAndTermsPage), PackIconKind.Function, new object[] { _nodeManager }));
            PageCollection.Add(new PageModel("Параметры", typeof(ParametersPage), PackIconKind.Tune, new object[] { _nodeManager }));
            PageCollection.Add(new PageModel("Тех. процесс", typeof(TechnologiesPage), PackIconKind.Cogs, new object[] { _technologiesManager }));
            PageCollection.Add(new PageModel("Детали", typeof(PartsListPage), PackIconKind.Cube, new object[] { _technologiesManager, nodeManager, _templateManager }));

            bool isDetail = SelectedNode?.Type == "Деталь";

            if (isAssembly)
            {
                foreach (var page in PageCollection)
                    VisibilitePageCollection.Add(page);
            }

            if (isDetail)
            {
                foreach (var page in PageCollection.Take(PageCollection.Count - 1))
                    VisibilitePageCollection.Add(page);
            }

            if (needParamsAndTech)
            {
                foreach (var page in PageCollection.Take(2))
                    VisibilitePageCollection.Add(page);
            }
        }

        private void UpdatePage(string Title, bool required, Type typeofPage, PackIconKind icon, object[] parameters)
        {
            var page = PageCollection.Last();

            if (page.ConstructorParameters.FirstOrDefault(p => p is NodeManager) is NodeManager nodeManager)
            {
                nodeManager.CurrentNode = SelectedNode;
                nodeManager.Nodes = SelectedNode.Nodes;
            }
        }

        private async void CopyNodeCurrentTemplate()
        {
            var dialog = new CopyNodeChoiceDialog(_templateManager.GetSelectedTemplate(), Nodes);
            await DialogHost.Show(dialog, "MainDialog");
            SetNodeGroup();
        }

        private async void CopyNodeAllTemplate()
        {
            var dialog = new CopyNodeChoiceDialog(_templateManager.GetReadyTemplate(), Nodes);
            await DialogHost.Show(dialog, "MainDialog");
            SetNodeGroup();
        }

        private void FilterNode()
        {
            Nodes = string.IsNullOrEmpty(FilterString)
                ? new(_nodeManager.Nodes)
                : new(_nodeManager.Nodes.Where(n => n.Name.Contains(FilterString)));
            SetNodeGroup();
        }
    }
}