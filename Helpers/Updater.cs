using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TemplateEngine_v3.Properties;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References.Files;

namespace TemplateEngine_v3.Helpers
{
    public static class Updater
    {
        private static readonly string _version = "1.2.8";
        private static ServerConnection _connection;
        private static FileReference _fileReference;
        private static FolderObject _updateFolder;
        private static string innerFolder = "Редактор шаблонов";
        private static string targetFolder = AppDomain.CurrentDomain.BaseDirectory;

        public static void CheckForUpdatesOnStartup(ServerConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _fileReference = new FileReference(_connection);

            _ = CheckForUpdatesSafeAsync(); // 🔄 однократная проверка
        }

        private static async Task CheckForUpdatesSafeAsync()
        {
            try
            {
                await CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке обновлений: {ex.Message}");
            }
        }

        public static async Task CheckForUpdatesAsync()
        {
            _updateFolder = _fileReference.FindByRelativePath(@"Генератор шаблонов\Программа редактора шаблонов") as FolderObject;

            if (_updateFolder == null)
            {
                MessageBox.Show("Папка не найдена!", "Ошибка");
                return;
            }

            var updateInfoFile = _fileReference.FindByRelativePath(@$"Генератор шаблонов\Программа редактора шаблонов\UpdateProgramm.json") as FileObject;
            if (updateInfoFile == null)
                return;

            FileReference.GetHeadRevision(new[] { updateInfoFile });

            string updateInfoString = File.ReadAllText(updateInfoFile.LocalPath);
            UpdateInfo updateInfo = new JsonSerializer().Deserialize<UpdateInfo>(updateInfoString);

            if (updateInfo.Version.Equals(_version))
                return;

            var updateArchive = _fileReference.FindByRelativePath(@$"Генератор шаблонов\Программа редактора шаблонов\Редактор шаблонов.zip") as FileObject;
            if (updateArchive == null)
                return;

            FileReference.GetHeadRevision(new[] { updateArchive });

            string updaterExe = Path.Combine(targetFolder, "Updater", "Updater.exe");

            if (!File.Exists(updaterExe))
            {
                MessageBox.Show("Файл Updater.exe не найден.", "Ошибка обновления");
                return;
            }

            if (!File.Exists(updateArchive.LocalPath))
            {
                MessageBox.Show("Архив обновления не найден.", "Ошибка обновления");
                return;
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

            _connection?.Close();
            Process.Start(psi);

            await Task.Delay(500);

            Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }
    }

    public class UpdateInfo
    {
        public string Version { get; set; }
    }
}
