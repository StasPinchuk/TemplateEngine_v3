using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.LogModels;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для LogsChoiceDialog.xaml
    /// </summary>
    public partial class LogsChoiceDialog : UserControl
    {
        public static DependencyProperty AllLogsProperty =
           DependencyProperty.Register(
               "AllLogs",
               typeof(ObservableCollection<DailyLog>),
               typeof(LogsChoiceDialog),
               new PropertyMetadata(new ObservableCollection<DailyLog>())
           );

        public ObservableCollection<DailyLog> AllLogs
        {
            get => (ObservableCollection<DailyLog>)GetValue(AllLogsProperty);
            set => SetValue(AllLogsProperty, value);
        }

        public LogsChoiceDialog()
        {
            LogManager.ReadLogs();
            AllLogs = new ObservableCollection<DailyLog>(LogManager.AllLogs);
            InitializeComponent();
            DataContext = this;
        }

    }
}
