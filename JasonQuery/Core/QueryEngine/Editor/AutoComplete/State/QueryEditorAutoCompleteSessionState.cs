namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.State
{
    internal sealed class QueryEditorAutoCompleteSessionState
    {
        public int TriggerPosition { get; private set; }

        public int CaretPosition { get; private set; }

        public bool IsOpen { get; private set; }

        public bool CommitByTab { get; private set; }

        public bool ReturnedToEditorByGridUp { get; private set; }

        public void Start(int triggerPosition, int caretPosition)
        {
            TriggerPosition = NormalizePosition(triggerPosition);
            CaretPosition = NormalizePosition(caretPosition);

            if (CaretPosition < TriggerPosition)
            {
                CaretPosition = TriggerPosition;
            }

            IsOpen = true;
            ResetTransientFlags();
        }

        public void UpdateTriggerPosition(int triggerPosition)
        {
            TriggerPosition = NormalizePosition(triggerPosition);

            if (CaretPosition < TriggerPosition)
            {
                CaretPosition = TriggerPosition;
            }
        }

        public void UpdateCaretPosition(int caretPosition)
        {
            CaretPosition = NormalizePosition(caretPosition);
        }

        public void MarkCommitByTab()
        {
            CommitByTab = true;
        }

        public bool ConsumeCommitByTab()
        {
            if (!CommitByTab)
            {
                return false;
            }

            CommitByTab = false;
            return true;
        }

        public void MarkReturnedToEditorByGridUp()
        {
            ReturnedToEditorByGridUp = true;
        }

        public bool ConsumeReturnedToEditorByGridUp()
        {
            if (!ReturnedToEditorByGridUp)
            {
                return false;
            }

            ReturnedToEditorByGridUp = false;
            return true;
        }

        public void Close(QueryEditorAutoCompleteSessionCloseReason reason)
        {
            IsOpen = false;
            ReturnedToEditorByGridUp = false;

            CommitByTab = reason == QueryEditorAutoCompleteSessionCloseReason.CommitByTab;
        }

        public void ResetTransientFlags()
        {
            CommitByTab = false;
            ReturnedToEditorByGridUp = false;
        }

        private static int NormalizePosition(int position)
        {
            return position < 0 ? 0 : position;
        }
    }
}