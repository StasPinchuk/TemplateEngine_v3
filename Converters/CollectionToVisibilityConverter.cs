using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TemplateEngine_v3.Converters
{
    /// <summary>
    /// Конвертер для преобразования коллекции в <see cref="Visibility"/>.
    /// Отображает <see cref="Visibility.Visible"/>, если коллекция содержит хотя бы один элемент, иначе <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public class CollectionToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует коллекцию в значение <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">Источник привязки, предполагаемый как <see cref="IEnumerable"/>.</param>
        /// <param name="targetType">Тип, к которому производится приведение (обычно <see cref="Visibility"/>).</param>
        /// <param name="parameter">Дополнительный параметр (не используется).</param>
        /// <param name="culture">Информация о культуре.</param>
        /// <returns><see cref="Visibility.Visible"/>, если коллекция не пуста; иначе <see cref="Visibility.Collapsed"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable enumerable)
            {
                foreach (var _ in enumerable)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Обратное преобразование не реализовано.
        /// </summary>
        /// <param name="value">Значение для преобразования обратно (не используется).</param>
        /// <param name="targetType">Тип, к которому нужно преобразовать (не используется).</param>
        /// <param name="parameter">Дополнительный параметр (не используется).</param>
        /// <param name="culture">Информация о культуре.</param>
        /// <returns>Никогда не возвращает, всегда выбрасывает исключение.</returns>
        /// <exception cref="NotImplementedException">Всегда выбрасывается, так как обратное преобразование не реализовано.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
