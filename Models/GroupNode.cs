using System.Collections.ObjectModel;

namespace TemplateEngine_v3.Models
{
    public class GroupNode
    {
        public string Name { get; set; }
        public ObservableCollection<object> Children { get; set; }

        public GroupNode(string name, ObservableCollection<object> children)
        {
            Name = name;
            Children = children;
        }
    }
}
