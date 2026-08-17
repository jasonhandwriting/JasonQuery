using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;

namespace JasonQuery.UI.QueryEditor.AutoComplete.State
{
    internal sealed class QueryEditorAutoCompleteSession
    {
        private readonly QueryEditorAutoCompleteSessionState _state = new QueryEditorAutoCompleteSessionState();

        public QueryEditorAutoCompleteSession(QueryEditorAutoCompletePopupKind kind, C1TrueDBGrid grid, QueryEditorAutoCompleteReplaceMode replaceMode)
        {
            Kind = kind;
            Grid = grid;
            ReplaceMode = replaceMode;
        }

        public QueryEditorAutoCompletePopupKind Kind { get; }

        public C1TrueDBGrid Grid { get; }

        public QueryEditorAutoCompleteReplaceMode ReplaceMode { get; }

        /// <summary>
        /// AutoComplete 的觸發位置。
        /// Period / Space: 通常是取代起點
        /// </summary>
        public int TriggerPosition
        {
            get { return _state.TriggerPosition; }
            set { _state.UpdateTriggerPosition(value); }
        }

        /// <summary>
        /// AutoComplete popup session 中的「editor 虛擬 caret」。
        /// Grid 取得焦點後，仍透過這個位置回寫 editor。
        /// </summary>
        public int CaretPosition
        {
            get { return _state.CaretPosition; }
            set { _state.UpdateCaretPosition(value); }
        }

        /// <summary>
        /// 某些模式在 editor 按 Up 時直接關閉 popup。
        /// </summary>
        public bool HideOnEditorUp { get; set; }

        /// <summary>
        /// 某些模式在 editor 按 Up 時直接關閉 popup。
        /// </summary>
        public bool CommitByTab
        {
            get { return _state.CommitByTab; }
        }

        /// <summary>
        /// 本次 AutoComplete 是否由 Grid 第 0 列按 Up 返回 editor。
        /// </summary>
        public bool ReturnedToEditorByGridUp
        {
            get { return _state.ReturnedToEditorByGridUp; }
        }

        public bool IsOpen
        {
            get { return _state.IsOpen; }
        }

        public bool IsVisible
        {
            get { return Grid != null && Grid.Visible; }
        }

        public void SetPositions(int triggerPosition, int caretPosition)
        {
            _state.Start(triggerPosition, caretPosition);
        }

        public void MarkCommitByTab()
        {
            _state.MarkCommitByTab();
        }

        public bool ConsumeCommitByTab()
        {
            return _state.ConsumeCommitByTab();
        }

        public void MarkReturnedToEditorByGridUp()
        {
            _state.MarkReturnedToEditorByGridUp();
        }

        public bool ConsumeReturnedToEditorByGridUp()
        {
            return _state.ConsumeReturnedToEditorByGridUp();
        }

        public void Close(QueryEditorAutoCompleteSessionCloseReason reason)
        {
            _state.Close(reason);
        }

        public void ResetTransientFlags()
        {
            _state.ResetTransientFlags();
        }
    }
}