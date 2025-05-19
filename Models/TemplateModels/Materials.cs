using TemplateEngine_v3.Services;

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
                if (!string.IsNullOrEmpty(_unit) && !_unit.Equals(value))
                {
                }
                _unit = value;
                OnPropertyChanged(nameof(Unit));
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
    }
}
