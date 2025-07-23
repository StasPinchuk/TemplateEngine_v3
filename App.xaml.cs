using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Services.ServerServices;
using TemplateEngine_v3.Views.Windows;

namespace TemplateEngine_v3
{
    /// <summary>
    /// Логика взаимодействия для App.xaml.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Менеджер соединения с сервером.
        /// </summary>
        private ServerManager _serverManager;

        /// <summary>
        /// Обработчик события выхода из приложения.
        /// </summary>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppClose();
        }

        /// <summary>
        /// Обработчик события запуска приложения.
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loadWindow = new LoadWindow();
            loadWindow.Show();

            _serverManager = new ServerManager();

            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                // Попытка установить соединение с сервером
                bool connectionEstablished = await Task.Run(() => _serverManager.SetServerConnection());

                Window windowToShow = connectionEstablished
                    ? new MainWindow(_serverManager)
                    : new SignInWindow(_serverManager);

                windowToShow.Show();
                loadWindow.Close();
            });
        }

        /// <summary>
        /// Обработчик неперехваченных исключений на уровне диспетчера WPF.
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            AppClose();
        }

        /// <summary>
        /// Завершение работы приложения: очистка ресурсов, отключение от сервера и сброс шаблонов.
        /// </summary>
        private void AppClose()
        {
            if (_serverManager.IsConnected())
            {
                var tabs = NavigationService.GetTabs();

                foreach (var tab in tabs)
                {
                    var templateManager = tab.Page.ConstructorParameters.FirstOrDefault(param => param is TemplateManager) as TemplateManager;
                    if (templateManager != null)
                        templateManager.ClearTemplate();
                }

                _serverManager.Disconnect();
            }
        }
    }
}
