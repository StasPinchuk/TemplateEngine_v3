using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для страницы с основной информацией узла.
    /// Управляет текущим узлом, его типом и связанными действиями.
    /// </summary>
    public class MainNodePageVM : BaseNotifyPropertyChanged, IDisposable

    {
        private string _lastNodeName = string.Empty;

        /// <summary>
        /// Текущий узел, получаемый из менеджера узлов.
        /// </summary>
        public Node CurrentNode
        {
            get
            {
                _lastNodeName = _nodeManager.CurrentNode.Name;
                return _nodeManager.CurrentNode;
            }
        }

        /// <summary>
        /// Список доступных типов узлов.
        /// Используется для отображения в ComboBox или аналогичном элементе управления.
        /// </summary>
        public ObservableCollection<string> NodeTypes => NodeTypeManager.NodeTypes;

        private readonly Action _updatePage;
        private readonly Action _updateNodeGroup;
        private readonly NodeManager _nodeManager;

        private string _nodeType = string.Empty;

        /// <summary>
        /// Тип узла, отображаемый и редактируемый через интерфейс.
        /// При изменении автоматически обновляет текущий узел и вызывает обновление страницы и группы узлов.
        /// </summary>
        public string NodeType
        {
            get => _nodeType;
            set
            {
                if (SetValue(ref _nodeType, value, nameof(NodeType)))
                {
                    if (CurrentNode != null && CurrentNode.Type != value)
                    {
                        bool sameName = CurrentNode.Name.Equals(_lastNodeName);

                        _updatePage?.Invoke();
                        CurrentNode.Type = value;

                        if (sameName)
                        {
                            _updateNodeGroup?.Invoke();
                        }

                        // Обновляем имя в любом случае после смены типа
                        _lastNodeName = CurrentNode.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Конструктор ViewModel страницы основного узла.
        /// </summary>
        /// <param name="nodeManager">Менеджер узлов, управляющий текущим состоянием.</param>
        /// <param name="updatePage">Действие, вызываемое при необходимости обновления страницы.</param>
        /// <param name="updateNodeGroup">Действие, вызываемое при необходимости обновления группы узлов.</param>
        public MainNodePageVM(NodeManager nodeManager, Action updatePage, Action updateNodeGroup)
        {
            _nodeManager = nodeManager;
            _updatePage = updatePage;
            _updateNodeGroup = updateNodeGroup;

            _nodeManager.CurrentNodeChanged += OnCurrentNodeChanged;

            if (CurrentNode != null)
                NodeType = CurrentNode.Type;
        }

        /// <summary>
        /// Обработчик события изменения текущего узла.
        /// Обновляет свойство CurrentNode и синхронизирует NodeType.
        /// </summary>
        /// <param name="node">Новый текущий узел.</param>
        private void OnCurrentNodeChanged(Node node)
        {
            OnPropertyChanged(nameof(CurrentNode));
            NodeType = node?.Type ?? string.Empty;
        }

        public void Dispose()
        {
            _nodeManager.CurrentNodeChanged -= OnCurrentNodeChanged;
        }

        public async Task<ContextMenu> GetContextMenu()
        {
            await _nodeManager.MenuHelper.UpdateContextMenuAsync();
            return _nodeManager.GetContextMenu();
        }
    }
}
