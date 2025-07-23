using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет связь шаблона с комплектующей документацией и связанными технологиями.
    /// </summary>
    public class TemplateRelations : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Уникальный идентификатор связи (первые 8 символов GUID).
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);

        private string _designation = string.Empty;

        /// <summary>
        /// Обозначение комплектующей документации (КД).
        /// </summary>
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
        /// Коллекция технологий, связанных с данным шаблоном.
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
        /// Коллекция узлов (nodes), связанных с шаблоном.
        /// </summary>
        public ObservableCollection<Node> Nodes { get; set; } = new();

        private string _usageCondition = string.Empty;

        /// <summary>
        /// Условия применения комплектующей документации.
        /// </summary>
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

        /// <summary>
        /// Комментарий к обозначению комплектующей документации.
        /// </summary>
        public string DesignationComment
        {
            get => _designationComment;
            set
            {
                if (ShouldLogChange(_designationComment, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование комментария к КД с '{_designationComment}' на '{value}'");
                SetValue(ref _designationComment, value, nameof(DesignationComment));
            }
        }

        /// <summary>
        /// Создает глубокую копию текущего объекта <see cref="TemplateRelations"/>.
        /// </summary>
        /// <returns>Копия объекта <see cref="TemplateRelations"/>.</returns>
        public TemplateRelations Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<TemplateRelations>(json);
        }

        /// <summary>
        /// Применяет значения свойств из другого объекта <see cref="TemplateRelations"/>.
        /// </summary>
        /// <param name="source">Источник для копирования данных.</param>
        public void ApplyFrom(TemplateRelations source)
        {
            if (source == null) return;

            Designation = source.Designation;
            Technologies = source.Technologies; // если требуется копия, клонируйте вручную
            Nodes = new ObservableCollection<Node>(source.Nodes); // создаём новую коллекцию узлов
            UsageCondition = source.UsageCondition;
            DesignationComment = source.DesignationComment;
        }

        /// <summary>
        /// Проверяет, нужно ли логировать изменение свойства.
        /// </summary>
        /// <param name="oldValue">Старое значение.</param>
        /// <param name="newValue">Новое значение.</param>
        /// <returns>True, если необходимо логировать, иначе false.</returns>
        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }

        /// <summary>
        /// Флаг, включающий запись логов изменений.
        /// </summary>
        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        /// <summary>
        /// Метод, вызываемый после десериализации объекта.
        /// </summary>
        /// <param name="context">Контекст сериализации.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
    }
}
