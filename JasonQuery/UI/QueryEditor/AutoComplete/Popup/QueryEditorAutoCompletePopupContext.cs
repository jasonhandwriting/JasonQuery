using ScintillaNET;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Popup
{
    internal sealed class QueryEditorAutoCompletePopupContext
    {
        public Form OwnerForm { get; set; }

        public Scintilla Editor { get; set; }

        public string QueryEditorFontSize { get; set; }

        public int SplitterDistance { get; set; }

        public int MainFormLeft { get; set; }

        public int MainFormTop { get; set; }

        public string GridName { get; set; } = string.Empty;

        public bool HideRecordSelectors { get; set; } = true;

        public int[] FetchStyleColumnIndexes { get; set; } = Array.Empty<int>();

        public bool AutoResizePopup { get; set; } = true;

        public int PopupHeight { get; set; } = 181;

        public int Position { get; set; } = -1;

        public int ScrollBarRowThreshold { get; set; } = 9;

        public int WidthPaddingWithoutScrollBar { get; set; } = 3;

        public int WidthPaddingWithScrollBar { get; set; } = 5;

        public void Validate()
        {
            if (OwnerForm == null)
            {
                throw new ArgumentNullException(nameof(OwnerForm));
            }

            if (Editor == null)
            {
                throw new ArgumentNullException(nameof(Editor));
            }

            if (string.IsNullOrWhiteSpace(QueryEditorFontSize))
            {
                throw new ArgumentNullException(nameof(QueryEditorFontSize));
            }

            if (PopupHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(PopupHeight));
            }
        }
    }
}
