using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    public class TemplatePreviewVM : BaseNotifyPropertyChanged
    {
        private readonly TemplateManager _templateManager;
        private readonly BranchManager _branchManager;
        private readonly DrawerHost _drawerHost;
        private readonly Frame _frame;
        private EvaluatorManager _evaluatorManager;
        private TemplateRelations _editRelation;
        private string _orderString;

        private TemplateRelations _relations;
        public TemplateRelations Relations
        {
            get => _relations;
            set => SetValue(ref _relations, value, nameof(Relations));
        }
        /// <summary>
        /// Строка заказа, для формирования спецификации
        /// </summary>
        public string OrderString
        {
            get => _orderString;
            set => SetValue(ref _orderString, value, nameof(OrderString));
        }

        /// <summary>
        /// Коллекция деталей (узлов), отображаемых в интерфейсе.
        /// </summary>
        public ObservableCollection<Node> Details { get; private set; } = [];

        private ObservableCollection<string> _branches = [];

        /// <summary>
        /// Список доступных веток шаблона.
        /// </summary>
        public ObservableCollection<string> Branches
        {
            get => _branches;
            set => SetValue(ref _branches, value, nameof(Branches));
        }

        private string _branch = string.Empty;
        /// <summary>
        /// Выбранная пользователем ветка шаблона.
        /// </summary>
        public string Branch
        {
            get => _branch;
            set => SetValue(ref _branch, value, nameof(Branch));
        }

        private readonly List<Page> _pageHistory = [];

        /// <summary>
        /// Команда редактирования параметра
        /// </summary>
        public ICommand EditParameterCommand => new RelayCommand(EditParameter);

        /// <summary>
        /// Команда удаления параметра
        /// </summary>
        public ICommand DeleteParameterCommand => new RelayCommand(DeleteParameter);

        /// <summary>
        /// Команда редактирования операции
        /// </summary>
        public ICommand EditOperationCommand => new RelayCommand(EditOperation);

        /// <summary>
        /// Команда удаления операции
        /// </summary>
        public ICommand DeleteOperationCommand => new RelayCommand(DeleteOperation);

        /// <summary>
        /// Команда редактирования детали
        /// </summary>
        public ICommand EditNodeCommand => new RelayCommand(EditNode);

        /// <summary>
        /// Команда удаления команды
        /// </summary>
        public ICommand DeleteNodeCommand => new RelayCommand(DeleteNode);

        /// <summary>
        /// Команда удаления узла.
        /// </summary>
        public ICommand BackPageCommand => new RelayCommand(BackPage);

        /// <summary>
        /// Команда формирования спецификации
        /// </summary>
        public ICommand CreateSpecCommand => new RelayCommand(CreateSpec, CanCreateSpec);

        /// <summary>
        /// Команда формирования спецификации
        /// </summary>
        public ICommand GetMaterialsCommand => new RelayCommand(GetMaterials, CanGetMateria);

        /// <summary>
        /// Конструктор ViewModel. Загружает ветки и инициализирует зависимости.
        /// </summary>
        /// <param name="templateManager">Менеджер шаблонов</param>
        /// <param name="branchManager">Менеджер филиалов</param>
        /// <param name="drawerHost">Контейнер DrawerHost.</param>
        /// <param name="frame">Фрейм для навигации между страницами.</param>
        public TemplatePreviewVM(TemplateManager templateManager, BranchManager branchManager, DrawerHost drawerHost, Frame frame)
        {
            _templateManager = templateManager;
            _branchManager = branchManager;
            _drawerHost = drawerHost;
            _frame = frame;
            Branches = new(_templateManager.GetSelectedTemplate().Branches.Select(branch => branch.Name));
            if (Branches.Count == 1)
                Branch = Branches.FirstOrDefault();

        }

        /// <summary>
        /// Проверка возможности генерации спецификации
        /// </summary>
        /// <returns>
        /// Возвращает <c>true</c>, если указаны и строка заказа (<see cref="OrderString"/>), и ветка (<see cref="Branch"/>); иначе <c>false</c>.
        /// </returns>
        private bool CanCreateSpec(object parameter)
        {
            return !string.IsNullOrEmpty(OrderString) && !string.IsNullOrEmpty(Branch);
        }

        /// <summary>
        /// Создаёт спецификацию на основе текущего шаблона и строки заказа.
        /// </summary>
        public void CreateSpec(object parameter)
        {
            try
            {
                TemplateCalculatorService calculatorService = new TemplateCalculatorService(_templateManager);

                var template = calculatorService.Calculate(OrderString, Branch);

                Relations = template.TemplateRelations.FirstOrDefault();

                _editRelation = _templateManager.GetSelectedTemplate().TemplateRelations
                    .FirstOrDefault(realtion => realtion.Id.Equals(_relations.Id));

                _evaluatorManager = new EvaluatorManager(_editRelation);
                Details.Clear();
                Details = _relations.Nodes;
                OnPropertyChanged(nameof(Details));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Открывает страницу редактирования параметра в режиме предпросмотра.
        /// </summary>
        /// <param name="parameter">
        /// Массив из двух объектов, где:
        /// <list type="bullet">
        /// <item><description><see cref="ConditionEvaluator"/> — параметр для редактирования.</description></item>
        /// <item><description><see cref="DrawerHost"/> — контейнер, в котором будет открыт Drawer.</description></item>
        /// </list>
        /// </param>
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

        /// <summary>
        /// Удаляет указанный параметр
        /// </summary>
        /// <param name="parameter">
        /// Объект типа <see cref="ConditionEvaluator"/>, представляющий удаляемый параметр.
        /// </param>
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

        /// <summary>
        /// Рекурсивно удаляет параметр <paramref name="parameter"/> из коллекции узлов <paramref name="nodes"/>.
        /// </summary>
        /// <param name="parameter">Удаляемый параметр.</param>
        /// <param name="nodes">Коллекция узлов, в которых производится удаление.</param>
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

        /// <summary>
        /// Рекурсивно ищет параметр <paramref name="parameter"/> в коллекции узлов <paramref name="nodes"/>.
        /// При нахождении настраивает вычислитель для соответствующего узла.
        /// </summary>
        /// <param name="parameter">Параметр, который необходимо найти.</param>
        /// <param name="nodes">Коллекция узлов для поиска.</param>
        /// <returns>Найденный экземпляр <see cref="ConditionEvaluator"/>, либо <c>null</c>, если не найден.</returns>
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

        /// <summary>
        /// Открывает страницу редактирования операции с заданным идентификатором.
        /// </summary>
        /// <param name="parameter">
        /// Массив, содержащий объект <see cref="Operation"/> и <see cref="DrawerHost"/> для отображения формы редактирования.
        /// </param>
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

                DrawerHost.OpenDrawerCommand.Execute(Dock.Right, drawer);
            }
        }

        /// <summary>
        /// Рекурсивно ищет операцию <paramref name="operation"/> в коллекции <paramref name="nodes"/> 
        /// и настраивает вычислитель для соответствующего узла из <paramref name="_actualNodes"/>.
        /// </summary>
        /// <param name="operation">Операция, которую необходимо найти.</param>
        /// <param name="nodes">Коллекция узлов редактируемого шаблона.</param>
        /// <param name="_actualNodes">Коллекция актуальных узлов для настройки вычислений.</param>
        /// <returns>Найденная операция или <c>null</c>, если не найдена.</returns>
        private Operation FindOperation(Operation operation, ObservableCollection<Node> nodes, ObservableCollection<Node> _actualNodes)
        {
            foreach (var node in nodes)
            {
                Node actualNode = _actualNodes.FirstOrDefault(n => n.Id == node.Id);

                if (node.Technologies.Operations.Any(op => op.Id.Equals(operation.Id)))
                {
                    if (actualNode != null)
                    {
                        _evaluatorManager.SetNodeEvaluators(actualNode);
                    }

                    return node.Technologies.Operations.FirstOrDefault(op => op.Id.Equals(operation.Id));
                }

                if (node.Nodes.Count > 0)
                {
                    var foundOperation = FindOperation(operation, node.Nodes, actualNode.Nodes);
                    if (foundOperation != null)
                        return foundOperation;
                }
            }

            return null;
        }

        /// <summary>
        /// Удаляет операцию из обоих наборов узлов (_relations и _editRelation).
        /// </summary>
        /// <param name="parameter">Операция для удаления.</param>
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

        /// <summary>
        /// Рекурсивно удаляет указанную операцию из коллекции узлов.
        /// </summary>
        /// <param name="operation">Операция для удаления.</param>
        /// <param name="nodes">Коллекция узлов, в которых производится удаление.</param>
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

        /// <summary>
        /// Открывает страницу редактирования узла.
        /// </summary>
        /// <param name="parameter">Массив, содержащий Node и DrawerHost.</param>
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

                DrawerHost.OpenDrawerCommand.Execute(Dock.Right, drawer);
            }
        }

        /// <summary>
        /// Рекурсивно ищет узел по идентификатору и настраивает вычислители.
        /// </summary>
        /// <param name="currentNode">Искомый узел.</param>
        /// <param name="nodes">Коллекция узлов, в которой производится поиск.</param>
        /// <returns>Найденный узел или <c>null</c>.</returns>
        private Node FindNode(Node currentNode, ObservableCollection<Node> nodes)
        {
            if (nodes.Any(op => op.Id.Equals(currentNode.Id)))
            {
                var node = nodes.FirstOrDefault(n => n.Id.Equals(currentNode.Id));
                _evaluatorManager.SetNodeEvaluators(node);
                return node;
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

        /// <summary>
        /// Удаляет указанный узел из обоих наборов (_relations и _editRelation).
        /// </summary>
        /// <param name="parameter">Массив, содержащий Node и DrawerHost.</param>
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

        /// <summary>
        /// Рекурсивно удаляет узел по идентификатору из коллекции.
        /// </summary>
        /// <param name="currentNode">Узел для удаления.</param>
        /// <param name="nodes">Коллекция узлов.</param>
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

        /// <summary>
        /// Возвращает пользователя на предыдущую страницу или закрывает Drawer, если история пуста.
        /// </summary>
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

        /// <summary>
        /// Проверяет, доступна ли команда получения материалов.
        /// </summary>
        /// <param name="parameter">Не используется.</param>
        /// <returns>True, если _relations не равен null.</returns>
        private bool CanGetMateria(object parameter)
        {
            return _relations != null;
        }

        /// <summary>
        /// Собирает и сохраняет параметры материалов из дерева узлов и отображает их.
        /// </summary>
        /// <param name="parameter">Не используется.</param>
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

        /// <summary>
        /// Рекурсивно собирает материалы и их веса из узлов и их подузлов.
        /// </summary>
        /// <param name="nodes">Список узлов для обработки.</param>
        /// <returns>Словарь с материалами и их суммарными количествами.</returns>
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

                if (node.Type.Equals("Стандартное изделие") || node.Type.Equals("Прочее изделие") || node.Type.Equals("Материал"))
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

        /// <summary>
        /// Добавляет материал и его вес в словарь, если данные корректны.
        /// </summary>
        /// <param name="materials">Целевой словарь материалов.</param>
        /// <param name="name">Название материала.</param>
        /// <param name="weight">Вес материала в строковом формате.</param>
        private void AddMaterial(Dictionary<string, double> materials, string name, string weight)
        {
            if (name != null && weight != null)
            {
                if (materials.ContainsKey(name))
                {
                    if (!string.IsNullOrEmpty(weight))
                        materials[name] += double.Parse(weight.Replace('.', ','));
                }
                else
                {
                    if (!string.IsNullOrEmpty(weight))
                        materials.Add(name, double.Parse(weight.Replace('.', ',')));
                }
            }
        }

        /// <summary>
        /// Очищает историю переходов по страницам.
        /// </summary>
        private void ClearFrameHistory()
        {
            _pageHistory.Clear();
        }

    }
}
