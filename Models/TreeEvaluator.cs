using System.Collections.ObjectModel;

namespace TemplateEngine_v3.Models
{
    public class TreeEvaluator
    {
        public ConditionEvaluator ConditionEvaluator { get; set; }
        public ObservableCollection<TreeEvaluator> TreeEvaluators { get; set; } = new();
    }
}
