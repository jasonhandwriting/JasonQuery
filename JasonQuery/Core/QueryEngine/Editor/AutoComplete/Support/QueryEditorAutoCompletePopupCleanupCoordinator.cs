using System;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support
{
    internal static class QueryEditorAutoCompletePopupCleanupCoordinator
    {
        public static void Cleanup(Action clearDataSource, Action hidePopup)
        {
            try
            {
                clearDataSource?.Invoke();
            }
            finally
            {
                hidePopup?.Invoke();
            }
        }
    }
}