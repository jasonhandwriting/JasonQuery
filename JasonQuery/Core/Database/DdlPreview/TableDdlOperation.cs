namespace JasonQuery.Core.Database.DdlPreview
{
    internal enum TableDdlOperation
    {
        None,
        Comment,
        Drop,
        Rename,
        Truncate
    }
}
