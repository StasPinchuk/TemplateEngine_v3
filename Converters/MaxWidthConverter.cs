using System;
using System.Globalization;
using System.Windows.Data;

namespace TemplateEngine_v3.Converters
{
    /// <summary>
    /// Конвертер, вычисляющий максимальную ширину как процент от исходной ширины.
    /// </summary>
    public class MaxWidthConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует исходную ширину и процент в максимальную ширину.
        /// </summary>
        /// <param name="value">Исходная ширина (ожидается <see cref="double"/>).</param>
        /// <param name="targetType">Тип целевого свойства (обычно <see cref="double"/>).</param>
        /// <param name="parameter">Строковое значение процента (например, "75").</param>
        /// <param name="culture">Культура, используемая в преобразовании.</param>
        /// <returns>Процент от ширины в виде <see cref="double"/>, либо 0, если входные данные некорректны.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && parameter is string percentStr && double.TryParse(percentStr, out double percent))
            {
                return width * (percent / 100);
            }

            return 0;
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
