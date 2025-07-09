using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model;
using TFlex.PdmFramework.Resolve;

namespace TemplateEngine_v3.Services.ServerServices
{
    /// <summary>
    /// Сервис для подключения к серверу T-Flex PDM с использованием списка конфигураций.
    /// </summary>
    public class DefaultConnectionService : BaseNotifyPropertyChanged
    {
        // Список возможных конфигураций для подключения (GUID'ы конфигураций)
        readonly List<Guid> configurationIds = new()
        {
            new Guid("00000000-0000-0000-0000-000000000000"),
            new Guid("4117ec3f-910a-4ac8-8698-2fbb8485b44d"),
            new Guid("85a43024-54a0-4b4a-9ae4-e46183ebc5d7"),
            new Guid("469ba6ce-6438-4dd0-987b-cf4120c540ce")
        };

        /// <summary>
        /// Асинхронное подключение к серверу T-Flex с использованием заданных учетных данных.
        /// </summary>
        /// <param name="credentials">Данные пользователя для подключения (логин, пароль, IP, путь к API).</param>
        /// <returns>Объект подключения к серверу или null, если подключение не удалось.</returns>
        public async Task<ServerConnection> ConnectAsync(UserCredentials credentials)
        {
            try
            {
                // Добавляем путь к API в резолвер сборок,
                // чтобы загрузить необходимые библиотеки T-Flex динамически
                AssemblyResolver.Instance.AddDirectory(credentials.ApiPath);

                // Пытаемся открыть соединение к серверу
                // с использованием первого GUID из списка конфигураций
                return await ServerConnection.OpenAsync(
                    credentials.Login,
                    credentials.Password,
                    credentials.ServerIp,
                    configurationIds.First());
            }
            catch (Exception ex)
            {
                // В случае ошибки подключения удаляем первую конфигурацию
                // из списка (вероятно, чтобы попытаться с другой конфигурацией)
                configurationIds.RemoveAt(0);

                // Можно логировать ошибку, например:
                // Debug.WriteLine($"Ошибка подключения: {ex.Message}");

                // Возвращаем null, так как подключение не удалось
                return null;
            }
        }
    }
}
