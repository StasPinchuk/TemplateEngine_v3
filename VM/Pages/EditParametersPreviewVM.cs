using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для предварительного редактирования параметров ConditionEvaluator.
    /// </summary>
    public class EditParametersPreviewVM : BaseNotifyPropertyChanged
    {
        private readonly IEvaluatorManager _evaluatorManager;
        private readonly ConditionEvaluator _editEvaluator;

        private ConditionEvaluator _currentEvaluator = new();

        /// <summary>
        /// Текущий редактируемый экземпляр ConditionEvaluator.
        /// </summary>
        public ConditionEvaluator CurrentEvaluator
        {
            get => _currentEvaluator;
            set
            {
                SetValue(ref _currentEvaluator, value, nameof(CurrentEvaluator));
                var tree = BuildTreeEvaluator(value);
                CurrentEvaluatorTree = new() { tree };
            }
        }

        private ObservableCollection<TreeEvaluator> _currentEvaluatorTree = [];

        /// <summary>
        /// Дерево условий, соответствующее текущему ConditionEvaluator.
        /// </summary>
        public ObservableCollection<TreeEvaluator> CurrentEvaluatorTree
        {
            get => _currentEvaluatorTree;
            set => SetValue(ref _currentEvaluatorTree, value, nameof(CurrentEvaluatorTree));
        }

        private TreeEvaluator _parts;

        /// <summary>
        /// Дерево частей для выбранного ConditionEvaluator.
        /// </summary>
        public TreeEvaluator Parts
        {
            get => _parts;
            set => SetValue(ref _parts, value, nameof(Parts));
        }

        private ConditionEvaluator _selectedEvaluator = new();

        /// <summary>
        /// Выбранный ConditionEvaluator, с которым работает пользователь.
        /// </summary>
        public ConditionEvaluator SelectedEvaluator
        {
            get => _selectedEvaluator;
            set
            {
                SetValue(ref _selectedEvaluator, value, nameof(SelectedEvaluator));
                Parts = BuildTreeEvaluator(value);
            }
        }

        /// <summary>
        /// Системные Evaluator'ы, доступные для выбора.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> SystemEvaluators { get; set; }

        /// <summary>
        /// Все Evaluator'ы шаблона, включая пользовательские и системные.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> AllTemplateEvaluator { get; set; }

        /// <summary>
        /// Evaluator'ы, связанные с узлами.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> NodeEvaluators { get; set; }

        /// <summary>
        /// Список всех маркировок шаблонов.
        /// </summary>
        public ObservableCollection<string> TemplateMarkings { get; set; }

        /// <summary>
        /// Команда вставки системной формулы в текущий SelectedEvaluator.
        /// </summary>
        public ICommand SetSystemFormulaCommand { get; set; }

        /// <summary>
        /// Команда применения изменений к Evaluator.
        /// </summary>
        public ICommand ModifyEvalutorCommand { get; set; }

        /// <summary>
        /// Команда вставки ссылки на другой Evaluator.
        /// </summary>
        public ICommand SetCurrentEvaluatorCommand { get; set; }

        /// <summary>
        /// Конструктор ViewModel редактирования параметров Evaluator.
        /// </summary>
        /// <param name="editEvaluator">Редактируемый Evaluator.</param>
        /// <param name="evaluatorManager">Менеджер Evaluator'ов.</param>
        /// <param name="drawerHost">Компонент DrawerHost (не используется напрямую).</param>
        public EditParametersPreviewVM(ConditionEvaluator editEvaluator, IEvaluatorManager evaluatorManager, DrawerHost drawerHost)
        {
            _evaluatorManager = evaluatorManager;

            foreach (var eval in _evaluatorManager.AllTemplateParameters)
            {
                if (eval.Id.Equals(editEvaluator.Id))
                    _editEvaluator = eval;
            }

            if (_editEvaluator != null)
                CurrentEvaluator = _editEvaluator.Copy();

            NodeEvaluators = _evaluatorManager.NodeEvaluators;
            SystemEvaluators = new ObservableCollection<ConditionEvaluator>(_evaluatorManager.SystemEvaluators);
            AllTemplateEvaluator = _evaluatorManager.AllTemplateEvaluator;
            TemplateMarkings = new ObservableCollection<string>(_evaluatorManager.TemplateMarkings);

            SetSystemFormulaCommand = new RelayCommand(SetSystemFormula);
            ModifyEvalutorCommand = new RelayCommand(ModifyEvalutor, CanModifyEvalutor);
            SetCurrentEvaluatorCommand = new RelayCommand(SetCurrentEvaluator);
        }

        /// <summary>
        /// Создает дерево условий на основе указанного Evaluator.
        /// </summary>
        /// <param name="evaluator">Evaluator, из которого строится дерево.</param>
        /// <returns>Дерево условий, либо null в случае ошибки.</returns>
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
                return null;
            }
        }

        /// <summary>
        /// Добавляет текстовое представление системной формулы к значению SelectedEvaluator.
        /// </summary>
        /// <param name="parameter">Имя формулы как строка.</param>
        private void SetSystemFormula(object parameter)
        {
            if (parameter is string evaluator)
            {
                SelectedEvaluator.Value += evaluator;
            }
        }

        /// <summary>
        /// Определяет, можно ли применить изменения к текущему Evaluator.
        /// </summary>
        /// <param name="parameter">Не используется.</param>
        /// <returns>true, если имя Evaluator задано; иначе — false.</returns>
        private bool CanModifyEvalutor(object parameter)
        {
            return !string.IsNullOrEmpty(SelectedEvaluator.Name);
        }

        /// <summary>
        /// Применяет изменения к Evaluator.
        /// </summary>
        /// <param name="parameter">Не используется.</param>
        private void ModifyEvalutor(object parameter)
        {
            SelectedEvaluator = new();
            _editEvaluator.SetValue(CurrentEvaluatorTree.FirstOrDefault()?.ConditionEvaluator);
        }

        /// <summary>
        /// Добавляет ссылку на другой Evaluator в текущий SelectedEvaluator и обновляет дерево.
        /// </summary>
        /// <param name="parameter">Evaluator, на который создается ссылка.</param>
        private void SetCurrentEvaluator(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                SelectedEvaluator.Value += $"[{evaluator.Name}]";
                SelectedEvaluator.Parts.Add(evaluator.Id);
                CreatePartTree();
            }
        }

        /// <summary>
        /// Обновляет дерево Parts и CurrentEvaluatorTree для текущего SelectedEvaluator.
        /// </summary>
        private void CreatePartTree()
        {
            Parts = BuildTreeEvaluator(SelectedEvaluator);
            var tree = BuildTreeEvaluator(_editEvaluator);
            CurrentEvaluatorTree = new() { tree };
            OnPropertyChanged(nameof(Parts));
            OnPropertyChanged(nameof(CurrentEvaluatorTree));
        }
    }
}
