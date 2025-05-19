using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.UserControls;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для страницы работы с формулами и условиями.
    /// Управляет списками формул и условий, а также текущим выбранным и редактируемым элементом.
    /// </summary>
    public class FormulasAndTermsPageVM : BaseNotifyPropertyChanged
    {
        private Node _currentNode;

        /// <summary>
        /// Текущий узел дерева, для которого отображаются формулы и условия.
        /// При изменении обновляются списки формул и условий.
        /// </summary>
        public Node CurrentNode
        {
            get => _currentNode;
            set
            {
                SetValue(ref _currentNode, value, nameof(CurrentNode));

                if (_currentNode?.ExpressionRepository != null)
                {
                    Formulas = Evaluator = _currentNode.ExpressionRepository.Formulas ?? new ObservableCollection<ConditionEvaluator>();
                    Terms = _currentNode.ExpressionRepository.Terms ?? new ObservableCollection<ConditionEvaluator>();
                }
                else
                {
                    Formulas = Evaluator = new ObservableCollection<ConditionEvaluator>();
                    Terms = new ObservableCollection<ConditionEvaluator>();
                }
            }
        }

        private ConditionEvaluator _selectedEvaluator = null;

        /// <summary>
        /// Выбранный элемент из списка формул или условий.
        /// При изменении инициирует копирование для редактирования.
        /// </summary>
        public ConditionEvaluator SelectedEvaluator
        {
            get => _selectedEvaluator;
            set
            {
                SetValue(ref _selectedEvaluator, value, nameof(SelectedEvaluator));
                if (value != null)
                {
                    ChooseEvalutor();
                }
            }
        }

        private ConditionEvaluator _currentEvaluator = new();

        /// <summary>
        /// Текущий редактируемый элемент (формула или условие).
        /// При изменении обновляется дерево частей.
        /// </summary>
        public ConditionEvaluator CurrentEvaluator
        {
            get => _currentEvaluator;
            set
            {
                SetValue(ref _currentEvaluator, value, nameof(CurrentEvaluator));
                if (value != null)
                    CreatePartTree();
            }
        }

        private bool _selectedList = false;

        /// <summary>
        /// Определяет, отображать ли список условий (true) или формул (false).
        /// При изменении обновляет список отображаемых элементов и очищает текущий редактируемый.
        /// </summary>
        public bool SelectedList
        {
            get => _selectedList;
            set
            {
                SetValue(ref _selectedList, value, nameof(SelectedList));
                SelectedEvaluator = null;
                SetEvaluator();
                CurrentEvaluator = new();
            }
        }

        private TreeEvaluator _treeEvaluator = new();

        /// <summary>
        /// Дерево для отображения частей текущего условия или формулы.
        /// </summary>
        public TreeEvaluator Parts
        {
            get => _treeEvaluator;
            set => SetValue(ref _treeEvaluator, value, nameof(Parts));
        }

        private string _buttonText = "Добавить формулу";

        /// <summary>
        /// Текст на кнопке добавления/изменения формулы или условия.
        /// </summary>
        public string ButtonText
        {
            get => _buttonText;
            set => SetValue(ref _buttonText, value, nameof(ButtonText));
        }

        private readonly INodeManager _nodeManager;
        private readonly IEvaluatorManager _evaluatorManager;

        private ObservableCollection<ConditionEvaluator> Formulas { get; set; } = new ObservableCollection<ConditionEvaluator>();
        private ObservableCollection<ConditionEvaluator> Terms { get; set; } = new ObservableCollection<ConditionEvaluator>();

        /// <summary>
        /// Коллекция текущих элементов для отображения (формулы или условия).
        /// </summary>
        public ObservableCollection<ConditionEvaluator> Evaluator { get; set; } = new ObservableCollection<ConditionEvaluator>();

        public ObservableCollection<ConditionEvaluator> SystemEvaluators { get; set; }
        public ObservableCollection<ConditionEvaluator> AllTemplateEvaluator { get; set; }
        public ObservableCollection<ConditionEvaluator> NodeEvaluators { get; set; }
        public ObservableCollection<string> TemplateMarkings { get; set; }

        public ICommand CopyEvalutorCommand { get; set; }
        public ICommand RemoveEvalutorCommand { get; set; }
        public ICommand ModifyEvalutorCommand { get; set; }
        public ICommand CancelModifyEvalutorCommand { get; set; }
        public ICommand SetSystemFormulaCommand { get; set; }
        public ICommand SetMarkingCommand { get; set; }
        public ICommand SetCurrentEvaluatorCommand { get; set; }
        public ICommand SetTableCommand { get; set; }

        /// <summary>
        /// Конструктор ViewModel. Подписывается на событие изменения текущего узла.
        /// Инициализирует команды и начальные данные.
        /// </summary>
        /// <param name="nodeManager">Менеджер узлов</param>
        public FormulasAndTermsPageVM(INodeManager nodeManager)
        {
            _nodeManager = nodeManager;
            _evaluatorManager = _nodeManager.EvaluatorManager;
            CurrentNode = _nodeManager.CurrentNode;
            _nodeManager.CurrentNodeChanged += OnCurrentNodeChanged;
            _nodeManager.EvaluatorChanged += OnNodeChanged;

            SystemEvaluators = new ObservableCollection<ConditionEvaluator>(_evaluatorManager.SystemEvaluators);
            AllTemplateEvaluator = new ObservableCollection<ConditionEvaluator>(_evaluatorManager.AllTemplateEvaluator);
            TemplateMarkings = new ObservableCollection<string>(_evaluatorManager.TemplateMarkings);
            SetEvaluator();

            InitializeCommand();
        }

        private void InitializeCommand()
        {
            CopyEvalutorCommand = new RelayCommand(CopyEvalutor);
            RemoveEvalutorCommand = new RelayCommand(RemoveEvalutor);
            ModifyEvalutorCommand = new RelayCommand(ModifyEvalutor, CanModifyEvalutor);
            CancelModifyEvalutorCommand = new RelayCommand(CancelModifyEvalutor, CanCancelModifyEvalutor);
            SetSystemFormulaCommand = new RelayCommand(SetSystemFormula);
            SetCurrentEvaluatorCommand = new RelayCommand(SetCurrentEvaluator);
            SetMarkingCommand = new RelayCommand(SetMarking);
            SetTableCommand = new RelayCommand(SetTableAsync);
        }

        /// <summary>
        /// Обработчик события смены текущего узла.
        /// Обновляет списки формул и условий.
        /// </summary>
        /// <param name="node">Новый текущий узел</param>
        private void OnCurrentNodeChanged(Node node)
        {
            _nodeManager.CurrentNodeChanged += OnCurrentNodeChanged;
            CurrentNode = node;
            _evaluatorManager.SetNodeEvaluators(node);
            OnPropertyChanged(nameof(CurrentNode));

            Formulas = node.ExpressionRepository.Formulas;
            Terms = node.ExpressionRepository.Terms;
            NodeEvaluators = _evaluatorManager.NodeEvaluators;
            OnPropertyChanged(nameof(NodeEvaluators));
            SetEvaluator();
        }
        
        private void OnNodeChanged()
        {
            _evaluatorManager.SetNodeEvaluators(CurrentNode);
            NodeEvaluators = _evaluatorManager.NodeEvaluators;
            OnPropertyChanged(nameof(NodeEvaluators));
        }

        /// <summary>
        /// Обновляет коллекцию отображаемых элементов (формулы или условия) в зависимости от SelectedList.
        /// </summary>
        private void SetEvaluator()
        {
            Evaluator = SelectedList ? Terms : Formulas;
            ButtonText = SelectedList ? "Добавить условие" : "Добавить формулу";
            _evaluatorManager.SetNodeEvaluators(CurrentNode);
            NodeEvaluators = new ObservableCollection<ConditionEvaluator>(_evaluatorManager.NodeEvaluators);

            OnPropertyChanged(nameof(NodeEvaluators));
            OnPropertyChanged(nameof(Evaluator));
        }

        /// <summary>
        /// Копирует выбранный элемент для редактирования и меняет текст кнопки.
        /// </summary>
        private void ChooseEvalutor()
        {
            CurrentEvaluator = SelectedEvaluator.Copy();
            ButtonText = SelectedList ? "Изменить условие" : "Изменить формулу";
        }

        /// <summary>
        /// Копирует переданный элемент и добавляет копию в текущую коллекцию Evaluator.
        /// </summary>
        /// <param name="parameter">Объект ConditionEvaluator для копирования</param>
        private void CopyEvalutor(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                var copyEvaluator = evaluator.Copy();
                copyEvaluator.Id = Guid.NewGuid().ToString();
                if (SelectedList)
                {
                    Terms.Add(copyEvaluator);
                }
                else
                {

                    Formulas.Add(copyEvaluator);
                }
                _nodeManager.NotifyChange();
            }
        }

        /// <summary>
        /// Удаляет переданный элемент из текущей коллекции Evaluator.
        /// Очищает текущий редактируемый элемент и обновляет списки.
        /// </summary>
        /// <param name="parameter">Объект ConditionEvaluator для удаления</param>
        private void RemoveEvalutor(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                if (SelectedList)
                {
                    var removeTerm = Terms.FirstOrDefault(term => term.Id.Equals(evaluator.Id));
                    Terms.Remove(removeTerm);
                }
                else
                {

                    var removeFormula = Formulas.FirstOrDefault(term => term.Id.Equals(evaluator.Id));
                    Formulas.Remove(removeFormula);
                }
                CurrentEvaluator = new ConditionEvaluator();
                _nodeManager.NotifyChange();
                UpdateLists();
            }
        }

        /// <summary>
        /// Проверяет возможность редактирования текущего элемента.
        /// Возвращает true, если Name и Value не пустые.
        /// </summary>
        private bool CanModifyEvalutor(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CurrentEvaluator.Name) && !string.IsNullOrWhiteSpace(CurrentEvaluator.Value);
        }

        /// <summary>
        /// Выполняет редактирование или добавление элемента в зависимости от выбранного элемента.
        /// </summary>
        private void ModifyEvalutor(object parameter)
        {
            if (SelectedEvaluator != null)
                EditEvalutor();
            else
                AddEvalutor();

            _evaluatorManager.SetNodeEvaluators(CurrentNode);
            _nodeManager.NotifyChange();
        }

        /// <summary>
        /// Добавляет текущий элемент в коллекцию Evaluator и очищает CurrentEvaluator.
        /// </summary>
        private void AddEvalutor()
        {
            if (SelectedList)
            {
                Terms.Add(CurrentEvaluator);
            }
            else
            {
                Formulas.Add(CurrentEvaluator);
            }
            CurrentEvaluator = new ConditionEvaluator();
            UpdateLists();
        }

        /// <summary>
        /// Обновляет выбранный элемент значениями из CurrentEvaluator, затем очищает текущий и обновляет списки.
        /// </summary>
        private void EditEvalutor()
        {
            SelectedEvaluator.SetValue(CurrentEvaluator);
            if (!SelectedEvaluator.EditName.Equals(SelectedEvaluator.Name))
            {
                SelectedEvaluator.EditName = SelectedEvaluator.Name.Trim();
            }
            if (SelectedList)
            {
                var editTerm = Terms.FirstOrDefault(term => term.Id.Equals(SelectedEvaluator.Id));
                editTerm.SetValue(SelectedEvaluator);
            }
            else
            {
                var editFormula = Formulas.FirstOrDefault(term => term.Id.Equals(SelectedEvaluator.Id));
                editFormula.SetValue(SelectedEvaluator);
            }
            SelectedEvaluator = null;
            CurrentEvaluator = new ConditionEvaluator();
            UpdateLists();
        }

        /// <summary>
        /// Проверяет возможность отмены редактирования (если текущий элемент валиден).
        /// </summary>
        private bool CanCancelModifyEvalutor(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CurrentEvaluator.Name) && !string.IsNullOrWhiteSpace(CurrentEvaluator.Value);
        }

        /// <summary>
        /// Отменяет редактирование: сбрасывает текст кнопки, очищает выбранный и текущий элементы.
        /// </summary>
        private void CancelModifyEvalutor(object parameter)
        {
            ButtonText = SelectedList ? "Добавить условие" : "Добавить формулу";
            SelectedEvaluator = null;
            CurrentEvaluator = new ConditionEvaluator();
        }

        /// <summary>
        /// Добавляет в значение текущего Evaluator заданную системную формулу (строку).
        /// </summary>
        /// <param name="parameter">Строка с формулой</param>
        private void SetSystemFormula(object parameter)
        {
            if (parameter is string evaluator)
            {
                CurrentEvaluator.Value += evaluator;
            }
        }

        private void SetMarking(object parameter)
        {
            if (parameter is string evaluator)
            {
                CurrentEvaluator.Value += $"[{evaluator}]";
            }
        }

        /// <summary>
        /// Добавляет ссылку на другой Evaluator в текущее значение и список частей, затем обновляет дерево частей.
        /// </summary>
        /// <param name="parameter">Объект ConditionEvaluator для вставки</param>
        private void SetCurrentEvaluator(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                CurrentEvaluator.Value += $"[{evaluator.Name}]";
                CurrentEvaluator.Parts.Add(evaluator.Id);
                CreatePartTree();
            }
        }

        /// <summary>
        /// Очищает список частей текущего Evaluator и обновляет дерево.
        /// </summary>
        private void ClearParts()
        {
            CurrentEvaluator.Parts.Clear();
            CreatePartTree();
        }

        /// <summary>
        /// Создает дерево частей для текущего Evaluator.
        /// </summary>
        private void CreatePartTree()
        {
            Parts = BuildTreeEvaluator(CurrentEvaluator);
            OnPropertyChanged(nameof(Parts));
        }

        /// <summary>
        /// Обновляет списки Evaluator в менеджере и вызывает обновление контекстного меню.
        /// </summary>
        private void UpdateLists()
        {
            _evaluatorManager.UpdateTemplateEvaluator();
            _evaluatorManager.SetNodeEvaluators(CurrentNode);
            NodeEvaluators = new ObservableCollection<ConditionEvaluator>(_evaluatorManager.NodeEvaluators);
            AllTemplateEvaluator = new ObservableCollection<ConditionEvaluator>(_evaluatorManager.AllTemplateEvaluator);

            _nodeManager.MenuHelper.UpdateContextMenuAsync();

            OnPropertyChanged(nameof(NodeEvaluators));
            OnPropertyChanged(nameof(AllTemplateEvaluator));
        }

        /// <summary>
        /// Рекурсивно строит дерево Evaluator для заданного ConditionEvaluator.
        /// </summary>
        /// <param name="evaluator">Исходный Evaluator</param>
        /// <returns>Дерево Evaluator</returns>
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
            catch (Exception ex)
            {
                // TODO: добавить логирование ошибки
                return null;
            }
        }

        private async void SetTableAsync()
        {
            var dialog = new TableChoiceDialog(_nodeManager.TableManager, _nodeManager.MenuHelper, CurrentEvaluator);
            await DialogHost.Show(dialog, "MainDialog");
        }
    }
}
