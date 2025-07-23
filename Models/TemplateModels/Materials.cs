using Newtonsoft.Json;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет материал с характеристиками: названием, расходом и единицей измерения.
    /// </summary>
    public class Materials : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Название материала (оборачивается в <see cref="ConditionEvaluator"/>).
        /// </summary>
        private ConditionEvaluator _name = new() { Name = "Название материала" };

        /// <summary>
        /// Получает или задает название материала.
        /// </summary>
        public ConditionEvaluator Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Расход материала (оборачивается в <see cref="ConditionEvaluator"/>).
        /// </summary>
        private ConditionEvaluator _consumption = new() { Name = "Расход материала" };

        /// <summary>
        /// Получает или задает расход материала.
        /// </summary>
        public ConditionEvaluator Consumption
        {
            get => _consumption;
            set
            {
                _consumption = value;
                OnPropertyChanged(nameof(Consumption));
            }
        }

        /// <summary>
        /// Единица измерения расхода материала.
        /// </summary>
        private string _unit = string.Empty;

        /// <summary>
        /// Получает или задает единицу измерения расхода материала.
        /// </summary>
        public string Unit
        {
            get => _unit;
            set
            {
                if (ShouldLogChange(_unit, value.Trim()))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия детали с '{_unit}' на '{value.Trim()}'");
                SetValue(ref _unit, value.Trim(), nameof(Unit));
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Materials"/>.
        /// </summary>
        public Materials() { }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Materials"/> копированием данных из другого экземпляра.
        /// </summary>
        /// <param name="material">Экземпляр материала для копирования.</param>
        public Materials(Materials material)
        {
            Name = material.Name;
            Consumption = material.Consumption;
            Unit = material.Unit;
        }

        /// <summary>
        /// Проверяет необходимость логирования изменения значения.
        /// </summary>
        /// <param name="oldValue">Старое значение.</param>
        /// <param name="newValue">Новое значение.</param>
        /// <returns>Возвращает <c>true</c>, если необходимо логировать изменение.</returns>
        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }

        /// <summary>
        /// Флаг включения логирования изменений.
        /// </summary>
        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        /// <summary>
        /// Метод, вызываемый после десериализации, для установки внутреннего состояния.
        /// </summary>
        /// <param name="context">Контекст сериализации.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
    }
}
