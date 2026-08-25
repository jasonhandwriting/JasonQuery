using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.Core.Text;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal interface IQueryEditorPeriodTriggerCoordinatorHost
    {
        bool IsEditorFocused { get; }

        bool IsAutoListMembersEnabled { get; }

        bool IsSqlServer { get; }

        bool IsMySql { get; }

        string EditorText { get; }

        int EditorCurrentPosition { get; }

        QueryEditorAutoCompleteSession GetActiveAutoCompleteSession();

        bool HandleAutoCompletePeriodKey(int triggerPositionOverride);

        void HideSpacePopup();

        void HidePeriodPopup();

        void ShowException(Exception ex);
    }

    internal sealed class QueryEditorPeriodTriggerCoordinator
    {
        private readonly IQueryEditorPeriodTriggerCoordinatorHost _host;
        private readonly QueryEditorCtrlJPeriodResolver _ctrlJPeriodResolver;

        private bool _isCtrlJKeyPressPending; //記住是否按下 Ctrl+J (KeyUp 無法偵測 Ctrl+J)
        private bool _isCtrlJPeriodFallbackToAll; //20260425 Ctrl+J 的 Period 關鍵字無命中時，是否退回顯示全部

        public QueryEditorPeriodTriggerCoordinator(IQueryEditorPeriodTriggerCoordinatorHost host, QueryEditorCtrlJPeriodResolver ctrlJPeriodResolver)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _ctrlJPeriodResolver = ctrlJPeriodResolver ?? throw new ArgumentNullException(nameof(ctrlJPeriodResolver));
        }

        public bool IsCtrlJKeyPressPending
        {
            get { return _isCtrlJKeyPressPending; }
        }

        public bool IsCtrlJPeriodFallbackToAll
        {
            get { return _isCtrlJPeriodFallbackToAll; }
        }

        public void ResetCtrlJKeyPressPending()
        {
            _isCtrlJKeyPressPending = false;
            _isCtrlJPeriodFallbackToAll = false;
        }

        public bool TryHandleCtrlJAutoComplete()
        {
            try
            {
                if (!CanHandleCtrlJAutoComplete())
                {
                    return false;
                }

                var triggerInfo = _ctrlJPeriodResolver.Resolve(_host.IsSqlServer, _host.IsMySql);

                if (!triggerInfo.CanTrigger)
                {
                    return false;
                }

                //先設狀態，再進入 HandleAutoCompletePeriodKey，避免 Ctrl+J 對應的 KeyUp / finalize 流程先跑進來時，還看不到 pending 狀態
                _isCtrlJKeyPressPending = true;
                _isCtrlJPeriodFallbackToAll = triggerInfo.HasKeyword;

                _host.HideSpacePopup();

                if (!_host.HandleAutoCompletePeriodKey(triggerInfo.PeriodPosition))
                {
                    ResetCtrlJKeyPressPending();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ResetCtrlJKeyPressPending();
                _host.HidePeriodPopup();
                _host.ShowException(ex);
                return false;
            }
        }

        public bool TryTriggerPeriodAutoCompleteOnIdentifierKeyUp(KeyEventArgs e)
        {
            try
            {
                if (_host.GetActiveAutoCompleteSession() != null)
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

                if (!TextHelper.IsEngAlphabetOrNumber(ch, '_'))
                {
                    return false;
                }

                var triggerInfo = _ctrlJPeriodResolver.Resolve(_host.IsSqlServer, _host.IsMySql);

                if (!triggerInfo.CanTrigger || !triggerInfo.HasKeyword)
                {
                    return false;
                }

                return _host.HandleAutoCompletePeriodKey(triggerInfo.PeriodPosition);
            }
            catch (Exception ex)
            {
                _host.HidePeriodPopup();
                _host.ShowException(ex);
                return false;
            }
        }

        private bool CanHandleCtrlJAutoComplete()
        {
            return _host.IsEditorFocused && _host.IsAutoListMembersEnabled;
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
    }
}
