using System;
using C1.Win.C1TrueDBGrid;
using ScintillaNET;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Contexts
{
    internal sealed class QueryEditorAutoCompleteUiContext //把 editor、Hide()、_bKeyUpFromAutoCompleteGrid 這些 UI 依賴集中起來
    {
        public Scintilla Editor { get; set; }

        public Action<C1TrueDBGrid> HidePopup { get; set; } //重構前的 .Hide()

        public Action MarkKeyUpFromAutoCompleteGrid { get; set; } //重構前的 _bKeyUpFromAutoCompleteGrid
    }
}