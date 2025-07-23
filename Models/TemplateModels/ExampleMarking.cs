namespace TemplateEngine_v3.Models.TemplateModels
{
    /// <summary>
    /// Пример класса с простым текстовым свойством для демонстрации реализации INotifyPropertyChanged.
    /// </summary>
    public class ExampleMarking : BaseNotifyPropertyChanged
    {
        private string _text = string.Empty;

        /// <summary>
        /// Текстовое свойство с поддержкой уведомления об изменении.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    SetValue(ref _text, value, nameof(Text));
                }
            }
        }
    }
}
