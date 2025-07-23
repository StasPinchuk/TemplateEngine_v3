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
    /// Представляет операцию в технологическом процессе.
    /// </summary>
    public class Operation : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Уникальный идентификатор операции.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);

        private string _name = string.Empty;

        /// <summary>
        /// Название операции.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (ShouldLogChange(_name, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование названия операции с '{_name}' на '{value}'");
                SetValue(ref _name, value, nameof(Name));
            }
        }

        private string _order = string.Empty;

        /// <summary>
        /// Порядковый номер выполнения операции.
        /// </summary>
        public string Order
        {
            get => _order;
            set
            {
                if (ShouldLogChange(_order, value))
                    LogManager.CreateLogEntry(LogActionType.Edit, $"Редактирование номера операции с '{_order}' на '{value}'");
                SetValue(ref _order, value, nameof(Order));
            }
        }

        private ObservableCollection<BranchDivisionDetails> _branchDivisionDetails = new();

        /// <summary>
        /// Коллекция деталей разделения по подразделениям для операции.
        /// </summary>
        public ObservableCollection<BranchDivisionDetails> BranchDivisionDetails
        {
            get => _branchDivisionDetails;
            set
            {
                SetValue(ref _branchDivisionDetails, value, nameof(BranchDivisionDetails));
            }
        }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="Operation"/>.
        /// </summary>
        public Operation() { }

        /// <summary>
        /// Обновляет текущий объект значениями из другого объекта <see cref="Operation"/>.
        /// </summary>
        /// <param name="operation">Объект для копирования значений.</param>
        public void SetValue(Operation operation)
        {
            Id = operation.Id;
            Name = operation.Name;
            Order = operation.Order;
            BranchDivisionDetails = new ObservableCollection<BranchDivisionDetails>(operation.BranchDivisionDetails);
        }

        /// <summary>
        /// Создает глубокую копию текущей операции.
        /// </summary>
        /// <returns>Новый объект <see cref="Operation"/>, являющийся копией текущего.</returns>
        public Operation Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Operation>(json);
        }

        /// <summary>
        /// Включена ли запись логов изменений.
        /// </summary>
        [JsonIgnore]
        public bool IsLoggingEnabled { get; set; } = true;

        [JsonIgnore]
        private bool _onDeserialized = false;

        /// <summary>
        /// Обработчик, вызываемый после десериализации объекта.
        /// </summary>
        /// <param name="context">Контекст сериализации.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _onDeserialized = true;
        }

        /// <summary>
        /// Проверяет, следует ли записывать изменения в лог.
        /// </summary>
        /// <param name="oldValue">Старое значение.</param>
        /// <param name="newValue">Новое значение.</param>
        /// <returns>True, если изменение следует логировать; иначе false.</returns>
        private bool ShouldLogChange(string oldValue, string newValue)
        {
            return IsLoggingEnabled && _onDeserialized && !string.IsNullOrEmpty(oldValue) && oldValue != newValue;
        }
    }
}
