using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.VM.Pages
{
    public class ParametersPageVM : BaseNotifyPropertyChanged
    {
        private ConditionEvaluator _selectedEvaluator = new();
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
        public TreeEvaluator Parts
        {
            get => _treeEvaluator;
            set => SetValue(ref _treeEvaluator, value, nameof(Parts));
        }

        private string _buttonText = "Добавить параметр";

        public string ButtonText
        {
            get => _buttonText; set => SetValue(ref _buttonText, value, nameof(ButtonText));
        }

        private readonly INodeManager _nodeManager;
        private readonly IEvaluatorManager _evaluatorManager;

        public ObservableCollection<ConditionEvaluator> Evaluator { get; set; } = [];
        public ObservableCollection<ConditionEvaluator> SystemEvaluators { get; set; }
        public ObservableCollection<ConditionEvaluator> AllTemplateEvaluator { get; set; }
        public ObservableCollection<ConditionEvaluator> NodeEvaluators { get; set; }

        public ICommand CopyEvalutorCommand { get; set; }
        public ICommand RemoveEvalutorCommand { get; set; }
        public ICommand ModifyEvalutorCommand { get; set; }
        public ICommand CancelModifyEvalutorCommand { get; set; }
        public ICommand SetSystemFormulaCommand { get; set; }
        public ICommand SetCurrentEvaluatorCommand { get; set; }

        public ParametersPageVM(INodeManager nodeManager)
        {
            _nodeManager = nodeManager;
            Evaluator = nodeManager.CurrentNode.Parameters;

            nodeManager.CurrentNodeChanged += OnCurrentNodeChanged;
            _evaluatorManager = nodeManager.EvaluatorManager;
            SystemEvaluators = new(_evaluatorManager.SystemEvaluators);
            AllTemplateEvaluator = new(_evaluatorManager.AllTemplateEvaluator);
            NodeEvaluators = new(_evaluatorManager.NodeEvaluators);

            CopyEvalutorCommand = new RelayCommand(CopyEvalutor);
            RemoveEvalutorCommand = new RelayCommand(RemoveEvalutor);
            ModifyEvalutorCommand = new RelayCommand(ModifyEvalutor, CanModifyEvalutor);
            CancelModifyEvalutorCommand = new RelayCommand(CancelModifyEvalutor, CanCancelModifyEvalutor);
            SetSystemFormulaCommand = new RelayCommand(SetSystemFormula);
            SetCurrentEvaluatorCommand = new RelayCommand(SetCurrentEvaluator);
        }

        private void OnCurrentNodeChanged(Node node)
        {
            Evaluator = node.Parameters;

            _evaluatorManager.SetNodeEvaluators(node);
            NodeEvaluators = new(_evaluatorManager.NodeEvaluators);
            OnPropertyChanged(nameof(NodeEvaluators));
            OnPropertyChanged(nameof(Evaluator));
        }

        private void ChooseEvalutor()
        {
            CurrentEvaluator = SelectedEvaluator.Copy();
            ButtonText = "Изменить параметр";
        }

        private void CopyEvalutor(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                var copyEvaluator = evaluator.Copy();
                copyEvaluator.Id = Guid.NewGuid().ToString();
                Evaluator.Add(copyEvaluator);
            }
        }

        private void RemoveEvalutor(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                Evaluator.Remove(evaluator);
                CurrentEvaluator = new();
                UpdateLists();
            }
        }

        private bool CanModifyEvalutor(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CurrentEvaluator.Name) && !string.IsNullOrWhiteSpace(CurrentEvaluator.Value);
        }

        private void ModifyEvalutor(object parameter)
        {
            if (SelectedEvaluator != null)
                EditEvalutor();
            else
                AddEvalutor();
        }

        private void AddEvalutor()
        {
            Evaluator?.Add(CurrentEvaluator);
            CurrentEvaluator = new();
            UpdateLists();
        }

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

        private bool CanCancelModifyEvalutor(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CurrentEvaluator.Name) && !string.IsNullOrWhiteSpace(CurrentEvaluator.Value);
        }

        private void CancelModifyEvalutor(object parameter)
        {
            ButtonText = "Добавить параметр";
            SelectedEvaluator = null;
            CurrentEvaluator = new();
            ClearParts();
        }

        private void SetSystemFormula(object parameter)
        {
            if (parameter is string evaluator)
            {
                CurrentEvaluator.Value += evaluator;
            }
        }

        private void SetCurrentEvaluator(object parameter)
        {
            if (parameter is ConditionEvaluator evaluator)
            {
                CurrentEvaluator.Value += $"[{evaluator.Name}]";
                CurrentEvaluator.Parts.Add(evaluator.Id);
                CreatePartTree();
            }
        }

        private void ClearParts()
        {
            CurrentEvaluator.Parts.Clear();
            CreatePartTree();
        }

        private void CreatePartTree()
        {
            Parts = BuildTreeEvaluator(CurrentEvaluator);
            OnPropertyChanged(nameof(Parts));
        }

        private void UpdateLists()
        {
            _evaluatorManager.UpdateTemplateEvaluator();
            _evaluatorManager.SetNodeEvaluators(_nodeManager.CurrentNode);
            NodeEvaluators = new(_evaluatorManager.NodeEvaluators);
            AllTemplateEvaluator = new(_evaluatorManager.AllTemplateEvaluator);

            OnPropertyChanged(nameof(NodeEvaluators));
            OnPropertyChanged(nameof(AllTemplateEvaluator));
        }

        private TreeEvaluator BuildTreeEvaluator(ConditionEvaluator evaluator)
        {
            try
            {
                if(evaluator == null)
                    return null;
                var treeEvaluator = new TreeEvaluator { ConditionEvaluator = evaluator };

                if (evaluator.Parts.Count > 0)
                {
                    List<ConditionEvaluator> conditions = _evaluatorManager.AllTemplateEvaluator.Where(cond => evaluator.Parts.Contains(cond.Id)).ToList();
                    conditions = conditions.GroupBy(p => p.Id)
                                           .Select(group => group.First())
                                           .ToList();
                    foreach (var condition in conditions)
                    {
                        treeEvaluator.TreeEvaluators.Add(BuildTreeEvaluator(condition));
                    }
                }

                return treeEvaluator;

            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
