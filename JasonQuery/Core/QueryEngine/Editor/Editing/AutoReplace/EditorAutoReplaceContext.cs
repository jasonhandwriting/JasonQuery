using ScintillaNET;
using System;
using System.Collections.Generic;

namespace JasonQuery.Core.QueryEngine.Editor.Editing.AutoReplace
{
    internal sealed class EditorAutoReplaceContext
    {
        public Scintilla Editor { get; set; }

        public IDictionary<string, string> AutoReplaceMap { get; set; }

        public void Validate()
        {
            if (Editor == null)
            {
                throw new ArgumentNullException(nameof(Editor));
            }

            if (AutoReplaceMap == null)
            {
                throw new ArgumentNullException(nameof(AutoReplaceMap));
            }
        }
    }
}