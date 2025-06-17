using System.Collections.ObjectModel;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет группу узлов, сгруппированных по ключу.
    /// </summary>
    public class NodeGroup : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Ключ группы (например, название категории или группы).
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Коллекция узлов, входящих в эту группу.
        /// </summary>
        public ObservableCollection<Node> Nodes { get; set; }

        private bool _isExpanded;

        /// <summary>
        /// Определяет, развернута ли группа в UI.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetValue(ref _isExpanded, value, nameof(IsExpanded));
        }
    }
}
