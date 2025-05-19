using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TemplateEngine_v3.Services.FileServices
{
    /// <summary>
    /// Класс для сериализации и десериализации объектов в/из JSON.
    /// </summary>
    public class JsonSerializer
    {
        /// <summary>
        /// Сериализует объект в строку JSON.
        /// </summary>
        /// <typeparam name="T">Тип объекта, который будет сериализован.</typeparam>
        /// <param name="item">Объект для сериализации.</param>
        /// <returns>Строка, содержащая сериализованный JSON.</returns>
        public string Serialize<T>(T item)
        {
            return JsonConvert.SerializeObject(item, Formatting.Indented);
        }

        /// <summary>
        /// Десериализует строку JSON в объект указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип объекта, в который будет десериализован JSON.</typeparam>
        /// <param name="jsonString">Строка JSON для десериализации.</param>
        /// <returns>Объект типа <typeparamref name="T"/>.</returns>
        public T Deserialize<T>(string jsonString)
        {
            var invalidIds = new Dictionary<string, string>();
            var idRegex = new Regex(@"""id""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase);

            // Собираем все невалидные ID
            foreach (Match match in idRegex.Matches(jsonString))
            {
                string idValue = match.Groups[1].Value;
                if (!Guid.TryParse(idValue, out _) && !invalidIds.ContainsKey(idValue))
                {
                    invalidIds[idValue] = Guid.NewGuid().ToString();
                }
            }

            // Если нет невалидных ID, возвращаем десериализацию сразу
            if (invalidIds.Count == 0)
                return JsonConvert.DeserializeObject<T>(jsonString);

            // Один проход по строке: заменяем невалидные ID
            var result = new StringBuilder(jsonString);

            foreach (var kvp in invalidIds)
            {
                result.Replace($"\"{kvp.Key}\"", $"\"{kvp.Value}\"");
            }

            return JsonConvert.DeserializeObject<T>(result.ToString());
        }

    }
}
