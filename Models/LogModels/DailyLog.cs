using System;
using System.Collections.Generic;

namespace TemplateEngine_v3.Models.LogModels
{
    public class DailyLog
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date.Date;
            set => _date = value;
        }
        public List<LogObjectGroup> ObjectGroups { get; set; } = new();

        public DailyLog() { }

        public DailyLog(DateTime date)
        {
            Date = DateTime.Now;
        }
    }
}
