using System;
using System.Security.Cryptography;
using System.Text;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Представляет учетные данные пользователя, необходимые для подключения к серверу.
    /// Все поля, кроме пути к API, хранятся в зашифрованном виде.
    /// </summary>
    public class UserCredentials
    {
        private string _encryptedServerIp;

        /// <summary>
        /// IP-адрес сервера (автоматически шифруется/расшифровывается).
        /// </summary>
        public string ServerIp
        {
            get => _encryptedServerIp;
            set => _encryptedServerIp = value;
        }

        /// <summary>
        /// Путь к API TFlex DOCs (хранится в открытом виде).
        /// </summary>
        public string ApiPath { get; set; }

        /// <summary>
        /// Шифрует строку с помощью DPAPI.
        /// </summary>
        public UserCredentials Encrypt()
        {
            var credentials = new UserCredentials();
            var bytes = Encoding.UTF8.GetBytes(ServerIp);
            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);

            credentials.ServerIp = Convert.ToBase64String(encrypted);
            credentials.ApiPath = ApiPath;

            return credentials;
        }

        /// <summary>
        /// Расшифровывает строку, зашифрованную методом <see cref="Encrypt"/>.
        /// </summary>
        public void Decrypt()
        {
            try
            {
                if (string.IsNullOrEmpty(ServerIp))
                    return;

                var encryptedBytes = Convert.FromBase64String(ServerIp);
                var decrypted = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.LocalMachine);
                ServerIp = Encoding.UTF8.GetString(decrypted);

            }catch (Exception ex)
            {
                return;
            }
        }
    }
}
