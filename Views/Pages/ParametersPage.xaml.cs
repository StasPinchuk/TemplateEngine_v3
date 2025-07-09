using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ParametersPage.xaml
    /// </summary>
    public partial class ParametersPage : Page
    {
        private CompletionWindow completionWindow;
        private readonly ParametersPageVM _viewModel;

        private readonly DispatcherTimer completionTimer;
        private string lastPrefix = "";

        public ParametersPage(NodeManager nodeManager)
        {
            InitializeComponent();

            _viewModel = new ParametersPageVM(nodeManager);
            DataContext = _viewModel;

            completionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            completionTimer.Tick += CompletionTimer_Tick;

            Editor.TextArea.TextEntered += TextArea_TextEntered;
            Editor.TextArea.TextEntering += TextArea_TextEntering;
            Editor.TextChanged += Editor_TextChanged;  

            Unloaded += ParametersPage_Unloaded;
        }

        private void ParametersPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel is IDisposable disposable)
                disposable.Dispose();
        }

        private void SetCurrentEvaluatorCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is ConditionEvaluator eval)
            {
                var _viewModel = DataContext as ParametersPageVM;
                if (_viewModel?.SetCurrentEvaluatorCommand.CanExecute(eval) == true)
                    _viewModel.SetCurrentEvaluatorCommand.Execute(eval);
            }
        }

        private void SetMarkingCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is string eval)
            {
                var _viewModel = DataContext as ParametersPageVM;
                if (_viewModel?.SetMarkingCommand.CanExecute(eval) == true)
                    _viewModel.SetMarkingCommand.Execute(eval);
            }
        }

        private void SetSystemFormulaCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is ConditionEvaluator eval)
            {
                var _viewModel = DataContext as ParametersPageVM;
                if (_viewModel?.SetSystemFormulaCommand.CanExecute(eval) == true)
                    _viewModel.SetSystemFormulaCommand.Execute(eval);
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            completionTimer.Stop();
            completionTimer.Start();
        }

        private void CompletionTimer_Tick(object sender, EventArgs e)
        {
            completionTimer.Stop();

            string prefix = GetWordAfterBracket();

            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
            {
                completionWindow?.Close();
                lastPrefix = "";
                return;
            }

            if (prefix == lastPrefix)
            {
                // Префикс не изменился — не обновляем подсказку
                return;
            }

            lastPrefix = prefix;
            ShowCompletion(prefix, _viewModel.CurrentEvaluator);
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (completionWindow == null) return;

            if (e.Text.Length > 0 && !char.IsLetterOrDigit(e.Text[0]))
            {
                // Закрываем подсказку, если введён не буквенно-цифровой символ (например, пробел, запятая и т.п.)
                completionWindow.Close();
            }
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            var currentWord = GetWordAfterBracket();
            if (!string.IsNullOrWhiteSpace(currentWord))
            {
                ShowCompletion(currentWord, _viewModel.CurrentEvaluator);
            }
        }

        private string GetWordAfterBracket()
        {
            int offset = Editor.CaretOffset;
            if (offset == 0) return null;

            string text = Editor.Text.Substring(0, offset);

            // Ищем ближайший слева '[' без пары ']'
            for (int i = offset - 1; i >= 0; i--)
            {
                if (text[i] == ']')
                    break; // Если встретили ], значит [ до него - закрытая скобка, дальше не смотрим

                if (text[i] == '[')
                {
                    // Возвращаем все символы после [
                    if (i + 1 < offset)
                        return text.Substring(i + 1, offset - (i + 1));
                    else
                        return string.Empty;
                }
            }

            return null;
        }


        private void ShowCompletion(string prefix, ConditionEvaluator currentFormula)
        {
            completionWindow?.Close();

            var matches = _viewModel.AllTemplateEvaluator
                .Where(f => f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && !currentFormula.Parts.Contains(f.Id))
                .ToList();

            if (matches.Count == 0) return;

            completionWindow = new CompletionWindow(Editor.TextArea);

            completionWindow.MinWidth = 200;
            completionWindow.MaxWidth = 600;
            completionWindow.Width = 600;

            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            foreach (ConditionEvaluator eval in matches)
                data.Add(new FormulaCompletionData(eval, currentFormula, _viewModel.CreatePartTree));

            var listBox = completionWindow.CompletionList.ListBox;

            double maxWidth = 0;
            foreach (var item in listBox.Items)
            {
                var container = listBox.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    container.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    if (container.DesiredSize.Width > maxWidth)
                        maxWidth = container.DesiredSize.Width;
                }
            }
            maxWidth += 20;
            completionWindow.Width = maxWidth;

            completionWindow.Show();
        }
    }
}
