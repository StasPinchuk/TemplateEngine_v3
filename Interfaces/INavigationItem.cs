using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для элементов навигации.
    /// </summary>
    public interface INavigationItem
    {
        /// <summary>
        /// Заголовок элемента навигации.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Страница, связанная с элементом навигации.
        /// </summary>
        Page Page { get; set; }

        /// <summary>
        /// Иконка элемента навигации.
        /// </summary>
        PackIconKind Icon { get; set; }

        /// <summary>
        /// Указывает, выбран ли элемент навигации.
        /// </summary>
        bool IsChecked { get; set; }

        /// <summary>
        /// Имя группы, к которой относится элемент навигации.
        /// </summary>
        string GroupName { get; set; }
    }
}
