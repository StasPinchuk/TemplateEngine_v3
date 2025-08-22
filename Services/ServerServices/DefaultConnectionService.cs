using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.Diagnostics;
using TFlex.PdmFramework.Resolve;

namespace TemplateEngine_v3.Services.ServerServices
{
    /// <summary>
    /// Сервис для подключения к серверу T-Flex PDM с использованием списка конфигураций.
    /// </summary>
    public class DefaultConnectionService : BaseNotifyPropertyChanged
    {
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
                    credentials.ServerIp);
            }
            catch (Exception ex)
            {

                if (ex is ServerNotFoundException exception)
                {
                    MessageBox.Show(exception.Message);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                }

                return null;
            }
        }
    }
}
