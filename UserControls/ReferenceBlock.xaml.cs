using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model.References;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ReferenceBlock.xaml
    /// </summary>
    public partial class ReferenceBlock : UserControl
    {
        public static DependencyProperty CurrentReferenceModelProperty =
            DependencyProperty.Register(
                    "CurrentReferenceModel",
                    typeof(ReferenceModelInfo),
                    typeof(ReferenceBlock),
                    new PropertyMetadata(null, OnSetReferenceObject)
                );

        public static readonly DependencyProperty NewIndicateProperty =
            DependencyProperty.Register(
                "NewIndicate",
                typeof(Visibility),
                typeof(ReferenceBlock),
                new PropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty EditIndicateProperty =
            DependencyProperty.Register(
                "EditIndicate",
                typeof(Visibility),
                typeof(ReferenceBlock),
                new PropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(
                "RemoveCommand",
                typeof(ICommand),
                typeof(ReferenceBlock),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CloneCommandProperty =
            DependencyProperty.Register(
                "CloneCommand",
                typeof(ICommand),
                typeof(ReferenceBlock),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(
                "EditCommand",
                typeof(ICommand),
                typeof(ReferenceBlock),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(
                "IsLocked",
                typeof(bool),
                typeof(ReferenceBlock),
                new PropertyMetadata(false));

        public ReferenceModelInfo CurrentReferenceModel
        {
            get => (ReferenceModelInfo)GetValue(CurrentReferenceModelProperty);
            set => SetValue(CurrentReferenceModelProperty, value);
        }

        public Visibility NewIndicate
        {
            get => (Visibility)GetValue(NewIndicateProperty);
            set => SetValue(NewIndicateProperty, value);
        }

        public Visibility EditIndicate
        {
            get => (Visibility)GetValue(EditIndicateProperty);
            set => SetValue(EditIndicateProperty, value);
        }

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public ICommand CloneCommand
        {
            get => (ICommand)GetValue(CloneCommandProperty);
            set => SetValue(CloneCommandProperty, value);
        }

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }

        public ReferenceBlock()
        {
            InitializeComponent();
        }

        private static void OnSetReferenceObject(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ReferenceBlock)d;
            if (control != null )
            {
                if (e.NewValue is ReferenceModelInfo reference)
                {
                    if (reference.CreateDate.Date == DateTime.Now.Date)
                        control.NewIndicate = Visibility.Visible;
                    if (reference.LastEditDate.Date == DateTime.Now.Date && reference.LastEditDate != reference.CreateDate)
                        control.EditIndicate = Visibility.Visible;
                }
                else
                    control.NewIndicate = Visibility.Collapsed;
            }
        }
    }
}
