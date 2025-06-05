using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TFlex.DOCs.Common;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References.Files;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    public class TableService
    {
        private readonly ServerConnection _connection;
        private FileReference _fileReference;
        private FolderObject _tableFolder;
        private Workbook _workbook;
        private Worksheet _currentWorksheet;

        public ObservableCollection<string> TableNames { get; set; } = [];

        public TableService(ServerConnection connection)
        {
            _connection = connection;
            GetTableList();
        }

        private void GetTableList()
        {
            _fileReference = new FileReference(_connection);
            _tableFolder = _fileReference.FindByRelativePath(@"Генератор шаблонов\Таблицы") as FolderObject;

            if (_tableFolder == null)
            {
                MessageBox.Show("Папка не найдена!", "Ошибка");
                return;
            }

            _tableFolder.Children.Reload();

            foreach(var file in _tableFolder.Children)
            {
                TableNames.Add(file.Name);
            }
        }

        public async Task<ObservableCollection<string>> GetWorksheets(string tableName)
        {
            ObservableCollection<string> worksheetNames = [];

            string tablePath = await GetSelectedTable(tableName);
            _workbook = new Workbook(tablePath);

            foreach(var worksheet in _workbook.Worksheets)
            {
                worksheetNames.Add(worksheet.Name);
            }

            return worksheetNames;
        }

        private async Task<string> GetSelectedTable(string tableName)
        {
            var file = await _fileReference.FindByRelativePathAsync(@$"Генератор шаблонов\Таблицы\{tableName}") as FileObject;

            if (file is null)
                return string.Empty;
            await FileReference.GetHeadRevisionAsync(new[] { file });

            return file.LocalPath;
        }

        public Dictionary<string, string> GetWorksheetParameters(string worksheetName)
        {
            Dictionary<string, string> parameterDictionary = [];
            _currentWorksheet = _workbook.Worksheets.FirstOrDefault(sheet => sheet.Name.Equals(worksheetName));
            int maxRow = _currentWorksheet.Cells.MaxDataRow;

            for (int rowIndex = 0; rowIndex <= maxRow; rowIndex++)
            {
                Cell leftCell = _currentWorksheet.Cells[rowIndex, 0]; // Первый столбец (A)

                if (leftCell == null || string.IsNullOrWhiteSpace(leftCell.StringValue))
                    continue;

                Aspose.Cells.Style style = leftCell.GetStyle();
                if (style.Font.IsBold)
                {
                    string parameterName = leftCell.StringValue;

                    Cell rightCell = _currentWorksheet.Cells[rowIndex, 1];
                    string parameterAddress = rightCell.Name;

                    parameterDictionary.Add(parameterName, parameterAddress);
                }
            }

            return parameterDictionary;
        }

        public void SetParameters(Dictionary<string, string> parameters)
        {
            foreach(var key in parameters.Keys)
            {
                _currentWorksheet.Cells[key].PutValue(parameters[key]);
            }

            _workbook.CalculateFormula();
        }

        public string GetFindParameter(string tableName, string width, string heigth, string[] parameterValue = null, bool isRange = false)
        {
            var _valueDict = GetWorksheetParameters(_currentWorksheet.Name);

            Dictionary<string, string> _parametersValueDict = [];

            foreach(var value in _valueDict.Values)
            {
                _parametersValueDict.Add(value, string.Empty);
            }

            var keys = _parametersValueDict.Keys.ToList();

            for (int i = 0; i < parameterValue.Count(); i++)
            {
                _parametersValueDict[keys[i]] = parameterValue[i];
            }
            SetParameters(_parametersValueDict);

            int collumnNumber = 3;

            int rowNumber = 4;
            int collumnsCount = _currentWorksheet.Cells.MaxDataColumn;
            int rowsCount = _currentWorksheet.Cells.MaxDataRow;


            string findValue = GetParameter(collumnNumber, rowNumber, collumnsCount, rowsCount, width, heigth);
            

            if(isRange)
            {
                _currentWorksheet.Cells[0, 1].PutValue(findValue);

                _workbook.CalculateFormula();

                return _currentWorksheet.Cells[1, 1].Value.ToString();
            }
            else
            {
                return findValue;
            }
        }

        private string GetParameter(int collumnNumber, int rowNumber, int collumnsCount, int rowsCount, string width, string heigth)
        {
            for (int i = collumnNumber; i < collumnsCount; i++)
            {
                if (int.Parse(_currentWorksheet.Cells[0, i].Value.ToString()) >= int.Parse(width))
                {
                    for (int j = rowNumber; j < rowsCount; j++)
                    {
                        if (int.Parse(_currentWorksheet.Cells[j, 2].Value.ToString()) >= int.Parse(heigth))
                        {
                            return _currentWorksheet.Cells[j, i].Value.ToString();
                        }
                    }
                }
            }

            return string.Empty;
        }

        public string SetFormula(string tableName, string sheetName, string width, string heigth, string[] parameterValue = null, bool isRange = false)
        {
            string parameterString = parameterValue.Where(param => !string.IsNullOrEmpty(param))?.Count() > 0 ? string.Join(",", parameterValue) : string.Empty;
            string formulaString = parameterValue == null || string.IsNullOrEmpty(parameterString) ? $"'{tableName}', {sheetName}, {width}, {heigth}, '{isRange}'" : $"'{tableName}', {sheetName}, {width}, {heigth}, {parameterString} '{isRange}'";
            return $"ReadTable({formulaString})";
        }
    }
}
