using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
using TFlex.DOCs.Model.References.Units;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Класс для оценки условий и хранения связанных с ними данных.
    /// </summary>
    public class ConditionEvaluator : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Уникальный идентификатор объекта.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private string _name = string.Empty;

        /// <summary>
        /// Имя компонента.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (ShouldLogChange(_name, value.Trim()))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия переменной с '{_name}' на '{value.Trim()}'");
                SetValue(ref _name, value.Trim(), nameof(Name));
            }
        }

        private string _editName = string.Empty;

        /// <summary>
        /// Имя компонента для редактирования (может отличаться от основного имени).
        /// </summary>
        public string EditName
        {
            get => _editName;
            set
            {
                _editName = value.Trim();
                OnPropertyChanged(nameof(EditName));
            }
        }

        private string _value = string.Empty;

        /// <summary>
        /// Значение переменной для отображения пользователю.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (ShouldLogChange(_value, value.Trim()))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование значения переменной с '{_value}' на '{value.Trim()}'");
                SetValue(ref _value, value, nameof(Value));

                // Если новое значение пустое, очищаем список частей
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    Parts.Clear();
                }
            }
        }

        private string _usageCondition = string.Empty;

        /// <summary>
        /// Условия применения переменной.
        /// </summary>
        public string UsageCondition
        {
            get => _usageCondition;
            set
            {
                if (ShouldLogChange(_usageCondition, value.Trim()))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование условий применения переменной с '{_usageCondition}' на '{value.Trim()}'");
                SetValue(ref _usageCondition, value.Trim(), nameof(UsageCondition));
            }
        }

        private ObservableCollection<string>? _parts;

        /// <summary>
        /// Коллекция частей, сгруппированных по ключам (или просто список частей).
        /// </summary>
        public ObservableCollection<string> Parts
        {
            get => _parts ??= new ObservableCollection<string>();
            set => _parts = value;
        }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public ConditionEvaluator()
        {
            _onDeserialized = true;
        }

        /// <summary>
        /// Создаёт копию текущего объекта через сериализацию.
        /// </summary>
        /// <returns>Копия объекта <see cref="ConditionEvaluator"/>.</returns>
        public ConditionEvaluator Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ConditionEvaluator>(json);
        }

        /// <summary>
        /// Копирует значения свойств из другого экземпляра <see cref="ConditionEvaluator"/>.
        /// </summary>
        /// <param name="evaluator">Источник для копирования.</param>
        public void SetValue(ConditionEvaluator evaluator)
        {
            Id = evaluator.Id;
            Name = evaluator.Name;
            Value = evaluator.Value;
            EditName = evaluator.EditName;
            UsageCondition = evaluator.UsageCondition;
            Parts = evaluator.Parts;
        }

        /// <summary>
        /// Проверяет необходимость логирования изменения значения.
        /// </summary>
        /// <param name="oldValue">Старое значение.</param>
        /// <param name="newValue">Новое значение.</param>
        /// <returns>True, если изменение должно быть залогировано.</returns>
        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }

        /// <summary>
        /// Включение/выключение логирования изменений.
        /// </summary>
        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        /// <summary>
        /// Метод вызывается после десериализации объекта.
        /// </summary>
        /// <param name="context">Контекст десериализации.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
    }
}
