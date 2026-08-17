using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support
{
    internal static class QueryEditorAutoCompleteSessionPolicy
    {
        public static int ClampEditorPosition(int position, int textLength)
        {
            var maximum = textLength < 0 ? 0 : textLength;

            if (position < 0)
            {
                return 0;
            }

            return position > maximum ? maximum : position;
        }

        public static bool ShouldHideOnEditorBackspace(int currentPosition, int triggerPosition)
        {
            return currentPosition <= triggerPosition;
        }

        public static bool ShouldHideAfterGridBackspace(int currentPosition, int triggerPosition)
        {
            return currentPosition < triggerPosition;
        }

        public static bool ShouldHideOnHorizontalMove(QueryEditorAutoCompleteTextRange activeRange, int currentPosition, int delta)
        {
            if (activeRange == null)
            {
                return true;
            }

            return !activeRange.ContainsPosition(currentPosition + delta);
        }
    }
}
