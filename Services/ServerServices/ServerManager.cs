using System;
using System.Threading.Tasks;
using System.Windows;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.UsersServices;
using TemplateEngine_v3.Storage;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Services.ServerServices
{
    /// <summary>
    /// Менеджер сервера, реализующий паттерн Singleton и управляющий подключением к серверу, пользователями и справочниками.
    /// </summary>
    public class ServerManager
    {
        public ServerManager() { }

        private readonly UserCredentialsStorage _credentialsStorage = new UserCredentialsStorage();
        private readonly DefaultConnectionService _connectionService = new DefaultConnectionService();
        private readonly DefaultManagerInitializer _managerInitializer = new DefaultManagerInitializer();

        private ServerConnection _connection;

        /// <summary>
        /// Менеджер пользователей.
        /// </summary>
        public UserManager UserManager { get; private set; }

        /// <summary>
        /// Менеджер справочников.
        /// </summary>
        public ReferenceManager ReferenceManager { get; private set; }

        /// <summary>
        /// Текущие учетные данные пользователя.
        /// </summary>
        public UserCredentials CurrentCredentials { get; set; }

        /// <summary>
        /// Устанавливает подключение к серверу с использованием учетных данных.
        /// </summary>
        /// <returns><c>true</c>, если соединение успешно установлено; иначе <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке подключения.</exception>
        public async Task<bool> SetServerConnection()
        {
            try
            {
                if (CurrentCredentials == null)
                {
                    if (!_credentialsStorage.TryLoad(out var creds))
                        return false;

                    CurrentCredentials = creds;
                }

                while (_connection == null)
                    _connection = await _connectionService.ConnectAsync(CurrentCredentials);

                if (_connection?.IsConnected == true)
                {
                    _credentialsStorage.Save(CurrentCredentials);

                    if (await Updater.CheckForUpdatesOnStartup(_connection))
                        return false;

                    UserManager = _managerInitializer.InitializeUserManager(_connection);
                    ReferenceManager = _managerInitializer.InitializeReferenceManager(_connection);
                    var templates = FileService.ReadeFromFolder("configs\\UnhandledException");
                    foreach (var template in templates)
                    {
                        await ReferenceManager.TemplateManager.SetTemplateAsync(template);
                        await ReferenceManager.TemplateManager.SaveTemplate("Ready");
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к серверу: {ex.Message}", "Ошибка");
                Application.Current.Shutdown();
                return false;
            }
        }

        /// <summary>
        /// Проверяет, установлено ли соединение с сервером.
        /// </summary>
        /// <returns><c>true</c>, если соединение активно; иначе <c>false</c>.</returns>
        public bool IsConnected()
        {
            return _connection?.IsConnected ?? false;
        }

        /// <summary>
        /// Закрывает текущее соединение с сервером.
        /// </summary>
        public void Disconnect()
        {
            _connection?.Close();
        }
    }
}
