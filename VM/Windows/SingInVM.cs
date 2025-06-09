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
    public class SingInVM
    {
        private readonly PasswordBox _passwordBox;
        private readonly Window _window;
        private readonly ServerManager _serverManager;
        public UserCredentials Credentials { get; set; } = new();

        public ICommand SingInCommand { get; private set; }
        public ICommand SetApiPathCommand { get; private set; }
        public ICommand AppExitCommand { get; private set; }

        public SingInVM(ServerManager serverManager, PasswordBox passwordBox, Window window)
        {
            _serverManager = serverManager;
            _passwordBox = passwordBox;
            _window = window;
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            SingInCommand = new RelayCommand(SingIn, CanSingIn);
            SetApiPathCommand = new RelayCommand(SetApiPath);
            AppExitCommand = new RelayCommand(AppExit);
        }

        private async void SingIn()
        {
            Credentials.Password = _passwordBox.Password;
            _serverManager.CurrentCredentials = Credentials;
            if (await _serverManager.SetServerConnection())
            {
                new MainWindow(_serverManager).Show();
                _window.Close();
            }
        }

        private bool CanSingIn()
        {
            return !string.IsNullOrEmpty(_passwordBox.Password)
                && !string.IsNullOrEmpty(Credentials.Login)
                && !string.IsNullOrEmpty(Credentials.ApiPath)
                && !string.IsNullOrEmpty(Credentials.ServerIp);
        }

        private void SetApiPath()
        {
            FolderBrowserDialog openFileDlg = new();
            var result = openFileDlg.ShowDialog();
            if (!string.IsNullOrEmpty(result.ToString()))
            {
                Credentials.ApiPath = openFileDlg.SelectedPath;
            }
        }

        private void AppExit()
        {
            _window.Close();
        }
    }
}
