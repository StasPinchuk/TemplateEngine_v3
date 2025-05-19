using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Views.Pages;
using static TFlex.DOCs.Model.References.Materials.MaterialReferenceObject;

namespace TemplateEngine_v3.VM.Pages
{
    public class TemplatePreviewVM : BaseNotifyPropertyChanged
    {
        private readonly ITemplateManager _templateManager;
        private readonly IBranchManager _branchManager;
        private readonly DrawerHost _drawerHost;
        private readonly Frame _frame;
        private IEvaluatorManager _evaluatorManager;
        private TemplateRelations _relations;
        private TemplateRelations _editRelation;
        private string _orderString;
        public string OrderString
        {
            get => _orderString;
            set => SetValue(ref _orderString, value, nameof(OrderString));
        }


        public ObservableCollection<Node> Details { get; private set; } = [];

        private ObservableCollection<string> _branches = [];
        public ObservableCollection<string> Branches
        {
            get => _branches;
            set => SetValue(ref _branches, value, nameof(Branches));
        }

        private string _branch = string.Empty;
        public string Branch
        {
            get => _branch;
            set => SetValue(ref _branch, value, nameof(Branch));
        }

        private readonly List<Page> _pageHistory = [];

        public ICommand EditParameterCommand => new RelayCommand(EditParameter);
        public ICommand DeleteParameterCommand => new RelayCommand(DeleteParameter);
        public ICommand EditOperationCommand => new RelayCommand(EditOperation);
        public ICommand DeleteOperationCommand => new RelayCommand(DeleteOperation);
        public ICommand EditNodeCommand => new RelayCommand(EditNode);
        public ICommand DeleteNodeCommand => new RelayCommand(DeleteNode);
        public ICommand BackPageCommand => new RelayCommand(BackPage);
        public ICommand CreateSpecCommand => new RelayCommand(CreateSpec, CanCreateSpec);
        public ICommand GetMaterialsCommand => new RelayCommand(GetMaterials, CanGetMateria);

        public TemplatePreviewVM(ITemplateManager templateManager, IBranchManager branchManager, DrawerHost drawerHost, Frame frame)
        {
            _templateManager = templateManager;
            _branchManager = branchManager;
            _drawerHost = drawerHost;
            _frame = frame;
            Branches = new(_templateManager.GetSelectedTemplate().Branches.Select(branch => branch.Name));
            if (Branches.Count == 1)
                Branch = Branches.FirstOrDefault();

        }

        private bool CanCreateSpec(object parameter)
        {
            return !string.IsNullOrEmpty(OrderString);
        }

        public void CreateSpec(object parameter)
        {
            try
            {
                CalculateHelper calculateHelper = new CalculateHelper();

                var template = calculateHelper.CalculateTemplate(_templateManager, OrderString, Branch);

                _relations = template.TemplateRelations.FirstOrDefault();

                _editRelation = _templateManager.GetSelectedTemplate().TemplateRelations
                    .FirstOrDefault(realtion => realtion.Id.Equals(_relations.Id));

                _evaluatorManager = new EvaluatorManager(_editRelation);
                Details.Clear();
                Details = _relations.Nodes;
                OnPropertyChanged(nameof(Details));
            }catch(Exception ex)
            {

            }
        }

        private void EditParameter(object parameter)
        {
            if (parameter is object[] args &&
                args.Length == 2 &&
                args[0] is ConditionEvaluator item &&
                args[1] is DrawerHost drawer)
            {
                ClearFrameHistory();
                var editParam = FindCondition(item, _relations.Nodes);
                if (editParam == null)
                    return;
                var page = new EditParametersPreviewPage(FindCondition(item, _editRelation.Nodes), _evaluatorManager, drawer);
                _frame.Navigate(page);
                DrawerHost.OpenDrawerCommand.Execute(Dock.Right, drawer);
            }
        }

        private void DeleteParameter(object parameter)
        {
            if (parameter is ConditionEvaluator item)
            {
                RemoveCondition(item, _relations.Nodes);
                RemoveCondition(item, _editRelation.Nodes);
                Details = _relations.Nodes;
                OnPropertyChanged(nameof(Details));
            }
        }

