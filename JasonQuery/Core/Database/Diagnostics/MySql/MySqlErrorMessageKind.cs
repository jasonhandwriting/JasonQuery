namespace JasonQuery.Core.Database.Diagnostics.MySql
{
    internal enum MySqlErrorMessageKind
    {
        None,
        UnknownColumn,
        AmbiguousColumn,
        DuplicateColumn,
        TableNotFound,
        UnknownDatabase,
        NoDatabaseSelected,
        SyntaxError
    }
}
