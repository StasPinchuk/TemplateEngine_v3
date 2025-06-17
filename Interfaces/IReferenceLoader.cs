using System.Collections.Generic;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для загрузки и сохранения справочников.
    /// </summary>
    public interface IReferenceLoader
    {
        /// <summary>
        /// Загружает справочники из указанного файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу справочников.</param>
        /// <returns>Словарь, где ключ — имя справочника, значение — объект <see cref="ReferenceInfo"/>.</returns>
        Dictionary<string, ReferenceInfo> LoadReferences(string filePath);

        /// <summary>
        /// Сохраняет справочники в указанный файл.
        /// </summary>
        /// <param name="referenceDictionary">Справочники для сохранения.</param>
        /// <param name="filePath">Путь к файлу, в который нужно сохранить.</param>
        void SaveReferences(Dictionary<string, ReferenceInfo> referenceDictionary, string filePath);
    }
}
