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
        private readonly PasswordBox _passwordBox;
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
        /// <param name="passwordBox">Контрол для ввода пароля.</param>
        /// <param name="window">Окно входа.</param>
        public SingInVM(ServerManager serverManager, PasswordBox passwordBox, Window window)
        {
            _serverManager = serverManager;
            _passwordBox = passwordBox;
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
            Credentials.Password = _passwordBox.Password;
            _serverManager.CurrentCredentials = Credentials;

            bool connected = await _serverManager.SetServerConnection();

            if (connected)
            {
                new MainWindow(_serverManager).Show();
                _window.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Не удалось подключиться к серверу. Проверьте введённые данные.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current?.Dispatcher.Invoke(() => System.Windows.Application.Current.Shutdown());
            }
        }

        /// <summary>
        /// Проверка возможности выполнения команды входа (валидность данных).
        /// </summary>
        /// <returns>True, если данные корректны.</returns>
        private bool CanSingIn()
        {
            return !string.IsNullOrEmpty(_passwordBox.Password)
                && !string.IsNullOrEmpty(Credentials.Login)
                && !string.IsNullOrEmpty(Credentials.ApiPath)
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
