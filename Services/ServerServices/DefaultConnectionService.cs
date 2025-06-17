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
    public class DefaultConnectionService : IConnectionService
    {
        // Список возможных конфигураций для подключения (GUID'ы конфигураций)
        readonly List<Guid> configurationIds = new()
        {
            new Guid("00000000-0000-0000-0000-000000000000"),
            new Guid("f92b4541-65c5-4331-8ce2-99a68ae3eae9"),
            new Guid("3ebc68d3-356b-4d6e-ac52-bc273622e3e4"),
            new Guid("4117ec3f-910a-4ac8-8698-2fbb8485b44d"),
            new Guid("85a43024-54a0-4b4a-9ae4-e46183ebc5d7"),
            new Guid("72ee925a-9a2e-4740-a902-81018b57fa3c"),
            new Guid("d2774894-6698-49f8-87b9-e7be049f3075"),
            new Guid("58dc9145-80b0-45c5-805d-e8d6bc9f3234"),
            new Guid("327f6200-713c-4b88-919b-51ab1fad6ffd"),
            new Guid("469ba6ce-6438-4dd0-987b-cf4120c540ce"),
            new Guid("7f6cbe74-e21a-4d62-8531-99eadc0cdd31"),
            new Guid("09e580c6-c395-4d54-bee6-b6dce3abb89a"),
            new Guid("9142bf04-efab-4a43-b8c6-3f3aa2c0e736"),
            new Guid("04f8a6e7-644d-43c2-9479-59b78a178d26"),
            new Guid("2b94d804-cff2-4f4a-ac41-f5d366c1df66"),
            new Guid("25a98789-1650-407a-8a52-10509a696eb7"),
            new Guid("bc188495-427c-4a34-9354-daad58ddc580"),
            new Guid("069a8aa6-27fd-4cae-b54d-221259b25c7d")
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
