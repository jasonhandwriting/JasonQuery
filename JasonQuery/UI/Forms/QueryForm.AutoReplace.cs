using JasonLibrary.Core;
using JasonQuery.Core.QueryEngine.Editor.Editing.AutoReplace;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        //Injected by MainForm before this editor is hosted or loaded.
        //Resolve the current published map at each trigger, never cache a copy.
        internal Func<IDictionary<string, string>> SharedAutoReplaceMapProvider { get; set; }

        private void HandleAutoReplaceOnSpaceKeyUp(KeyEventArgs e)
        {
            if (!MyLibrary.EnableAutoReplace || e.KeyCode != Keys.Space || e.Control)
            {
                return;
            }

            var map = SharedAutoReplaceMapProvider?.Invoke();

            if (map == null || map.Count == 0)
            {
                return;
            }

            var context = new EditorAutoReplaceContext
            {
                Editor = editor,
                AutoReplaceMap = map
            };

            _editorAutoReplaceService.TryHandleSpaceKeyUp(context, e);
        }

        private void InitializeAutoReplaceOnFormLoad()
        {
            tabAutoReplace.TabVisible = false;

            if (MyLibrary.EnableAutoReplace)
            {
                splitContainer3.SplitterWidth = 2;
            }
        }

        //Legacy designer event targets remain inert until the old UI is removed.
        private void c1GridAutoReplaceInfo_AfterUpdate(object sender, EventArgs e) { }
        private void c1GridAutoReplaceInfo_Enter(object sender, EventArgs e) { }
        private void c1GridAutoReplaceInfo_Leave(object sender, EventArgs e) { }
        private void c1GridAutoReplaceInfo_MouseMove(object sender, MouseEventArgs e) { }
        private void btnHelp_AutoReplace_Click(object sender, EventArgs e) { }
    }
}
