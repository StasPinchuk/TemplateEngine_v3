using System.Collections.Generic;
using System.Linq;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;
using TFlex.DOCs.Model;

public class ReferenceManager
{
    private readonly JsonReferenceLoader _referenceLoader;
    private OperationNamesManager _operationNamesManager;

    // Менеджеры для различных частей системы
    public TemplateManager TemplateManager;
    public BranchManager BranchManager;
    public TechnologiesManager TechnologiesManager;
    public MaterialManager MaterialManager;
    public TableService TableService;
    public TemplateStageService TemplateStageService;

    // Словарь с маппингом ключей на ReferenceInfo
    public Dictionary<string, ReferenceInfo> ReferenceMappings { get; private set; }

    public ReferenceManager(ServerConnection connection, JsonReferenceLoader referenceLoader)
    {
        _referenceLoader = referenceLoader;
        InitializeReferences(connection);
    }

    /// <summary>
    /// Основной метод инициализации - загружает ссылки из файла и дополняет их из сервера, затем создаёт менеджеры.
    /// </summary>
    public void InitializeReferences(ServerConnection connection)
    {
        ReferenceMappings = _referenceLoader.LoadReferences($"references.json") ?? new();

        FillMissingReferences(connection);

        CreateManagers(connection);
    }

    /// <summary>
    /// Заполняет отсутствующие ссылки, если их нет в json, но есть на сервере.
    /// </summary>
    private void FillMissingReferences(ServerConnection connection)
    {
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

        if (ReferenceMappings.TryGetValue("Стадии шаблонов", out var templateStage))
            TemplateStageService = new TemplateStageService(templateStage);

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
            LogManager.LogReferenceInfo = mainInfo;
            LogManager.ReadLogs();
            BranchManager = new BranchManager(loader, mainInfo);
            TechnologiesManager = new TechnologiesManager(loader, mainInfo, BranchManager, _operationNamesManager);
        }
    }
}
