using System;
using System.Collections.Generic;
using System.Windows;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.Structure;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Менеджер для получения списка наименований операций из справочника.
    /// </summary>
    public class OperationNamesManager
    {
        private readonly ReferenceInfo _operationInfo;
        private readonly Reference _reference;
        private readonly ParameterInfo _nameParameter;

        /// <summary>
        /// Список названий операций.
        /// </summary>
        public readonly List<string> OperationNameList = new();

        public OperationNamesManager(ReferenceInfo operationInfo)
        {
            _operationInfo = operationInfo ?? throw new ArgumentNullException(nameof(operationInfo));

            _reference = _operationInfo.CreateReference();
            _nameParameter = _reference.ParameterGroup.Parameters.FindByName("Наименование")
                             ?? throw new InvalidOperationException("Параметр 'Наименование' не найден.");

            LoadOperationNames();
        }

        /// <summary>
        /// Загружает и сортирует список названий операций из справочника.
        /// </summary>
        private void LoadOperationNames()
        {
            try
            {
                // Загружаем объекты из справочника
                _reference.Objects.Load();

                OperationNameList.Clear();

                // Проходим по всем объектам справочника и получаем значение параметра "Наименование"
                foreach (var obj in _reference.Objects)
                {
                    var nameValue = obj[_nameParameter.Guid]?.ToString();
                    if (!string.IsNullOrEmpty(nameValue))
                    {
                        OperationNameList.Add(nameValue);
                    }
                }

                // Сортируем список по алфавиту
                OperationNameList.Sort();
            }
            catch (Exception ex)
            {
                // Отображаем сообщение об ошибке
                MessageBox.Show($"Ошибка при загрузке названий операций: {ex.Message}");
            }
        }
    }
}
