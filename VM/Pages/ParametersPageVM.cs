using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Common;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel страницы параметров для управления условными оценщиками и их деревьями.
    /// </summary>
    public class ParametersPageVM : BaseNotifyPropertyChanged
    {
        private ConditionEvaluator _selectedEvaluator = null;

        /// <summary>
        /// Выбранный оценщик условия для редактирования.
        /// </summary>
        public ConditionEvaluator SelectedEvaluator
        {
            get => _selectedEvaluator;
            set
            {
                SetValue(ref _selectedEvaluator, value, nameof(SelectedEvaluator));
                if (value != null)
                    ChooseEvalutor();
            }
        }

        private ConditionEvaluator _currentEvaluator = new();

        /// <summary>
        /// Текущий оцениваемый параметр, используемый для создания дерева и редактирования.
        /// </summary>
        public ConditionEvaluator CurrentEvaluator
        {
            get => _currentEvaluator;
            set
            {
                SetValue(ref _currentEvaluator, value, nameof(CurrentEvaluator));
                CreatePartTree();
            }
        }

        private TreeEvaluator _treeEvaluator = new();

        /// <summary>
        /// Дерево оценщика условий, отображаемое в UI.
        /// </summary>
        public TreeEvaluator Parts
        {
            get => _treeEvaluator;
            set => SetValue(ref _treeEvaluator, value, nameof(Parts));
        }

        private string _buttonText = "Добавить параметр";

        /// <summary>
        /// Текст кнопки для добавления или изменения параметра.
        /// </summary>
        public string ButtonText
        {
            get => _buttonText; set => SetValue(ref _buttonText, value, nameof(ButtonText));
        }

        private readonly INodeManager _nodeManager;
        private readonly IEvaluatorManager _evaluatorManager;

        /// <summary>
        /// Коллекция условных оценщиков, привязанных к текущему узлу.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> Evaluator { get; set; } = [];

        /// <summary>
        /// Коллекция системных оценщиков условий.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> SystemEvaluators { get; set; }

        /// <summary>
        /// Все оценщики, доступные в шаблоне.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> AllTemplateEvaluator { get; set; }

        /// <summary>
        /// Оценщики, связанные с текущим узлом.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> NodeEvaluators { get; set; }

        /// <summary>
        /// Коллекция строковых маркировок шаблона.
        /// </summary>
        public ObservableCollection<string> TemplateMarkings { get; set; }

        private string _filterAllTemplateEvaluator = string.Empty;

        public string FilterAllTemplateEvaluators
        {
            get => _filterAllTemplateEvaluator;
            set
            {
                SetValue(ref _filterAllTemplateEvaluator, value, nameof(FilterAllTemplateEvaluators));

                Task.Run(() =>
                {
                    ObservableCollection<ConditionEvaluator> filtered = string.IsNullOrEmpty(value)
                      ? new ObservableCollection<ConditionEvaluator>(_evaluatorManager.AllTemplateEvaluator)
                      : new ObservableCollection<ConditionEvaluator>(_evaluatorManager.AllTemplateEvaluator
                        .Where(filterObject => filterObject.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
                        .ToList());

                    // Обновляем коллекцию на UI-потоке
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AllTemplateEvaluator.Clear();
                        foreach (var item in filtered)
                            AllTemplateEvaluator.Add(item);

                    });
                });
            }

        }

        /// <summary>
        /// Команды для взаимодействия с параметрами.
        /// </summary>
        public ICommand CopyEvalutorCommand { get; set; }
        public ICommand RemoveEvalutorCommand { get; set; }
        public ICommand ModifyEvalutorCommand { get; set; }
        public ICommand CancelModifyEvalutorCommand { get; set; }
        public ICommand SetSystemFormulaCommand { get; set; }
        public ICommand SetCurrentEvaluatorCommand { get; set; }
        public ICommand SetMarkingCommand { get; set; }

        /// <summary>
        /// Конструктор, инициализирующий VM и подписывающийся на события изменения узла и оценщиков.
        /// </summary>
        /// <param name="nodeManager">Менеджер узлов для получения текущего узла и его параметров.</param>
        public ParametersPageVM(INodeManager nodeManager)
        {
            _nodeManager = nodeManager;
            Evaluator = nodeManager.CurrentNode.Parameters;

            nodeManager.CurrentNodeChanged += OnCurrentNodeChanged;
            nodeManager.EvaluatorChanged += OnNodeChanged;

            _evaluatorManager = nodeManager.EvaluatorManager;
            _evaluatorManager.SetNodeEvaluators(_nodeManager.CurrentNode);
            SystemEvaluators = new(_evaluatorManager.SystemEvaluators);
            AllTemplateEvaluator = new(_evaluatorManager.AllTemplateEvaluator);
            NodeEvaluators = new(_evaluatorManager.NodeEvaluators);
            TemplateMarkings = new ObservableCollection<string>(_evaluatorManager.TemplateMarkings);

            InitializeCommand();
        }

        /// <summary>
        /// Инициализация команд.
        /// </summary>
        private void InitializeCommand()
        {
            CopyEvalutorCommand = new RelayCommand(CopyEvalutor);
            RemoveEvalutorCommand = new RelayCommand(RemoveEvalutor);
            ModifyEvalutorCommand = new RelayCommand(ModifyEvalutor, CanModifyEvalutor);
            CancelModifyEvalutorCommand = new RelayCommand(CancelModifyEvalutor, CanCancelModifyEvalutor);
            SetSystemFormulaCommand = new RelayCommand(SetSystemFormula);
            SetCurrentEvaluatorCommand = new RelayCommand(SetCurrentEvaluator);
            SetMarkingCommand = new RelayCommand(SetMarking);
        }

        /// <summary>
        /// Обработчик смены текущего узла.
        /// </summary>
        /// <param name="node">Новый текущий узел.</param>
        private void OnCurrentNodeChanged(Node node)
        {
            if(node != null)
            {
                Evaluator = node.Parameters;

                _evaluatorManager.SetNodeEvaluators(node);
                NodeEvaluators = new(_evaluatorManager.NodeEvaluators);
                OnPropertyChanged(nameof(NodeEvaluators));
                OnPropertyChanged(nameof(Evaluator));

            }
        }

        /// <summary>
        /// Обработчик изменения данных оценщиков.
        /// </summary>
        private void OnNodeChanged()
        {
            _evaluatorManager.SetNodeEvaluators(_nodeManager.CurrentNode);
            NodeEvaluators = new ObservableCollection<ConditionEvaluator>(_evaluatorManager.NodeEvaluators);
            OnPropertyChanged(nameof(NodeEvaluators));
        }

        /// <summary>
        /// Выбор оценщика для редактирования.
        /// </summary>
        private void ChooseEvalutor()
        {
            CurrentEvaluator = SelectedEvaluator.Copy();
            ButtonText = "Изменить параметр";
        }

        /// <summary>
        /// Копирование оценщика в список.
        /// </summary>
        /// <param name="parameter">Оценщик для копирования.</param>
        private void CopyEvalutor(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                var copyEvaluator = evaluator.Copy();
                copyEvaluator.Id = Guid.NewGuid().ToString();
                Evaluator.Add(copyEvaluator);
                _nodeManager.NotifyChange();
            }
        }

        /// <summary>
        /// Удаление выбранного оценщика.
        /// </summary>
        /// <param name="parameter">Оценщик для удаления.</param>
        private void RemoveEvalutor(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                Evaluator.Remove(evaluator);
                CurrentEvaluator = new();
                UpdateLists();
                _nodeManager.NotifyChange();
            }
        }

        /// <summary>
        /// Проверка, можно ли изменить оценщика (валидность данных).
        /// </summary>
        /// <param name="parameter">Параметры команды (не используются).</param>
        /// <returns>True, если имя и значение заполнены.</returns>
        private bool CanModifyEvalutor(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CurrentEvaluator.Name) && !string.IsNullOrWhiteSpace(CurrentEvaluator.Value);
        }

        /// <summary>
        /// Изменение или добавление оценщика.
        /// </summary>
        /// <param name="parameter">Параметры команды (не используются).</param>
        private void ModifyEvalutor(object parameter)
        {
            if (SelectedEvaluator != null)
                EditEvalutor();
            else
                AddEvalutor();
            _nodeManager.NotifyChange();
        }

        /// <summary>
        /// Добавление нового оценщика.
        /// </summary>
        private void AddEvalutor()
        {
            Evaluator?.Add(CurrentEvaluator);
            CurrentEvaluator = new();
            UpdateLists();
        }

        /// <summary>
        /// Редактирование выбранного оценщика.
        /// </summary>
        private void EditEvalutor()
        {
            SelectedEvaluator.SetValue(CurrentEvaluator);
            if (!SelectedEvaluator.EditName.Equals(SelectedEvaluator.Name))
            {
                SelectedEvaluator.EditName = SelectedEvaluator.Name;
            }
            SelectedEvaluator = null;
            CurrentEvaluator = new();
            UpdateLists();
        }

        /// <summary>
        /// Проверка возможности отмены изменений оценщика.
        /// </summary>
        /// <param name="parameter">Параметры команды (не используются).</param>
        /// <returns>True, если имя и значение заполнены.</returns>
        private bool CanCancelModifyEvalutor(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CurrentEvaluator.Name) && !string.IsNullOrWhiteSpace(CurrentEvaluator.Value);
        }

        /// <summary>
        /// Отмена редактирования оценщика.
        /// </summary>
        /// <param name="parameter">Параметры команды (не используются).</param>
        private void CancelModifyEvalutor(object parameter)
        {
            ButtonText = "Добавить параметр";
            SelectedEvaluator = null;
            CurrentEvaluator = new();
            ClearParts();
        }

        /// <summary>
        /// Добавление системной формулы к значению текущего оценщика.
        /// </summary>
        /// <param name="parameter">Строка формулы.</param>
        private void SetSystemFormula(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                CurrentEvaluator.Value += evaluator.Value;
            }
        }

        /// <summary>
        /// Добавление ссылки на текущий оценщик в значение и его частей.
        /// </summary>
        /// <param name="parameter">Оценщик, который добавляется.</param>
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
        /// Добавление маркировки к значению текущего оценщика.
        /// </summary>
        /// <param name="parameter">Строка маркировки.</param>
        private void SetMarking(object parameter)
        {
            if (parameter is string evaluator)
            {
                CurrentEvaluator.Value += $"[{evaluator}]";
            }
        }

        /// <summary>
        /// Очистка частей текущего оценщика и обновление дерева.
        /// </summary>
        private void ClearParts()
        {
            CurrentEvaluator.Parts.Clear();
            CreatePartTree();
        }

        /// <summary>
        /// Создание дерева из текущего оценщика для отображения в UI.
        /// </summary>
        private void CreatePartTree()
        {
            Parts = BuildTreeEvaluator(CurrentEvaluator);
            OnPropertyChanged(nameof(Parts));
        }

        /// <summary>
        /// Обновление списков оценщиков из менеджера.
        /// </summary>
        private void UpdateLists()
        {
            _evaluatorManager.UpdateTemplateEvaluator();
            _evaluatorManager.SetNodeEvaluators(_nodeManager.CurrentNode);
            NodeEvaluators = new(_evaluatorManager.NodeEvaluators);
            AllTemplateEvaluator = new(_evaluatorManager.AllTemplateEvaluator);

            OnPropertyChanged(nameof(NodeEvaluators));
            OnPropertyChanged(nameof(AllTemplateEvaluator));
        }

        /// <summary>
        /// Рекурсивное построение дерева оценщиков из заданного оценщика.
        /// </summary>
        /// <param name="evaluator">Оценщик, для которого строится дерево.</param>
        /// <returns>Дерево оценщика или null при ошибке.</returns>
        private TreeEvaluator BuildTreeEvaluator(ConditionEvaluator evaluator)
        {
            try
            {
                if (evaluator == null)
                    return null;

                var treeEvaluator = new TreeEvaluator { ConditionEvaluator = evaluator };

                if (evaluator.Parts.Count > 0)
                {
                    List<ConditionEvaluator> conditions = _evaluatorManager.AllTemplateEvaluator
                        .Where(cond => evaluator.Parts.Contains(cond.Id))
                        .GroupBy(p => p.Id)
                        .Select(group => group.First())
                        .ToList();

                    foreach (var condition in conditions)
                    {
                        treeEvaluator.TreeEvaluators.Add(BuildTreeEvaluator(condition));
                    }
                }

                return treeEvaluator;
            }
            catch (Exception)
            {
                // Логирование ошибки при необходимости
            }

            return null;
        }
    }
}
