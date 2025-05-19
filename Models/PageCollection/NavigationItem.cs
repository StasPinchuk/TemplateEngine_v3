using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;

namespace TemplateEngine_v3.Models.PageCollection
{
    /// <summary>
    /// Класс, представляющий элемент навигации с заголовком, страницей, иконкой и состоянием выбора.
    /// </summary>
    public class NavigationItem : INavigationItem
    {
        /// <summary>
        /// Заголовок навигационного элемента.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Страница, связанная с этим элементом навигации.
        /// </summary>
        public Page Page { get; set; }

        /// <summary>
        /// Иконка навигационного элемента.
        /// </summary>
        public PackIconKind Icon { get; set; }

        /// <summary>
        /// Указывает, выбран ли данный элемент навигации.
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Имя группы, к которой принадлежит данный элемент навигации.
        /// </summary>
        public string GroupName { get; set; }
    }
}
