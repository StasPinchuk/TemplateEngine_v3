using System;
using TemplateEngine_v3.Models.PageCollection;
using TFlex.DOCs.Model.Classes;

namespace TemplateEngine_v3.Models
{
    public enum ReferenceType
    {
        Template,
        Technologies,
        Branch
    }

    public class ReferenceModelInfo : BaseNotifyPropertyChanged
    {
        private Guid _id;
        private string _name = string.Empty;
        private ClassObject _type = null;
        private DateTime _createDate = DateTime.MinValue;
        private DateTime _lastEditDate = DateTime.MinValue;
        private string _objectStruct = string.Empty;
        private bool _isLocked = false;

        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value, nameof(Id));
        }

        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value, nameof(Name));
        }

        public ClassObject Type
        {
            get => _type;
            set => SetValue(ref _type, value, nameof(Type));
        }

        public DateTime CreateDate
        {
            get => _createDate;
            set => SetValue(ref _createDate, value, nameof(CreateDate));
        }

        public DateTime LastEditDate
        {
            get => _lastEditDate;
            set => SetValue(ref _lastEditDate, value, nameof(LastEditDate));
        }

        public string ObjectStruct
        {
            get => _objectStruct;
            set => SetValue(ref _objectStruct, value, nameof(ObjectStruct));
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetValue(ref _isLocked, value, nameof(IsLocked));
        }

        public ReferenceModelInfo Clone()
        {
            return (ReferenceModelInfo)MemberwiseClone();
        }
    }
}
