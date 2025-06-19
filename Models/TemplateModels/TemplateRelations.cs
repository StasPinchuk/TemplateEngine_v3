using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;

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
                if (_designation != value)
                {
                    if (ShouldLogChange(_designation, value))
                        LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия КД с '{_designation}' на '{value}'");
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
                SetValue(ref _technologies, value, nameof(Technologies));
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
                if (ShouldLogChange(_usageCondition, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование условий применения КД с '{_usageCondition}' на '{value}'"); 
                SetValue(ref _usageCondition, value, nameof(UsageCondition));
            }
        }

        private string _designationComment = string.Empty;
        public string DesignationComment
        {
            get => _designationComment;
            set
            {
                if (ShouldLogChange(_designationComment, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование условий применения КД с '{_designationComment}' на '{value}'");
                SetValue(ref _designationComment, value, nameof(DesignationComment));
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

        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }

        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
    }
}
