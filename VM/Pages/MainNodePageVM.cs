using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.VM.Pages
{
    public class MainNodePageVM : BaseNotifyPropertyChanged
    {
        public Node CurrentNode => _nodeManager?.CurrentNode;

        public List<string> NodeTypes => NodeTypeManager.NodeTypes;

        private Action _updatePage;

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
            }
        }

        public ContextMenu TextBoxMenu => _nodeManager.MenuHelper.GetContextMenuCopy();

        public MainNodePageVM(INodeManager nodeManager, Action updatePage)
        {
            _nodeManager = nodeManager;
            _updatePage = updatePage;
            if(CurrentNode != null)
                NodeType = CurrentNode?.Type;
            _nodeManager.CurrentNodeChanged += OnCurrentNodeChanged;
        }

        private void OnCurrentNodeChanged(Node node)
        {
            OnPropertyChanged(nameof(CurrentNode));
            NodeType = CurrentNode.Type;
        }
    }
}
