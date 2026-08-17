using System;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Popup
{
    internal interface IQueryEditorAutoCompletePopupPresenterHost
    {
        bool TryApplyResolvedPeriodRequest(QueryEditorAutoCompleteRequest request, int initialCaretPosition);

        bool TryApplyResolvedSpaceRequest(QueryEditorAutoCompleteRequest request, int initialCaretPosition);

        bool TryFinalizePeriodAutoComplete(int triggerPosition);

        bool TryFinalizeSpaceAutoComplete(int triggerPosition);

        void HidePeriodPopup();

        void HideSpacePopup();

        bool ShowResolvedPeriodAutoComplete(QueryEditorAutoCompleteRequest request);

        bool ShowResolvedSpaceAutoComplete(QueryEditorAutoCompleteRequest request);

        void ResizePeriodPopup();

        void ResizeSpacePopup();
    }

    internal sealed class QueryEditorAutoCompletePopupPresenter
    {
        private readonly IQueryEditorAutoCompletePopupPresenterHost _host;

        public QueryEditorAutoCompletePopupPresenter(IQueryEditorAutoCompletePopupPresenterHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public bool TryPresentPeriod(QueryEditorAutoCompleteRequest request, int initialCaretPosition)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!_host.TryApplyResolvedPeriodRequest(request, initialCaretPosition))
            {
                _host.HidePeriodPopup();
                return false;
            }

            if (!_host.TryFinalizePeriodAutoComplete(request.TriggerPosition))
            {
                _host.HidePeriodPopup();
                return false;
            }

            return true;
        }

        public bool TryPresentSpace(QueryEditorAutoCompleteRequest request, int initialCaretPosition)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!_host.TryApplyResolvedSpaceRequest(request, initialCaretPosition))
            {
                _host.HideSpacePopup();
                return false;
            }

            if (!_host.TryFinalizeSpaceAutoComplete(request.TriggerPosition))
            {
                _host.HideSpacePopup();
                return false;
            }

            return true;
        }

        public bool ShowResolvedPeriodAutoComplete(QueryEditorAutoCompleteRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _host.ShowResolvedPeriodAutoComplete(request);
        }

        public bool ShowResolvedSpaceAutoComplete(QueryEditorAutoCompleteRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _host.ShowResolvedSpaceAutoComplete(request);
        }

        public void ResizePeriodPopup()
        {
            _host.ResizePeriodPopup();
        }

        public void ResizeSpacePopup()
        {
            _host.ResizeSpacePopup();
        }
    }
}