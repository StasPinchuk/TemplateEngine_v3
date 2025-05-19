using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TemplateEngine_v3.Converters;

namespace TemplateEngine_v3.Models
{
    public class Template : BaseNotifyPropertyChanged, ITemplatedFile
    {
        /// <summary>
        /// Unique identifier of the template
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Name of the template
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
        /// Designation of the template.
        /// </summary>
        private ObservableCollection<TemplateRelations> _templateRelations = [];
        public ObservableCollection<TemplateRelations> TemplateRelations
        {
            get => _templateRelations;
            set
            {
                if (_templateRelations.Count < value.Count)
                {
                }
                _templateRelations = value;
                OnPropertyChanged(nameof(TemplateRelations));
            }
        }

        /// <summary>
        /// Коллекция веток, связанных с шаблоном.
        /// </summary>
        public ObservableCollection<Branch> Branches { get; set; } = [];

        /// <summary>
        /// Collection of product marking attributes.
        /// </summary>
        [JsonConverter(typeof(ProductMarkingAttributesNameListConverter))]
        public List<string> ProductMarkingAttributes = [];

        /// <summary>
        /// List of markings.
        /// </summary>
        public ObservableCollection<string> ExampleMarkings { get; set; } = [];

        /// <summary>
        /// Template creation date.
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
        /// Template last modified date.
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
        /// File name associated with the template.
        /// </summary>
        private string _templateLastId = string.Empty;
        public string TemplateLastId
        {
            get => _templateLastId; set => _templateLastId = value;
        }

        private bool _isTemplateComplete = false;

        public bool IsTemplateComplete
        {
            get => _isTemplateComplete;
            set
            {
                _isTemplateComplete = value;
                OnPropertyChanged(nameof(IsTemplateComplete));
            }
        }

        public Template() { }

        public Template Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Template>(json);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
