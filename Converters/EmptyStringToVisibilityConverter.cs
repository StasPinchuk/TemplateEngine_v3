using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TemplateEngine_v3.Converters
{
    /// <summary>
    /// Конвертер, преобразующий пустую строку в <see cref="Visibility.Collapsed"/>, а непустую — в <see cref="Visibility.Visible"/>.
    /// </summary>
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует строковое значение в значение <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">Входное значение (ожидается строка).</param>
        /// <param name="targetType">Тип целевого свойства (ожидается <see cref="Visibility"/>).</param>
        /// <param name="parameter">Не используется.</param>
        /// <param name="culture">Культура, используемая в преобразовании.</param>
        /// <returns><see cref="Visibility.Collapsed"/>, если строка пустая или null; иначе <see cref="Visibility.Visible"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        /// <summary>
        /// Обратное преобразование не реализовано.
        /// </summary>
        /// <param name="value">Значение, подлежащее преобразованию.</param>
        /// <param name="targetType">Тип, в который необходимо преобразовать.</param>
        /// <param name="parameter">Не используется.</param>
        /// <param name="culture">Культура, используемая в преобразовании.</param>
        /// <returns>Не возвращает значение, так как всегда выбрасывает исключение.</returns>
        /// <exception cref="NotImplementedException">Метод всегда выбрасывает исключение.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
