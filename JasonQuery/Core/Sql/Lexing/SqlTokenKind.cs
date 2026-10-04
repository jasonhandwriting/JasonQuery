namespace JasonQuery.Core.Sql.Lexing
{
    internal enum SqlTokenKind
    {
        None = 0,
        Word,
        DelimitedIdentifier,
        StringLiteral,
        LineComment,
        BlockComment,
        Symbol
    }
}
