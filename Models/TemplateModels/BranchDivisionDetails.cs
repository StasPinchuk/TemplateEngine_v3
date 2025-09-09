using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Детали подразделения филиала с набором свойств и материалов.
    /// </summary>
    public class BranchDivisionDetails : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Уникальный идентификатор объекта (первые 8 символов GUID).
        /// </summary>
        public string Id = Guid.NewGuid().ToString().Substring(0, 8);

        private Branch _branch = new();

        /// <summary>
        /// Филиал, к которому относится данное подразделение.
        /// </summary>
        public Branch Branch
        {
            get => _branch;
            set
            {
                if (value != null)
                    if (ShouldLogChange(_branch.Name, value.Name))
                        LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия филиала ТП с '{_branch.Name}' на '{value.Name}'");
                SetValue(ref _branch, value, nameof(Branch));
            }
        }

        private string _unitEquipment = string.Empty;

        /// <summary>
        /// Наименование оборудования подразделения.
        /// </summary>
        public string UnitEquipment
        {
            get => _unitEquipment;
            set
            {
                if (ShouldLogChange(_unitEquipment, value.Trim()))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия оборудования с '{_unitEquipment}' на '{value.Trim()}'");
                SetValue(ref _unitEquipment, value.Trim(), nameof(UnitEquipment));
            }
        }

        private string _divisionCode = string.Empty;

        /// <summary>
        /// Код подразделения.
        /// </summary>
        public string DivisionCode
        {
            get => _divisionCode;
            set
            {
                if (ShouldLogChange(_divisionCode, value.Trim()))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование кода подразделения с '{_divisionCode}' на '{value.Trim()}'");
                SetValue(ref _divisionCode, value.Trim(), nameof(DivisionCode));
            }
        }

        private Materials _materials = new();

        /// <summary>
        /// Коллекция материалов, связанных с подразделением.
        /// </summary>
        public Materials Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                OnPropertyChanged(nameof(Materials));
            }
        }

        private string _usageCondition = string.Empty;

        /// <summary>
        /// Условия применения операции.
        /// </summary>
        public string UsageCondition
        {
            get => _usageCondition;
            set
            {
                if (ShouldLogChange(_usageCondition, value.Trim()))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование условий применения с '{_usageCondition}' на '{value.Trim()}'");
                SetValue(ref _usageCondition, value.Trim(), nameof(UsageCondition));
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BranchDivisionDetails"/>.
        /// </summary>
        public BranchDivisionDetails()
        {
        }

        /// <summary>
        /// Копирует значения свойств из другого объекта <see cref="BranchDivisionDetails"/>.
        /// </summary>
        /// <param name="branchDivisionDetails">Источник данных.</param>
        public void SetValue(BranchDivisionDetails branchDivisionDetails)
        {
            this.Branch = branchDivisionDetails.Branch;
            this.DivisionCode = branchDivisionDetails.DivisionCode;
            this.Materials = branchDivisionDetails.Materials;
            this.UnitEquipment = branchDivisionDetails.UnitEquipment;
            this.UsageCondition = branchDivisionDetails.UsageCondition;
        }

        /// <summary>
        /// Создаёт неглубокую копию текущего объекта с помощью сериализации JSON.
        /// </summary>
        /// <returns>Копия объекта <see cref="BranchDivisionDetails"/>.</returns>
        public BranchDivisionDetails ShallowCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<BranchDivisionDetails>(json);
        }

        private ObservableCollection<GroupNode> _groups;

        /// <summary>
        /// Коллекция групп для отображения свойств подразделения.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<GroupNode> Groups
        {
            get
            {
                if (_groups == null)
                {
                    _groups = new ObservableCollection<GroupNode>
                    {
                        new GroupNode("Свойства", BuildPropertiesGroup())
                    };

                    // Подписка на изменения свойств для обновления группы "Свойства"
                    PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName is nameof(UnitEquipment) or
                                                nameof(DivisionCode) or
                                                nameof(UsageCondition) or
                                                nameof(Materials))
                        {
                            UpdatePropertiesGroup();
                        }
                    };
                }

                return _groups;
            }
        }

        /// <summary>
        /// Создаёт коллекцию свойств для отображения в группе "Свойства".
        /// </summary>
        /// <returns>Коллекция объектов со строковыми описаниями свойств.</returns>
        private ObservableCollection<object> BuildPropertiesGroup()
        {
            var children = new ObservableCollection<object>();

            if (!string.IsNullOrWhiteSpace(UnitEquipment))
                children.Add($"Оборудование: {UnitEquipment}");

            if (!string.IsNullOrWhiteSpace(DivisionCode))
                children.Add($"Код: {DivisionCode}");

            if (!string.IsNullOrWhiteSpace(UsageCondition))
                children.Add($"Условие: {UsageCondition}");

            if (!string.IsNullOrWhiteSpace(Materials?.Name?.Value))
            {
                children.Add($"Материал: {Materials.Name.Value}");

                if (!string.IsNullOrWhiteSpace(Materials.Consumption?.Value))
                    children.Add($"Расход материала: {Materials.Consumption.Value}");

                if (!string.IsNullOrWhiteSpace(Materials.Unit))
                    children.Add($"Единицы измерения: {Materials.Unit}");
            }

            return children;
        }

        /// <summary>
        /// Обновляет содержимое группы "Свойства" при изменении соответствующих свойств.
        /// </summary>
        private void UpdatePropertiesGroup()
        {
            var group = _groups?.FirstOrDefault(g => g.Name == "Свойства");
            if (group != null)
            {
                group.Children.Clear();
                foreach (var item in BuildPropertiesGroup())
                    group.Children.Add(item);
            }
        }

        /// <summary>
        /// Проверяет необходимость логирования изменения свойства.
        /// </summary>
        /// <param name="oldValue">Старое значение.</param>
        /// <param name="newValue">Новое значение.</param>
        /// <returns>True, если нужно логировать изменение; иначе false.</returns>
        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }

        /// <summary>
        /// Флаг разрешения логирования изменений.
        /// </summary>
        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        /// <summary>
        /// Метод вызывается после десериализации объекта из JSON.
        /// Устанавливает флаг для разрешения логирования.
        /// </summary>
        /// <param name="context">Контекст десериализации.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
    }
}
