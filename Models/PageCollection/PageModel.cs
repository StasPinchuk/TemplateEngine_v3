using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TemplateEngine_v3.Models.PageCollection
{
    /// <summary>
    /// Модель для представления страницы с дополнительными свойствами,
    /// такими как заголовок, тип страницы, иконка, состояние выбора и доступности.
    /// Позволяет создавать экземпляр страницы с параметрами конструктора.
    /// </summary>
    public class PageModel : BaseNotifyPropertyChanged
    {
        /// <summary>
        /// Заголовок страницы.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Тип страницы (наследник <see cref="Page"/>).
        /// </summary>
        public Type PageType { get; set; }

        /// <summary>
        /// Иконка, связанная со страницей.
        /// </summary>
        public PackIconKind Icon { get; set; }

        /// <summary>
        /// Имя группы, к которой принадлежит эта страница.
        /// </summary>
        public string GroupName { get; set; }

        private bool _isSelected = false;
        /// <summary>
        /// Выбрана ли текущая страница.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetValue(ref _isSelected, value, nameof(IsSelected));
        }

        private bool _isEnabled = false;
        /// <summary>
        /// Доступна ли текущая страница.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetValue(ref _isEnabled, value, nameof(IsEnabled));
        }

        /// <summary>
        /// Параметры конструктора, которые будут использованы при создании страницы.
        /// </summary>
        public object[] ConstructorParameters { get; set; }

        private Page _modelPage;
        /// <summary>
        /// Экземпляр страницы, создается при первом обращении.
        /// </summary>
        public Page ModelPage
        {
            get
            {
                if (_modelPage == null)
                {
                    _modelPage = CreatePage();
                }
                return _modelPage;
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> без параметров.
        /// </summary>
        public PageModel() { }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с указанным заголовком.
        /// </summary>
        public PageModel(string title)
        {
            Title = title;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком и типом страницы.
        /// </summary>
        public PageModel(string title, Type pageType)
        {
            Title = title;
            PageType = pageType;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком, типом страницы и иконкой.
        /// </summary>
        public PageModel(string title, Type pageType, PackIconKind icon)
        {
            Title = title;
            PageType = pageType;
            Icon = icon;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком, типом страницы и состоянием выбора.
        /// </summary>
        public PageModel(string title, Type pageType, bool isSelected)
        {
            Title = title;
            PageType = pageType;
            IsSelected = isSelected;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком, типом страницы, состоянием выбора и иконкой.
        /// </summary>
        public PageModel(string title, Type pageType, PackIconKind icon, bool isSelected)
        {
            Title = title;
            PageType = pageType;
            IsSelected = isSelected;
            Icon = icon;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком, типом страницы, состоянием выбора и доступности.
        /// </summary>
        public PageModel(string title, Type pageType, bool isSelected, bool isEnabled)
        {
            Title = title;
            PageType = pageType;
            IsSelected = isSelected;
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком, типом страницы и параметрами конструктора.
        /// </summary>
        public PageModel(string title, Type pageType, object[] constructorParameters)
        {
            Title = title;
            PageType = pageType;
            ConstructorParameters = constructorParameters;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком, типом страницы, иконкой и параметрами конструктора.
        /// </summary>
        public PageModel(string title, Type pageType, PackIconKind icon, object[] constructorParameters)
        {
            Title = title;
            PageType = pageType;
            ConstructorParameters = constructorParameters;
            Icon = icon;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком, типом страницы, состоянием выбора, иконкой и параметрами конструктора.
        /// </summary>
        public PageModel(string title, Type pageType, bool isSelected, PackIconKind icon, object[] constructorParameters)
        {
            Title = title;
            PageType = pageType;
            IsSelected = isSelected;
            ConstructorParameters = constructorParameters;
            Icon = icon;
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="PageModel"/> с заголовком, типом страницы, состоянием выбора, доступности и параметрами конструктора.
        /// </summary>
        public PageModel(string title, Type pageType, bool isSelected, bool isEnabled, object[] constructorParameters)
        {
            Title = title;
            PageType = pageType;
            IsSelected = isSelected;
            IsEnabled = isEnabled;
            ConstructorParameters = constructorParameters;
        }

        /// <summary>
        /// Создает экземпляр страницы, используя тип и параметры конструктора.
        /// </summary>
        /// <returns>Экземпляр страницы, или <c>null</c>, если конструктор не найден.</returns>
        private Page CreatePage()
        {
            if (PageType == null)
                return null;

            if (ConstructorParameters != null && ConstructorParameters.Length > 0)
            {
                var constructor = PageType.GetConstructor(ConstructorParameters.Select(p => p?.GetType()).ToArray());
                if (constructor != null)
                {
                    return (Page)constructor.Invoke(ConstructorParameters);
                }
            }

            var defaultConstructor = PageType.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor != null)
            {
                return (Page)defaultConstructor.Invoke(null);
            }

            MessageBox.Show("Для указанного типа страницы не найдено подходящего конструктора.", "Ошибка");

            return null;
        }

        /// <summary>
        /// Очищает созданный экземпляр страницы, чтобы при следующем обращении она была создана заново.
        /// </summary>
        public void ClearPage()
        {
            _modelPage = null;
        }
    }
}
