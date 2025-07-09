using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Services.UsersServices;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Services.ServerServices
{
    /// <summary>
    /// Класс инициализирует менеджеры пользователей и справочников,
    /// обеспечивая их создание с нужными зависимостями и параметрами подключения к серверу.
    /// </summary>
    public class DefaultManagerInitializer : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Инициализирует и возвращает менеджер пользователей.
        /// </summary>
        /// <param name="connection">Активное подключение к серверу T-Flex.</param>
        /// <returns>Экземпляр UserManager с подгруженными всеми пользователями и разрешёнными пользователями.</returns>
        public UserManager InitializeUserManager(ServerConnection connection)
        {
            // Создаем менеджер всех пользователей на основе подключения
            var allUsers = new AllUsersManager(connection);

            // Создаем менеджер пользователей, которым разрешён доступ
            var allowedUsers = new AllowedUsersManager(connection);

            // Создаем и возвращаем UserManager, объединяющий всю логику пользователей
            return new UserManager(connection, allUsers, allowedUsers);
        }

        /// <summary>
        /// Инициализирует и возвращает менеджер справочников.
        /// </summary>
        /// <param name="connection">Активное подключение к серверу T-Flex.</param>
        /// <returns>Экземпляр ReferenceManager, использующий загрузчик справочников из JSON.</returns>
        public ReferenceManager InitializeReferenceManager(ServerConnection connection)
        {
            // Создаем загрузчик справочников с сервера (в текущем методе он не используется)
            var referenceLoader = new ServerReferenceLoader();

            // Создаем загрузчик справочников из JSON
            var jsonreferenceLoader = new JsonReferenceLoader();
            // Возвращаем ReferenceManager, который в текущей реализации
            // использует JSON-загрузчик для справочников
            return new ReferenceManager(connection, jsonreferenceLoader);
        }
    }
}
