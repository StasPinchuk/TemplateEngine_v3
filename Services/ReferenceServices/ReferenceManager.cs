using System.Collections.Generic;
using System.Linq;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Services.ReferenceServices;
using TFlex.DOCs.Model;

public class ReferenceManager
{
    private readonly IReferenceLoader _referenceLoader;
    private OperationNamesManager _operationNamesManager;

    // Менеджеры для различных частей системы
    public ITemplateManager TemplateManager;
    public IBranchManager BranchManager;
    public ITechnologiesManager TechnologiesManager;
    public MaterialManager MaterialManager;
    public TableService TableService;

    // Словарь с маппингом ключей на ReferenceInfo
    public Dictionary<string, ReferenceInfo> ReferenceMappings { get; private set; }

    public ReferenceManager(ServerConnection connection, IReferenceLoader referenceLoader)
    {
        _referenceLoader = referenceLoader;
        InitializeReferences(connection);
    }

    /// <summary>
    /// Основной метод инициализации - загружает ссылки из файла и дополняет их из сервера, затем создаёт менеджеры.
    /// </summary>
    public void InitializeReferences(ServerConnection connection)
    {
        // Загружаем из json, если не найдено - создаём пустой словарь
        ReferenceMappings = _referenceLoader.LoadReferences($"references.json") ?? new();

        // Проверяем на null и заполняем отсутствующие ссылки из сервера
        FillMissingReferences(connection);

        // Создаём необходимые менеджеры по загруженным ReferenceInfo
        CreateManagers(connection);
    }

    /// <summary>
    /// Заполняет отсутствующие ссылки, если их нет в json, но есть на сервере.
    /// </summary>
    private void FillMissingReferences(ServerConnection connection)
    {
        // Чтобы избежать изменения коллекции во время итерации - копируем ключи
        foreach (var key in ReferenceMappings.Keys.ToList())
        {
            if (ReferenceMappings[key] == null)
            {
                var reference = connection.ReferenceCatalog.Find(key);
                if (reference != null)
                    ReferenceMappings[key] = reference;
            }
        }
    }

    /// <summary>
    /// Создаёт менеджеры для работы с разными справочниками и ссылками.
    /// </summary>
    private void CreateManagers(ServerConnection connection)
    {
        var loader = new ServerReferenceLoader();

        // Менеджер материалов
        if (ReferenceMappings.TryGetValue("Материалы", out var materialsInfo))
            MaterialManager = new MaterialManager(loader, materialsInfo);

        // Менеджер названий операций
        if (ReferenceMappings.TryGetValue("Названия операций для ТП", out var operationInfo))
            _operationNamesManager = new OperationNamesManager(operationInfo);

        // Менеджер типов изделий (статический)
        if (ReferenceMappings.TryGetValue("Типы изделий", out var nodeTypes))
        {
            NodeTypeManager.NodeTypeInfo = nodeTypes;
            NodeTypeManager.SetNodeTypesList();
        }

        // Основные менеджеры: шаблоны, филиалы, технологии
        if (ReferenceMappings.TryGetValue("Изделия для генератора", out var mainInfo))
        {
            TableService = new TableService(connection);
            TemplateManager = new TemplateManager(loader, mainInfo, MaterialManager, TableService);
            BranchManager = new BranchManager(loader, mainInfo);
            TechnologiesManager = new TechnologiesManager(loader, mainInfo, BranchManager, _operationNamesManager);
        }
    }
}
