using System;

namespace JasonQuery.Core.Sql.Lexing
{
    [Flags]
    internal enum SqlTokenizerOptions
    {
        None = 0,

        //Preserves the defensive behavior used by the existing AutoComplete tokenizer.
        RecognizeForeignDelimitedIdentifiers = 1,

        //Compatibility options used while existing consumers are migrated one at a time.
        MySqlDashCommentWithoutWhitespace = 2,
        MySqlBacktickIdentifierAllowsBackslashEscape = 4,
        MySqlDoubleQuotedTextAllowsBackslashEscape = 8,
        MySqlHashStartsCommentInsideWord = 16
    }
}
