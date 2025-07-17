using System;
using TFlex.DOCs.Model.Classes;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Тип справочного объекта.
    /// </summary>
    public enum ReferenceType
    {
        Template,
        Technologies,
        Branch
    }

    /// <summary>
    /// Модель, содержащая основную информацию о справочном объекте.
    /// </summary>
    public class ReferenceModelInfo : BaseNotifyPropertyChanged
    {
        private Guid _id;
        private Guid _stage;
        private string _name = string.Empty;
        private ClassObject _type;
        private DateTime _createDate = DateTime.MinValue;
        private DateTime _lastEditDate = DateTime.MinValue;
        private string _objectStruct = string.Empty;
        private bool _isLocked;

        /// <summary>
        /// Уникальный идентификатор объекта.
        /// </summary>
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value, nameof(Id));
        }

        public Guid Stage
        {
            get => _stage;
            set => SetValue(ref _stage, value, nameof(Stage));
        }

        /// <summary>
        /// Имя объекта.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value, nameof(Name));
        }

        /// <summary>
        /// Тип объекта (класс из TFlex).
        /// </summary>
        public ClassObject Type
        {
            get => _type;
            set => SetValue(ref _type, value, nameof(Type));
        }

        /// <summary>
        /// Дата создания объекта.
        /// </summary>
        public DateTime CreateDate
        {
            get => _createDate;
            set => SetValue(ref _createDate, value, nameof(CreateDate));
        }

        /// <summary>
        /// Дата последнего изменения объекта.
        /// </summary>
        public DateTime LastEditDate
        {
            get => _lastEditDate;
            set => SetValue(ref _lastEditDate, value, nameof(LastEditDate));
        }

        /// <summary>
        /// Структура объекта (в виде сериализованной строки).
        /// </summary>
        public string ObjectStruct
        {
            get => _objectStruct;
            set => SetValue(ref _objectStruct, value, nameof(ObjectStruct));
        }

        /// <summary>
        /// Заблокирован ли объект для редактирования.
        /// </summary>
        public bool IsLocked
        {
            get => _isLocked;
            set => SetValue(ref _isLocked, value, nameof(IsLocked));
        }

        /// <summary>
        /// Создает неглубокую копию текущего объекта.
        /// </summary>
        /// <returns>Копия объекта.</returns>
        public ReferenceModelInfo Clone()
        {
            return (ReferenceModelInfo)MemberwiseClone();
        }
    }
}
