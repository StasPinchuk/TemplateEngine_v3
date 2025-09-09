using NCalc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;
using TFlex.DOCs.Common.DataStructures.Intervals;

namespace TemplateEngine_v3.Helpers
{
    public class ExpressionResolver
    {
        private readonly TableService _tableService;
        private Dictionary<string, string> _markDictionary = new();
        private List<ConditionEvaluator> _formulas = new();
        private List<ConditionEvaluator> _parameters = new();
        private List<ConditionEvaluator> _materials = new();
        private string _materialName = string.Empty;
        private string _branchName = string.Empty;

        private Dictionary<string, Func<FunctionArgs, object>> _customFunctions;

        public ExpressionResolver(TableService tableService)
        {
            _tableService = tableService;
            _customFunctions = InitFunctions(tableService);
        }

        public void SetMarkDictionary(Dictionary<string, string> marks)
        {
            _markDictionary = marks;
        }

        public void Prepare(IEnumerable<Node> nodes, string materialName, string branchName)
        {
            _materialName = materialName;
            _branchName = branchName;
            _formulas = new();
            _parameters = new();
            _materials = new();

            foreach (var node in nodes)
            {
                AddNodeExpressions(node);
            }
        }

        public void ResolveAll()
        {
            ResolveEvaluators(_formulas);
            ResolveEvaluators(_parameters);
            ResolveEvaluators(_materials);
        }

        public object? Calculate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            value = ReplaceMarking(value);
            value = ReplaceFunctions(value);

            if (!HasOperators(value)) return value;
            if (value.Contains('[') || value.Contains(']'))
            {
                value = Regex.Replace(value, @"\[[^\]]+\]", "0");
            }
            try
            {
                var expr = new Expression(value);
                expr.EvaluateFunction += HandleFunction;
                var result = expr.Evaluate();
                return FormatResult(result);
            }
            catch
            {
                return value;
            }
        }

        public string ReplaceDesignation(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            foreach (var kvp in _markDictionary)
            {
                text = ReplaceWithQuotesIfNeeded(text, kvp.Key, kvp.Value);
            }

            foreach (var formula in _formulas)
            {
                text = ReplaceWithQuotesIfNeeded(text, formula.Name, Sanitize(formula.Value));
            }

            text = Regex.Replace(text, @"-\[[^\]]+\]", "");

            return text;
        }

        private string ReplaceWithQuotesIfNeeded(string input, string key, string value)
        {
            bool isNumber = double.TryParse(value, out _);

            string inQuotesPattern = $@"'(\[{Regex.Escape(key)}\])'";
            input = Regex.Replace(input, inQuotesPattern, m =>
            {
                return $"'{value}'";
            });

            string noQuotesPattern = $@"\[{Regex.Escape(key)}\]";
            input = Regex.Replace(input, noQuotesPattern, m =>
            {
                if (isNumber)
                    return value;
                else
                    return $"'{value}'";
            });

            return input;
        }

        public bool IsNodeUsed(Node node)
        {
            if (string.IsNullOrWhiteSpace(node.UsageCondition)) return true;
            return bool.TryParse(node.UsageCondition, out var used) && used;
        }

        private void AddNodeExpressions(Node node)
        {
            _formulas.AddRange(node.ExpressionRepository.Formulas);
            _formulas.AddRange(node.ExpressionRepository.Terms);
            if (!string.IsNullOrEmpty(node.Amount?.Value)) _formulas.Add(node.Amount);
            if (node.Parameters != null) _parameters.AddRange(node.Parameters);

            foreach (var op in node.Technologies.Operations.ToList())
            {
                var division = op.BranchDivisionDetails.FirstOrDefault(b => b.Branch?.Name == _branchName);
                if (division != null)
                {
                    division.UsageCondition = Calculate(division.UsageCondition)?.ToString();
                    if (!IsNodeUsed(new Node { UsageCondition = division.UsageCondition }))
                    {
                        node.Technologies.Operations.Remove(op);
                        continue;
                    }

                    if (division.Materials?.Name.Parts.Count > 0) _materials.Add(division.Materials.Name);
                    if (division.Materials?.Consumption.Parts.Count > 0) _materials.Add(division.Materials.Consumption);
                }
            }
        }

        private void ResolveEvaluators(List<ConditionEvaluator> evaluators)
        {
            foreach (var ev in evaluators)
            {
                ev.Value = ResolveParts(ev).Replace("'", "");
                if (ev.Name.ToLower().Contains("покрытие") && string.IsNullOrEmpty(ev.Value))
                    ev.Value = $"'{_materialName}'";

                if (_markDictionary.ContainsKey(ev.Name))
                    _markDictionary[ev.Name] = ev.Value;
            }
        }

        private string ResolveParts(ConditionEvaluator evaluator)
        {
            string result = evaluator.Value;
            foreach (var partId in evaluator.Parts)
            {
                var child = _formulas.FirstOrDefault(f => f.Id == partId);
                if (child == null) continue;

                string val = ResolveParts(child);
                val = WrapStringIfNeeded(val);
                string placeholder = $"[{child.Name}]";

                int index = result.IndexOf(placeholder);
                if (index >= 0)
                {
                    bool insideQuotes = false;

                    if (index > 0 && result[index - 1] == '\'')
                        insideQuotes = true;

                    if (index + placeholder.Length < result.Length && result[index + placeholder.Length] == '\'')
                        insideQuotes = true;

                    string newVal = insideQuotes ? val.Trim('\'') : val;
                    if (string.IsNullOrEmpty(newVal))
                        newVal = "''";
                    result = result.Replace(placeholder, newVal);
                }
            }

            if (result.StartsWith("-") &&
                !result.Contains("тогда") &&
                !result.Contains("иначе") &&
                !result.Contains(":"))
                return result;
            else
                return Calculate(result)?.ToString() ?? result;
        }

