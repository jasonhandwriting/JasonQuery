using System;

namespace JasonQuery.Core.Sql.Lexing
{
    internal sealed class SqlToken
    {
        public SqlToken(SqlTokenKind kind, string text, int start, int endExclusive, int depth)
        {
            Kind = kind;
            Text = text ?? string.Empty;
            Start = Math.Max(0, start);
            EndExclusive = Math.Max(Start, endExclusive);
            Depth = Math.Max(0, depth);
        }

        public SqlTokenKind Kind { get; }

        public string Text { get; }

        public int Start { get; }

        public int EndExclusive { get; }

        public int Length => EndExclusive - Start;

        public int Depth { get; }

        public bool IsComment => Kind == SqlTokenKind.LineComment || Kind == SqlTokenKind.BlockComment;

        public bool IsProtectedText => Kind == SqlTokenKind.StringLiteral || IsComment;

        public bool SuppressesKeywordMatching => IsProtectedText || Kind == SqlTokenKind.DelimitedIdentifier;

        public bool IsWord(string value)
        {
            return Kind == SqlTokenKind.Word && string.Equals(Text, value, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSymbol(string value)
        {
            return Kind == SqlTokenKind.Symbol && string.Equals(Text, value, StringComparison.Ordinal);
        }

        public bool ContainsPosition(int position)
        {
            return position >= Start && position < EndExclusive;
        }
    }
}
