using System.Collections.Generic;
using System.Collections.ObjectModel;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для управления коллекциями оценщиков условий (ConditionEvaluator).
    /// </summary>
    public interface IEvaluatorManager
    {
        /// <summary>
        /// Системные оценщики условий.
        /// </summary>
        List<ConditionEvaluator> SystemEvaluators { get; set; }

        /// <summary>
        /// Все оценщики условий, связанные с шаблоном.
        /// </summary>
        ObservableCollection<ConditionEvaluator> AllTemplateEvaluator { get; set; }

        /// <summary>
        /// Оценщики условий, связанные с узлами.
        /// </summary>
        ObservableCollection<ConditionEvaluator> NodeEvaluators { get; set; }

        /// <summary>
        /// Устанавливает все оценщики шаблона на основе переданных связей шаблона.
        /// </summary>
        /// <param name="relations">Связи шаблона.</param>
        void SetAllTemplateEvaluator(ref TemplateRelations relations);

        /// <summary>
        /// Устанавливает оценщики условий для заданного узла.
        /// </summary>
        /// <param name="node">Узел для установки оценщиков.</param>
        void SetNodeEvaluators(Node node);

        /// <summary>
        /// Получает список оценщиков условий для заданной коллекции узлов.
        /// </summary>
        /// <param name="node">Коллекция узлов.</param>
        /// <returns>Список оценщиков условий.</returns>
        List<ConditionEvaluator> GetNodeEvaluators(ObservableCollection<Node> node);

        /// <summary>
        /// Обновляет коллекцию оценщиков шаблона.
        /// </summary>
        void UpdateTemplateEvaluator();
    }
}
