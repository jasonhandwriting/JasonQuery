namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// sqlite_schema/sqlite_master object kinds supported by the migration contract.
    /// </summary>
    public enum DatabaseStorageMigrationSchemaObjectKind : byte
    {
        Table = 1,
        Index = 2,
        View = 3,
        Trigger = 4
    }
}
