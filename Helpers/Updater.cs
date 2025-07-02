using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References.Files;

namespace TemplateEngine_v3.Helpers
{
    public static class Updater
    {
        private static readonly string _version = "1.2.11";
        private static ServerConnection _connection;
        private static FileReference _fileReference;
        private static FolderObject _updateFolder;
        private static readonly string innerFolder = "Редактор шаблонов";
        private static readonly string targetFolder = AppDomain.CurrentDomain.BaseDirectory;

        public static async Task<bool> CheckForUpdatesOnStartup(ServerConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _fileReference = new FileReference(_connection);

            return await CheckForUpdatesSafeAsync();
        }

        private static async Task<bool> CheckForUpdatesSafeAsync()
        {
            try
            {
                return await CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке обновлений: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> CheckForUpdatesAsync()
        {
            _updateFolder = _fileReference.FindByRelativePath(@"Генератор шаблонов\Программа редактора шаблонов") as FolderObject;

            if (_updateFolder == null)
            {
                MessageBox.Show("Папка не найдена!", "Ошибка");
                return false;
            }

            var updateInfoFile = _fileReference.FindByRelativePath(@$"Генератор шаблонов\Программа редактора шаблонов\UpdateProgramm.json") as FileObject;
            if (updateInfoFile == null)
                return false;

            FileReference.GetHeadRevision(new[] { updateInfoFile });
            await Task.Delay(1000);

            string updateInfoString = File.ReadAllText(updateInfoFile.LocalPath);
            UpdateInfo updateInfo = new JsonSerializer().Deserialize<UpdateInfo>(updateInfoString);

            if (!IsNewVersionAvailable(updateInfo.Version))
                return false;

            var updateArchive = _fileReference.FindByRelativePath(@$"Генератор шаблонов\Программа редактора шаблонов\Редактор шаблонов.zip") as FileObject;
            if (updateArchive == null)
                return false;

            FileReference.GetHeadRevision(new[] { updateArchive });

            string updaterExe = Path.Combine(targetFolder, "Updater", "Updater.exe");

            if (!File.Exists(updaterExe))
            {
                MessageBox.Show("Файл Updater.exe не найден.", "Ошибка обновления");
                return false;
            }

            if (!File.Exists(updateArchive.LocalPath))
            {
                MessageBox.Show("Архив обновления не найден.", "Ошибка обновления");
                return false;
            }

            string savePath = Path.Combine("Updater", "update_path.txt");

            File.WriteAllText(savePath, updateArchive.LocalPath);

            await Task.Delay(1000);

            MessageBox.Show("Обнаружено обновление. Программа будет обновлена сейчас.", "Обновление");

            var psi = new ProcessStartInfo
            {
                FileName = updaterExe,
                UseShellExecute = true
            };

            await Task.Delay(1000);
            _connection?.Close();
            Process.Start(psi);

            Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());
            return true;
        }

        public static bool IsNewVersionAvailable(string receivedVersion)
        {
            if (!Version.TryParse(_version, out var current))
                throw new ArgumentException("Некорректный формат текущей версии.");

            if (!Version.TryParse(receivedVersion, out var received))
                throw new ArgumentException("Некорректный формат полученной версии.");

            return received.CompareTo(current) > 0;
        }


        public class UpdateInfo
        {
            public string Version { get; set; }
        }
    }
}
