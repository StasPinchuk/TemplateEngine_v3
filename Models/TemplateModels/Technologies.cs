using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет технологию, состоящую из набора операций.
    /// </summary>
    public class Technologies : BaseNotifyPropertyChanged, ITemplatedFile
    {
        /// <summary>
        /// Включена ли запись логов изменений.
        /// </summary>
        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        /// <summary>
        /// Уникальный идентификатор технологии.
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        private string _name = string.Empty;

        /// <summary>
        /// Название технологии.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                SetValue(ref _name, value.Trim(), nameof(Name));
            }
        }

        private Guid _stage;

        /// <summary>
        /// Идентификатор этапа технологии.
        /// </summary>
        public Guid Stage
        {
            get => _stage;
            set => SetValue(ref _stage, value, nameof(Stage));
        }

        private string _editName = string.Empty;

        /// <summary>
        /// Название технологии для редактирования (альтернативное имя).
        /// </summary>
        public string EditName
        {
            get => _editName;
            set
            {
                SetValue(ref _editName, value.Trim(), nameof(EditName));
            }
        }

        private ObservableCollection<Operation> _operations = new();

        /// <summary>
        /// Коллекция операций, связанных с технологией.
        /// </summary>
        public ObservableCollection<Operation> Operations
        {
            get => _operations;
            set
            {
                SetValue(ref _operations, value, nameof(Operations));
            }
        }

        private DateTime _creationDate = DateTime.Now;

        /// <summary>
        /// Дата создания технологии.
        /// </summary>
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                SetValue(ref _creationDate, value, nameof(CreationDate));
            }
        }

        private DateTime _lastModifiedDate = DateTime.MinValue;

        /// <summary>
        /// Дата последнего изменения технологии.
        /// </summary>
        public DateTime LastModifiedDate
        {
            get => _lastModifiedDate;
            set
            {
                SetValue(ref _lastModifiedDate, value, nameof(LastModifiedDate));
            }
        }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="Technologies"/>.
        /// </summary>
        public Technologies() { }

        /// <summary>
        /// Создает глубокую копию текущего объекта <see cref="Technologies"/>.
        /// </summary>
        /// <returns>Новая копия объекта технологии.</returns>
        public Technologies Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Technologies>(json);
        }

        /// <summary>
        /// Копирует значения из другого объекта <see cref="Technologies"/> в текущий.
        /// Создает новый идентификатор для текущей технологии.
        /// </summary>
        /// <param name="technologies">Объект для копирования значений.</param>
        public void SetValue(Technologies technologies)
        {
            Id = Guid.NewGuid();
            Name = technologies.Name;
            Operations = new ObservableCollection<Operation>(technologies.Operations);
            foreach (var operation in Operations)
            {
                operation.BranchDivisionDetails = new ObservableCollection<BranchDivisionDetails>(
                    operation.BranchDivisionDetails
                        .Select(division =>
                        {
                            division.Materials = new();
                            return division;
                        }));

            }
            CreationDate = technologies.CreationDate;
            LastModifiedDate = technologies.LastModifiedDate;
        }

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
