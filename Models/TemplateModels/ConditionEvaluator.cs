using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using TemplateEngine_v3.Services;

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
                if (!string.IsNullOrEmpty(_name) && !_name.Equals(value))
                {
                }
                _name = value.Trim();
                OnPropertyChanged(nameof(Name));
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
                if (!string.IsNullOrEmpty(_value) && !_value.Equals(value))
                {
                }
                _value = value.Trim();
                OnPropertyChanged(nameof(Value));
            }
        }

        private string _usageCondition = string.Empty;
        public string UsageCondition
        {
            get => _usageCondition;
            set
            {
                if (!string.IsNullOrEmpty(_usageCondition) && !_usageCondition.Equals(value))
                {
                }
                _usageCondition = value.Trim();
                OnPropertyChanged(nameof(UsageCondition));
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
    }
}
