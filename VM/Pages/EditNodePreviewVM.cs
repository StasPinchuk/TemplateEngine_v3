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
    public class EditNodePreviewVM : BaseNotifyPropertyChanged
    {
        private readonly Operation _editOperation;
        private readonly IEvaluatorManager _evaluatorManager;
        private readonly IBranchManager _branchManager;
        private readonly DrawerHost _drawer;
        private readonly ContextMenuHelper _contextMenuHelper;

        private readonly Node _editNode;
        private Node _currentNode;
        public Node CurrentNode
        {
            get => _currentNode; set => SetValue(ref _currentNode, value, nameof(CurrentNode));
        }

        private string _nodeType = string.Empty;
        public string NodeType
        {
            get => _nodeType;
            set
            {
                SetValue(ref _nodeType, value, nameof(NodeType));
                CurrentNode.Type = value;
            }
        }

        private ObservableCollection<TreeEvaluator> _parts = new ObservableCollection<TreeEvaluator>();
        public ObservableCollection<TreeEvaluator> Parts
        {
            get => _parts;
            set => SetValue(ref _parts, value, nameof(Parts));
        }

        public ObservableCollection<string> NodeTypes => NodeTypeManager.NodeTypes;

        public ContextMenu TextBoxContextMenu => _contextMenuHelper.GetContextMenu();

        public ICommand EditNodeCommand { get; set; }

        public EditNodePreviewVM(Node node, IEvaluatorManager evaluatorManager, ContextMenuHelper contextMenuHelper, DrawerHost drawerHost)
        {
            _editNode = node;
            CurrentNode = node.Copy();
            NodeType = node.Type;
            _evaluatorManager = evaluatorManager;
            _contextMenuHelper = contextMenuHelper;
            _drawer = drawerHost;

            EditNodeCommand = new RelayCommand(EditNode);
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
            if (!_evaluatorManager.AllTemplateEvaluator.Any(eval => eval.Id.Equals(evaluator.Id)))
                return;
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

        private void EditNode()
        {
            _editNode.SetValue(CurrentNode);
            DrawerHost.CloseDrawerCommand.Execute(Dock.Right, _drawer);
        }
    }
}
