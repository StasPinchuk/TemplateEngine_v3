using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TemplateEngine_v3.Services;

namespace TemplateEngine_v3.Models
{
    public class Operation : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Unique identifier of the operation.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);

        /// <summary>
        /// Name of the operation.
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
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Execution order of the operation.
        /// </summary>
        private string _order = string.Empty;
        public string Order
        {
            get => _order;
            set
            {
                if (!string.IsNullOrEmpty(_order) && !_order.Equals(value))
                {
                }
                _order = value;
                OnPropertyChanged(nameof(Order));
            }
        }

        /// <summary>
        /// Collection of branch division details for the operation.
        /// </summary>
        private ObservableCollection<BranchDivisionDetails> _branchDivisionDetails = [];
        public ObservableCollection<BranchDivisionDetails> BranchDivisionDetails
        {
            get => _branchDivisionDetails;
            set
            {
                if (_branchDivisionDetails.Count < value.Count)
                {
                }
                else if (_branchDivisionDetails.Count > value.Count)
                {
                    var list = _branchDivisionDetails.Where(x =>
                        !value.Any(v => v.Branch.Name?.Equals(x.Branch.Name, StringComparison.OrdinalIgnoreCase) == true));
                    string logString = string.Empty;
                    if(list.Count() > 1)
                    {
                        logString = $"Удалены детали для филиалов: {string.Join(",", list)}";
                    }
                }
                _branchDivisionDetails = value;
                OnPropertyChanged(nameof(BranchDivisionDetails));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Operation"/> class.
        /// </summary>
        public Operation() { }

        public void SetValue(Operation operation)
        {
            Id = operation.Id;
            Name = operation.Name;
            Order = operation.Order;
            BranchDivisionDetails = new(operation.BranchDivisionDetails);
        }
    }
}
