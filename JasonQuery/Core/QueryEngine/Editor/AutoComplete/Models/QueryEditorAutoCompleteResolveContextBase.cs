using JasonQuery.Core.Database.Connection;
using ScintillaNET;
using System;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models
{
    internal abstract class QueryEditorAutoCompleteResolveContextBase
    {
        public Scintilla Editor { get; set; }

        public DataSourceType CurrentSourceType { get; set; }

        public bool IsDataSourceSqlServer { get; set; }

        public bool IsDataSourceMySql { get; set; }

        public Func<bool, string> SelectCurrentBlock { get; set; }

        public Func<string, bool, bool, string> FormatSql { get; set; }

        public virtual void Validate()
        {
            if (Editor == null)
            {
                throw new ArgumentNullException(nameof(Editor));
            }

            if (SelectCurrentBlock == null)
            {
                throw new ArgumentNullException(nameof(SelectCurrentBlock));
            }

            if (FormatSql == null)
            {
                throw new ArgumentNullException(nameof(FormatSql));
            }
        }
    }
}
