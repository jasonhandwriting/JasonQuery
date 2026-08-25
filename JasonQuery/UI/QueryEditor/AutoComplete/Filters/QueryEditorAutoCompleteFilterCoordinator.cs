using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.Core.Text;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Filters
{
    internal interface IQueryEditorAutoCompleteFilterCoordinatorHost
    {
        string EditorText { get; }

        int EditorCurrentPosition { get; }

        DataTable PeriodAutoCompleteTable { get; }

        DataTable SpaceAutoCompleteTable { get; }

        int PeriodTriggerPosition { get; }

        int SpaceTriggerPosition { get; }

        QueryEditorAutoCompleteSession GetActiveAutoCompleteSession();

        void HidePeriodPopup();

        void HideSpacePopup();

        void ShowException(Exception ex);
    }

    internal sealed class QueryEditorAutoCompleteFilterCoordinator
    {
        private readonly IQueryEditorAutoCompleteFilterCoordinatorHost _host;
        private readonly QueryEditorAutoCompletePopupPresenter _popupPresenter;

        public QueryEditorAutoCompleteFilterCoordinator
        (
            IQueryEditorAutoCompleteFilterCoordinatorHost host,
            QueryEditorAutoCompletePopupPresenter popupPresenter
        )
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _popupPresenter = popupPresenter ?? throw new ArgumentNullException(nameof(popupPresenter));
        }

        public bool TryApplyPeriodKeywordFilterFromEditor(int triggerPosition, bool isCtrlJPeriodFallbackToAll)
        {
            if (!TryGetPeriodKeywordAfterTrigger(triggerPosition, out var keyword))
            {
                return TryShowAllPeriodCandidates(triggerPosition);
            }

            return TryApplyPeriodKeywordFilter(keyword, isCtrlJPeriodFallbackToAll);
        }

        private bool TryShowAllPeriodCandidates(int triggerPosition)
        {
            try
            {
                var table = _host.PeriodAutoCompleteTable;

                if (table == null || table.Rows.Count == 0)
                {
                    return false;
                }

                var request = new QueryEditorAutoCompleteRequest
                {
                    TriggerPosition = triggerPosition > 0 ? triggerPosition : _host.PeriodTriggerPosition,
                    Data = table
                };

                return _popupPresenter.ShowResolvedPeriodAutoComplete(request);
            }
            catch (Exception ex)
            {
                CloseActiveSession(QueryEditorAutoCompletePopupKind.Period, QueryEditorAutoCompleteSessionCloseReason.RefreshFailed);
                _host.HidePeriodPopup();
                _host.ShowException(ex);
                return false;
            }
        }

        public bool TryApplySpaceKeywordFilterFromEditor(int triggerPosition)
        {
            if (!TryGetSpaceKeywordAfterTrigger(triggerPosition, out var keyword))
            {
                return true; // 空白後面沒有關鍵字，表示不需要過濾
            }

            return TryApplySpaceKeywordFilter(keyword);
        }

        public bool TryCloseActiveAutoCompletePopupOnInvalidCharKeyUp(KeyEventArgs e)
        {
            var session = _host.GetActiveAutoCompleteSession();

            if (session == null || session.Grid == null || !session.IsVisible)
            {
                return false;
            }

            if (!QueryEditorAutoCompleteKeyUpHelper.ShouldProcessAutoCompleteTextKeyUp(e))
            {
                return false;
            }

            if (!TryGetLastTypedEditorChar(out var ch))
            {
                return false;
            }

            if (IsAllowedAutoCompleteChar(session.Kind, ch))
            {
                return false;
            }

            session.Close(QueryEditorAutoCompleteSessionCloseReason.InvalidInput);
            HidePopupByKind(session.Kind);
            return true;
        }

        private bool TryApplyPeriodKeywordFilter(string keyword, bool fallbackToAllWhenNoMatch = false)
        {
            try
            {
                var table = _host.PeriodAutoCompleteTable;

                if (table == null || table.Rows.Count == 0)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return true;
                }

                var dtFiltered = QueryEditorAutoCompleteDataFilter.FilterPeriodByKeyword(table, keyword);

                if (dtFiltered == null || dtFiltered.Rows.Count == 0)
                {
                    if (!fallbackToAllWhenNoMatch)
                    {
                        CloseActiveSession(QueryEditorAutoCompletePopupKind.Period, QueryEditorAutoCompleteSessionCloseReason.NoCandidates);
                        _host.HidePeriodPopup();
                        return false;
                    }

                    var fallbackRequest = new QueryEditorAutoCompleteRequest
                    {
                        TriggerPosition = _host.PeriodTriggerPosition,
                        Data = table
                    };

                    return _popupPresenter.ShowResolvedPeriodAutoComplete(fallbackRequest);
                }

                var request = new QueryEditorAutoCompleteRequest
                {
                    TriggerPosition = _host.PeriodTriggerPosition,
                    Data = dtFiltered
                };

                return _popupPresenter.ShowResolvedPeriodAutoComplete(request);
            }
            catch (Exception ex)
            {
                CloseActiveSession(QueryEditorAutoCompletePopupKind.Period, QueryEditorAutoCompleteSessionCloseReason.RefreshFailed);
                _host.HidePeriodPopup();
                _host.ShowException(ex);
                return false;
            }
        }

        private bool TryApplySpaceKeywordFilter(string keyword)
        {
            try
            {
                var table = _host.SpaceAutoCompleteTable;

                if (table == null || table.Rows.Count == 0)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return true;
                }

                var dtFiltered = SpaceAutoCompleteDataFilter.FilterByKeyword(table, keyword);

                if (dtFiltered == null || dtFiltered.Rows.Count == 0)
                {
                    CloseActiveSession(QueryEditorAutoCompletePopupKind.Space, QueryEditorAutoCompleteSessionCloseReason.NoCandidates);
                    _host.HideSpacePopup();
                    return false;
                }

                var request = new QueryEditorAutoCompleteRequest
                {
                    TriggerPosition = _host.SpaceTriggerPosition,
                    Data = dtFiltered
                };

                return _popupPresenter.ShowResolvedSpaceAutoComplete(request);
            }
            catch (Exception ex)
            {
                CloseActiveSession(QueryEditorAutoCompletePopupKind.Space, QueryEditorAutoCompleteSessionCloseReason.RefreshFailed);
                _host.HideSpacePopup();
                _host.ShowException(ex);
                return false;
            }
        }

        private bool TryGetPeriodKeywordAfterTrigger(int triggerPosition, out string keyword)
        {
            return TryGetAutoCompleteKeywordAfterTrigger(triggerPosition, IsPeriodKeywordChar, out keyword);
        }

        private bool TryGetSpaceKeywordAfterTrigger(int triggerPosition, out string keyword)
        {
            return TryGetAutoCompleteKeywordAfterTrigger(triggerPosition, IsSpaceKeywordChar, out keyword);
        }

        private bool TryGetAutoCompleteKeywordAfterTrigger(int triggerPosition, Func<char, bool> isKeywordChar, out string keyword)
        {
            keyword = string.Empty;

            if (isKeywordChar == null)
            {
                return false;
            }

            var text = _host.EditorText ?? string.Empty;

            if (triggerPosition < 0 || triggerPosition >= text.Length)
            {
                return false;
            }

            var startIndex = triggerPosition;
            var endIndex = startIndex;

            while (endIndex < text.Length && isKeywordChar(text[endIndex]))
            {
                endIndex++;
            }

            if (endIndex <= startIndex)
            {
                return false;
            }

            keyword = text.Substring(startIndex, endIndex - startIndex).Trim();
            return !string.IsNullOrWhiteSpace(keyword);
        }

        private bool TryGetLastTypedEditorChar(out char ch)
        {
            ch = '\0';

            var text = _host.EditorText ?? string.Empty;
            var currentPosition = _host.EditorCurrentPosition;

            if (currentPosition <= 0 || currentPosition > text.Length)
            {
                return false;
            }

            ch = text[currentPosition - 1];
            return true;
        }

        private bool IsAllowedAutoCompleteChar(QueryEditorAutoCompletePopupKind kind, char ch)
        {
            switch (kind)
            {
                case QueryEditorAutoCompletePopupKind.Period:
                    {
                        return IsPeriodKeywordChar(ch);
                    }
                case QueryEditorAutoCompletePopupKind.Space:
                    {
                        return IsSpaceKeywordChar(ch);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private void CloseActiveSession(QueryEditorAutoCompletePopupKind kind, QueryEditorAutoCompleteSessionCloseReason closeReason)
        {
            var session = _host.GetActiveAutoCompleteSession();

            if (session != null && session.Kind == kind)
            {
                session.Close(closeReason);
            }
        }

        private void HidePopupByKind(QueryEditorAutoCompletePopupKind kind)
        {
            switch (kind)
            {
                case QueryEditorAutoCompletePopupKind.Period:
                    {
                        _host.HidePeriodPopup();
                        break;
                    }
                case QueryEditorAutoCompletePopupKind.Space:
                    {
                        _host.HideSpacePopup();
                        break;
                    }
            }
        }

        private static bool IsPeriodKeywordChar(char ch)
        {
            return TextHelper.IsEngAlphabetOrNumber(ch, '_');
        }

        private static bool IsSpaceKeywordChar(char ch)
        {
            return TextHelper.IsEngAlphabetOrNumber(ch, '_', '.');
        }
    }
}
