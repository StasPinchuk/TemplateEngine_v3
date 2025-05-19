using System.Collections.ObjectModel;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.VM.Pages
{
    public class NodePreviewVM 
    {
        private readonly TemplateRelations _relations;

        public ObservableCollection<Node> Details { get; private set; } 

        public NodePreviewVM(TemplateRelations relations)
        {
            _relations = relations;

            Details = relations.Nodes;
        }
    }
}
