using System;
using System.Diagnostics;

namespace TemplateEngine_v3.Services.FileServices
{
    /// <summary>
    /// Класс для работы с JSON файлами, включая их сериализацию и десериализацию.
    /// </summary>
    public class JsonFileManager
    {
        private readonly JsonSerializer _jsonSerializer;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="JsonFileManager"/>.
        /// </summary>
        public JsonFileManager()
        {
            _jsonSerializer = new JsonSerializer();
        }

        /// <summary>
        /// Записывает объект в JSON файл.
        /// </summary>
        /// <typeparam name="T">Тип объекта, который будет сериализован в JSON.</typeparam>
        /// <param name="itemToJson">Объект для сериализации.</param>
        /// <param name="fileName">Путь к файлу, в который будет записан JSON.</param>
        /// <returns>Возвращает <c>true</c> если запись прошла успешно, иначе <c>false</c>.</returns>
        public bool WriteToJson<T>(T itemToJson, string fileName)
        {
            try
            {
                string jsonString = _jsonSerializer.Serialize(itemToJson);

                FileService.WriteAllText(fileName, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing to JSON file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Читает объект из JSON файла.
        /// </summary>
        /// <typeparam name="T">Тип объекта, который будет десериализован.</typeparam>
        /// <param name="fileName">Путь к файлу, из которого будет считан JSON.</param>
        /// <returns>Возвращает десериализованный объект или значение по умолчанию, если возникла ошибка.</returns>
        public T ReadFromJson<T>(string fileName)
        {
            try
            {
                string jsonString = FileService.ReadAllText(fileName);
                return _jsonSerializer.Deserialize<T>(jsonString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading from JSON file: {ex.Message}");
                return default;
            }
        }
    }
}
