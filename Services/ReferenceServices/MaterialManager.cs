using System;
using System.Collections.Generic;
using System.Linq;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Менеджер для загрузки и управления материалами.
    /// </summary>
    public class MaterialManager
    {
        private readonly JsonFileManager _jsonFileManager;
        private readonly ServerReferenceLoader _serverReferenceLoader;
        private readonly ReferenceInfo _materialInfo;

        private List<string> _materials;

        /// <summary>
        /// Инициализирует менеджер материалов.
        /// </summary>
        /// <param name="connection">Соединение с сервером.</param>
        /// <param name="serverReferenceLoader">Объект для загрузки материалов с сервера.</param>
        public MaterialManager(ServerReferenceLoader serverReferenceLoader, ReferenceInfo materialInfo)
        {
            _serverReferenceLoader = serverReferenceLoader;
            _materialInfo = materialInfo;
            LoadMaterials();
        }

        /// <summary>
        /// Загружает материалы на основе информации о справочнике материалов.
        /// </summary>
        /// <param name="materialsInfo">Информация о материалах для загрузки.</param>
        private void LoadMaterials()
        {
            try
            {
                _materials = _serverReferenceLoader.LoadMaterials(_materialInfo).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading materials: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает список загруженных материалов.
        /// </summary>
        /// <returns>Список материалов.</returns>
        public List<string> GetMaterials()
        {
            return _materials ?? new List<string>();
        }
    }
}
