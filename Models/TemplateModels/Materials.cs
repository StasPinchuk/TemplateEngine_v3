using Newtonsoft.Json;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Models
{
    public class Materials : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Название материала.
        /// </summary>
        private ConditionEvaluator _name = new() { Name = "Название материала" };
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
        /// Расход материала.
        /// </summary>
        private ConditionEvaluator _consumption = new() { Name = "Расход материала" };
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
        public string Unit
        {
            get => _unit;
            set
            {
                if (_onDeserialized)
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия детали с '{_unit}' на '{value}'");
                SetValue(ref _unit, value, nameof(Unit));
            }
        }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Materials() { }

        /// <summary>
        /// Конструктор, создающий копию указанного материала.
        /// </summary>
        /// <param name="material">Материал, который необходимо скопировать.</param>
        public Materials(Materials material)
        {
            Name = material.Name;
            Consumption = material.Consumption;
            Unit = material.Unit;
        }

        [JsonIgnore]
        private bool _onDeserialized = false;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
    }
}
