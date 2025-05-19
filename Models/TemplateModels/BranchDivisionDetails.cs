using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TemplateEngine_v3.Services;

namespace TemplateEngine_v3.Models
{
    public class BranchDivisionDetails : BaseNotifyPropertyChanged
    {
        public string Id = Guid.NewGuid().ToString().Substring(0,8);

        /// <summary>
        /// The branch to which this division belongs.
        /// </summary>
        private Branch _branch = new();
        public Branch Branch
        {
            get => _branch;
            set
            {
                _branch = value;
                OnPropertyChanged(nameof(Branch));
            }
        }

        /// <summary>
        /// The name of the division.
        /// </summary>
        private string _unitEquipment = string.Empty;
        public string UnitEquipment
        {
            get => _unitEquipment;
            set
            {
                if (!string.IsNullOrEmpty(_unitEquipment) && !_unitEquipment.Equals(value))
                {
                }
                _unitEquipment = value;
                OnPropertyChanged(nameof(UnitEquipment));
            }
        }

        /// <summary>
        /// The code of the division.
        /// </summary>
        private string _divisionCode = string.Empty;
        public string DivisionCode
        {
            get => _divisionCode;
            set
            {
                if (!string.IsNullOrEmpty(_divisionCode) && !_divisionCode.Equals(value))
                {
                }
                _divisionCode = value;
                OnPropertyChanged(nameof(DivisionCode));
            }
        }

        /// <summary>
        /// A collection of materials associated with this division.
        /// </summary>
        private Materials _materials = new();
        public Materials Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                OnPropertyChanged(nameof(Materials));
            }
        }

        /// <summary>
        /// Условия использования операции.
        /// </summary>
        private string _usageCondition = string.Empty;
        public string UsageCondition
        {
            get => _usageCondition;
            set
            {
                if (!string.IsNullOrEmpty(_usageCondition) && !_usageCondition.Equals(value))
                {
                }
                _usageCondition = value;
                OnPropertyChanged(nameof(UsageCondition));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchDivisionDetails"/> class and adds an empty material to the collection.
        /// </summary>
        public BranchDivisionDetails()
        {
        }

        public void SetValue(BranchDivisionDetails branchDivisionDetails)
        {
            this.Branch = branchDivisionDetails.Branch;
            this.DivisionCode = branchDivisionDetails.DivisionCode;
            this.Materials = branchDivisionDetails.Materials;
            this.UnitEquipment = branchDivisionDetails.UnitEquipment;
            this.UsageCondition = branchDivisionDetails.UsageCondition;
        }

        public BranchDivisionDetails ShallowCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<BranchDivisionDetails>(json);
        }

        private ObservableCollection<GroupNode> _groups;
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

                    // Пример: подписка на изменения нужных свойств
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
    }
}
