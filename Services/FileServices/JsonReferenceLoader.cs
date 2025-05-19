using System.Collections.Generic;
using TemplateEngine_v3.Interfaces;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Services.FileServices
{
    public class JsonReferenceLoader : IReferenceLoader
    {
        private readonly JsonFileManager _jsonFileManager;

        public JsonReferenceLoader()
        {
            _jsonFileManager = new JsonFileManager();
        }

        public Dictionary<string, ReferenceInfo> LoadReferences(string filePath)
        {
            return _jsonFileManager.ReadFromJson<Dictionary<string, ReferenceInfo>>(filePath);
        }
    }
}
