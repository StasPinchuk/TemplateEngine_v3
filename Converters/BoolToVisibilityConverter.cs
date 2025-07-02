using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TemplateEngine_v3.Converters
{
    /// <summary>
    /// Конвертер для преобразования логического значения в <see cref="Visibility"/>.
    /// true → Visible, false → Collapsed (по умолчанию).
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Если установлено в true, то логика инвертируется: true → Collapsed, false → Visible.
        /// </summary>
        public bool Inverted { get; set; }

        /// <summary>
        /// Если установлено в true, то вместо <see cref="Visibility.Collapsed"/> будет использоваться <see cref="Visibility.Hidden"/>.
        /// </summary>
        public bool UseHidden { get; set; }

        /// <summary>
        /// Преобразует значение <see cref="bool"/> в <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">Значение типа <see cref="bool"/>.</param>
        /// <param name="targetType">Тип назначения (обычно <see cref="Visibility"/>).</param>
        /// <param name="parameter">Не используется.</param>
        /// <param name="culture">Информация о культуре.</param>
        /// <returns><see cref="Visibility.Visible"/> или <see cref="Visibility.Collapsed"/>/<see cref="Visibility.Hidden"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;

            if (Inverted)
                flag = !flag;

            if (flag)
                return Visibility.Visible;

            return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        /// <summary>
        /// Преобразует значение <see cref="Visibility"/> обратно в <see cref="bool"/>.
        /// </summary>
        /// <param name="value">Значение типа <see cref="Visibility"/>.</param>
        /// <param name="targetType">Тип назначения (обычно <see cref="bool"/>).</param>
        /// <param name="parameter">Не используется.</param>
        /// <param name="culture">Информация о культуре.</param>
        /// <returns>true, если Visible, иначе false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;
                return Inverted ? !result : result;
            }

            return false;
        }
    }
}
