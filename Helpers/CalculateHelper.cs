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
            return currentTemplate;
        }

        private static void Clear()
        {
            MarkDictionary = new();
            Nodes = new();
            FormulasAndTerms = new();
            Materials = new();
            Parameters = new();
        }

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
                catch (Exception) { }
            }

            return value;
        }

        private static List<Node> GetAllNode(List<Node> nodes)
        {
            return nodes.SelectMany(node =>
            {
                SetLists(node);
                return new[] { node }.Concat(GetAllNode(node.Nodes?.ToList() ?? new List<Node>()));
            }).ToList();
        }

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

        private static void SetLists(Node node)
        {
            AddFormulasAndTerms(node);
            AddParameters(node);
            AddMaterials(node.Technologies);
        }

        private static void AddFormulasAndTerms(Node node)
        {
            FormulasAndTerms.AddRange(node.ExpressionRepository.Formulas);
            FormulasAndTerms.AddRange(node.ExpressionRepository.Terms);
            if (!string.IsNullOrEmpty(node.Amount.Value))
                FormulasAndTerms.Add(node.Amount);
        }

        private static void AddParameters(Node node)
        {
            if (node.Parameters != null)
                Parameters.AddRange(node.Parameters);
        }

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
                    var material = branchDivision.Materials;
                    if (material.Name.Parts.Count > 0)
                        Materials.Add(material.Name);
                    if (material.Consumption.Parts.Count > 0)
                        Materials.Add(material.Consumption);

                    branchDivision.UsageCondition = Calculate(branchDivision.UsageCondition)?.ToString();
                }
            }
        }

        private static void CalculateEvaluator()
        {
            foreach (var evaluator in FormulasAndTerms)
            {
                var count = FormulasAndTerms.Where(ff => ff.Id.Equals("6d14dbe3-8fcb-419f-bc93-837c961e794c"));
                if (evaluator != null)
                {
                    evaluator.Value = GetEvaluatorForId(evaluator.Id);

                    if (evaluator.Name.ToLower().Contains("покрытие") && evaluator.Value == "''")
                    {
                        evaluator.Value = $"'{materialName}'";
                    }

                    if (MarkDictionary.ContainsKey(evaluator.Name))
                        MarkDictionary[evaluator.Name] = evaluator.Value;
                }
            }
        }

        private static string GetEvaluatorForId(string id)
        {
            ConditionEvaluator evaluator = FormulasAndTerms.FirstOrDefault(evaluator => evaluator.Id.Equals(id));
            if (evaluator == null)
                return string.Empty;
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

                            // Определим, перед и после ли есть кавычки
                            var match = Regex.Match(evaluator.Value, pattern);

                            if (match.Success && match.Value.Equals($"[{cond.Name}]"))
                            {
                                // Если перед и после [{cond.Name}] есть одинарные кавычки, и resultPart пустой, то удаляем
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
                                // Иначе подставляем resultPart в нужном формате
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

        private static void ReplaceParametersOrMaterialValue(List<ConditionEvaluator> evaluators)
        {
            foreach (var evaluator in evaluators)
            {
                // Замена в UsageCondition
                if (!string.IsNullOrEmpty(evaluator.UsageCondition))
                {
                    evaluator.UsageCondition = ReplaceValues(evaluator.UsageCondition);
                    evaluator.UsageCondition = ReplaceMarkingParameter(evaluator.UsageCondition);
                    evaluator.UsageCondition = Calculate(evaluator.UsageCondition)?.ToString();
                }

                // Обработка Parts
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
                    // Словарь для отслеживания количества обработок каждого Id
                    var idProcessingCount = new Dictionary<string, int>();

                    while (true)
                    {
                        // Найти первый совпадающий элемент
                        var replaceEvaluator = FormulasAndTerms.FirstOrDefault(eval => evaluator.Value.Contains(eval.Name));
                        if (replaceEvaluator == null) break;

                        // Проверить количество обработок для текущего Id
                        if (idProcessingCount.TryGetValue(replaceEvaluator.Id, out var count))
                        {
                            if (count >= 500)
                            {
                                break;
                            }
                            idProcessingCount[replaceEvaluator.Id] = count + 1; // Увеличить счётчик обработок
                        }
                        else
                        {
                            idProcessingCount[replaceEvaluator.Id] = 1; // Инициализация для нового Id
                        }

                        // Выполнить замену значения
                        ReplaceEvaluatorValue(evaluator, replaceEvaluator.Id);
                    }

                }

                // Финальная замена в Value
                evaluator.Value = ReplaceMarkingParameter(evaluator.Value.Replace("'", ""));
            }
        }

        private static void ReplaceEvaluatorValue(ConditionEvaluator evaluator, string part)
        {
            var replaceEvaluator = FormulasAndTerms.FirstOrDefault(eval => eval.Id.Equals(part));
            if (replaceEvaluator == null) return;

            string replacement = replaceEvaluator.Value.All(char.IsDigit)
                ? replaceEvaluator.Value
                : replaceEvaluator.Value.Replace("[", "").Replace("]", "").Replace("'", "");

            evaluator.Value = evaluator.Value.Replace($"[{replaceEvaluator.Name}]", replacement);
        }

        private static string ReplaceDesignation(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Создаем карту замен из MarkDictionary
            var parameters = MarkDictionary
                .Where(mark => value.Contains(mark.Key))
                .ToDictionary(mark => mark.Key, mark => mark.Value);

            // Замена значений из MarkDictionary
            foreach (var mark in parameters)
            {
                value = mark.Value.All(char.IsDigit)
                    ? value.Replace($"[{mark.Key}]", mark.Value)
                    : value.Replace($"[{mark.Key}]", $"{mark.Value}");
            }

            // Замена значений из FormulasAndTerms
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

            // Определим, перед и после ли есть кавычки
            value = Regex.Replace(value, pattern, "");

            return value;
        }

        private static string ReplaceValues(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Замена значений из FormulasAndTerms
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

        private static string ReplaceMarkingParameter(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Получаем список параметров, которые нужно заменить
            var parameters = MarkDictionary
                .Where(mark => value.Contains(mark.Key))
                .ToDictionary(mark => mark.Key, mark => mark.Value);

            // Выполняем замену
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

            // Заменяем текст в квадратных скобках на 0
            value = Regex.Replace(value, @"\[[^\]]*\]", "0");

            return value;
        }

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

        public static bool HasNoOperators(string input)
        {
            // Список символов, которые нужно проверить
            char[] operators = { '(', ')', '?', '=', '<', '>', '+', '*', '/', '-' };

            // Проверяем, содержит ли строка хотя бы один из символов
            return !input.Any(c => operators.Contains(c));
        }

        public static object? EvaluateExpression(string expression)
        {
            try
            {
                var expr = new Expression(expression);

                // Подключаем обработку функций
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

                // Вычисляем выражение
                return expr.Evaluate();
            }
            catch (Exception ex)
            {
                // Логируем ошибку (например, в файл)
                Console.WriteLine($"Ошибка при вычислении выражения: {ex.Message}");
                return null; // Или любое значение по умолчанию
            }
        }

        private static readonly Dictionary<string, Func<NCalc.FunctionArgs, object>> FunctionHandlers = new()
        {
            ["Floor"] = args => Math.Floor(Convert.ToDouble(args.Parameters[0].Evaluate())),
            ["Min"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Min(),
            ["Max"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Max(),
            ["Avg"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Average(),
            ["Round"] = args => Math.Round(Convert.ToDouble(args.Parameters[0].Evaluate())),
            ["Ceil"] = args => Math.Ceiling(Convert.ToDouble(args.Parameters[0].Evaluate())),
            ["Abs"] = args => Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate())),
            ["Sqrt"] = args => Math.Sqrt(Convert.ToDouble(args.Parameters[0].Evaluate())),
            ["IsEven"] = args => Convert.ToDouble(args.Parameters[0].Evaluate()) % 2 == 0,
            ["Contains"] = args =>
            {
                var firstParameter = args.Parameters[0].Evaluate()?.ToString();
                var secondParameter = args.Parameters[1].Evaluate()?.ToString();

                if (firstParameter == null || secondParameter == null)
                {
                    throw new ArgumentException("Both parameters for Contains must be strings.");
                }

                return firstParameter.Contains(secondParameter);
            },
            ["Equals"] = args =>
            {
                var firstParameter = args.Parameters[0].Evaluate()?.ToString();
                var secondParameter = args.Parameters[1].Evaluate()?.ToString();

                if (firstParameter == null || secondParameter == null)
                {
                    throw new ArgumentException("Both parameters for Contains must be strings.");
                }

                return firstParameter.Contains(secondParameter);
            },
            ["IsNumber"] = args => args.Parameters[0].Evaluate() is double or int || double.TryParse(args.Parameters[0].Evaluate()?.ToString(), out _), // Функция проверки числа
            ["ReadTable"] = args =>
            {
                var count = args.Parameters.Count();
                var tableName = args.Parameters[0].Evaluate()?.ToString();
                string width = string.Empty;
                if (Guid.TryParse(args.Parameters[1].Evaluate()?.ToString(), out Guid widthId))
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
                    width = args.Parameters[1].Evaluate()?.ToString();
                }
                string heigth = string.Empty;
                if (Guid.TryParse(args.Parameters[2].Evaluate()?.ToString(), out Guid heigthId))
                {
                    var findCondition = FormulasAndTerms.FirstOrDefault(cond => cond.Id.Equals(heigthId.ToString()));
                    if (findCondition != null)
                    {
                        findCondition.Value = GetEvaluatorForId(heigthId.ToString());
                        heigth = findCondition.Value;
                    }
                }
                else
                {
                    heigth = args.Parameters[2].Evaluate()?.ToString();
                }

                List<string> parameters = [];

                for (int i = 3; i < args.Parameters.Length - 1; i++)
                {
                    parameters.Add(args.Parameters[i].Evaluate()?.ToString());
                }

                var isRange = bool.Parse(args.Parameters.Last().Evaluate()?.ToString());
                if (tableName == null || width == null || heigth == null || parameters.Count == 0)
                {
                    throw new ArgumentException("Both parameters for Contains must be strings.");
                }

                return _tableService.GetFindParameter(tableName, width, heigth, parameters.ToArray(), isRange);
            },
            ["InRange"] = args =>
            {
                var parameters = args.Parameters.ToList();
                var findValue = parameters.Last();

                parameters.Remove(parameters.Last());

                if (parameters == null || findValue == null)
                {
                    throw new ArgumentException("Both parameters for Contains must be strings.");
                }

                return parameters.Contains(findValue);
            },

        };
    }
}
