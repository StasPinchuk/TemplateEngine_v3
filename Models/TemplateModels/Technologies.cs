using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using TemplateEngine_v3.Services;

namespace TemplateEngine_v3.Models
{
    public class Technologies : BaseNotifyPropertyChanged, ITemplatedFile
    {
        /// <summary>
        /// Unique identifier of the technology.
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Name of the technology.
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
        /// Name of the technology.
        /// </summary>
        private string _editName = string.Empty;
        public string EditName
        {
            get => _editName;
            set
            {
                _editName = value;
            }
        }

        /// <summary>
        /// Collection of operations associated with the technology.
        /// </summary>
        private ObservableCollection<Operation> _operations = [];
        public ObservableCollection<Operation> Operations
        {
            get => _operations;
            set
            {
                if (_operations.Count < value.Count)
                {
                }
                _operations = value;
                OnPropertyChanged(nameof(Operations));
            }
        }

        /// <summary>
        /// Technologies creation date.
        /// </summary>
        private DateTime _creationDate = DateTime.Now;
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                _creationDate = value;
                OnPropertyChanged(nameof(CreationDate));
            }
        }

        /// <summary>
        /// Technologies last modified date.
        /// </summary>
        private DateTime _lastModifiedDate = DateTime.MinValue;
        public DateTime LastModifiedDate
        {
            get => _lastModifiedDate;
            set
            {
                _lastModifiedDate = value;
                OnPropertyChanged(nameof(LastModifiedDate));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Technologies"/> class.
        /// </summary>
        public Technologies() { }

        public Technologies Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Technologies>(json);
        }

        public void SetValue(Technologies technologies)
        {
            Id = Guid.NewGuid();
            Name = technologies.Name;
            Operations = new(technologies.Operations);
            foreach(var operation in Operations)
            {
                operation.BranchDivisionDetails = new(operation.BranchDivisionDetails
                    .Select(division =>
                    {
                        division.Materials = new();
                        return division;
                    }));
            }
            CreationDate = technologies.CreationDate;
            LastModifiedDate = technologies.LastModifiedDate;
        }
    }
}
