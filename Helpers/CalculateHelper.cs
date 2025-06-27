using NCalc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Helpers
{
    public class CalculateHelper
    {
        private static TableService _tableService;
        private static Dictionary<string, string> MarkDictionary = new();
        private static List<Node> Nodes = new();
        private static List<ConditionEvaluator> FormulasAndTerms = new();
        private static List<ConditionEvaluator> Materials = new();
        private static List<ConditionEvaluator> Parameters = new();
        private static readonly List<ConditionEvaluator> Amounts = new();

        private static string BranchName = string.Empty;
        private static string materialName = string.Empty;

        /// <summary>
        /// Выполняет вычисление и построение шаблона на основе текущего выбранного шаблона из менеджера,
        /// строки заказа и названия ветки (branch).
        /// </summary>
        /// <param name="templateManager">Менеджер шаблонов, предоставляющий доступ к текущему шаблону и вспомогательным сервисам.</param>
        /// <param name="orderString">Строка заказа, используемая для обработки маркировок и подбора материалов.</param>
        /// <param name="branchString">Название ветки (branch), используемое для фильтрации шаблона по веткам.</param>
        /// <returns>
        /// Возвращает вычисленный шаблон <see cref="Template"/>, в котором отфильтрованы и обработаны отношения и узлы.
        /// Если шаблон не содержит указанной ветки, возвращается <c>null</c>.
        /// </returns>
        public Template CalculateTemplate(ITemplateManager templateManager, string orderString, string branchString)
        {
            Clear();
            _tableService = templateManager.TableService;
            Template currentTemplate = templateManager.GetSelectedTemplate().Copy();
            TemplateRelations currentRelations = null;
            BranchName = branchString;
            string pattern = @"RAL\d+";

            if (!currentTemplate.Branches.Any(branch => branch.Name.Equals(branchString)))
                return null;
            Match match = Regex.Match(orderString, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                materialName = Regex.Replace(match.Value, @"RAL\s*(\d+)", "RAL $1");
            }

            if (!string.IsNullOrEmpty(materialName))
            {
                var materials = templateManager.MaterialManager.GetMaterials();
                materialName = materials.FirstOrDefault(mat => mat.Contains(materialName));
            }

            MarkDictionary = MarkingSpliter.ProcessMarking(orderString, currentTemplate.ExampleMarkings.ToList());
            var removeRelations = new List<TemplateRelations>();
            foreach (var relation in currentTemplate.TemplateRelations)
            {
                relation.IsLoggingEnabled = false;
                relation.UsageCondition = Calculate(relation.UsageCondition)?.ToString();

                if (string.IsNullOrEmpty(relation.UsageCondition) ||
                    (bool.TryParse(relation.UsageCondition, out bool usage) && usage))
                {
                    currentRelations = relation;

                    break;
                }
                else
                {
                    removeRelations.Add(relation);
                }
            }

            foreach (var relation in removeRelations)
                currentTemplate.TemplateRelations.Remove(relation);

            if (currentRelations == null)
            {
                currentTemplate.TemplateRelations = null;
                return currentTemplate;
            }

            Nodes = GetAllNode(currentRelations.Nodes.ToList());
            try
            {
                CalculateEvaluator();
            }
            catch (ArgumentException ex)
            {
            }
            ReplaceParametersOrMaterialValue(Parameters);
            ReplaceParametersOrMaterialValue(Materials);
            foreach (var node in Nodes)
            {
                node.Designation = ReplaceDesignation(node.Designation);
                node.Name = ReplaceDesignation(node.Name);
                node.UsageCondition = ReplaceValues(node.UsageCondition);
                node.UsageCondition = Calculate(node.UsageCondition)?.ToString();
                node.IsLoggingEnabled = true;
            }

            currentRelations.Designation = ReplaceDesignation(currentRelations.Designation);

            currentRelations.Nodes = new(
                currentRelations.Nodes.Where(node =>
                    (string.IsNullOrEmpty(node.UsageCondition) || (bool.TryParse(node.UsageCondition, out var usage) && usage))
                    &&
                    (!string.Equals(node.Amount?.Value, "0", StringComparison.OrdinalIgnoreCase))
                )
            );

            RemoveNodeIsNotUsage(currentRelations.Nodes);
            currentRelations.IsLoggingEnabled = true;
            return currentTemplate;
        }

        /// <summary>
        /// Очищает все статические коллекции и словари, используемые для хранения маркировок,
        /// узлов, формул, материалов и параметров.
        /// </summary>
        private static void Clear()
        {
            MarkDictionary = new();
            Nodes = new();
            FormulasAndTerms = new();
            Materials = new();
            Parameters = new();
        }

        /// <summary>
        /// Выполняет вычисление выражения, предварительно заменяя параметры маркировок и функции.
        /// Возвращает вычисленное значение, округлённое до 3 знаков после запятой, если результат числовой.
        /// Если вычисление невозможно, возвращается исходная строка после замен.
        /// </summary>
        /// <param name="value">Выражение или строка для вычисления.</param>
        /// <returns>
        /// Результат вычисления как объект.
        /// Если вычисление прошло успешно, возвращается строковое представление результата с округлением,
        /// иначе возвращается исходное значение с применёнными заменами.
        /// </returns>
        public static object Calculate(string value)
        {
            value = ReplaceMarkingParameter(value);
            value = ReplaceFunction(value);

            if (!string.IsNullOrEmpty(value) && !HasNoOperators(value))
            {
                try
                {
                    object result = EvaluateExpression(value);
                    if (result != null)
                    {
                        if (result is double doubleResult)
                        {
                            value = Math.Round(doubleResult, 3).ToString();
                        }
                        else if (result is int intResult)
                        {
                            value = $"{intResult}";
                        }
                        else if (result is bool boolResult)
                        {
                            value = $"{boolResult}";
                        }
                        else
                        {
                            value = !string.IsNullOrEmpty(result.ToString()) ? result.ToString() : "''";
                        }
                    }
                    return value;
                }
                catch (Exception)
                {
                    // Игнорируем ошибки вычисления и возвращаем исходное значение
                }
            }

            return value;
        }


        /// <summary>
        /// Рекурсивно получает список всех узлов, включая вложенные дочерние узлы.
        /// Также обновляет связанные списки формул, параметров и материалов для каждого узла.
        /// </summary>
        /// <param name="nodes">Список узлов для обхода.</param>
        /// <returns>Плоский список всех узлов из дерева.</returns>
        private static List<Node> GetAllNode(List<Node> nodes)
        {
            return nodes.SelectMany(node =>
            {
                node.IsLoggingEnabled = false;
                SetLists(node);
                return new[] { node }.Concat(GetAllNode(node.Nodes?.ToList() ?? new List<Node>()));
            }).ToList();
        }

        /// <summary>
        /// Рекурсивно удаляет из коллекции узлы, у которых условие использования (UsageCondition) равно false.
        /// Если условие не может быть распознано как булево, узел считается используемым.
        /// </summary>
        /// <param name="nodes">Коллекция узлов для очистки.</param>
        private static void RemoveNodeIsNotUsage(ObservableCollection<Node> nodes)
        {
            List<Node> removeNodes = new();
            foreach (var node in nodes)
            {
                if (bool.TryParse(node.UsageCondition, out bool result) && !result)
                {
                    removeNodes.Add(node);
                }
                else
                {
                    if (node.Nodes.Count > 0)
                        RemoveNodeIsNotUsage(node.Nodes);
                }
            }
            foreach (var removeNode in removeNodes)
            {
                nodes.Remove(removeNode);
            }
        }

        /// <summary>
        /// Обновляет внутренние списки формул, параметров и материалов для указанного узла.
        /// </summary>
        /// <param name="node">Узел, для которого обновляются списки.</param>
        private static void SetLists(Node node)
        {
            AddFormulasAndTerms(node);
            AddParameters(node);
            AddMaterials(node.Technologies);
        }

        /// <summary>
        /// Добавляет формулы, термы и количество из ExpressionRepository узла в глобальный список FormulasAndTerms.
        /// </summary>
        /// <param name="node">Узел, из которого извлекаются формулы и термы.</param>
        private static void AddFormulasAndTerms(Node node)
        {
            FormulasAndTerms.AddRange(node.ExpressionRepository.Formulas);
            FormulasAndTerms.AddRange(node.ExpressionRepository.Terms);
            if (!string.IsNullOrEmpty(node.Amount.Value))
                FormulasAndTerms.Add(node.Amount);
        }

        /// <summary>
        /// Добавляет параметры из узла в глобальный список Parameters, если они существуют.
        /// </summary>
        /// <param name="node">Узел, из которого извлекаются параметры.</param>
        private static void AddParameters(Node node)
        {
            if (node.Parameters != null)
                Parameters.AddRange(node.Parameters);
        }

        /// <summary>
        /// Добавляет материалы из операций технологий, соответствующих текущему названию ветки (BranchName).
        /// Включает в список Materials материалы, имеющие непустые части и вычисляет условие использования.
        /// </summary>
        /// <param name="technologies">Объект технологий, содержащий операции.</param>
        private static void AddMaterials(Technologies technologies)
        {
            var operations = technologies.Operations;

            foreach (Operation operation in operations)
            {
                var branchDivision = operation.BranchDivisionDetails
                    .FirstOrDefault(division => division?.Branch != null &&
                                                division.Branch.Name.Equals(BranchName));

                if (branchDivision != null && branchDivision?.Materials != null)
                {
                    branchDivision.IsLoggingEnabled = false;
                    var material = branchDivision.Materials;
                    material.IsLoggingEnabled = false;
                    material.Name.IsLoggingEnabled = false;
                    material.Consumption.IsLoggingEnabled = false;
                    if (material.Name.Parts.Count > 0)
                        Materials.Add(material.Name);
                    if (material.Consumption.Parts.Count > 0)
                        Materials.Add(material.Consumption);

                    branchDivision.UsageCondition = Calculate(branchDivision.UsageCondition)?.ToString();
                }
            }
        }

        /// <summary>
        /// Выполняет расчет значений для всех формул и условий (FormulasAndTerms),
        /// обновляя их значение и учитывая специфику для покрытия и маркировок.
        /// </summary>
        private static void CalculateEvaluator()
        {
            foreach (var evaluator in FormulasAndTerms)
            {
                var count = FormulasAndTerms.Where(ff => ff.Id.Equals("6d14dbe3-8fcb-419f-bc93-837c961e794c"));
                if (evaluator != null)
                {
                    evaluator.IsLoggingEnabled = false;
                    evaluator.Value = GetEvaluatorForId(evaluator.Id);

                    if (evaluator.Name.ToLower().Contains("покрытие") && evaluator.Value == "''")
                    {
                        evaluator.Value = $"'{materialName}'";
                    }
                    evaluator.IsLoggingEnabled = true;

                    if (MarkDictionary.ContainsKey(evaluator.Name))
                        MarkDictionary[evaluator.Name] = evaluator.Value;
                }
            }
        }

        /// <summary>
        /// Рекурсивно вычисляет значение ConditionEvaluator по его Id, подставляя значения из дочерних частей.
        /// </summary>
        /// <param name="id">Идентификатор ConditionEvaluator.</param>
        /// <returns>Вычисленное значение в виде строки.</returns>
        private static string GetEvaluatorForId(string id)
        {
            ConditionEvaluator evaluator = FormulasAndTerms.FirstOrDefault(evaluator => evaluator.Id.Equals(id));
            if (evaluator == null)
                return string.Empty;
            evaluator.IsLoggingEnabled = false;
            if (evaluator.Parts.Count > 0)
            {
                foreach (var part in evaluator.Parts)
                {
                    var resultPart = GetEvaluatorForId(part.ToString());
                    var cond = FormulasAndTerms.FirstOrDefault(evaluator => evaluator.Id.Equals(part));
                    if (cond == null)
                        continue;

                    if (decimal.TryParse(resultPart.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        evaluator.Value = evaluator.Value.Replace($"[{cond.Name}]", resultPart.Replace(",", "."));
                    }
                    else
                    {
                        if (evaluator.Value.Contains($"[{cond.Name}]"))
                        {
                            string pattern = @"(?<=['-])\[[^\]]+\](?=')";
                            var match = Regex.Match(evaluator.Value, pattern);

                            if (match.Success && match.Value.Equals($"[{cond.Name}]"))
                            {
                                if (resultPart == "''")
                                {
                                    evaluator.Value = Regex.Replace(evaluator.Value, $@"\[{cond.Name}\]", "");
                                }
                                else
                                {
                                    evaluator.Value = evaluator.Value.Replace($"[{cond.Name}]",
                                        (resultPart.StartsWith("'") && resultPart.EndsWith("'"))
                                            ? $"{resultPart.Replace("'", "")}"
                                            : $"'{resultPart.Replace(",", ".")}'");
                                }
                            }
                            else
                            {
                                evaluator.Value = evaluator.Value.Replace($"[{cond.Name}]",
                                    (resultPart.StartsWith("'") && resultPart.EndsWith("'"))
                                        ? $"{resultPart}"
                                        : $"'{resultPart.Replace(",", ".")}'");
                            }
                        }
                    }
                }

                evaluator.Parts.Clear();

                return Calculate(evaluator.Value).ToString();
            }
            else
            {
                return Calculate(evaluator.Value).ToString();
            }
        }

        /// <summary>
        /// Заменяет значения параметров или материалов внутри списка ConditionEvaluator,
        /// обновляя UsageCondition, значения и части с учетом маркировок и вычислений.
        /// </summary>
        /// <param name="evaluators">Список ConditionEvaluator для обработки.</param>
        private static void ReplaceParametersOrMaterialValue(List<ConditionEvaluator> evaluators)
        {
            foreach (var evaluator in evaluators)
            {
                evaluator.IsLoggingEnabled = false;
                if (!string.IsNullOrEmpty(evaluator.UsageCondition))
                {
                    evaluator.UsageCondition = ReplaceValues(evaluator.UsageCondition);
                    evaluator.UsageCondition = ReplaceMarkingParameter(evaluator.UsageCondition);
                    evaluator.UsageCondition = Calculate(evaluator.UsageCondition)?.ToString();
                }

                if (evaluator.Parts.Count > 0)
                {
                    var parts = new ObservableCollection<string>(evaluator.Parts);
                    foreach (var part in parts)
                    {
                        ReplaceEvaluatorValue(evaluator, part);
                    }
                }
                else
                {
                    var idProcessingCount = new Dictionary<string, int>();

                    while (true)
                    {
                        var replaceEvaluator = FormulasAndTerms.FirstOrDefault(eval => evaluator.Value.Contains(eval.Name));
                        if (replaceEvaluator == null) break;

                        if (idProcessingCount.TryGetValue(replaceEvaluator.Id, out var count))
                        {
                            if (count >= 500)
                            {
                                break;
                            }
                            idProcessingCount[replaceEvaluator.Id] = count + 1;
                        }
                        else
                        {
                            idProcessingCount[replaceEvaluator.Id] = 1;
                        }

                        ReplaceEvaluatorValue(evaluator, replaceEvaluator.Id);
                    }
                }

                evaluator.Value = ReplaceMarkingParameter(evaluator.Value.Replace("'", ""));
                evaluator.IsLoggingEnabled = true;
            }
        }

        /// <summary>
        /// Заменяет в значении <paramref name="evaluator"/> вхождение с именем из <paramref name="part"/> на его фактическое значение.
        /// </summary>
        /// <param name="evaluator">Объект ConditionEvaluator, в котором происходит замена.</param>
        /// <param name="part">Идентификатор (Id) подстановочного элемента для замены.</param>
        private static void ReplaceEvaluatorValue(ConditionEvaluator evaluator, string part)
        {
            var replaceEvaluator = FormulasAndTerms.FirstOrDefault(eval => eval.Id.Equals(part));
            if (replaceEvaluator == null) return;

            string replacement = replaceEvaluator.Value.All(char.IsDigit)
                ? replaceEvaluator.Value
                : replaceEvaluator.Value.Replace("[", "").Replace("]", "").Replace("'", "");

            evaluator.Value = evaluator.Value.Replace($"[{replaceEvaluator.Name}]", replacement);
        }

        /// <summary>
        /// Выполняет замену параметров и формул в строке <paramref name="value"/> на их значения из словаря маркировок и списка формул.
        /// Удаляет из строки подстроки вида -[...].
        /// </summary>
        /// <param name="value">Исходная строка с параметрами и формулами для замены.</param>
        /// <returns>Строка с замененными значениями.</returns>
        private static string ReplaceDesignation(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var parameters = MarkDictionary
                .Where(mark => value.Contains(mark.Key))
                .ToDictionary(mark => mark.Key, mark => mark.Value);

            foreach (var mark in parameters)
            {
                value = mark.Value.All(char.IsDigit)
                    ? value.Replace($"[{mark.Key}]", mark.Value)
                    : value.Replace($"[{mark.Key}]", $"{mark.Value}");
            }

            int count = 0;
            string id = string.Empty;
            while (true)
            {
                var replaceEvaluator = FormulasAndTerms?.FirstOrDefault(eval => !string.IsNullOrEmpty(eval.Name) && value.Contains($"[{eval.Name}]"));
                if (replaceEvaluator != null && id.Equals(replaceEvaluator.Id))
                    id = replaceEvaluator?.Id;
                else
                    count++;
                if (replaceEvaluator == null) break;

                string replacement = replaceEvaluator.Value.All(char.IsDigit)
                    ? replaceEvaluator.Value
                    : replaceEvaluator.Value.Replace("[", "").Replace("]", "").Replace("'", "");

                value = value.Replace($"[{replaceEvaluator.Name}]", replacement);
                if (count >= 10)
                    break;
            }

            string pattern = @"-\[[^\]]+\]";
            value = Regex.Replace(value, pattern, "");

            return value;
        }

        /// <summary>
        /// Выполняет замену параметров и формул в строке <paramref name="value"/> на их значения из списка формул.
        /// </summary>
        /// <param name="value">Исходная строка с параметрами и формулами для замены.</param>
        /// <returns>Строка с замененными значениями.</returns>
        private static string ReplaceValues(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            int count = 0;
            string id = string.Empty;
            while (true)
            {
                var replaceEvaluator = FormulasAndTerms?.FirstOrDefault(eval => !string.IsNullOrEmpty(eval.Name) && value.Contains($"[{eval.Name}]"));
                if (replaceEvaluator != null && id.Equals(replaceEvaluator.Id))
                    id = replaceEvaluator?.Id;
                else
                    count++;
                if (replaceEvaluator == null) break;

                string replacement = replaceEvaluator.Value.All(char.IsDigit)
                    ? replaceEvaluator.Value
                    : replaceEvaluator.Value.Replace("[", "").Replace("]", "").Replace("'", "");

                value = value.Replace($"[{replaceEvaluator.Name}]", replacement);
                if (count >= 10)
                    break;
            }

            return value;
        }

        /// <summary>
        /// Выполняет замену параметров маркировки в строке <paramref name="value"/> на их значения из словаря маркировок.
        /// После замены, все оставшиеся подстроки в квадратных скобках заменяются на "0".
        /// </summary>
        /// <param name="value">Исходная строка с параметрами маркировки.</param>
        /// <returns>Строка с произведёнными заменами.</returns>
        private static string ReplaceMarkingParameter(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var parameters = MarkDictionary
                .Where(mark => value.Contains(mark.Key))
                .ToDictionary(mark => mark.Key, mark => mark.Value);

            foreach (var mark in parameters)
            {
                if (mark.Value.All(char.IsDigit))
                {
                    value = value.Replace($"[{mark.Key}]", mark.Value);
                }
                else
                {
                    value = value.Replace($"[{mark.Key}]", $"'{mark.Value}'");
                }
            }

            value = Regex.Replace(value, @"\[[^\]]*\]", "0");

            return value;
        }

        /// <summary>
        /// Заменяет в строке функции и логические операторы на соответствующие ключевые слова или операторы для вычислителя.
        /// Например, заменяет "ОКРУГЛВНИЗ" на "Floor", "тогда" на "?" и т.д.
        /// </summary>
        /// <param name="value">Исходная строка с функциями на русском языке.</param>
        /// <returns>Строка с заменёнными функциями и операторами.</returns>
        private static string ReplaceFunction(string value)
        {
            value = value.Replace("ОКРУГЛВНИЗ", "Floor")
                         .Replace("МИН", "Min")
                         .Replace("МАКС", "Max")
                         .Replace("СРЕДНЕЕ", "Avg")
                         .Replace("ОКРУГЛЕНИЕ", "Round")
                         .Replace("МОДУЛЬ", "Abs")
                         .Replace("тогда", "?")
                         .Replace("иначе", ":")
                         .Replace("КОРЕНЬ", "Sqrt")
                         .Replace(" и ", " && ")
                         .Replace(" или ", " || ")
                         .Replace("Если", "")
                         .Replace("Содержит", "Contains")
                         .Replace("ТаблПоиск", "ReadFromTable")
                         .Trim();
            return value;
        }

        /// <summary>
        /// Проверяет, содержит ли входная строка операторы или скобки для определения,
        /// является ли выражение потенциально вычислимым.
        /// </summary>
        /// <param name="input">Входная строка с выражением.</param>
        /// <returns>True, если строка не содержит операторов; иначе false.</returns>
        public static bool HasNoOperators(string input)
        {
            // Список символов операторов для проверки
            char[] operators = { '(', ')', '?', '=', '<', '>', '+', '*', '/', '-' };

            // Возвращает true, если ни один из операторов не найден в строке
            return !input.Any(c => operators.Contains(c));
        }

        /// <summary>
        /// Вычисляет математическое или логическое выражение, используя класс Expression.
        /// Поддерживает подключаемые функции через словарь FunctionHandlers.
        /// </summary>
        /// <param name="expression">Строка с выражением для вычисления.</param>
        /// <returns>Результат вычисления выражения, либо null при ошибке.</returns>
        public static object? EvaluateExpression(string expression)
        {
            try
            {
                var expr = new Expression(expression);

                // Подписка на обработчик функций
                expr.EvaluateFunction += (name, args) =>
                {
                    if (FunctionHandlers.TryGetValue(name, out var handler))
                    {
                        args.Result = handler(args);
                    }
                    else
                    {
                        throw new ArgumentException($"Неизвестная функция: {name}");
                    }
                };

                // Вычисление выражения
                return expr.Evaluate();
            }
            catch (Exception ex)
            {
                // Логирование ошибки, например, в консоль
                Console.WriteLine($"Ошибка при вычислении выражения: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Словарь обработчиков пользовательских функций для NCalc, 
        /// где ключ — название функции, используемой в выражениях,
        /// а значение — делегат, реализующий логику функции.
        /// </summary>
        private static readonly Dictionary<string, Func<NCalc.FunctionArgs, object>> FunctionHandlers = new()
        {
            /// <summary>Округление вниз (пол).</summary>
            ["Floor"] = args => Math.Floor(Convert.ToDouble(args.Parameters[0].Evaluate())),

            /// <summary>Минимальное значение из аргументов.</summary>
            ["Min"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Min(),

            /// <summary>Максимальное значение из аргументов.</summary>
            ["Max"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Max(),

            /// <summary>Среднее арифметическое из аргументов.</summary>
            ["Avg"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Average(),

            /// <summary>Округление по математическим правилам.</summary>
            ["Round"] = args => Math.Round(Convert.ToDouble(args.Parameters[0].Evaluate())),

            /// <summary>Округление вверх (потолок).</summary>
            ["Ceil"] = args => Math.Ceiling(Convert.ToDouble(args.Parameters[0].Evaluate())),

            /// <summary>Модуль числа.</summary>
            ["Abs"] = args => Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate())),

            /// <summary>Квадратный корень.</summary>
            ["Sqrt"] = args => Math.Sqrt(Convert.ToDouble(args.Parameters[0].Evaluate())),

            /// <summary>Проверка, чётное ли число.</summary>
            ["IsEven"] = args => Convert.ToDouble(args.Parameters[0].Evaluate()) % 2 == 0,

            /// <summary>Проверяет, содержит ли первая строка вторую.</summary>
            ["Contains"] = args =>
            {
                var firstParameter = args.Parameters[0].Evaluate()?.ToString();
                var secondParameter = args.Parameters[1].Evaluate()?.ToString();

                if (firstParameter == null || secondParameter == null)
                    throw new ArgumentException("Both parameters for Contains must be strings.");

                return firstParameter.Contains(secondParameter);
            },

            /// <summary>Проверяет равенство двух строк (недоработано — сейчас работает как Contains, стоит поправить).</summary>
            ["Equals"] = args =>
            {
                var firstParameter = args.Parameters[0].Evaluate()?.ToString();
                var secondParameter = args.Parameters[1].Evaluate()?.ToString();

                if (firstParameter == null || secondParameter == null)
                    throw new ArgumentException("Both parameters for Equals must be strings.");

                // Здесь логика неверная: сейчас проверяется Contains, а нужно сравнение
                return firstParameter == secondParameter;
            },

            /// <summary>Проверяет, является ли параметр числом.</summary>
            ["IsNumber"] = args => args.Parameters[0].Evaluate() is double or int || double.TryParse(args.Parameters[0].Evaluate()?.ToString(), out _),

            /// <summary>Чтение значения из таблицы (обращение к _tableService).</summary>
            ["ReadTable"] = args =>
            {
                var count = args.Parameters.Count();
                var tableName = args.Parameters[0].Evaluate()?.ToString();
                var sheetName = args.Parameters[1].Evaluate()?.ToString();

                string width = string.Empty;
                if (Guid.TryParse(args.Parameters[2].Evaluate()?.ToString(), out Guid widthId))
                {
                    var findCondition = FormulasAndTerms.FirstOrDefault(cond => cond.Id.Equals(widthId.ToString()));
                    if (findCondition != null)
                    {
                        findCondition.Value = GetEvaluatorForId(widthId.ToString());
                        width = findCondition.Value;
                    }
                }
                else
                {
                    width = args.Parameters[2].Evaluate()?.ToString();
                }

                string height = string.Empty;
                if (Guid.TryParse(args.Parameters[3].Evaluate()?.ToString(), out Guid heightId))
                {
                    var findCondition = FormulasAndTerms.FirstOrDefault(cond => cond.Id.Equals(heightId.ToString()));
                    if (findCondition != null)
                    {
                        findCondition.Value = GetEvaluatorForId(heightId.ToString());
                        height = findCondition.Value;
                    }
                }
                else
                {
                    height = args.Parameters[3].Evaluate()?.ToString();
                }

                List<string> parameters = new();

                // Обратите внимание: корректно ли тут args.Parameters.Length - 1?
                for (int i = 4; i < args.Parameters.Length - 1; i++)
                {
                    parameters.Add(args.Parameters[i].Evaluate()?.ToString());
                }

                var isRange = bool.Parse(args.Parameters.Last().Evaluate()?.ToString());

                if (tableName == null || width == null || height == null )
                {
                    throw new ArgumentException("Invalid parameters for ReadTable.");
                }

                return _tableService.GetFindParameter(tableName, sheetName, width, height, parameters.ToArray(), isRange);
            },

            /// <summary>Проверяет, содержится ли последний параметр в остальных (InRange).</summary>
            ["InRange"] = args =>
            {
                var parameters = args.Parameters.ToList();
                var findValue = parameters.Last();
                parameters.Remove(findValue);

                List<string> parametersList = [];

                var rangeItem = parameters.FirstOrDefault(p => p.ParsedExpression.ToString().Contains("-"));
                while(rangeItem != null)
                {
                    parameters.Remove(rangeItem);

                    var parts = rangeItem.ParsedExpression.ToString().Split('-');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int start) &&
                        int.TryParse(parts[1], out int end))
                    {
                        for (int i = start; i <= end; i++)
                        {
                            parametersList.Add(i.ToString());
                        }
                    }

                    rangeItem = parameters.FirstOrDefault(p => p.ParsedExpression != null && p.ParsedExpression.ToString().Contains("-"));
                }

                foreach(var param in parameters)
                {
                    parametersList.Add(param.ParsedExpression.ToString());
                }

                if (parametersList == null || findValue == null)
                {
                    throw new ArgumentException("Invalid parameters for InRange.");
                }

                return parametersList.Contains(findValue.ParsedExpression.ToString());

            },
        };

    }
}
