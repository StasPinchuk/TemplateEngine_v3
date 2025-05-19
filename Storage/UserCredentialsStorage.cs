using Newtonsoft.Json;
using System;
using System.IO;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.Storage
{
    /// <summary>
    /// Класс для хранения и загрузки учетных данных пользователя из локального JSON-файла.
    /// </summary>
    public class UserCredentialsStorage : IUserCredentialsStorage
    {
        private readonly string _filePath;

        /// <summary>
        /// Инициализирует путь к файлу хранения учетных данных.
        /// </summary>
        public UserCredentialsStorage()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _filePath = Path.Combine(documentsPath, "TemplateEngine", "credentials.json");
        }

        /// <summary>
        /// Пытается загрузить учетные данные из файла.
        /// </summary>
        /// <param name="credentials">Возвращаемые учетные данные, если загрузка успешна.</param>
        /// <returns><c>true</c>, если данные успешно загружены и расшифрованы; иначе <c>false</c>.</returns>
        public bool TryLoad(out UserCredentials credentials)
        {
            credentials = null;
            if (!File.Exists(_filePath))
                return false;

            var json = File.ReadAllText(_filePath);
            credentials = JsonConvert.DeserializeObject<UserCredentials>(json);
            credentials?.Decrypt();
            return credentials != null;
        }

        /// <summary>
        /// Сохраняет учетные данные в зашифрованном виде в файл.
        /// </summary>
        /// <param name="credentials">Учетные данные для сохранения.</param>
        public void Save(UserCredentials credentials)
        {
            var encryptedCredentials = credentials.Encrypt();
            var json = JsonConvert.SerializeObject(encryptedCredentials, Formatting.Indented);

            var directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_filePath, json);
        }
    }
}
