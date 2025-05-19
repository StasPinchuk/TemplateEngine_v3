using System.ComponentModel;

namespace TemplateEngine_v3.Models
{
    public class ParameterItem : BaseNotifyPropertyChanged
    {
        private string _key;

        public string Key
        {
            get => _key;
            set
            {
                _key = value;

                SetValue(ref _key, value, nameof(Key));
            }
        }

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    SetValue(ref _value, value, nameof(Value));
                }
            }
        }
    }

}
