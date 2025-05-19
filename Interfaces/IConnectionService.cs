using System.Threading.Tasks;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для установления соединения с сервером.
    /// </summary>
    public interface IConnectionService
    {
        /// <summary>
        /// Асинхронно устанавливает соединение с сервером используя указанные учетные данные пользователя.
        /// </summary>
        /// <param name="credentials">Учетные данные пользователя.</param>
        /// <returns>Задача, возвращающая объект <see cref="ServerConnection"/> при успешном соединении.</returns>
        Task<ServerConnection> ConnectAsync(UserCredentials credentials);
    }
}
