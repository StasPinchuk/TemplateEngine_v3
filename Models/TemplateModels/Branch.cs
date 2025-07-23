using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Модель данных для представления филиала.
    /// </summary>
    public class Branch : BaseNotifyPropertyChanged, ITemplatedFile
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор филиала.
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        private string _name = string.Empty;

        /// <summary>
        /// Имя филиала.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (ShouldLogChange(_name, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия филиала с '{_name}' на '{value}'");
                SetValue(ref _name, value, nameof(Name));
            }
        }

        private Guid _stage;

        /// <summary>
        /// Идентификатор стадии, к которой относится филиал.
        /// </summary>
        public Guid Stage
        {
            get => _stage;
            set => SetValue(ref _stage, value, nameof(Stage));
        }

        private string _editName = string.Empty;

        /// <summary>
        /// Временное редактируемое имя филиала (не сохраняется напрямую).
        /// </summary>
        public string EditName
        {
            get => _editName;
            set => _editName = value;
        }

        private string _designation = string.Empty;

        /// <summary>
        /// Обозначение филиала (например, код или номер).
        /// </summary>
        public string Designation
        {
            get => _designation;
            set
            {
                if (ShouldLogChange(_designation, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование обозначения филиала с '{_designation}' на '{value}'");
                SetValue(ref _designation, value, nameof(Designation));
            }
        }

        private DateTime _creationDate = DateTime.Now;

        /// <summary>
        /// Дата создания филиала.
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
        /// Дата последнего изменения филиала.
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

        #endregion

        #region Конструкторы

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Branch"/>.
        /// </summary>
        public Branch() { }

        #endregion

        #region Методы

        /// <summary>
        /// Создаёт глубокую копию объекта <see cref="Branch"/>.
        /// </summary>
        /// <returns>Глубокая копия текущего объекта <see cref="Branch"/>.</returns>
        public Branch Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Branch>(json);
        }

        /// <summary>
        /// Копирует значения из другого объекта <see cref="Branch"/> в текущий.
        /// </summary>
        /// <param name="branch">Источник данных.</param>
        public void SetBranch(Branch branch)
        {
            Name = branch.Name;
            Designation = branch.Designation;
            CreationDate = branch.CreationDate;
            LastModifiedDate = branch.LastModifiedDate;
        }

        /// <summary>
        /// Проверяет необходимость логирования изменения свойства.
        /// </summary>
        /// <param name="oldValue">Старое значение.</param>
        /// <param name="newValue">Новое значение.</param>
        /// <returns><c>true</c>, если требуется логирование; иначе <c>false</c>.</returns>
        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }

        /// <summary>
        /// Флаг, указывающий, разрешено ли логирование.
        /// </summary>
        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        /// <summary>
        /// Метод, вызываемый после десериализации JSON.
        /// Устанавливает флаг для разрешения логирования.
        /// </summary>
        /// <param name="context">Контекст десериализации.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }

        #endregion
    }
}
