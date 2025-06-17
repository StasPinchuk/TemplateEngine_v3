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
                if (_onDeserialized)
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия филиала с '{_name}' на '{value}'");
                SetValue(ref _name, value, nameof(Name));
            }
        }

        private string _editName = string.Empty;

        /// <summary>
        /// Редактируемое имя филиала (временное значение).
        /// </summary>
        public string EditName
        {
            get => _editName;
            set
            {
                _editName = value;
            }
        }

        private string _designation = string.Empty;

        /// <summary>
        /// Обозначение филиала.
        /// </summary>
        public string Designation
        {
            get => _designation;
            set
            {
                if (_onDeserialized)
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
        /// Конструктор по умолчанию.
        /// </summary>
        public Branch() { }

        #endregion

        #region Методы

        /// <summary>
        /// Создаёт глубокую копию объекта Branch.
        /// </summary>
        /// <returns>Копия текущего объекта Branch.</returns>
        public Branch Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Branch>(json);
        }

        /// <summary>
        /// Устанавливает значения текущего филиала на основе другого объекта Branch.
        /// </summary>
        /// <param name="branch">Объект Branch, из которого копируются значения.</param>
        public void SetBranch(Branch branch)
        {
            Name = branch.Name;
            Designation = branch.Designation;
            CreationDate = branch.CreationDate;
            LastModifiedDate = branch.LastModifiedDate;
        }

        [JsonIgnore]
        private bool _onDeserialized = false;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }
        #endregion
    }
}
