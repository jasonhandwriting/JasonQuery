using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using ScintillaNET;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Contexts
{
    internal sealed class QueryEditorAutoCompleteVisiblePopupRefreshContext
    {
        public Scintilla Editor { get; set; }

        public QueryEditorAutoCompleteSession Session { get; set; }

        public KeyEventArgs KeyEventArgs { get; set; }

        public Func<string, DataTable> BuildData { get; set; }

        public Action<QueryEditorAutoCompleteRequest> ShowResolvedRequest { get; set; }

        public Action<C1TrueDBGrid> HidePopup { get; set; }

        public Action AfterShow { get; set; }

        public void Validate()
        {
            if (Editor == null)
            {
                throw new ArgumentNullException(nameof(Editor));
            }

            if (Session == null)
            {
                throw new ArgumentNullException(nameof(Session));
            }

            if (KeyEventArgs == null)
            {
                throw new ArgumentNullException(nameof(KeyEventArgs));
            }

            if (BuildData == null)
            {
                throw new ArgumentNullException(nameof(BuildData));
            }

            if (ShowResolvedRequest == null)
            {
                throw new ArgumentNullException(nameof(ShowResolvedRequest));
            }

            if (HidePopup == null)
            {
                throw new ArgumentNullException(nameof(HidePopup));
            }
        }
    }
}