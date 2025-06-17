using System.Collections.ObjectModel;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для предварительного просмотра узлов (Nodes) из заданных отношений шаблона.
    /// </summary>
    public class NodePreviewVM
    {
        private readonly TemplateRelations _relations;

        /// <summary>
        /// Коллекция узлов, относящихся к текущим отношениям шаблона.
        /// </summary>
        public ObservableCollection<Node> Details { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр NodePreviewVM с заданными отношениями шаблона.
        /// </summary>
        /// <param name="relations">Отношения шаблона, из которых берутся узлы.</param>
        public NodePreviewVM(TemplateRelations relations)
        {
            _relations = relations;

            Details = relations.Nodes;
        }
    }
}
