using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Менеджер для работы с узлами (Node) шаблона.
    /// Управляет текущим выбранным узлом и коллекцией узлов, а также связывает с EvaluatorManager и ContextMenuHelper.
    /// </summary>
    public class NodeManager : INodeManager
    {
        /// <summary>
        /// Менеджер вычислителей, связанный с текущим узлом.
        /// </summary>
        public IEvaluatorManager EvaluatorManager { get; set; }

        /// <summary>
        /// Помощник для создания контекстного меню.
        /// </summary>
        public ContextMenuHelper MenuHelper { get; set; }

        /// <summary>
        /// Коллекция узлов (Node), которыми управляет этот менеджер.
        /// </summary>
        public ObservableCollection<Node> Nodes { get; set; } = new ObservableCollection<Node>();

        /// <summary>
        /// Событие, вызываемое при изменении текущего узла.
        /// Передаёт новый текущий узел.
        /// </summary>
        public event Action<Node> CurrentNodeChanged;

        private Node _currentNode;

        /// <summary>
        /// Текущий выбранный узел.
        /// При изменении вызывает обновление вычислителей в EvaluatorManager и событие CurrentNodeChanged.
        /// </summary>
        public Node CurrentNode
        {
            get => _currentNode;
            set
            {
                if (_currentNode != value)
                {
                    _currentNode = value;
                    // Обновляем вычислители для нового текущего узла
                    EvaluatorManager?.SetNodeEvaluators(_currentNode);
                    // Уведомляем подписчиков, что текущий узел изменился
                    CurrentNodeChanged?.Invoke(_currentNode);
                }
            }
        }

        /// <summary>
        /// Получить копию контекстного меню, созданного MenuHelper.
        /// Возвращает null, если MenuHelper не инициализирован.
        /// </summary>
        /// <returns>Объект ContextMenu или null.</returns>
        public ContextMenu GetContextMenu()
        {
            return MenuHelper?.GetContextMenuCopy();
        }
    }
}
