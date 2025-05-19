using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TemplateEngine_v3.Models
{
    public class NodeGroup : BaseNotifyPropertyChanged
    {
        public string Key { get; set; }
        public ObservableCollection<Node> Nodes { get; set; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetValue(ref _isExpanded, value, nameof(IsExpanded));
        }
    }

}
