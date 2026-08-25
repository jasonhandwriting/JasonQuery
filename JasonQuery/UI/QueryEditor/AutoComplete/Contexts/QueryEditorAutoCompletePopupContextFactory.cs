using JasonQuery.Core.Config;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Support;
using ScintillaNET;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Contexts
{
    internal interface IQueryEditorAutoCompletePopupContextFactoryHost
    {
        Form OwnerForm { get; }

        Scintilla Editor { get; }

        string QueryEditorFontSize { get; }

        int SplitterDistance { get; }
    }

    internal sealed class QueryEditorAutoCompletePopupContextFactory
    {
        private readonly IQueryEditorAutoCompletePopupContextFactoryHost _host;
        private readonly QueryEditorAutoCompleteUiContextBuilder _uiContextBuilder;

        public QueryEditorAutoCompletePopupContextFactory(IQueryEditorAutoCompletePopupContextFactoryHost host, QueryEditorAutoCompleteUiContextBuilder uiContextBuilder)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _uiContextBuilder = uiContextBuilder ?? throw new ArgumentNullException(nameof(uiContextBuilder));
        }

        public QueryEditorAutoCompletePopupContext CreatePopupContext(int[] fetchStyleColumnIndexes, int position = -1)
        {
            return new QueryEditorAutoCompletePopupContext
            {
                OwnerForm = _host.OwnerForm,
                Editor = _host.Editor,
                QueryEditorFontSize = _host.QueryEditorFontSize,
                SplitterDistance = _host.SplitterDistance,
                MainFormLeft = AppConfigHelper.MainFormLeft,
                MainFormTop = AppConfigHelper.MainFormTop,
                HideRecordSelectors = true,
                FetchStyleColumnIndexes = fetchStyleColumnIndexes ?? Array.Empty<int>(),
                AutoResizePopup = true,
                PopupHeight = 181,
                ScrollBarRowThreshold = 9,
                WidthPaddingWithoutScrollBar = 3,
                WidthPaddingWithScrollBar = 5,
                Position = position
            };
        }

        public QueryEditorAutoCompletePopupContext CreatePeriodPopupContext(QueryEditorAutoCompleteRequest request, int[] fetchStyleColumnIndexes)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return CreatePopupContext(fetchStyleColumnIndexes, request.TriggerPosition);
        }

        public QueryEditorAutoCompletePopupContext CreateSpacePopupContext(int[] fetchStyleColumnIndexes)
        {
            return CreatePopupContext(fetchStyleColumnIndexes);
        }

        public QueryEditorAutoCompleteVisiblePopupRefreshContext CreateVisiblePopupRefreshContext(QueryEditorAutoCompleteSession session, KeyEventArgs e, Func<string, DataTable> buildData,
                                                                                                  Action<QueryEditorAutoCompleteRequest> showResolvedRequest, Action afterShow)
        {
            return _uiContextBuilder.CreateVisiblePopupRefreshContext(session, e, buildData, showResolvedRequest, afterShow);
        }
    }
}
