namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// sqlite_schema/sqlite_master object kinds supported by the migration contract.
    /// </summary>
    public enum JasonQueryDbStorageMigrationSchemaObjectKind : byte
    {
        Table = 1,
        Index = 2,
        View = 3,
        Trigger = 4
    }
}
