using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TemplateEngine_v3.Models
{
    public class NodeGroup
    {
        public string Key { get; set; }
        public ObservableCollection<Node> Nodes { get; set; }
    }

}
