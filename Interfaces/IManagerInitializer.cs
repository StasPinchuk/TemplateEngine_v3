using TemplateEngine_v3.Services.UsersServices;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для инициализации менеджеров с использованием подключения к серверу.
    /// </summary>
    public interface IManagerInitializer
    {
        /// <summary>
        /// Инициализирует менеджер пользователей на основе подключения к серверу.
        /// </summary>
        /// <param name="connection">Объект подключения к серверу.</param>
        /// <returns>Экземпляр <see cref="UserManager"/>.</returns>
        UserManager InitializeUserManager(ServerConnection connection);

        /// <summary>
        /// Инициализирует менеджер справочников на основе подключения к серверу.
        /// </summary>
        /// <param name="connection">Объект подключения к серверу.</param>
        /// <returns>Экземпляр <see cref="ReferenceManager"/>.</returns>
        ReferenceManager InitializeReferenceManager(ServerConnection connection);
    }
}
