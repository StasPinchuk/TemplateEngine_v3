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

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для редактирования операции в режиме предварительного просмотра.
    /// </summary>
    public class EditOperationPreviewEditVM : BaseNotifyPropertyChanged
    {
        private readonly Operation _editOperation;
        private readonly IEvaluatorManager _evaluatorManager;
        private readonly IBranchManager _branchManager;
        private readonly DrawerHost _drawer;
        private readonly ContextMenuHelper _contextMenuHelper;

        private BranchDivisionDetails _currentBranchDivision;

        /// <summary>
        /// Текущая выбранная ветка подразделения филиала.
        /// </summary>
        public BranchDivisionDetails CurrentBranchDivision
        {
            get => _currentBranchDivision;
            set => SetValue(ref _currentBranchDivision, value, nameof(CurrentBranchDivision));
        }

        private ObservableCollection<TreeEvaluator> _parts = new();

        /// <summary>
        /// Коллекция условий в виде дерева, отображаемая при выборе условия.
        /// </summary>
        public ObservableCollection<TreeEvaluator> Parts
        {
            get => _parts;
            set => SetValue(ref _parts, value, nameof(Parts));
        }

        /// <summary>
        /// Контекстное меню для текстовых полей.
        /// </summary>
        public ContextMenu TextBoxContextMenu => _contextMenuHelper.GetContextMenu();

        /// <summary>
        /// Коллекция всех подразделений, связанных с операцией.
        /// </summary>
        public ObservableCollection<BranchDivisionDetails> BranchDivision { get; set; }

        /// <summary>
        /// Команда для применения изменений операции.
        /// </summary>
        public ICommand EditOperationCommand => new RelayCommand(EditOperation);

        /// <summary>
        /// Конструктор ViewModel редактирования операции.
        /// </summary>
        /// <param name="operation">Редактируемая операция.</param>
        /// <param name="evaluatorManager">Менеджер условий.</param>
        /// <param name="contextMenuHelper">Помощник контекстного меню.</param>
        /// <param name="drawer">DrawerHost, в котором происходит редактирование.</param>
        public EditOperationPreviewEditVM(Operation operation, IEvaluatorManager evaluatorManager, ContextMenuHelper contextMenuHelper, DrawerHost drawer)
        {
            _editOperation = operation;
            BranchDivision = operation.Copy().BranchDivisionDetails;
            _evaluatorManager = evaluatorManager;
            _contextMenuHelper = contextMenuHelper;
            _drawer = drawer;

            if (BranchDivision.Count > 0)
                CurrentBranchDivision = BranchDivision[0];
        }

        /// <summary>
        /// Применяет изменения к операции и закрывает Drawer.
        /// </summary>
        private void EditOperation()
        {
            if (CurrentBranchDivision != null)
            {
                _editOperation.BranchDivisionDetails = BranchDivision;
            }

            DrawerHost.CloseDrawerCommand.Execute(Dock.Right, _drawer);
        }

        /// <summary>
        /// Построение дерева условий на основе <see cref="ConditionEvaluator"/>.
        /// </summary>
        /// <param name="evaluator">Условие, из которого строится дерево.</param>
        /// <returns>Корень дерева условий, либо null в случае ошибки.</returns>
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
        /// Устанавливает дерево условий для отображения в интерфейсе.
        /// </summary>
        /// <param name="evaluator">Корневое условие для построения дерева.</param>
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
        /// Очищает отображаемое дерево условий.
        /// </summary>
        public void ClearParts()
        {
            Parts.Clear();
        }
    }
}
