using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для редактирования узла в предварительном просмотре.
    /// </summary>
    public class EditNodePreviewVM : BaseNotifyPropertyChanged
    {
        private readonly Operation _editOperation;
        private readonly EvaluatorManager _evaluatorManager;
        private readonly BranchManager _branchManager;
        private readonly DrawerHost _drawer;
        private readonly ContextMenuHelper _contextMenuHelper;

        private readonly Node _editNode;
        private Node _currentNode;

        /// <summary>
        /// Текущий редактируемый узел.
        /// </summary>
        public Node CurrentNode
        {
            get => _currentNode;
            set => SetValue(ref _currentNode, value, nameof(CurrentNode));
        }

        private string _nodeType = string.Empty;

        /// <summary>
        /// Тип текущего узла.
        /// </summary>
        public string NodeType
        {
            get => _nodeType;
            set
            {
                SetValue(ref _nodeType, value, nameof(NodeType));
                CurrentNode.Type = value;
            }
        }

        private ObservableCollection<TreeEvaluator> _parts = new();

        /// <summary>
        /// Коллекция связанных условий (Parts) в виде дерева.
        /// </summary>
        public ObservableCollection<TreeEvaluator> Parts
        {
            get => _parts;
            set => SetValue(ref _parts, value, nameof(Parts));
        }

        /// <summary>
        /// Доступные типы узлов.
        /// </summary>
        public ObservableCollection<string> NodeTypes => NodeTypeManager.NodeTypes;

        /// <summary>
        /// Контекстное меню для текстовых полей.
        /// </summary>
        public ContextMenu TextBoxContextMenu => _contextMenuHelper.GetContextMenu();

        /// <summary>
        /// Команда для применения изменений узла.
        /// </summary>
        public ICommand EditNodeCommand { get; set; }

        /// <summary>
        /// Конструктор ViewModel для редактирования узла.
        /// </summary>
        /// <param name="node">Редактируемый узел.</param>
        /// <param name="evaluatorManager">Менеджер условий и выражений.</param>
        /// <param name="contextMenuHelper">Помощник для создания контекстного меню.</param>
        /// <param name="drawerHost">DrawerHost для закрытия окна редактирования.</param>
        public EditNodePreviewVM(Node node, EvaluatorManager evaluatorManager, ContextMenuHelper contextMenuHelper, DrawerHost drawerHost)
        {
            _editNode = node;
            CurrentNode = node.Copy();
            NodeType = node.Type;
            _evaluatorManager = evaluatorManager;
            _contextMenuHelper = contextMenuHelper;
            _drawer = drawerHost;

            EditNodeCommand = new RelayCommand(EditNode);
        }

        /// <summary>
        /// Построение дерева условий на основе <see cref="ConditionEvaluator"/>.
        /// </summary>
        /// <param name="evaluator">Условие, из которого строится дерево.</param>
        /// <returns>Корень дерева условий.</returns>
        private TreeEvaluator BuildTreeEvaluator(ConditionEvaluator evaluator)
        {
            try
            {
                var treeEvaluator = new TreeEvaluator { ConditionEvaluator = evaluator };

                if (evaluator.Parts.Count > 0)
                {
                    var conditions = _evaluatorManager.AllTemplateEvaluator
                        .Where(cond => evaluator.Parts.Contains(cond.Id))
                        .GroupBy(p => p.Id)
                        .Select(group => group.First());

                    foreach (var condition in conditions)
                    {
                        treeEvaluator.TreeEvaluators.Add(BuildTreeEvaluator(condition));
                    }
                }

                return treeEvaluator;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Устанавливает дерево условий (Parts) для отображения.
        /// </summary>
        /// <param name="evaluator">Корневое условие.</param>
        public void SetParts(ConditionEvaluator evaluator)
        {
            
            var root = BuildTreeEvaluator(evaluator);
            if (root != null)
            {
                Parts = new ObservableCollection<TreeEvaluator> { root };
            }
            else
            {
                Parts.Clear();
            }
        }

        /// <summary>
        /// Очищает коллекцию Parts.
        /// </summary>
        public void ClearParts()
        {
            Parts.Clear();
        }

        /// <summary>
        /// Применяет изменения в узле и закрывает Drawer.
        /// </summary>
        private void EditNode()
        {
            _editNode.SetValue(CurrentNode);
            DrawerHost.CloseDrawerCommand.Execute(Dock.Right, _drawer);
        }
    }
}
