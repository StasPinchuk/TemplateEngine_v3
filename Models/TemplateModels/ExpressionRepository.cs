using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TemplateEngine_v3.Models
{
    public class ExpressionRepository : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Коллекция формул.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> Formulas { get; set; } = [];

        /// <summary>
        /// Коллекция переменных терминов.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> Terms { get; set; } = [];

        public ExpressionRepository Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ExpressionRepository>(json);
        }
    }
}
