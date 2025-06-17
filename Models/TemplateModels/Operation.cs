using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;

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
                if (_onDeserialized)
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия операции с '{_name}' на '{value}'");
                SetValue(ref _name, value, nameof(Name));
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
                if (_onDeserialized)
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование номера операции с '{_order}' на '{value}'");
                SetValue(ref _order, value, nameof(Order));
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
                SetValue(ref _branchDivisionDetails, value, nameof(BranchDivisionDetails));
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

        public Operation Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Operation>(json);
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
