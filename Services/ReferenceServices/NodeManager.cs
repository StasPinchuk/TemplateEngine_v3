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
    public class NodeManager : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Менеджер вычислителей, связанный с текущим узлом.
        /// </summary>
        public EvaluatorManager EvaluatorManager { get; set; }

        public TableService TableManager { get; set; }

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
        public event Action EvaluatorChanged;

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
                    EvaluatorManager?.SetNodeEvaluators(_currentNode);
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
            return MenuHelper?.GetContextMenu();
        }

        public void NotifyChange()
        {
            EvaluatorChanged?.Invoke(); 
        }

        public void ClearAction()
        {
            CurrentNodeChanged = null;
            EvaluatorChanged = null;
        }
    }
}
