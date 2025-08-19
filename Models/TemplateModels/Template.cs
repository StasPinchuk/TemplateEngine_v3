using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using TemplateEngine_v3.Converters;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет шаблон, содержащий информацию о технологических ветках и связях.
    /// </summary>
    public class Template : BaseNotifyPropertyChanged, ITemplatedFile
    {
        [JsonIgnore]
        private bool _onDeserialized = false;

        /// <summary>
        /// Уникальный идентификатор шаблона.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _name = string.Empty;

        /// <summary>
        /// Название шаблона.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (ShouldLogChange(_name, value))
                {
                    var currentPage = NavigationService.GetPageHistory().FirstOrDefault(page => page.Title.Equals(_name));
                    if(currentPage != null)
                        currentPage.Title = value;
                    NavigationService.UpdateHistory?.Invoke();
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия шаблона с '{_name}' на '{value}'");
                    Services.NavigationService.RenameSelectedTab(value);
                }
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private Guid _stage;

        /// <summary>
        /// Идентификатор этапа шаблона.
        /// </summary>
        public Guid Stage
        {
            get => _stage;
            set => SetValue(ref _stage, value, nameof(Stage));
        }

        private ObservableCollection<TemplateRelations> _templateRelations = new();

        /// <summary>
        /// Коллекция связей шаблона.
        /// </summary>
        public ObservableCollection<TemplateRelations> TemplateRelations
        {
            get => _templateRelations;
            set
            {
                _templateRelations = value;
                OnPropertyChanged(nameof(TemplateRelations));
            }
        }

        /// <summary>
        /// Коллекция веток, связанных с шаблоном.
        /// </summary>
        public ObservableCollection<Branch> Branches { get; set; } = new();

        /// <summary>
        /// Список атрибутов маркировки продукта.
        /// </summary>
        [JsonConverter(typeof(ProductMarkingAttributesNameListConverter))]
        public List<string> ProductMarkingAttributes = new();

        /// <summary>
        /// Коллекция примеров маркировок.
        /// </summary>
        public ObservableCollection<string> ExampleMarkings { get; set; } = new();

        private DateTime _creationDate = DateTime.Now;

        /// <summary>
        /// Дата создания шаблона.
        /// </summary>
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                _creationDate = value;
                OnPropertyChanged(nameof(CreationDate));
            }
        }

        private DateTime _lastModifiedDate = DateTime.MinValue;

        /// <summary>
        /// Дата последнего изменения шаблона.
        /// </summary>
        public DateTime LastModifiedDate
        {
            get => _lastModifiedDate;
            set
            {
                _lastModifiedDate = value;
                OnPropertyChanged(nameof(LastModifiedDate));
            }
        }

        private string _templateLastId = string.Empty;

        /// <summary>
        /// Имя файла, связанного с шаблоном.
        /// </summary>
        public string TemplateLastId
        {
            get => _templateLastId;
            set => _templateLastId = value;
        }

        private bool _isTemplateComplete = false;

        /// <summary>
        /// Флаг, указывающий, завершён ли шаблон.
        /// </summary>
        public bool IsTemplateComplete
        {
            get => _isTemplateComplete;
            set
            {
                _isTemplateComplete = value;
                OnPropertyChanged(nameof(IsTemplateComplete));
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Template"/>.
        /// </summary>
        public Template() { }

        /// <summary>
        /// Создает глубокую копию текущего шаблона.
        /// </summary>
        /// <returns>Копия объекта <see cref="Template"/>.</returns>
        public Template Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Template>(json);
        }

        /// <summary>
        /// Возвращает строковое представление шаблона.
        /// </summary>
        /// <returns>Название шаблона.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Определяет, следует ли логировать изменение значения.
        /// </summary>
        /// <param name="oldValue">Старое значение.</param>
        /// <param name="newValue">Новое значение.</param>
        /// <returns>True, если изменение нужно логировать, иначе false.</returns>
        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }

        /// <summary>
        /// Включена ли запись логов изменений.
        /// </summary>
        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        /// <summary>
        /// Обработчик, вызываемый после десериализации объекта.
        /// </summary>
        /// <param name="context">Контекст сериализации.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
    }
}
