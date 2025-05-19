using System.Windows;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ServerServices;
using TemplateEngine_v3.Views.Windows;

namespace TemplateEngine_v3
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServerManager _serverManager;

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppClose();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loadWindow = new LoadWindow();
            loadWindow.Show();

            _serverManager = new ServerManager();

            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                var connectionEstablished = _serverManager.SetServerConnection();

                Window windowToShow = await connectionEstablished
                    ? new MainWindow(_serverManager)
                    : new SignInWindow(_serverManager);

                windowToShow.Show();

                loadWindow.Close();
            });
        }


        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            AppClose();
        }

        private void AppClose()
        {
            if (_serverManager.IsConnected())
                _serverManager.Disconnect();
        }
    }
}
