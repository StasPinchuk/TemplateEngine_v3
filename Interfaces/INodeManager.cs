using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для управления узлами и связанной с ними логикой.
    /// </summary>
    public interface INodeManager
    {
        /// <summary>
        /// Менеджер оценщиков условий, связанных с узлами.
        /// </summary>
        IEvaluatorManager EvaluatorManager { get; set; }
        TableService TableManager { get; set; }

        /// <summary>
        /// Помощник для работы с контекстным меню.
        /// </summary>
        ContextMenuHelper MenuHelper { get; set; }

        /// <summary>
        /// Текущий выбранный узел.
        /// </summary>
        Node CurrentNode { get; set; }

        /// <summary>
        /// Событие, вызываемое при изменении текущего узла.
        /// </summary>
        event Action<Node> CurrentNodeChanged;
        event Action EvaluatorChanged;

        /// <summary>
        /// Коллекция всех узлов.
        /// </summary>
        ObservableCollection<Node> Nodes { get; set; }

        /// <summary>
        /// Получает контекстное меню, связанное с узлами.
        /// </summary>
        /// <returns>Контекстное меню.</returns>
        ContextMenu GetContextMenu();

        void NotifyChange();
    }
}
