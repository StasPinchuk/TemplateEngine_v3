using Aspose.Cells;
using SharpCompress.Common;
using System;
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
    /// Сервис для работы с таблицами Excel из папки "Генератор шаблонов\Таблицы" через TFlex и Aspose.Cells.
    /// Позволяет получать список таблиц, листов, параметры листов и работать с их значениями.
    /// </summary>
    public class TableService
    {
        private readonly ServerConnection _connection;
        private FileReference _fileReference;
        private FolderObject _tableFolder;
        private Workbook _workbook;
        private Worksheet _currentWorksheet;

        /// <summary>
        /// Коллекция имён таблиц (файлов) в целевой папке.
        /// </summary>
        public ObservableCollection<string> TableNames { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Создаёт экземпляр TableService с указанным соединением.
        /// Инициализирует список таблиц.
        /// </summary>
        /// <param name="connection">Соединение с сервером TFlex.</param>
        public TableService(ServerConnection connection)
        {
            _connection = connection;
            GetTableList();
        }

        /// <summary>
        /// Загружает список файлов-таблиц из папки "Генератор шаблонов\Таблицы" и обновляет TableNames.
        /// Если папка не найдена — показывает сообщение об ошибке.
        /// </summary>
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

        private void SetWorkbookAsync(string tableName)
        {
            string tablePath = GetSelectedTable(tableName);
            if (!string.IsNullOrEmpty(tablePath))
                _workbook = new Workbook(tablePath);
        }

        /// <summary>
        /// Асинхронно получает список листов указанной таблицы.
        /// </summary>
        /// <param name="tableName">Имя таблицы (файла).</param>
        /// <returns>Коллекция имён листов Excel в таблице.</returns>
        public ObservableCollection<string> GetWorksheets(string tableName)
        {
            var worksheetNames = new ObservableCollection<string>();

            string tablePath = GetSelectedTable(tableName);
            if (string.IsNullOrEmpty(tablePath))
                return worksheetNames;

            _workbook = new Workbook(tablePath);

            foreach (var worksheet in _workbook.Worksheets)
            {
                worksheetNames.Add(worksheet.Name);
            }

            return worksheetNames;
        }

        /// <summary>
        /// Асинхронно получает локальный путь к файлу таблицы по её имени.
        /// </summary>
        /// <param name="tableName">Имя таблицы (файла).</param>
        /// <returns>Локальный путь к файлу таблицы либо пустая строка, если файл не найден.</returns>
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
            _currentWorksheet = _workbook.Worksheets.FirstOrDefault(sheet => sheet.Name.Equals(worksheetName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Получает параметры текущего листа, анализируя его первый и второй столбцы.
        /// Параметры определяются по ячейкам первого столбца с жирным шрифтом.
        /// </summary>
        /// <param name="worksheetName">Имя листа Excel.</param>
        /// <returns>Словарь параметров: имя параметра => адрес ячейки со значением.</returns>
        public Dictionary<string, string> GetWorksheetParameters(string worksheetName)
        {
            SetCurrentWorkSheet(worksheetName);
            var parameterDictionary = new Dictionary<string, string>();
            if (_currentWorksheet == null)
                return parameterDictionary;

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
                    if (rightCell == null)
                        continue;

                    string parameterAddress = rightCell.Name;

                    parameterDictionary[parameterName] = parameterAddress;
                }
            }

            return parameterDictionary;
        }

        /// <summary>
        /// Записывает значения параметров в ячейки текущего листа.
        /// </summary>
        /// <param name="parameters">Словарь параметров: адрес ячейки => значение.</param>
        public void SetParameters(Dictionary<string, string> parameters)
        {
            if (_currentWorksheet == null)
                return;

            foreach (var kvp in parameters)
            {
                _currentWorksheet.Cells[kvp.Key].PutValue(kvp.Value);
            }

            _workbook.CalculateFormula();
        }

        /// <summary>
        /// Выполняет поиск параметра в таблице по заданным размерам и параметрам.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="width">Ширина для поиска.</param>
        /// <param name="heigth">Высота для поиска.</param>
        /// <param name="parameterValue">Массив значений параметров для подстановки.</param>
        /// <param name="isRange">Флаг, указывающий, возвращать ли диапазон значений.</param>
        /// <returns>Найденное значение или пустая строка.</returns>
        public string GetFindParameter(string tableName, string workSheetName, string width, string heigth, string[] parameterValue = null, bool isRange = false)
        {
            SetWorkbookAsync(tableName);
            SetCurrentWorkSheet(workSheetName);
            var valueDict = GetWorksheetParameters(_currentWorksheet?.Name ?? string.Empty);

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
            int columnsCount = _currentWorksheet.Cells.MaxDataColumn;
            int rowsCount = _currentWorksheet.Cells.MaxDataRow;

            string findValue = GetParameter(columnNumber, rowNumber, columnsCount, rowsCount, width, heigth);

            if (isRange)
            {
                _currentWorksheet.Cells[0, 1].PutValue(findValue);
                _workbook.CalculateFormula();
                _workbook = null;
                return _currentWorksheet.Cells[1, 1].Value?.ToString() ?? string.Empty;
            }
            else
            {
                _workbook = null;
                return findValue;
            }
        }

        /// <summary>
        /// Вспомогательный метод для поиска параметра внутри таблицы по заданным координатам.
        /// </summary>
        /// <param name="columnNumber">Стартовый номер столбца.</param>
        /// <param name="rowNumber">Стартовый номер строки.</param>
        /// <param name="columnsCount">Общее количество столбцов.</param>
        /// <param name="rowsCount">Общее количество строк.</param>
        /// <param name="width">Значение ширины для поиска.</param>
        /// <param name="heigth">Значение высоты для поиска.</param>
        /// <returns>Найденное значение или пустая строка.</returns>
        private string GetParameter(int columnNumber, int rowNumber, int columnsCount, int rowsCount, string width, string heigth)
        {
            if(int.TryParse(width, out int colValue) && colValue != 0)
                for (int i = columnNumber; i < columnsCount; i++)
                {
                    if (int.TryParse(_currentWorksheet.Cells[0, i].Value?.ToString(), out int colVal) && colVal >= int.Parse(width))
                    {
                        for (int j = rowNumber; j < rowsCount; j++)
                        {
                            if (int.TryParse(_currentWorksheet.Cells[j, 2].Value?.ToString(), out int rowVal) && rowVal >= int.Parse(heigth))
                            {
                                return _currentWorksheet.Cells[j, i].Value?.ToString() ?? string.Empty;
                            }
                        }
                    }
                }
            else
                for (int j = rowNumber; j < rowsCount; j++)
                {
                    if (int.TryParse(_currentWorksheet.Cells[j, 2].Value?.ToString(), out int rowVal) && rowVal >= int.Parse(heigth))
                    {
                        return _currentWorksheet.Cells[j, 3].Value?.ToString() ?? string.Empty;
                    }
                    if (_currentWorksheet.Cells[j, 2].Value?.ToString().Contains(heigth) == true)
                    {
                        return _currentWorksheet.Cells[j, 3].Value?.ToString() ?? string.Empty;
                    }
                }
            return string.Empty;
        }

        /// <summary>
        /// Формирует строку формулы для вызова функции ReadTable с параметрами таблицы и листа.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="sheetName">Имя листа.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="heigth">Высота.</param>
        /// <param name="parameterValue">Значения дополнительных параметров.</param>
        /// <param name="isRange">Флаг диапазона.</param>
        /// <returns>Строка формулы для вставки.</returns>
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
