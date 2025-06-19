using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
using TFlex.DOCs.Model.References.Units;

namespace TemplateEngine_v3.Models
{
    public class ConditionEvaluator : BaseNotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Получает или задает имя компонента.
        /// </summary>
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (ShouldLogChange(_name, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия переменной с '{_name}' на '{value}'");
                SetValue(ref _name, value, nameof(Name));
            }
        }

        /// <summary>
        /// Получает или задает имя компонента.
        /// </summary>
        private string _editName = string.Empty;
        public string EditName
        {
            get => _editName;
            set
            {
                _editName = value.Trim();
                OnPropertyChanged(nameof(EditName));
            }
        }

        /// <summary>
        /// Получает или задает значение компонента для отображения пользователю.
        /// </summary>
        private string _value = string.Empty;
        public string Value
        {
            get => _value;
            set
            {
                if (ShouldLogChange(_value, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование значения переменной с '{_value}' на '{value}'");
                SetValue(ref _value, value, nameof(Value));
                if (string.IsNullOrEmpty(value))
                {
                    Parts.Clear();
                }
            }
        }

        private string _usageCondition = string.Empty;
        public string UsageCondition
        {
            get => _usageCondition;
            set
            {
                if (ShouldLogChange(_usageCondition, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование условий применения переменной с '{_usageCondition}' на '{value}'");
                SetValue(ref _usageCondition, value, nameof(UsageCondition));
            }
        }

        /// <summary>
        /// Получает или задает словарь, содержащий список частей, сгруппированных по ключам.
        /// </summary>
        private ObservableCollection<string>? _parts;
        public ObservableCollection<string> Parts
        {
            get => _parts ??= new ObservableCollection<string>();
            set => _parts = value;
        }

        public ConditionEvaluator()
        {
            _onDeserialized = true;
        }

        public ConditionEvaluator Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ConditionEvaluator>(json);
        }

        public void SetValue(ConditionEvaluator evaluator)
        {
            Id = evaluator.Id;
            Name = evaluator.Name;
            Value = evaluator.Value;
            EditName = evaluator.EditName;
            UsageCondition = evaluator.UsageCondition;
            Parts = evaluator.Parts;
        }

        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }

        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
    }
}
