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
    public class EditOperationPreviewEditVM : BaseNotifyPropertyChanged
    {
        private readonly Operation _editOperation;
        private readonly IEvaluatorManager _evaluatorManager;
        private readonly IBranchManager _branchManager;
        private readonly DrawerHost _drawer;
        private BranchDivisionDetails _currentBranchDivision;

        public BranchDivisionDetails CurrentBranchDivision
        {
            get => _currentBranchDivision; set => SetValue(ref _currentBranchDivision, value, nameof(CurrentBranchDivision));
        }

        private ObservableCollection<TreeEvaluator> _parts = new ObservableCollection<TreeEvaluator>();
        public ObservableCollection<TreeEvaluator> Parts
        {
            get => _parts;
            set => SetValue(ref _parts, value, nameof(Parts));
        }

        private readonly ContextMenuHelper _contextMenuHelper;

        public ContextMenu TextBoxContextMenu => _contextMenuHelper.GetContextMenu();

        public ObservableCollection<BranchDivisionDetails> BranchDivision { get; set; }

        public ICommand EditOperationCommand => new RelayCommand(EditOperation);

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

        private void EditOperation()
        {
            if (CurrentBranchDivision != null)
            {
                _editOperation.BranchDivisionDetails = BranchDivision;
            }

            DrawerHost.CloseDrawerCommand.Execute(Dock.Right, _drawer);
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
            catch (Exception)
            {
                return null;
            }
        }

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

        public void ClearParts()
        {
            Parts.Clear();
        }
    }
}
