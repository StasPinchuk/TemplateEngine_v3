using Newtonsoft.Json;
using System;
using TemplateEngine_v3.Services;

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
                if (!string.IsNullOrEmpty(_name) && !_name.Equals(value))
                {
                }
                _name = value;
                OnPropertyChanged(nameof(Name));
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
                if (!string.IsNullOrEmpty(_designation) && !_designation.Equals(value))
                {
                }
                _designation = value;
                OnPropertyChanged(nameof(Designation));
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

        #endregion
    }
}
