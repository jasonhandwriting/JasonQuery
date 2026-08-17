namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal enum ColumnAddBuildFailureKind
    {
        None,
        MissingRequest,
        UnsupportedDataSource,
        MissingTableName,
        MissingColumnName,
        UnsafeColumnName,
        MissingDataType,
        InvalidDataType,
        InvalidDefaultValue
    }
}