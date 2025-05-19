using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace TemplateEngine_v3.Converters
{
    /// <summary>
    /// JSON-конвертер для десериализации списка строк, представляющих имена атрибутов маркировки продукции.
    /// Поддерживает как объекты с полем "Name", так и обычные строки.
    /// </summary>
    public class ProductMarkingAttributesNameListConverter : JsonConverter<List<string>>
    {
        /// <summary>
        /// Десериализует JSON-массив, содержащий строки или объекты с полем "Name", в список строк.
        /// </summary>
        /// <param name="reader">Читатель JSON.</param>
        /// <param name="objectType">Тип объекта, ожидаемого на выходе.</param>
        /// <param name="existingValue">Существующее значение объекта (если есть).</param>
        /// <param name="hasExistingValue">Указывает, существует ли уже значение.</param>
        /// <param name="serializer">Сериализатор, используемый для вложенной сериализации.</param>
        /// <returns>Список строк, полученных из массива.</returns>
        public override List<string>? ReadJson(JsonReader reader, Type objectType, List<string>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jArray = JArray.Load(reader);
            var result = new List<string>();

            foreach (var item in jArray)
            {
                if (item.Type == JTokenType.Object && item["Name"] != null)
                    result.Add(item["Name"]!.ToString());
                else if (item.Type == JTokenType.String)
                    result.Add(item.ToString());
            }

            return result;
        }

        /// <summary>
        /// Сериализует список строк в JSON-массив.
        /// </summary>
        /// <param name="writer">Писатель JSON.</param>
        /// <param name="value">Список строк для сериализации.</param>
        /// <param name="serializer">Сериализатор, используемый для сериализации значений.</param>
        public override void WriteJson(JsonWriter writer, List<string>? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
