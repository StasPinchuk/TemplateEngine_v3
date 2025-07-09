using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;

namespace TemplateEngine_v3.Models
{
    public class FormulaCompletionData : ICompletionData
    {
        private readonly ConditionEvaluator _formula;
        private readonly ConditionEvaluator _innerFormula;
        private readonly string _formulaId;
        private readonly string _formulaName;
        private readonly Action _updatePartTree;

        public FormulaCompletionData(ConditionEvaluator innerFormula, ConditionEvaluator formula, Action updatePartTree)
        {
            _innerFormula = innerFormula;
            _formulaName = innerFormula.Name;
            _formulaId = innerFormula.Id;
            _formula = formula;
            _updatePartTree = updatePartTree;
        }

        public string Text => _formulaName;
        public object Content => _formulaName;
        public object Description => $"Формула: {_innerFormula.Value}";
        public double Priority => 0;
        public System.Windows.Media.ImageSource Image => null;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs e)
        {
            var doc = textArea.Document;
            int caretOffset = textArea.Caret.Offset;

            // Найти позицию последней открывающей скобки перед текущим курсором
            int bracketOffset = doc.Text.LastIndexOf('[', caretOffset - 1);
            if (bracketOffset < 0) return;

            // Проверяем, есть ли после курсора закрывающая скобка
            bool hasClosingBracketAfterCaret = false;
            if (caretOffset < doc.TextLength)
            {
                char nextChar = doc.GetCharAt(caretOffset);
                hasClosingBracketAfterCaret = nextChar == ']';
            }

            // Длина заменяемого текста - всё после [ до курсора
            int lengthToReplace = caretOffset - (bracketOffset + 1);

            // Формируем строку замены - с ] если после курсора нет ]
            string replacementText = hasClosingBracketAfterCaret ? _formulaName : _formulaName + "]";

            doc.Replace(bracketOffset + 1, lengthToReplace, replacementText);

            // Устанавливаем курсор после вставленного текста (после ] если она была добавлена)
            textArea.Caret.Offset = bracketOffset + 1 + replacementText.Length;

            // Добавить ID в Parts
            if (!_formula.Parts.Contains(_formulaId))
            {
                _formula.Parts.Add(_formulaId);
                _updatePartTree?.Invoke();
            }
        }



    }

}