        private void RemoveCondition(ConditionEvaluator parameter, ObservableCollection<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Parameters.Any(param => param.Id.Equals(parameter.Id)))
                {
                    var param = node.Parameters.FirstOrDefault(param => param.Id.Equals(parameter.Id));
                    node.Parameters.Remove(param);
                    return;
                }
                if (node.Nodes.Count > 0)
                    RemoveCondition(parameter, node.Nodes);
            }
        }

        private ConditionEvaluator FindCondition(ConditionEvaluator parameter, ObservableCollection<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Parameters.Any(param => param.Id.Equals(parameter.Id)))
                {
                    var param = node.Parameters.FirstOrDefault(param => param.Id.Equals(parameter.Id));
                    _evaluatorManager.SetNodeEvaluators(node);
                    return param;
                }
                if (node.Nodes.Count > 0)
                {
                    var findParam = FindCondition(parameter, node.Nodes);
                    if (findParam != null)
                        return findParam;
                }
            }
            return null;
        }

        private void EditOperation(object parameter)
        {
            if (parameter is object[] args &&
                args.Length == 2 &&
                args[0] is Operation item &&
                args[1] is DrawerHost drawer)
            {
                ClearFrameHistory();

                var page = new EditOperationPreviewPage(FindOperation(item, _editRelation.Nodes, _relations.Nodes), _evaluatorManager, _templateManager.MenuHelper, drawer, _frame);
                _pageHistory.Add(page);
                _frame.Navigate(page);
                // Открываем правый Drawer
                DrawerHost.OpenDrawerCommand.Execute(Dock.Right, drawer);
            }
        }

        private Operation FindOperation(Operation operation, ObservableCollection<Node> nodes, ObservableCollection<Node> _actualNodes)
        {
            foreach (var node in nodes)
            {
                Node actualNode = new();
                // Проверка текущего уровня
                actualNode = _actualNodes.FirstOrDefault(n => n.Id == node.Id);
                if (node.Technologies.Operations.Any(op => op.Id.Equals(operation.Id)))
                {
                    // Найти соответствующий node в _actualNodes по Id
                    if (actualNode != null)
                    {
                        _evaluatorManager.SetNodeEvaluators(actualNode);
                    }

                    return node.Technologies.Operations.FirstOrDefault(op => op.Id.Equals(operation.Id));
                }

                // Проверка вложенных узлов
                if (node.Nodes.Count > 0)
                {
                    var foundOperation = FindOperation(operation, node.Nodes, actualNode.Nodes);
                    if (foundOperation != null)
                        return foundOperation;
                }
            }

            return null;
        }


        private void DeleteOperation(object parameter)
        {
            if (parameter is Operation operation)
            {
                RemoveOperation(operation, _relations.Nodes);
                RemoveOperation(operation, _editRelation.Nodes);
                Details = _relations.Nodes;
                OnPropertyChanged(nameof(Details));
            }
        }

        private void RemoveOperation(Operation operation, ObservableCollection<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Technologies.Operations.Any(op => op.Id.Equals(operation.Id)))
                {
                    var oper = node.Technologies.Operations.FirstOrDefault(op => op.Id.Equals(operation.Id));
                    node.Technologies.Operations.Remove(oper);
                    return;
                }
                if (node.Nodes.Count > 0)
                    RemoveOperation(operation, node.Nodes);
            }
        }

        private void EditNode(object parameter)
        {
            if (parameter is object[] args &&
                args.Length == 2 &&
                args[0] is Node item &&
                args[1] is DrawerHost drawer)
            {
                ClearFrameHistory();
                var page = new EditNodePreviewPage(FindNode(item, _editRelation.Nodes), _evaluatorManager, _templateManager.MenuHelper, drawer, _frame);
                _frame.Navigate(page);
                // Открываем правый Drawer
                DrawerHost.OpenDrawerCommand.Execute(Dock.Right, drawer);
            }
        }

        private Node FindNode(Node currentNode, ObservableCollection<Node> nodes)
        {
            if (nodes.Any(op => op.Id.Equals(currentNode.Id)))
            {
                var node = nodes.FirstOrDefault(n => n.Id.Equals(currentNode.Id));
                _evaluatorManager.SetNodeEvaluators(node);
                return nodes.FirstOrDefault(n => n.Id.Equals(currentNode.Id));
            }
            foreach (var node in nodes)
            {
                if (node.Nodes.Count > 0)
                {
                    var finNode = FindNode(currentNode, node.Nodes);
                    if (finNode != null)
                        return finNode;
                }
            }

            return null;
        }

        private void DeleteNode(object parameter)
        {
            if (parameter is object[] args &&
                args.Length == 2 &&
                args[0] is Node item &&
                args[1] is DrawerHost drawer)
            {
                ClearFrameHistory();
                RemoveNode(item, _relations.Nodes);
                RemoveNode(item, _editRelation.Nodes);
                Details = _relations.Nodes;
                OnPropertyChanged(nameof(Details));
            }
        }

        private void RemoveNode(Node currentNode, ObservableCollection<Node> nodes)
        {
            if (nodes.Any(op => op.Id.Equals(currentNode.Id)))
            {
                var removeNode = nodes.FirstOrDefault(op => op.Id.Equals(currentNode.Id));
                nodes.Remove(removeNode);
                return;
            }
            foreach (var node in nodes)
            {
                if (node.Nodes.Count > 0)
                    RemoveNode(currentNode, node.Nodes);
            }
        }

        private void BackPage()
        {
            if (_frame != null && _pageHistory.Count == 1)
            {
                _frame.Navigate(_pageHistory[_pageHistory.Count - 1]);
            }
            else if (_drawerHost != null)
            {
                DrawerHost.CloseDrawerCommand.Execute(Dock.Right, _drawerHost);
            }
        }

        private bool CanGetMateria(object parameter)
        {
            return _relations != null;
        }


        private void GetMaterials(object parameter)
        {
            if (_relations != null)
            {
                Dictionary<string, double> parameters = GetMaterialParameters(_relations.Nodes.ToList());
                string msg = string.Join("\n", parameters.Select(param => $"{param.Key} : {param.Value}"));
                File.WriteAllText("mat.txt", string.Join("\n", parameters.Select(param => $"{param.Key} : {param.Value}")));
                MessageBox.Show(msg, "Материалы");
            }
        }

        private Dictionary<string, double> GetMaterialParameters(List<Node> nodes)
        {
            Dictionary<string, double> materials = new();

            foreach (Node node in nodes)
            {
                var materialName = node.Parameters.FirstOrDefault(param => param.Name.Equals("Материал"))?.Value;
                var materialWeight = node.Parameters.FirstOrDefault(param => param.Name.Contains("Масса"))?.Value;
                var coatingName = node.Parameters.FirstOrDefault(param => param.Name.Equals("Покрытие"))?.Value;
                var coatingWeight = node.Parameters.FirstOrDefault(param => param.Name.Contains("Расход покрытия"))?.Value;

                AddMaterial(materials, materialName, materialWeight);
                AddMaterial(materials, coatingName, coatingWeight);

                if (node.Type.Equals("Стандартное изделие") || node.Type.Equals("Прочее изделие"))
                {
                    AddMaterial(materials, node.Name, node.Amount?.Value);
                }

                if (node.Nodes.Count > 0)
                {
                    var secondaryMaterials = GetMaterialParameters(node.Nodes.ToList());
                    foreach (var item in secondaryMaterials)
                    {
                        if (materials.ContainsKey(item.Key))
                        {
                            materials[item.Key] += item.Value;
                        }
                        else
                        {
                            materials.Add(item.Key, item.Value);
                        }
                    }
                }
            }

            return materials.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private void AddMaterial(Dictionary<string, double> materials, string name, string weight)
        {
            if (name != null && weight != null)
            {
                if (materials.ContainsKey(name))
                {
                    materials[name] += double.Parse(weight.Replace('.', ','));
                }
                else
                {
                    materials.Add(name, double.Parse(weight.Replace('.', ',')));
                }
            }
        }

        private void ClearFrameHistory()
        {
            _pageHistory.Clear();
        }
    }
}
