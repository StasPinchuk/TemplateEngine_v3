using System;
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

        public BranchDivisionDetails ShallowCopy()
        {
            return (BranchDivisionDetails)this.MemberwiseClone();
        }
    }
}
