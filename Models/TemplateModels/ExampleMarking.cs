using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateEngine_v3.Models.TemplateModels
{
    public class ExampleMarking : BaseNotifyPropertyChanged
    {
        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    SetValue(ref _text, value, nameof(Text));
                }
            }
        }
    }
}
