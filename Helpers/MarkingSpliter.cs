using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TemplateEngine_v3.Helpers
{
    public static class MarkingSpliter
    {
        internal static readonly char[] separator = { '*', '-', ' ', '_', 'x' };
        internal static readonly char[] separatorArray = { ' ' };
        internal static readonly char[] separatorArray0 = { ' ', '_' };

        internal static readonly Dictionary<string, string[]> patterDictionary = new()
        {
            { "РЕГ-СМАРТ", new string[] {
                    @"РЕГ-СМАРТ",
                    @"\d+\*ф",
                    @"LM\d+VAV\d+(-V)?",
                }
            },
            { "", new string[] {
                    @"_.*$",
                    @"(?<=^ЭПВ-)([A-Za-z0-9\-]+(?:-[A-Za-z0-9]+)*)",
                    @"(?<=_)[A-Za-zА-ЯЁа-яё]+-[A-Za-zА-ЯЁа-яё\d]+(?=_)",
                    @"(?<!\d)([A-Za-z]{3,}-[A-Za-zА-ЯЁ]{1,2}-)",
                    @"[A-Za-z]+\d+-[A-Za-z]+(?:-[A-Za-z])?",
                    @"(?<!\d)([A-Za-zА-ЯЁ]+[0-9]+[A-Za-zА-ЯЁ0-9]*-[A-Za-z]{1}(?:-[A-Za-z]{1})?)",
                    @"(\d+\*[A-Za-zА-Яа-яЁё]{1}-)",
                    @"_\d{9}-\d{2}-[А-Я]{3}",
                    @"\b[A-Z]{2,}\d*-[A-Z]{2,}-[A-Z0-9]+-[A-Z]\b",
                }
            }
        };

        public static Dictionary<string, string> ProcessMarking(string markingText, List<string> templates)
        {
            Dictionary<string, string> MarkingMap = new Dictionary<string, string>();

            foreach (string template in templates)
            {
                string marking = markingText;


                string[] patterns = null;

                foreach (var key in patterDictionary.Keys)
                {
                    if (marking.Contains(key))
                    {
                        patterns = patterDictionary[key];
                        break;
                    }
                }

                string upperPattern = @"([A-ZА-ЯЁ]{2,})-([A-ZА-ЯЁ]{2,})|([A-ZА-ЯЁ]{2,}) ([A-ZА-ЯЁ]{2,})";

                if (!TryExtractUppercaseStart(ref marking, out int index))
                {
                    return null;
                }

                var replacements = new Dictionary<string, string>();

                marking = Regex.Replace(marking, upperPattern, "$1-$2");

                // Заменяем найденные паттерны на временные ключи
                ReplacePatterns(ref marking, patterns, replacements);

                // Извлекаем и обрабатываем шаблон
                string filteredString = new(template.Where(c => c == '*' || c == '-' || c == ' ' || c == '_' || c == 'x').ToArray());
                string[] parts = template.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                if (!IsMatchingSequence(filteredString, marking))
                {
                    continue;
                }
                // Заменяем символы * и - на пробел
                string newMarking = marking.Replace('*', ' ').Replace('-', ' ').Replace('_', ' ').Replace('x', ' ');

                // Разделяем marking на части
                string[] values = newMarking.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != parts.Length)
                    ReplaceDoubleNameFormat(ref marking, replacements);
                newMarking = marking.Replace('*', ' ').Replace('-', ' ').Replace('_', ' ');
                values = newMarking.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != parts.Length)
                    continue;
                MarkingMap = CreateValueMap(parts, marking, replacements);
                if (MarkingMap == null)
                    continue;
                foreach (var key in MarkingMap.Keys.ToList())
                {
                    if (replacements.TryGetValue(MarkingMap[key], out var newValue))
                    {
                        MarkingMap[key] = newValue;

                    }
                }
                return MarkingMap;
            }
            return null;
        }

        /// <summary>
        /// Извлекает и выводит имя из строки маркировки.
        /// </summary>
        /// <param name="marking">Строка маркировки.</param>
        private static bool IsMatchingSequence(string template, string marking)
        {
            int index = 0;
            foreach (char c in template)
            {
                index = marking.IndexOf(c, index);
                if (index == -1)
                    return false;
                index++;
            }
            return true;
        }

        /// <summary>
        /// Находит и извлекает первое слово в верхнем регистре.
        /// </summary>
        /// <param name="marking">Строка маркировки.</param>
        /// <param name="index">Индекс начала найденного слова.</param>
        /// <returns>True, если слово найдено; иначе False.</returns>
        private static bool TryExtractUppercaseStart(ref string marking, out int index)
        {
            Match match = Regex.Match(marking, @"\b[A-ZА-ЯЁ]{2,}[A-ZА-ЯЁ0-9]*(?:-[A-ZА-ЯЁ0-9]+)?\b");
            if (match.Success)
            {
                index = match.Index;
                marking = marking.Substring(index);
                return true;
            }
            else
            {
                index = marking.IndexOf("Torne");
                marking = marking.Substring(index);
                if (index != -1)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Заменяет паттерны в строке маркировки на временные ключи.
        /// </summary>
        /// <param name="marking">Строка маркировки.</param>
        /// <param name="patterns">Список регулярных выражений для замены.</param>
        /// <param name="replacements">Словарь для сохранения оригинальных значений.</param>
        private static void ReplacePatterns(ref string marking, string[] patterns, Dictionary<string, string> replacements)
        {
            foreach (var pattern in patterns)
            {
                MatchCollection matches = Regex.Matches(marking, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    var tempKey = $"TEMP?{replacements.Count}"; // Создаем временный ключ
                    replacements[tempKey.TrimEnd('_')] = match.Value.TrimEnd('-'); // Сохраняем оригинал

                    // Проверяем, находится ли совпадение в конце строки
                    if (marking.EndsWith(match.Value))
                    {
                        marking = marking.Replace(match.Value.TrimEnd('-'), "_" + tempKey); // Добавляем дефис перед заменой
                    }
                    else
                    {
                        marking = marking.Replace(match.Value.TrimEnd('-'), "_" + tempKey); // Заменяем на временный ключ
                    }
                }
            }

        }

        /// <summary>
        /// Заменяет формат двойных имен на временные ключи.
        /// </summary>
        /// <param name="marking">Строка маркировки.</param>
        /// <param name="replacements">Словарь для сохранения оригинальных значений.</param>
        private static void ReplaceDoubleNameFormat(ref string marking, Dictionary<string, string> replacements)
        {
            string hermikPattern = @"^\s*([A-ZА-ЯЁ]{2,}-(?:[A-ZА-ЯЁ]+|\d{1}))\b";
            MatchCollection matches = Regex.Matches(marking, hermikPattern);
            foreach (Match match in matches)
            {
                string tempKey = $"TEMP?{replacements.Count}_"; // Создаем временный ключ
                replacements[tempKey.TrimEnd('_')] = match.Value; // Сохраняем оригинал
                marking = marking.Replace(match.Value, tempKey); // Заменяем на временный ключ
            }
        }

        /// <summary>
        /// Создает карту значений на основе частей шаблона и маркировки.
        /// </summary>
        /// <param name="parts">Части шаблона.</param>
        /// <param name="marking">Строка маркировки.</param>
        /// <returns>Словарь значений или null, если части не совпадают по длине.</returns>
        private static Dictionary<string, string>? CreateValueMap(string[] parts, string marking, Dictionary<string, string> replacements)
        {
            // Восстанавливаем временные значения перед разбиением

            marking = marking.Replace('*', ' ').Replace('-', ' ');

            // Разделяем на части, включая поддержание '_'
            string[] values = marking.Split(separatorArray0, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length < parts.Length)
            {
                return null;
            }

            Dictionary<string, string> map = parts.ToDictionary(part => part, part => string.Empty);

            for (int i = 0; i < parts.Length; i++)
            {
                map[parts[i]] = values[i];
            }

            foreach (var replacement in replacements)
            {
                if (map.Values.Any(x => x.Contains(replacement.Key)))
                {
                    var key = map.FirstOrDefault(x => x.Value.Contains(replacement.Key)).Key;
                    map[key] = map[key].Replace(replacement.Key, replacement.Value).Trim('_');
                }
            }

            return map;
        }

    }
}
