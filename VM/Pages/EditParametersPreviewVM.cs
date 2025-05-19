using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.VM.Pages
{
    public class EditParametersPreviewVM : BaseNotifyPropertyChanged
    {
        private readonly IEvaluatorManager _evaluatorManager;
        private ConditionEvaluator _editEvaluator;

        private ConditionEvaluator _currentEvaluator = new();
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
        public ObservableCollection<TreeEvaluator> CurrentEvaluatorTree
        {
            get => _currentEvaluatorTree;
            set => SetValue(ref _currentEvaluatorTree, value, nameof(CurrentEvaluatorTree));
        }

        private TreeEvaluator _parts;
        public TreeEvaluator Parts
        {
            get => _parts;
            set => SetValue(ref _parts, value, nameof(Parts));
        }

        private ConditionEvaluator _selectedEvaluator = new();
        public ConditionEvaluator SelectedEvaluator
        {
            get => _selectedEvaluator;
            set
            {
                SetValue(ref _selectedEvaluator, value, nameof(SelectedEvaluator));
                Parts = BuildTreeEvaluator(value);
            }
        }

        public ObservableCollection<ConditionEvaluator> SystemEvaluators { get; set; }
        public ObservableCollection<ConditionEvaluator> AllTemplateEvaluator { get; set; }
        public ObservableCollection<ConditionEvaluator> NodeEvaluators { get; set; }
        public ObservableCollection<string> TemplateMarkings { get; set; }

        public ICommand SetSystemFormulaCommand { get; set; }
        public ICommand ModifyEvalutorCommand { get; set; }
        public ICommand SetCurrentEvaluatorCommand { get; set; }

        public EditParametersPreviewVM(ConditionEvaluator editEvaluator, IEvaluatorManager evaluatorManager, DrawerHost drawerHost) 
        {
            _evaluatorManager = evaluatorManager;
            foreach(var eval in _evaluatorManager.AllTemplateParameters)
            {
                if(eval.Id.Equals(editEvaluator.Id))
                    _editEvaluator = eval;
            }
            foreach(var eval in _evaluatorManager.AllTemplateParameters)
            {
                if(eval.Id.Equals(editEvaluator.Id))
                    _editEvaluator = eval;
            }
            if(_editEvaluator != null)
                CurrentEvaluator = _editEvaluator.Copy();

            NodeEvaluators = _evaluatorManager.NodeEvaluators;
            SystemEvaluators = new ObservableCollection<ConditionEvaluator>(_evaluatorManager.SystemEvaluators);
            AllTemplateEvaluator = _evaluatorManager.AllTemplateEvaluator;
            TemplateMarkings = new ObservableCollection<string>(_evaluatorManager.TemplateMarkings);
            SetSystemFormulaCommand = new RelayCommand(SetSystemFormula);
            ModifyEvalutorCommand = new RelayCommand(ModifyEvalutor, CanModifyEvalutor);
            SetCurrentEvaluatorCommand = new RelayCommand(SetCurrentEvaluator);
        }

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

        private void SetSystemFormula(object parameter)
        {
            if (parameter is string evaluator)
            {
                SelectedEvaluator.Value += evaluator;
            }
        }

        private bool CanModifyEvalutor(object parameter)
        {
            return !string.IsNullOrEmpty(SelectedEvaluator.Name);
        }

        private void ModifyEvalutor(object parameter)
        {
            SelectedEvaluator = new();
            _editEvaluator.SetValue(CurrentEvaluatorTree.FirstOrDefault().ConditionEvaluator);
        }

        /// <summary>
        /// Добавляет ссылку на другой Evaluator в текущее значение и список частей, затем обновляет дерево частей.
        /// </summary>
        /// <param name="parameter">Объект ConditionEvaluator для вставки</param>
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
        /// Создает дерево частей для текущего Evaluator.
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
