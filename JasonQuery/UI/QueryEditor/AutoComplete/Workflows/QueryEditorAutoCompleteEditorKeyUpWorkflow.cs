using System;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal interface IQueryEditorAutoCompleteEditorKeyUpWorkflowHost
    {
        bool IsAutoListMembersEnabled { get; }

        void ClearHighlightSelectionCopyState();

        bool ShouldSkipEditorContentCheckOnKeyUp(KeyEventArgs e);

        bool TryConsumeCompoundCtrlShiftKeyUp();

        bool ShouldIgnoreEditorKeyUp(KeyEventArgs e);

        void HandleAutoReplaceOnSpaceKeyUp(KeyEventArgs e);

        bool TryHandleAutoCompleteEditorKeyUp(KeyEventArgs e, ref bool checkEditorContent);

        bool TryTriggerPeriodAutoCompleteOnIdentifierKeyUp(KeyEventArgs e);

        bool TryCloseActiveAutoCompletePopupOnInvalidCharKeyUp(KeyEventArgs e);

        void RefreshVisibleAutoCompleteOnEditorKeyUp(KeyEventArgs e);

        void CheckEditorContent();

        void ShowException(Exception ex);
    }

    internal sealed class QueryEditorAutoCompleteEditorKeyUpWorkflow
    {
        private readonly IQueryEditorAutoCompleteEditorKeyUpWorkflowHost _host;

        public QueryEditorAutoCompleteEditorKeyUpWorkflow(IQueryEditorAutoCompleteEditorKeyUpWorkflowHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public void Handle(KeyEventArgs e)
        {
            var checkEditorContent = !_host.ShouldSkipEditorContentCheckOnKeyUp(e);

            _host.ClearHighlightSelectionCopyState();

            if (_host.TryConsumeCompoundCtrlShiftKeyUp())
            {
                return;
            }

            try
            {
                if (_host.ShouldIgnoreEditorKeyUp(e))
                {
                    return;
                }

                _host.HandleAutoReplaceOnSpaceKeyUp(e);

                if (!_host.IsAutoListMembersEnabled)
                {
                    return;
                }

                if (_host.TryHandleAutoCompleteEditorKeyUp(e, ref checkEditorContent))
                {
                    return;
                }

                if (_host.TryTriggerPeriodAutoCompleteOnIdentifierKeyUp(e))
                {
                    return;
                }

                if (_host.TryCloseActiveAutoCompletePopupOnInvalidCharKeyUp(e))
                {
                    return;
                }

                _host.RefreshVisibleAutoCompleteOnEditorKeyUp(e);
            }
            catch (Exception ex)
            {
                _host.ShowException(ex);
            }
            finally
            {
                if (checkEditorContent)
                {
                    _host.CheckEditorContent();
                }
            }
        }
    }
}