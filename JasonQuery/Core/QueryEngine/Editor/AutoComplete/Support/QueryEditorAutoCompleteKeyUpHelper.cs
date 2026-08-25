using System.Windows.Forms;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support
{
    internal static class QueryEditorAutoCompleteKeyUpHelper
    {
        /// <summary>
        /// 這個方法用來排除：
        /// Ctrl/Alt 組合鍵
        /// 方向鍵
        /// Home/End/PageUp/PageDown
        /// Shift/Ctrl/Apps 等非輸入字元鍵
        /// Enter/Tab/Escape/Back/Delete 這些不是「新增一個識別字字元」的鍵
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool ShouldProcessAutoCompleteTextKeyUp(KeyEventArgs e)
        {
            if (e == null)
            {
                return false;
            }

            if (e.Control || e.Alt)
            {
                return false;
            }

            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.ControlKey:
                case Keys.ShiftKey:
                case Keys.Menu:
                case Keys.ProcessKey:
                case Keys.Apps:
                case Keys.Tab:
                case Keys.Enter:
                case Keys.Escape:
                case Keys.Back:
                case Keys.Delete:
                    {
                        return false;
                    }
                default:
                    {
                        return true;
                    }
            }
        }
    }
}