        private string WrapStringIfNeeded(string input)
        {

            if (input.Equals("''") || string.IsNullOrWhiteSpace(input))
                return string.Empty;
            else
            if (!double.TryParse(input, out _) && !bool.TryParse(input, out _))
            {
                if (!input.StartsWith("'"))
                    return $"'{input.Replace("'", "")}'";
            }
            else
            {
                return input.Replace(",", ".");
            }

            return input;
        }


        private void HandleFunction(string name, FunctionArgs args)
        {
            if (_customFunctions.TryGetValue(name, out var handler))
                args.Result = handler(args);
            else
                throw new ArgumentException($"Unknown function: {name}");
        }


        private string ReplaceMarking(string input)
        {
            foreach (var kvp in _markDictionary)
            {
                input = input.Replace($"[{kvp.Key}]", kvp.Value.All(char.IsDigit) ? kvp.Value : $"'{kvp.Value}'");
            }
            input = Regex.Replace(input, @"\[[^\]]+\]", "0");
            return input;
        }

        private string ReplaceFunctions(string value)
        {
            return value
                .Replace("ОКРУГЛВНИЗ", "Floor")
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
                .Replace("ТаблПоиск", "ReadTable")
                .Trim();
        }

        private bool HasOperators(string expression)
        {
            return expression.Any(c => "+-*/<>=()?".Contains(c));
        }


        private string Sanitize(string input) =>
            input?.Replace("[", "").Replace("]", "").Replace("'", "") ?? "";

        private string FormatResult(object result) =>
            result switch
            {
                double d => Math.Round(d, 3).ToString(),
                int i => i.ToString(),
                bool b => b.ToString(),
                _ => result?.ToString() ?? "''"
            };

        private Dictionary<string, Func<FunctionArgs, object>> InitFunctions(TableService service)
        {
            return new Dictionary<string, Func<FunctionArgs, object>>
            {
                // Округление вниз
                ["Floor"] = args => Math.Floor(Convert.ToDouble(args.Parameters[0].Evaluate())),

                // Минимальное из аргументов
                ["Min"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Min(),

                // Максимальное из аргументов
                ["Max"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Max(),

                // Среднее арифметическое
                ["Avg"] = args => args.Parameters.Select(p => Convert.ToDouble(p.Evaluate())).Average(),

                // Округление по стандарту
                ["Round"] = args => Math.Round(Convert.ToDouble(args.Parameters[0].Evaluate())),

                // Округление вверх
                ["Ceil"] = args => Math.Ceiling(Convert.ToDouble(args.Parameters[0].Evaluate())),

                // Модуль
                ["Abs"] = args => Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate())),

                // Квадратный корень
                ["Sqrt"] = args => Math.Sqrt(Convert.ToDouble(args.Parameters[0].Evaluate())),

                // Проверка на чётность
                ["IsEven"] = args => Convert.ToDouble(args.Parameters[0].Evaluate()) % 2 == 0,

                // Строка содержит подстроку
                ["Contains"] = args =>
                {
                    var a = args.Parameters[0].Evaluate()?.ToString();
                    var b = args.Parameters[1].Evaluate()?.ToString();
                    return a?.Contains(b ?? "") ?? false;
                },

                // Строгое сравнение строк
                ["Equals"] = args =>
                {
                    var a = args.Parameters[0].Evaluate()?.ToString();
                    var b = args.Parameters[1].Evaluate()?.ToString();
                    return string.Equals(a, b, StringComparison.Ordinal);
                },

                // Проверка на число
                ["IsNumber"] = args =>
                {
                    var val = args.Parameters[0].Evaluate()?.ToString();
                    return double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
                },

                // Чтение значения из таблицы
                ["ReadTable"] = args =>
                {
                    var tableName = args.Parameters[0].Evaluate()?.ToString();
                    var sheetName = args.Parameters[1].Evaluate()?.ToString();
                    var width = args.Parameters[2].Evaluate()?.ToString();
                    var height = args.Parameters[3].Evaluate()?.ToString();

                    var extraParams = args.Parameters.Skip(4).Take(args.Parameters.Length - 5)
                        .Select(p => p.Evaluate()?.ToString()).ToArray();

                    var isRange = bool.TryParse(args.Parameters.Last().Evaluate()?.ToString(), out var range) && range;

                    if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(width) || string.IsNullOrEmpty(height))
                        throw new ArgumentException("Недопустимые параметры для ReadTable");

                    return _tableService.GetFindParameter(tableName, sheetName, width, height, extraParams, isRange);
                },

                // Проверка попадания в диапазон
                ["InRange"] = args =>
                {
                    var parameters = args.Parameters.ToList();
                    var findValue = parameters.Last().Evaluate()?.ToString();

                    var rangeItems = parameters.Take(parameters.Count - 1)
                        .Select(p => p.ParsedExpression?.ToString())
                        .Where(s => !string.IsNullOrEmpty(s)).ToList();

                    var expandedRange = new List<string>();

                    foreach (var item in rangeItems)
                    {
                        if (item.Contains("-"))
                        {
                            var parts = item.Split('-');
                            if (parts.Length == 2 &&
                                int.TryParse(parts[0], out int start) &&
                                int.TryParse(parts[1], out int end))
                            {
                                for (int i = start; i <= end; i++)
                                    expandedRange.Add(i.ToString());
                            }
                        }
                        else
                        {
                            expandedRange.Add(item);
                        }
                    }

                    return expandedRange.Contains(findValue);
                }
            };
        }

    }

}
