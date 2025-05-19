using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для SideNavigation.xaml
    /// </summary>
    public partial class SideNavigation : UserControl
    {
        public static readonly DependencyProperty MenuListProperty =
            DependencyProperty.Register(
                    nameof(MenuList),
                    typeof(ObservableCollection<PageModel>),
                    typeof(SideNavigation),
                    new PropertyMetadata(new ObservableCollection<PageModel>())
                );

        public static readonly DependencyProperty OpenPageCommandProperty =
            DependencyProperty.Register(
                    nameof(OpenPageCommand),
                    typeof(ICommand),
                    typeof(SideNavigation),
                    new PropertyMetadata(null)
                );

        public static readonly DependencyProperty FrameProperty =
            DependencyProperty.Register(
                    nameof(CurrentFrame),
                    typeof(Frame),
                    typeof(SideNavigation),
                    new PropertyMetadata(null)
                );

        public static readonly DependencyProperty SideBarProperty =
            DependencyProperty.Register(
                    nameof(SideBar),
                    typeof(ColumnDefinition),
                    typeof(SideNavigation),
                    new PropertyMetadata(null)
                );

        public ObservableCollection<PageModel> MenuList
        {
            get => (ObservableCollection<PageModel>)GetValue(MenuListProperty);
            set => SetValue(MenuListProperty, value);
        }

        public ICommand OpenPageCommand
        {
            get => (ICommand)GetValue(OpenPageCommandProperty);
            set => SetValue(OpenPageCommandProperty, value);
        }

        public Frame CurrentFrame
        {
            get => (Frame)GetValue(FrameProperty);
            set => SetValue(FrameProperty, value);
        }

        public ColumnDefinition SideBar
        {
            get => (ColumnDefinition)GetValue(SideBarProperty);
            set => SetValue(SideBarProperty, value);
        }

        public SideNavigation()
        {
            InitializeComponent();
        }
    }
}
