using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TemplateEngine_v3.Services;

namespace TemplateEngine_v3.Models
{
    public class TemplateRelations : BaseNotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);

        private string _designation = string.Empty;
        public string Designation
        {
            get => _designation;
            set
            {
                if (!string.IsNullOrEmpty(_designation))
                {
                }
                if (_designation != value)
                {
                    SetValue(ref _designation, value, nameof(Designation));
                }
            }
        }

        private Technologies _technologies = new();

        /// <summary>
        /// Gets or sets the collection of technologies associated with the template.
        /// </summary>
        public Technologies Technologies
        {
            get => _technologies;
            set
            {
                _technologies = value;
                OnPropertyChanged(nameof(Technologies));
            }
        }

        /// <summary>
        /// Gets or sets the collection of nodes associated with the template.
        /// </summary>
        public ObservableCollection<Node> Nodes { get; set; } = [];

        private string _usageCondition = string.Empty;
        public string UsageCondition
        {
            get => _usageCondition;
            set
            {
                if (!string.IsNullOrEmpty(_usageCondition))
                {
                }
                _usageCondition = value;
                OnPropertyChanged(nameof(UsageCondition));
            }
        }

        private string _designationComment = string.Empty;
        public string DesignationComment
        {
            get => _designationComment;
            set
            {
                if (!string.IsNullOrEmpty(_designationComment))
                {
                }
                _designationComment = value;
                OnPropertyChanged(nameof(DesignationComment));
            }
        }

        public TemplateRelations Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<TemplateRelations>(json);
        }

        public void ApplyFrom(TemplateRelations source)
        {
            if (source == null) return;

            Designation = source.Designation;
            Technologies = source.Technologies; // если требуется копия, клонируй вручную
            Nodes = new ObservableCollection<Node>(source.Nodes); // создаём новую коллекцию
            UsageCondition = source.UsageCondition;
            DesignationComment = source.DesignationComment;
        }

    }
}
