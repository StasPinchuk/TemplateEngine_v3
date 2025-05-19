using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.VM.Pages
{
    public class MainNodePageVM : BaseNotifyPropertyChanged
    {
        public Node CurrentNode => _nodeManager.CurrentNode;

        public ObservableCollection<string> NodeTypes => NodeTypeManager.NodeTypes;

        private readonly Action _updatePage;
        private readonly Action _updateNodeGroup;

        private readonly INodeManager _nodeManager;

        private string _nodeType = string.Empty;
        public string NodeType
        {
            get => _nodeType;
            set
            {
                SetValue(ref _nodeType, value, nameof(NodeType));
                CurrentNode.Type = value;
                _updatePage?.Invoke();

                _updateNodeGroup?.Invoke();
            }
        }

        public ContextMenu TextBoxMenu => _nodeManager.MenuHelper.GetContextMenu();

        public MainNodePageVM(INodeManager nodeManager, Action updatePage, Action updateNodeGroup)
        {
            _nodeManager = nodeManager;
            _updatePage = updatePage;
            _updateNodeGroup = updateNodeGroup;
            if (CurrentNode != null)
                NodeType = CurrentNode?.Type;
            _nodeManager.CurrentNodeChanged += OnCurrentNodeChanged;
        }

        private void OnCurrentNodeChanged(Node node)
        {
            OnPropertyChanged(nameof(CurrentNode)); // <- важно!
            NodeType = node.Type;
        }
    }
}
