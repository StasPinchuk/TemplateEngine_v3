using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References.Files;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Сервис для работы с таблицами Excel из папки "Генератор шаблонов\Таблицы" через TFlex и NPOI.
    /// Позволяет получать список таблиц, листов, параметры листов и работать с их значениями.
    /// </summary>
    public class TableService
    {
        private readonly ServerConnection _connection;
        private FileReference _fileReference;
        private FolderObject _tableFolder;
        private IWorkbook _workbook;
        private ISheet _currentWorksheet;

        public ObservableCollection<string> TableNames { get; set; } = new ObservableCollection<string>();

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
            TableNames.Clear();
            foreach (var file in _tableFolder.Children)
            {
                TableNames.Add(file.Name);
            }
        }

        private void SetWorkbook(string tableName)
        {
            string tablePath = GetSelectedTable(tableName);
            if (!string.IsNullOrEmpty(tablePath))
            {
                using (FileStream fs = new FileStream(tablePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    _workbook = new XSSFWorkbook(fs);
                }
            }
        }

        public ObservableCollection<string> GetWorksheets(string tableName)
        {
            var worksheetNames = new ObservableCollection<string>();

            string tablePath = GetSelectedTable(tableName);
            if (string.IsNullOrEmpty(tablePath))
                return worksheetNames;

            using (FileStream fs = new FileStream(tablePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                _workbook = new XSSFWorkbook(fs);
            }

            for (int i = 0; i < _workbook.NumberOfSheets; i++)
            {
                worksheetNames.Add(_workbook.GetSheetName(i));
            }

            return worksheetNames;
        }

        private string GetSelectedTable(string tableName)
        {
            var file = _fileReference.FindByRelativePath(@$"Генератор шаблонов\Таблицы\{tableName}") as FileObject;

            string newFilePath = $@"configs\Таблицы\{tableName}";
            if (!Directory.Exists(@"configs\Таблицы"))
                Directory.CreateDirectory(@"configs\Таблицы");

            if (File.Exists(newFilePath))
                if (file.LastChangeDate != File.GetCreationTime(newFilePath))
                {
                    FileInfo fileInfo = new FileInfo(newFilePath);
                    if (fileInfo.IsReadOnly)
                        fileInfo.IsReadOnly = false;
                    File.Delete(newFilePath);
                }

            File.Copy(file.LocalPath, newFilePath, true);

            FileInfo fileInfo2 = new FileInfo(newFilePath);
            if (fileInfo2.IsReadOnly)
                fileInfo2.IsReadOnly = false;

            return $@"configs\Таблицы\{tableName}";
        }

        public void SetCurrentWorkSheet(string worksheetName)
        {
            _currentWorksheet = _workbook.GetSheet(worksheetName);
        }

        public Dictionary<string, string> GetWorksheetParameters(string worksheetName)
        {
            SetCurrentWorkSheet(worksheetName);
            var parameterDictionary = new Dictionary<string, string>();
            if (_currentWorksheet == null)
                return parameterDictionary;

            for (int rowIndex = 0; rowIndex <= _currentWorksheet.LastRowNum; rowIndex++)
            {
                IRow row = _currentWorksheet.GetRow(rowIndex);
                if (row == null) continue;

                ICell leftCell = row.GetCell(0); // A-столбец
                if (leftCell == null || string.IsNullOrWhiteSpace(leftCell.ToString()))
                    continue;

                IFont font = _workbook.GetFontAt(leftCell.CellStyle.FontIndex);
                if (font != null && font.IsBold)
                {
                    string parameterName = leftCell.ToString();
                    ICell rightCell = row.GetCell(1);
                    if (rightCell == null) continue;

                    string parameterAddress = new CellReference(rowIndex, 1).FormatAsString();
                    parameterDictionary[parameterName] = parameterAddress;
                }
            }

            return parameterDictionary;
        }

        public void SetParameters(Dictionary<string, string> parameters)
        {
            if (_currentWorksheet == null)
                return;

            foreach (var kvp in parameters)
            {
                CellReference refCell = new CellReference(kvp.Key);
                IRow row = _currentWorksheet.GetRow(refCell.Row) ?? _currentWorksheet.CreateRow(refCell.Row);
                ICell cell = row.GetCell(refCell.Col) ?? row.CreateCell(refCell.Col);
                cell.SetCellValue(kvp.Value);
            }
        }

        public string GetFindParameter(string tableName, string workSheetName, string width, string heigth, string[] parameterValue = null, bool isRange = false)
        {
            SetWorkbook(tableName);
            SetCurrentWorkSheet(workSheetName);
            var valueDict = GetWorksheetParameters(_currentWorksheet?.SheetName ?? string.Empty);

            Dictionary<string, string> parametersValueDict = valueDict.Values.ToDictionary(value => value, value => string.Empty);

            if (parameterValue != null && parameterValue.Length != 0)
            {
                var keys = parametersValueDict.Keys.ToList();
                for (int i = 0; i < parameterValue.Length && i < keys.Count; i++)
                {
                    parametersValueDict[keys[i]] = parameterValue[i];
                }
            }

            SetParameters(parametersValueDict);

            int columnNumber = 3;
            int rowNumber = 4;
            int columnsCount = _currentWorksheet.GetRow(0)?.LastCellNum ?? 0;
            int rowsCount = _currentWorksheet.LastRowNum;

            string findValue = GetParameter(columnNumber, rowNumber, columnsCount, rowsCount, width, heigth);

            return findValue;
        }

        private string GetParameter(int columnNumber, int rowNumber, int columnsCount, int rowsCount, string width, string heigth)
        {
            if (int.TryParse(width, out int colValue) && colValue != 0)
                for (int i = columnNumber; i < columnsCount; i++)
                {
                    ICell headerCell = _currentWorksheet.GetRow(0)?.GetCell(i);
                    if (headerCell != null && int.TryParse(headerCell.ToString(), out int colVal) && colVal >= int.Parse(width))
                    {
                        for (int j = rowNumber; j < rowsCount; j++)
                        {
                            ICell rowCell = _currentWorksheet.GetRow(j)?.GetCell(2);
                            if (rowCell != null && int.TryParse(rowCell.ToString(), out int rowVal) && rowVal >= int.Parse(heigth))
                            {
                                return _currentWorksheet.GetRow(j)?.GetCell(i)?.ToString() ?? string.Empty;
                            }
                        }
                    }
                }
            else
                for (int j = rowNumber; j < rowsCount; j++)
                {
                    ICell rowCell = _currentWorksheet.GetRow(j)?.GetCell(2);
                    if (rowCell != null && int.TryParse(rowCell.ToString(), out int rowVal) && rowVal >= int.Parse(heigth))
                    {
                        return _currentWorksheet.GetRow(j)?.GetCell(3)?.ToString() ?? string.Empty;
                    }
                    if (rowCell != null && rowCell.ToString().Contains(heigth))
                    {
                        return _currentWorksheet.GetRow(j)?.GetCell(3)?.ToString() ?? string.Empty;
                    }
                }
            return string.Empty;
        }

        public string SetFormula(string tableName, string sheetName, string width, string heigth, string[] parameterValue = null, bool isRange = false)
        {
            string parameterString = parameterValue != null && parameterValue.Any()
                ? string.Join(",", parameterValue)
                : string.Empty;

            string formulaString = string.IsNullOrEmpty(parameterString)
                ? $"'{tableName}', '{sheetName}', {width}, {heigth}, '{isRange}'"
                : $"'{tableName}', '{sheetName}', {width}, {heigth}, {parameterString}, '{isRange}'";

            return $"ReadTable({formulaString})";
        }
    }
}
