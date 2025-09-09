using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private static ReferenceObject _logReferenceObject;
        private static ParameterInfo _objectStruc;


        public static void ReadLogs()
        {
            try
            {
                if (_logReference == null)
                {
                    _logReference = LogReferenceInfo.CreateReference();

                    if (_logReference == null)
                        return;
                }

                _objectStruc = _logReference.ParameterGroup.Parameters.FindByName("Структура файла");

                _logReference.Objects.Reload();
                _logReferenceObject = _logReference.Objects.FirstOrDefault(referenceObject => referenceObject.ToString().Equals("LogsObject (Не удалять)"));

                if (_logReferenceObject == null)
                    return;

                AllLogs = new JsonSerializer().Deserialize<List<DailyLog>>(_logReferenceObject[_objectStruc].Value.ToString());

                if (AllLogs == null)
                    AllLogs = [];
                CurrentDailyLog = AllLogs.FirstOrDefault(dailyLog => dailyLog.Date.Equals(DateTime.Now.Date));
                if (CurrentDailyLog == null)
                    CreateDailyLog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
            try
            {
                var logEntry = new LogEntry(actionType, message);
                _currentLogObjectGroup.Entries.Add(logEntry);

            }
            catch (Exception)
            {

            }
        }

        public static async Task<bool> SaveLog()
        {
            try
            {
                CurrentDailyLog.ObjectGroups.Add(_currentLogObjectGroup);

                await _logReferenceObject.BeginChangesAsync();

                _logReferenceObject[_objectStruc].Value = new JsonSerializer().Serialize(AllLogs);

                await _logReferenceObject.EndChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
