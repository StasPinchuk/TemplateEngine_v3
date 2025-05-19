using System.Collections.Generic;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для загрузки справочников из файлов.
    /// </summary>
    public interface IReferenceLoader
    {
        /// <summary>
        /// Загружает справочники из указанного файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу со справочниками.</param>
        /// <returns>Словарь, где ключ — имя справочника, значение — объект <see cref="ReferenceInfo"/>.</returns>
        Dictionary<string, ReferenceInfo> LoadReferences(string filePath);
    }
}
