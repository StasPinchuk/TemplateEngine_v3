using System.ComponentModel;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет пару ключ-значение параметра с поддержкой уведомления об изменениях.
    /// </summary>
    public class ParameterItem : BaseNotifyPropertyChanged
    {
        private string _key;

        /// <summary>
        /// Ключ параметра.
        /// </summary>
        public string Key
        {
            get => _key;
            set => SetValue(ref _key, value, nameof(Key));
        }

        private string _value;

        /// <summary>
        /// Значение параметра.
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetValue(ref _value, value, nameof(Value));
        }
    }
}
