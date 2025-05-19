using ICSharpCode.AvalonEdit;
using System;
using System.Windows;

namespace TemplateEngine_v3.Helpers
{
    /// <summary>
    /// Вспомогательный класс для привязки текста в <see cref="TextEditor"/> (AvalonEdit) с использованием привязки данных WPF.
    /// </summary>
    public static class AvalonEditHelper
    {
        /// <summary>
        /// Присоединённое свойство для привязки текста в <see cref="TextEditor"/>.
        /// Обеспечивает двустороннюю привязку текста к свойству ViewModel.
        /// </summary>
        public static readonly DependencyProperty BoundTextProperty =
            DependencyProperty.RegisterAttached(
                "BoundText",
                typeof(string),
                typeof(AvalonEditHelper),
                new FrameworkPropertyMetadata(
                    default(string),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnBoundTextChanged));

        /// <summary>
        /// Получает значение свойства <c>BoundText</c>.
        /// </summary>
        /// <param name="obj">Объект, к которому привязано свойство.</param>
        /// <returns>Строковое значение, привязанное к редактору.</returns>
        public static string GetBoundText(DependencyObject obj)
        {
            return (string)obj.GetValue(BoundTextProperty);
        }

        /// <summary>
        /// Устанавливает значение свойства <c>BoundText</c>.
        /// </summary>
        /// <param name="obj">Объект, к которому привязано свойство.</param>
        /// <param name="value">Новое строковое значение.</param>
        public static void SetBoundText(DependencyObject obj, string value)
        {
            obj.SetValue(BoundTextProperty, value);
        }

        /// <summary>
        /// Вызывается при изменении значения свойства <c>BoundText</c>.
        /// Обновляет текст редактора и подписывает обработчик событий.
        /// </summary>
        /// <param name="d">Объект, у которого изменилось свойство.</param>
        /// <param name="e">Информация об изменении свойства.</param>
        private static void OnBoundTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (d is TextEditor editor)
                {
                    editor.TextChanged -= Editor_TextChanged;

                    var newText = e.NewValue as string;
                    if (editor.Text != newText)
                        editor.Text = newText ?? "";

                    editor.TextChanged += Editor_TextChanged;
                }

            }catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Обработчик события <c>TextChanged</c> редактора.
        /// Обновляет привязанное значение текста.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private static void Editor_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextEditor editor)
            {
                SetBoundText(editor, editor.Text);
            }
        }
    }
}
