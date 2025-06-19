using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для TableChoiceDialog.xaml
    /// </summary>
    public partial class TableChoiceDialog : UserControl
    {
        public static DependencyProperty TableListProperty =
            DependencyProperty.Register(
                "TableList",
                typeof(ObservableCollection<string>),
                typeof(TableChoiceDialog),
                new PropertyMetadata(null)
            );

        public static DependencyProperty SelectedTableProperty =
            DependencyProperty.Register(
                "SelectedTable",
                typeof(string),
                typeof(TableChoiceDialog),
                new PropertyMetadata(string.Empty, OnSelectedTableChanged)
            );

        public static DependencyProperty SheetProperty =
            DependencyProperty.Register(
                "Sheet",
                typeof(ObservableCollection<string>),
                typeof(TableChoiceDialog),
                new PropertyMetadata(null)
            );

        public static DependencyProperty FindParameterListProperty =
            DependencyProperty.Register(
                "FindParameterList",
                typeof(ObservableCollection<string>),
                typeof(TableChoiceDialog),
                new PropertyMetadata(null)
            );

        public static DependencyProperty SelectedSheetProperty =
            DependencyProperty.Register(
                "SelectedSheet",
                typeof(string),
                typeof(TableChoiceDialog),
                new PropertyMetadata(string.Empty, OnSelectedSheetChanged)
            );

        public static DependencyProperty FindParameterProperty =
            DependencyProperty.Register(
                "FindParameter",
                typeof(string),
                typeof(TableChoiceDialog),
                new PropertyMetadata(string.Empty)
            );

        public static DependencyProperty ColStringProperty =
            DependencyProperty.Register(
                "ColString",
                typeof(ConditionEvaluator),
                typeof(TableChoiceDialog),
                new PropertyMetadata(new ConditionEvaluator())
            );

        public static DependencyProperty RowStringProperty =
            DependencyProperty.Register(
                "RowString",
                typeof(ConditionEvaluator),
                typeof(TableChoiceDialog),
                new PropertyMetadata(new ConditionEvaluator())
            );

        public static DependencyProperty SetTableFormulaCommandProperty =
            DependencyProperty.Register(
                "SetTableFormulaCommand",
                typeof(ICommand),
                typeof(TableChoiceDialog),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty ParametersCollectionProperty =
            DependencyProperty.Register(
                "ParametersCollection",
                typeof(ObservableCollection<ParameterItem>),
                typeof(TableChoiceDialog),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty ParametersMenuProperty =
            DependencyProperty.Register(
                "ParametersMenu",
                typeof(ContextMenu),
                typeof(TableChoiceDialog),
                new PropertyMetadata(null)
            );

        public ObservableCollection<string> TableList
        {
            get => (ObservableCollection<string>)GetValue(TableListProperty);
            set => SetValue(TableListProperty, value);
        }

        public string SelectedTable
        {
            get => (string)GetValue(SelectedTableProperty);
            set => SetValue(SelectedTableProperty, value);
        }

        public ObservableCollection<string> Sheet
        {
            get => (ObservableCollection<string>)GetValue(SheetProperty);
            set => SetValue(SheetProperty, value);
        }

        public ObservableCollection<string> FindParameterList
        {
            get => (ObservableCollection<string>)GetValue(FindParameterListProperty);
            set => SetValue(FindParameterListProperty, value);
        }

        public string FindParameter
        {
            get => (string)GetValue(FindParameterProperty);
            set => SetValue(FindParameterProperty, value);
        }

        public string SelectedSheet
        {
            get => (string)GetValue(SelectedSheetProperty);
            set => SetValue(SelectedSheetProperty, value);
        }

        public ConditionEvaluator ColString
        {
            get => (ConditionEvaluator)GetValue(ColStringProperty);
            set => SetValue(ColStringProperty, value);
        }

        public ConditionEvaluator RowString
        {
            get => (ConditionEvaluator)GetValue(RowStringProperty);
            set => SetValue(RowStringProperty, value);
        }

        public ObservableCollection<ParameterItem> ParametersCollection
        {
            get => (ObservableCollection<ParameterItem>)GetValue(ParametersCollectionProperty);
            set => SetValue(ParametersCollectionProperty, value);
        }

        public ContextMenu ParametersMenu
        {
            get => (ContextMenu)GetValue(ParametersMenuProperty);
            set => SetValue(ParametersMenuProperty, value);
        }

        public ICommand SetTableFormulaCommand
        {
            get => (ICommand)GetValue(SetTableFormulaCommandProperty);
            set => SetValue (SetTableFormulaCommandProperty, value);
        }

        private TableService _tableService;
        private ConditionEvaluator _evaluator;
        private ContextMenuHelper _contextMenuHelper;

        public TableChoiceDialog(TableService tableService, ContextMenuHelper contextMenuHelper, ConditionEvaluator evaluator)
        {
            InitializeComponent();
            _tableService = tableService;
            _evaluator = evaluator;
            _contextMenuHelper = contextMenuHelper;
            _contextMenuHelper.DataContext = this;
            ParametersMenu = _contextMenuHelper.GetContextMenu();
            TableList = _tableService.TableNames;
            FindParameterList = new()
            {
                "Значение",
                "Диапазон"
            };
            SetTableFormulaCommand = new RelayCommand(SetTableFormula, CanSetTable);
        }

        private static void OnSelectedTableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TableChoiceDialog)d;
            string selectedTable = e.NewValue as string;

            if (!string.IsNullOrEmpty(selectedTable))
            {
                control.LoadWorksheetsAsync(selectedTable);
            }
        }

        private void LoadWorksheetsAsync(string selectedTable)
        {
            var sheets = _tableService.GetWorksheets(selectedTable);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Sheet = new ObservableCollection<string>(sheets);
            });
        }


        private static void OnSelectedSheetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TableChoiceDialog)d;
            string selectedSheet = e.NewValue as string;

            if (!string.IsNullOrEmpty(selectedSheet))
            {
                control.ParametersCollection = new ObservableCollection<ParameterItem>(
                    control._tableService.GetWorksheetParameters(selectedSheet)
                        .Select(kv => new ParameterItem { Key = kv.Key, Value = string.Empty })
                );
            }
        }

        private bool CanSetTable(object parameter)
        {
            return !string.IsNullOrEmpty(SelectedTable) 
                && !string.IsNullOrEmpty(SelectedSheet) 
                && !string.IsNullOrEmpty(FindParameter) 
                && !string.IsNullOrEmpty(ColString.Value) 
                && !string.IsNullOrEmpty(RowString.Value);
        }

        private void SetTableFormula(object parameter)
        {
            try
            {
                var parameters = ParametersCollection.Select(param => param.Value).ToArray();
                var isRange = FindParameter.Equals("Значение") ? false : true;
                _evaluator.Value += _tableService.SetFormula(SelectedTable, SelectedSheet, ColString.Value, RowString.Value, parameters, isRange);
                if(ColString.Parts.Count > 0)
                    _evaluator.Parts.Add(ColString.Parts?.First());
                if(RowString.Parts.Count > 0)
                    _evaluator.Parts.Add(RowString.Parts?.First());
                ColString = new();
                RowString = new();
                SelectedTable = null;
                SelectedSheet = null;
                FindParameter = null;
                ParametersCollection = new();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
