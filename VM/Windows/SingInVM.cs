using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ServerServices;
using TemplateEngine_v3.Views.Windows;

namespace TemplateEngine_v3.VM.Windows
{
    /// <summary>
    /// ViewModel окна входа в приложение.
    /// </summary>
    public class SingInVM
    {
        private readonly Window _window;
        private readonly ServerManager _serverManager;

        /// <summary>
        /// Модель с данными пользователя (логин, пароль, путь к API и IP сервера).
        /// </summary>
        public UserCredentials Credentials { get; set; } = new();

        /// <summary>
        /// Команда входа в систему.
        /// </summary>
        public ICommand SingInCommand { get; private set; }

        /// <summary>
        /// Команда выбора пути к API.
        /// </summary>
        public ICommand SetApiPathCommand { get; private set; }

        /// <summary>
        /// Команда выхода из приложения.
        /// </summary>
        public ICommand AppExitCommand { get; private set; }

        /// <summary>
        /// Конструктор ViewModel.
        /// </summary>
        /// <param name="serverManager">Менеджер для подключения к серверу.</param>
        /// <param name="window">Окно входа.</param>
        public SingInVM(ServerManager serverManager, Window window)
        {
            _serverManager = serverManager;
            _window = window;
            InitializeCommands();
        }

        /// <summary>
        /// Инициализация команд.
        /// </summary>
        private void InitializeCommands()
        {
            SingInCommand = new RelayCommand(SingIn, CanSingIn);
            SetApiPathCommand = new RelayCommand(SetApiPath);
            AppExitCommand = new RelayCommand(AppExit);
        }

        /// <summary>
        /// Обработчик команды входа.
        /// </summary>
        private async void SingIn()
        {
            _serverManager.CurrentCredentials = Credentials;

            var loadWindow = new LoadWindow();
            _window.Hide();
            loadWindow.Show();

            // Дать UI возможность отрисовать окно и начать анимацию
            await Task.Delay(100); // 100 мс достаточно для начала рендеринга и анимации

            // Выполнить подключение в фоне
            bool connected = await Task.Run(() => _serverManager.SetServerConnection().Result);

            if (connected)
            {
                loadWindow.Close();
                new MainWindow(_serverManager).Show();
                _window.Close();
            }
            else
            {
                loadWindow.Close();
                System.Windows.MessageBox.Show(
                    "Не удалось подключиться к серверу. Проверьте введённые данные.",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                _window.Show();
            }
        }



        /// <summary>
        /// Проверка возможности выполнения команды входа (валидность данных).
        /// </summary>
        /// <returns>True, если данные корректны.</returns>
        private bool CanSingIn()
        {
            return !string.IsNullOrEmpty(Credentials.ApiPath)
                && !string.IsNullOrEmpty(Credentials.ServerIp);
        }

        /// <summary>
        /// Открытие диалога выбора папки для API пути.
        /// </summary>
        private void SetApiPath()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                var result = folderDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    Credentials.ApiPath = folderDialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Выход из приложения.
        /// </summary>
        private void AppExit()
        {
            _window.Close();
        }
    }
}
