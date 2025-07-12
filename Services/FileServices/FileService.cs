using System;
using System.Collections.Generic;
using System.IO;
using TemplateEngine_v3.Models;
using Newtonsoft.Json; // или другая библиотека для сериализации

namespace TemplateEngine_v3.Services.FileServices
{
    /// <summary>
    /// Класс для работы с файлами, включая операции чтения и записи текста.
    /// </summary>
    public static class FileService
    {
        private static readonly string _folderName = "configs";

        /// <summary>
        /// Читает все текстовое содержимое из файла.
        /// </summary>
        /// <param name="fileName">Относительный путь к файлу внутри папки configs.</param>
        /// <returns>Содержимое файла или null, если файл не найден или произошла ошибка.</returns>
        public static string ReadAllText(string fileName)
        {
            string path = GetFilePath(fileName);

            if (!File.Exists(path))
                return null;

            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file {path}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Записывает текст в файл. Автоматически добавляет расширение .json, если его нет.
        /// </summary>
        /// <param name="fileName">Имя файла внутри папки configs. Можно с расширением или без.</param>
        /// <param name="content">Содержимое для записи.</param>
        public static void WriteAllText(string fileName, string content)
        {
            EnsureDirectoryExists();
            string path = GetFilePath(fileName);

            // Добавим расширение .json, если его нет
            if (!Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                path += ".json";
            }

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, content);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error writing to file {path}: {ex.Message}");
            }
        }

        /// <summary>
        /// Возвращает полный путь к файлу внутри папки configs.
        /// </summary>
        private static string GetFilePath(string fileName)
        {
            return Path.Combine(_folderName, fileName);
        }

        /// <summary>
        /// Проверяет и создает папку configs, если она не существует.
        /// </summary>
        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_folderName))
            {
                Directory.CreateDirectory(_folderName);
            }
        }

        /// <summary>
        /// Записывает коллекцию элементов в указанный каталог, используя заданные функции для имени файла и содержимого.
        /// </summary>
        public static void WriteToFolder<T>(string folderPath, IEnumerable<T> items, Func<T, string> getName, Func<T, string> getContent)
        {
            foreach (var item in items)
            {
                string filePath = Path.Combine(folderPath, getName(item));
                WriteAllText(filePath, getContent(item));
            }
        }

        /// <summary>
        /// Считывает все шаблоны из указанной папки, десериализуя каждый файл.
        /// </summary>
        public static List<Template> ReadeFromFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath); // создаем, если нет

            var files = Directory.GetFiles(folderPath);
            List<Template> templates = new List<Template>();

            foreach (var file in files)
            {
                try
                {   
                    string content = File.ReadAllText(file);
                    var template = JsonConvert.DeserializeObject<Template>(content);
                    if (template != null)
                    {
                        templates.Add(template);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or deserializing file {file}: {ex.Message}");
                }
            }

            return templates;
        }
    }
}
