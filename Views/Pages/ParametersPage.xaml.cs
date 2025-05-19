using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ParametersPage.xaml
    /// </summary>
    public partial class ParametersPage : Page
    {
        public ParametersPage(INodeManager nodeManager)
        {
            InitializeComponent(); 
            
            Editor.TextArea.TextEntered += (sender, e) =>
            {
                if (e.Text == "(")
                {
                    Editor.Document.Insert(Editor.CaretOffset, ")");
                    Editor.CaretOffset--; // вернуть курсор между скобками
                }
                else if (e.Text == "[")
                {
                    Editor.Document.Insert(Editor.CaretOffset, "]");
                    Editor.CaretOffset--;
                }
                else if (e.Text == "{")
                {
                    Editor.Document.Insert(Editor.CaretOffset, "}");
                    Editor.CaretOffset--;
                }
                else if (e.Text == "\"")
                {
                    Editor.Document.Insert(Editor.CaretOffset, "\"");
                    Editor.CaretOffset--;
                }
            };

            DataContext = new ParametersPageVM(nodeManager);
        }
    }
}
