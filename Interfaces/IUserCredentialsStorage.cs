using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для хранения и загрузки учетных данных пользователя.
    /// </summary>
    public interface IUserCredentialsStorage
    {
        /// <summary>
        /// Пытается загрузить учетные данные пользователя.
        /// </summary>
        /// <param name="credentials">Выходной параметр для загруженных учетных данных.</param>
        /// <returns>Возвращает <c>true</c>, если данные успешно загружены, иначе <c>false</c>.</returns>
        bool TryLoad(out UserCredentials credentials);

        /// <summary>
        /// Сохраняет учетные данные пользователя.
        /// </summary>
        /// <param name="credentials">Учетные данные для сохранения.</param>
        void Save(UserCredentials credentials);
    }
}
