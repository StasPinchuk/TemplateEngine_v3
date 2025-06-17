using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Базовый класс, реализующий INotifyPropertyChanged для поддержки привязки данных.
    /// </summary>
    public class BaseNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Уведомляет об изменении свойства.
        /// </summary>
        /// <param name="propertyName">Имя изменившегося свойства (автоматически подставляется).</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Устанавливает значение и вызывает уведомление об изменении.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="field">Ссылка на поле.</param>
        /// <param name="value">Новое значение.</param>
        /// <param name="propertyName">Имя свойства (автоматически подставляется).</param>
        /// <returns><c>true</c>, если значение было изменено; иначе <c>false</c>.</returns>
        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
