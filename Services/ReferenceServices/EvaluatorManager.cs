using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using static TFlex.DOCs.Model.References.Materials.MaterialReferenceObject;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Менеджер для работы с ConditionEvaluator (условными вычислителями).
    /// Содержит список системных вычислителей и методы для сбора вычислителей из шаблонных узлов.
    /// </summary>
    public class EvaluatorManager : IEvaluatorManager
    {
        private TemplateRelations _relations;

        /// <summary>
        /// Предопределённый набор системных вычислителей с именами и шаблонами значений.
        /// </summary>
        public List<ConditionEvaluator> SystemEvaluators { get; set; } = new()
        {
            new(){ Name = "Среднее значение", Value = "Avg( , , )"},
            new(){ Name = "Минимальное число", Value = "Min( , , )"},
            new(){ Name = "Максимальное число", Value = "Max( , , )"},
            new(){ Name = "Корень", Value = "Sqrt( , , )"},
            new(){ Name = "Округление вниз", Value = "Floor()"},
            new(){ Name = "Округление вверх", Value = "Round()"},
            new(){ Name = "Модуль", Value = "Abs()"},
            new(){ Name = "Содержит", Value = "Содержит(,)"},
            new(){ Name = "Ceil", Value = "Ceil()"},
            new(){ Name = "Четное", Value = "IsEven()"},
            new(){ Name = "Число", Value = "IsNumber()"}
        };

        /// <summary>
        /// Все вычислители, найденные в шаблоне (рекурсивно в узлах).
        /// </summary>
        public ObservableCollection<ConditionEvaluator> AllTemplateEvaluator { get; set; } = [];
        public ObservableCollection<ConditionEvaluator> AllTemplateParameters{ get; set; } = [];
        public ObservableCollection<string> TemplateMarkings{ get; set; } = [];

        /// <summary>
        /// Вычислители, принадлежащие конкретному узлу.
        /// </summary>
        public ObservableCollection<ConditionEvaluator> NodeEvaluators { get; set; }

        /// <summary>
        /// Конструктор, инициализирует менеджер, собирая все вычислители из переданных связей шаблона.
        /// </summary>
        /// <param name="relations">Объект с отношениями шаблона (узлы, связи и т.д.).</param>
        public EvaluatorManager(TemplateRelations relations)
        {
            SetAllTemplateEvaluator(ref relations);
            SetAllTemplateParameters(ref relations);
        }
        public EvaluatorManager(Template template, TemplateRelations relations)
        {
            TemplateMarkings = new(template.ProductMarkingAttributes);
            SetAllTemplateEvaluator(ref relations);
            SetAllTemplateParameters(ref relations);
        }

        /// <summary>
        /// Рекурсивно собирает все ConditionEvaluator из списка узлов.
        /// Для каждого узла собирает формулы и термы, а также рекурсивно их дочерние узлы.
        /// </summary>
        /// <param name="nodes">Коллекция узлов для поиска вычислителей.</param>
        /// <returns>Список всех вычислителей из переданных узлов и их потомков.</returns>
        public List<ConditionEvaluator> GetNodeEvaluators(ObservableCollection<Node> nodes)
        {
            // Создаём пустой список
            List<ConditionEvaluator> conditionEvaluators = new();

            // Обходим все узлы
            foreach (var currentNode in nodes)
            {
                // Добавляем вычислители из формул и термов текущего узла
                conditionEvaluators.AddRange(currentNode.ExpressionRepository.Formulas);
                conditionEvaluators.AddRange(currentNode.ExpressionRepository.Terms);

                // Рекурсивно получаем вычислители из дочерних узлов
                var newEvaluatorsList = GetNodeEvaluators(currentNode.Nodes);
                conditionEvaluators.AddRange(newEvaluatorsList);
            }

            return conditionEvaluators;
        }

        public List<ConditionEvaluator> GetNodeParameters(ObservableCollection<Node> nodes)
        {
            // Создаём пустой список
            List<ConditionEvaluator> conditionEvaluators = new();

            // Обходим все узлы
            foreach (var currentNode in nodes)
            {
                // Добавляем вычислители из формул и термов текущего узла
                conditionEvaluators.AddRange(currentNode.Parameters);

                // Рекурсивно получаем вычислители из дочерних узлов
                var newEvaluatorsList = GetNodeParameters(currentNode.Nodes);
                conditionEvaluators.AddRange(newEvaluatorsList);
            }

            return conditionEvaluators;
        }

        /// <summary>
        /// Инициализирует поле AllTemplateEvaluator — собирает все вычислители из переданных отношений шаблона.
        /// </summary>
        /// <param name="relations">Отношения шаблона, которые нужно проанализировать.</param>
        public void SetAllTemplateEvaluator(ref TemplateRelations relations)
        {
            _relations = relations;
            AllTemplateEvaluator = new ObservableCollection<ConditionEvaluator>(GetNodeEvaluators(relations.Nodes));
        }

        public void SetAllTemplateParameters(ref TemplateRelations relations)
        {
            _relations = relations;
            AllTemplateParameters = new ObservableCollection<ConditionEvaluator>(GetNodeParameters(relations.Nodes));
        }

        /// <summary>
        /// Устанавливает вычислители для одного узла.
        /// Берёт формулы и термы из ExpressionRepository узла.
        /// </summary>
        /// <param name="node">Узел, вычислители которого нужно установить.</param>
        public void SetNodeEvaluators(Node node)
        {
            List<ConditionEvaluator> evaluators = new();
            evaluators.AddRange(node.ExpressionRepository.Formulas);
            evaluators.AddRange(node.ExpressionRepository.Terms);
            NodeEvaluators = new ObservableCollection<ConditionEvaluator>(evaluators);
        }

        /// <summary>
        /// Обновляет коллекцию всех вычислителей, пересобирая её из текущих отношений шаблона.
        /// </summary>
        public void UpdateTemplateEvaluator()
        {
            AllTemplateEvaluator = new ObservableCollection<ConditionEvaluator>(GetNodeEvaluators(_relations.Nodes));
        }
    }
}
