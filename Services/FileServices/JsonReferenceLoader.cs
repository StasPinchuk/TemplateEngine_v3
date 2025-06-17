using System.Collections.Generic;
using TemplateEngine_v3.Interfaces;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Services.FileServices
{
    /// <summary>
    /// Класс для загрузки и сохранения справочников в формате JSON.
    /// Реализует интерфейс <see cref="IReferenceLoader"/>.
    /// </summary>
    public class JsonReferenceLoader : IReferenceLoader
    {
        /// <summary>
        /// Менеджер для работы с JSON файлами.
        /// </summary>
        private readonly JsonFileManager _jsonFileManager;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="JsonReferenceLoader"/>.
        /// Создаёт внутренний объект <see cref="JsonFileManager"/> для операций с JSON.
        /// </summary>
        public JsonReferenceLoader()
        {
            _jsonFileManager = new JsonFileManager();
        }

        /// <summary>
        /// Загружает справочники из JSON файла по указанному пути.
        /// </summary>
        /// <param name="filePath">Путь к JSON файлу.</param>
        /// <returns>Словарь справочников с ключом типа <see cref="string"/> и значением <see cref="ReferenceInfo"/>.</returns>
        public Dictionary<string, ReferenceInfo> LoadReferences(string filePath)
        {
            return _jsonFileManager.ReadFromJson<Dictionary<string, ReferenceInfo>>(filePath);
        }

        /// <summary>
        /// Сохраняет словарь справочников в JSON файл по указанному пути.
        /// </summary>
        /// <param name="referenceDictionary">Словарь справочников для сохранения.</param>
        /// <param name="filePath">Путь к JSON файлу, куда будет выполнена запись.</param>
        public void SaveReferences(Dictionary<string, ReferenceInfo> referenceDictionary, string filePath)
        {
            _jsonFileManager.WriteToJson(referenceDictionary, filePath);
        }
    }
}
