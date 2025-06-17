using System;
using System.Collections.Generic;
using System.Linq;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.Structure;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    public static class LogManager
    {
        public static ReferenceInfo LogReferenceInfo { get; set; }
        public static List<DailyLog> AllLogs { get; set; } = [];
        public static DailyLog CurrentDailyLog { get; set; }
        public static string UserName { get; set; }

        private static LogObjectGroup _currentLogObjectGroup;
        private static LogEntry _currentLogEntry;
        private static Reference _logReference;
        private static ReferenceObject _logReferenceObjec;
        private static ParameterInfo _objectStruc;


        public static void ReadLogs()
        {
            _logReference = LogReferenceInfo.CreateReference();

            if (_logReference == null)
                return;

            _objectStruc = _logReference.ParameterGroup.Parameters.FindByName("Структура файла");

            _logReference.Objects.Reload();
            _logReferenceObjec = _logReference.Objects.FirstOrDefault(referenceObject => referenceObject.ToString().Equals("LogsObject (Не удалять)"));

            if (_logReferenceObjec == null)
                return;

            AllLogs = new JsonSerializer().Deserialize<List<DailyLog>>(_logReferenceObjec[_objectStruc].Value.ToString());

            if (AllLogs == null)
                AllLogs = [];
            CurrentDailyLog = AllLogs.FirstOrDefault(dailyLog => dailyLog.Date.Equals(DateTime.Now));
            if (CurrentDailyLog == null)
                CreateDailyLog();
        }

        private static void CreateDailyLog()
        {
            CurrentDailyLog = new DailyLog(DateTime.Now);
            AllLogs.Add(CurrentDailyLog);
        }

        public static void CreateLogObjectGroup(string objectName, string objectType)
        {
            _currentLogObjectGroup = new(objectName, objectType, UserName);
        }

        public static void CreateLogEntry(LogActionType actionType, string message)
        {
            var logEntry = new LogEntry(actionType, message);
            _currentLogObjectGroup.Entries.Add(logEntry);
        }
    }
}
