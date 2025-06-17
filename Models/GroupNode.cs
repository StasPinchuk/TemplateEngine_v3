using System.Collections.ObjectModel;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет группу узлов с именем и дочерними элементами.
    /// </summary>
    public class GroupNode
    {
        /// <summary>
        /// Имя группы.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Коллекция дочерних элементов группы.
        /// </summary>
        public ObservableCollection<object> Children { get; set; }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="GroupNode"/>.
        /// </summary>
        /// <param name="name">Имя группы.</param>
        /// <param name="children">Коллекция дочерних элементов.</param>
        public GroupNode(string name, ObservableCollection<object> children)
        {
            Name = name;
            Children = children;
        }
    }
}
